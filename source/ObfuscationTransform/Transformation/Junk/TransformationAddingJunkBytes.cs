using System;
using System.Collections.Generic;
using System.Linq;
using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.Extensions;

namespace ObfuscationTransform.Transformation.Junk
{
    public class TransformationAddingJunkBytes : TransformationBase, ITransformationAddingJunkBytes
    {
        private readonly IJunkBytesProvider m_junkBytesProvider;
        private readonly IDisassemblerFactory m_disassemblerFactory;

        public TransformationAddingJunkBytes(IInstructionWithAddressOperandDecider instructionWithAddressOperandDecider,
            IInstructionWithAddressOperandTransform instructionWithAddressOperandTransform,
            ICodeInMemoryLayoutFactory codeInMemoryLayoutFactory,
            ICodeFactory codeFactory,
            IInstructionWithAddressOperandTransform jumpInstrucionTransform,
            ICodeTransform codeTransform,
            IStatistics statistics,
            IRelocationDirectoryFromNewCode relocationDirectoryFromNewCode,
            IJunkBytesProvider junkBytesProvider,
            IDisassemblerFactory disassemblerFactory) :
            base(instructionWithAddressOperandDecider, instructionWithAddressOperandTransform,
                codeFactory, codeInMemoryLayoutFactory, codeTransform, statistics, 
                relocationDirectoryFromNewCode)
        {
            m_junkBytesProvider = junkBytesProvider ?? throw new ArgumentNullException(nameof(junkBytesProvider));
            m_disassemblerFactory = disassemblerFactory ?? throw new ArgumentNullException(nameof(disassemblerFactory));
        }

        public override ICode Transform(ICode code)
        {
            return base.Transform(code);
        }

        protected override ICode TransformCode(ICode code,
            Dictionary<ulong, IAssemblyInstructionForTransformation> addressToInstructionMap,
            Dictionary<ulong, ulong> addressesInInstructionMap)
        {
            //inicate weather to try insert junk in the next instruction.
            //a junk will be inserted if the instruction right after is the first in basic block
            bool insertJunk = false;


            //define the delegate that transform a single instruction. 
            //this delegate is passes to a code transformer that execute this delegate on each assembly instruction
            TryTransformInstructionDelegate transformInstructionDelegate =
                (IAssemblyInstructionForTransformation instruction, IBasicBlock basicBlock, IFunction function,
                 out List<IAssemblyInstructionForTransformation> listOfTransformedInstructions) =>
                {
                    var wasTransformed = false;
                    listOfTransformedInstructions = null;

                    //insert junk only inside a basic block
                    if (basicBlock == null)
                    {
                        //if the should insert junk is set, but next instructions are not nops turn off the junk
                        //insertion flag, because there is no basic block to insert the junk into
                        if (!instruction.IsNopInstruction()) insertJunk = false;
                        return wasTransformed;
                    }

                    //try insert junk instruction
                    if (insertJunk)
                    {
                        if (TryGenerateJunkInstruction(basicBlock, m_junkBytesProvider,m_statistics, 
                            out IAssemblyInstructionForTransformation junkInstruction))
                        {
                            m_statistics.IncrementAddedInstructions(1,(uint)junkInstruction.Bytes.Length);
                            listOfTransformedInstructions = new List<IAssemblyInstructionForTransformation>
                            {
                                //insert junk insruction before the first instruction of the basic block
                                junkInstruction,
                                instruction
                            };
                            insertJunk = false;
                            wasTransformed = true;
                        }
                        //can not insert junk in the basic block. do not try to insert junk bytes
                        else
                        {
                            insertJunk = false;
                            return wasTransformed;
                        }
                    }

                    //The it is last instruction in basic block, and if it is unconditional
                    if (basicBlock.AssemblyInstructions.Last() == instruction)
                    {
                        if (m_instructionWithAddressOperandDecider.IsUnconditionalJumpInstruction(instruction.Mnemonic) ||
                            m_instructionWithAddressOperandDecider.IsReturnInstruction(instruction))
                        {
                            insertJunk = true;
                        }
                    }

                    return wasTransformed;
                };


            return m_codeTransform.Transform(code, transformInstructionDelegate);
        }


        private bool TryGenerateJunkInstruction(IBasicBlock basicBlock,
            IJunkBytesProvider junkBytesProvider,
            IStatistics statistics,
            out IAssemblyInstructionForTransformation junkInstruction)
        {
            junkInstruction = null;
            bool retVal = false;

            //get junk bytes
            byte[] junkBytes = junkBytesProvider.GetJunkBytes();
            if (junkBytes == null) throw new NullReferenceException(nameof(junkBytes));

            //get basic block bytes
            byte[] basicBlockBytes = basicBlock.GetAllBytes();

            //the size of the junk we try to insert
            uint junkBytesSize = (uint)junkBytes.Count();

            //the inde of the most far insruction that has been syncronized
            uint mostFarSyncInstructionIdx = 0;
            //the partial instruction that causes the synchronization to be far as possible
            uint bestPartialInstructionSize = 0;

            //check what sub array of the inserted junk causes the most of confusion
            //so instruction are disassembled not correctly as far as possible
            while (junkBytesSize > 0)
            {
                uint sizeOfNewBytesArray = basicBlock.NumberOfBytes + junkBytesSize;
                byte[] newBasicBlockBytes = new byte[sizeOfNewBytesArray];

                //copy junk bytes and then basic block bytes
                Array.Copy(junkBytes, newBasicBlockBytes, junkBytesSize);
                basicBlockBytes.CopyTo(newBasicBlockBytes, junkBytesSize);

                //the index of first identical disassembled instruction to original
                uint? indexOfFirstIdenticalInstruction = CompareDisassembleToOriginal(newBasicBlockBytes, basicBlock, 
                    m_disassemblerFactory);

                //in case that there is no even one instruction that synchronizes, so try another sequence
                //because the parsing error can not proceed to the next block
                if (!indexOfFirstIdenticalInstruction.HasValue)
                {
                    junkBytesSize--;
                    continue;
                }

                if (indexOfFirstIdenticalInstruction.Value > mostFarSyncInstructionIdx)
                {
                    mostFarSyncInstructionIdx = indexOfFirstIdenticalInstruction.Value;
                    bestPartialInstructionSize = junkBytesSize;
                }

                junkBytesSize--;
            }

            //update basic block with the instruction that causes the synchronization to be as far as
            //possible
            if (mostFarSyncInstructionIdx > 0)
            {
                byte[] junkBytesSelected = new byte[bestPartialInstructionSize];
                for (int i = 0; i < bestPartialInstructionSize; i++) junkBytesSelected[i] = junkBytes[i];

                var disasm = m_disassemblerFactory.Create(junkBytesSelected);
                junkInstruction = disasm.Disassemble().First();
                junkInstruction.IsNew = true;

                statistics.IncrementMissinterpretedInstructions(mostFarSyncInstructionIdx);
                statistics.IncrementJunkInstructions(1,(uint)junkInstruction.Length);
                retVal = true;
            }

            return retVal;

        }

        /// <summary>
        /// Find the first instrucion index that is not affected by the junk insertion in the new block.
        /// In other words: it Finds the first instruction in the new block which is identical to instruction
        /// in the original basic block. the identical instruction start an identical sequence of instructions 
        /// untill the end of the two blocks.
        /// </summary>
        /// <param name="newBasicBlockBytes">new basic block bytes</param>
        /// <param name="basicBlock">original basic block</param>
        /// <param name="disassemblerFactory">diassembler factory</param>
        /// <returns>the first index of unaffected instruction in the original basic block, or not o value otherwise</returns>
        private uint? CompareDisassembleToOriginal(byte[] newBasicBlockBytes, IBasicBlock basicBlock,
            IDisassemblerFactory disassemblerFactory)
        {
            //disassemble new bytes after junk have been inserted
            var newAssemblyInstructions = new List<IAssemblyInstructionForTransformation>();
            IDisassembler disasm = disassemblerFactory.Create(newBasicBlockBytes);
            foreach (var instruction in disasm.Disassemble())
            {
                newAssemblyInstructions.Add(instruction);
            }

            //not enought instruction have been decoded right in this block!
            if (newAssemblyInstructions.Count <= 1) return null;

            //compare to original basic block instructions
            //start from index 1 because first instruction is the inserted junk...
            for (int i = 1; i < newAssemblyInstructions.Count(); i++)
            {
                for (int j = 0; j < basicBlock.AssemblyInstructions.Count; j++)
                {
                    //compare instruction until the end if they are equal
                    if (newAssemblyInstructions[i].BytesEquals(basicBlock.AssemblyInstructions[j]))
                    {
                        if (instructionAreIdenticalUntilTheEnd(newAssemblyInstructions, i + 1, basicBlock.AssemblyInstructions, j + 1))
                        {
                            return (uint?)j;
                        }
                    }
                }
            }

            return null;
        }

        private bool instructionAreIdenticalUntilTheEnd(IReadOnlyList<IAssemblyInstructionForTransformation> list1, int index1,
            IReadOnlyList<IAssemblyInstructionForTransformation> list2, int index2)
        {
            if (list1 == null) throw new ArgumentNullException(nameof(list1));
            if (list2 == null) throw new ArgumentNullException(nameof(list2));

            if (list1.Count - index1 != list2.Count - index2) return false;

            int instructionToCompare = (list1.Count - index1);
            for (int i = 0; i < instructionToCompare; i++)
            {
                if (!list1[index1 + i].BytesEquals(list2[index2 + i])) return false;
            }

            return true;
        }


    }
}
