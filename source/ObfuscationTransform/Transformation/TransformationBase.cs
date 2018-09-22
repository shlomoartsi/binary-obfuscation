using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.Extensions;
using ObfuscationTransform.Transformation.Junk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    public abstract class TransformationBase
    {
        #region private variables
        //injected member
        protected readonly IInstructionWithAddressOperandDecider m_instructionWithAddressOperandDecider;
        protected readonly IInstructionWithAddressOperandTransform m_instructionWithAddressOperandTransform;
        protected readonly ICodeInMemoryLayoutFactory m_codeInMemoryLayoutFactory;
        protected readonly ICodeTransform m_codeTransform;
        protected readonly IStatistics m_statistics;
        protected readonly IRelocationDirectoryFromNewCode m_relocationDirectoryFromNewCode;

        //private members
        protected Dictionary<ulong, IAssemblyInstructionForTransformation> m_addressToInstructionMap;
        protected Dictionary<ulong, ulong> m_addressesInInstructionMap;
        
        #endregion

        public TransformationBase(IInstructionWithAddressOperandDecider instructionWithAddressOperandDecider,
            IInstructionWithAddressOperandTransform instructionWithAddressOperandTransform,
            ICodeFactory codeFactory,
            ICodeInMemoryLayoutFactory codeInMemoryLayoutFactory,
            ICodeTransform codeTransform,
            IStatistics statistics,
            IRelocationDirectoryFromNewCode relocationDirectoryFromNewCode)
        {
            m_instructionWithAddressOperandDecider = instructionWithAddressOperandDecider ?? throw new ArgumentNullException(nameof(instructionWithAddressOperandDecider));
            m_instructionWithAddressOperandTransform = instructionWithAddressOperandTransform ?? throw new ArgumentNullException(nameof(instructionWithAddressOperandTransform));
            m_codeInMemoryLayoutFactory = codeInMemoryLayoutFactory ?? throw new ArgumentNullException(nameof(codeInMemoryLayoutFactory));
            m_codeTransform = codeTransform ?? throw new ArgumentNullException(nameof(codeTransform));
            m_statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
            m_relocationDirectoryFromNewCode = relocationDirectoryFromNewCode ?? throw new ArgumentNullException(nameof(relocationDirectoryFromNewCode));
        }

        public virtual ICode Transform(ICode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            //create a map from offset address to instruction
            m_addressToInstructionMap = CreateAddressToInstructionMap(code);

            //create a map with the addresses in operands of instructions offset address,
            //leaving value empty
            m_addressesInInstructionMap = CreateAddresesInInstructionMap(code, m_addressToInstructionMap, m_instructionWithAddressOperandDecider);
                        
            var newCode = ExpandTargetAddressOfJumpInstructionTo4Bytes(code,
                                m_instructionWithAddressOperandDecider,
                                m_instructionWithAddressOperandTransform, m_codeTransform, m_statistics);

            newCode = TransformCode(newCode,m_addressToInstructionMap,m_addressesInInstructionMap);

            newCode = UpdateJumpInstructionsTargetAddress(newCode, m_instructionWithAddressOperandDecider,
                                                  m_instructionWithAddressOperandTransform, m_codeTransform,
                                                  m_addressToInstructionMap, m_codeInMemoryLayoutFactory,
                                                  m_relocationDirectoryFromNewCode,
                                                  m_addressesInInstructionMap);

            ValidateCodeJumpInstructions(newCode, m_instructionWithAddressOperandDecider);
            return newCode;
        }

        protected abstract ICode TransformCode(ICode code,
            Dictionary<ulong, IAssemblyInstructionForTransformation> addressToInstructionMap,
            Dictionary<ulong, ulong> addressesInInstructionMap);

        protected virtual Dictionary<ulong, IAssemblyInstructionForTransformation> CreateAddressToInstructionMap(ICode code)
        {
            var addressToInstructionMap = new Dictionary<ulong, IAssemblyInstructionForTransformation>();
            foreach (var instruction in code.AssemblyInstructions)
            {
                addressToInstructionMap.Add(instruction.Offset, instruction);
            }
            return addressToInstructionMap;
        }

        /// <summary>
        /// Create a dictionary that holds the addresses that appears in dictionary
        /// </summary>
        /// <param name="code"></param>
        /// <param name="addressToInstructionMap"></param>
        /// <param name="InstructionWithAddressOperandDecider"></param>
        /// <returns></returns>
        private Dictionary<ulong, ulong> CreateAddresesInInstructionMap(ICode code,
            Dictionary<ulong, IAssemblyInstructionForTransformation> addressToInstructionMap,
            IInstructionWithAddressOperandDecider InstructionWithAddressOperandDecider)
        {
            var addressesInInstructionsDictionary = new Dictionary<ulong, ulong>();

            //this loop prepares the map, and create entry for each old target
            foreach (var instruction in code.AssemblyInstructions)
            {
                bool result = false;
                ulong instructionTarget = ulong.MaxValue;

                bool isJumpTargetOperand =
                    InstructionWithAddressOperandDecider.IsJumpInstructionWithRelativeAddressOperand(instruction) ||
                    InstructionWithAddressOperandDecider.IsCallInstructionWithAddressOperand(instruction);

                bool isAbsoluteAddressInOperand =
                    InstructionWithAddressOperandDecider.IsInstructionWithAbsoluteAddressOperand(
                        instruction, code.CodeInMemoryLayout, out instructionTarget);


                if (isJumpTargetOperand)
                {
                    result = instruction.TryGetAbsoluteAddressFromRelativeAddress(out instructionTarget);
                    if (result && !addressToInstructionMap.ContainsKey(instructionTarget))
                    {
                        throw new ApplicationException($"instruction: '{instruction}' with target address {instructionTarget} do not exist!");
                    }
                }
                else if (isAbsoluteAddressInOperand)
                {
                    instructionTarget -= (code.CodeInMemoryLayout.ImageBaseAddress + code.CodeInMemoryLayout.CodeVirtualAddress);
                    result = true;
                }


                if (result)
                {
                    //Create entry for the instruction target address 
                    addressesInInstructionsDictionary[instructionTarget] = 0;
                }
            }

            //add the entry point address to the map, becuase we need to update its new address
            addressesInInstructionsDictionary[code.CodeInMemoryLayout.EntryPointOffset] = 0;

            return addressesInInstructionsDictionary;
        }

        private ICode ExpandTargetAddressOfJumpInstructionTo4Bytes(ICode code,
            IInstructionWithAddressOperandDecider InstructionWithAddressOperandDecider,
            IInstructionWithAddressOperandTransform InstructionWithAddressOperandTransform,
            ICodeTransform codeTransform,
            IStatistics statistics)
        {
            TryTransformInstructionDelegate transformInstructionDelegate =
                (IAssemblyInstructionForTransformation instruction, IBasicBlock basicBlock, IFunction function,
                 out List<IAssemblyInstructionForTransformation> listOfTransformedInstructions) =>
                {
                    listOfTransformedInstructions = null;
                    var wasTransformed = false;
                    //replace jump instruction to jump instructions with 4 bytes operand=
                    if (InstructionWithAddressOperandDecider.IsJumpInstructionWithRelativeAddressOperand(instruction) &&
                         IsJumpInstructionToCodeSection(code, instruction))
                    {
                        if (InstructionWithAddressOperandTransform.TryTransformJumpInstructionTo4BytesOperandSize(instruction,
                            out IAssemblyInstructionForTransformation transformedInstruction))
                        {
                            wasTransformed = true;
                            listOfTransformedInstructions = new List<IAssemblyInstructionForTransformation>() { transformedInstruction };
                            statistics.IncrementInstructionExpanded((uint)(transformedInstruction.Bytes.Length - instruction.Bytes.Length));
                        }
                    }

                    return wasTransformed;
                };

            return codeTransform.Transform(code, transformInstructionDelegate);
        }

        private bool IsJumpInstructionToCodeSection(ICode code, IAssemblyInstructionForTransformation instruction)
        {
            return instruction.GetAbsoluteAddressFromRelativeAddress() < (code.CodeInMemoryLayout.CodeVirtualAddress + code.CodeInMemoryLayout.CodeActualSizeInBytes);
        }
        
        private ICode UpdateJumpInstructionsTargetAddress(ICode code,
            IInstructionWithAddressOperandDecider InstructionWithAddressOperandDecider,
            IInstructionWithAddressOperandTransform InstructionWithAddressOperandTransform,
            ICodeTransform codeTransform,
            Dictionary<ulong, IAssemblyInstructionForTransformation> addressToInstructionMap,
            ICodeInMemoryLayoutFactory codeInMemoryLayoutFactory,
            IRelocationDirectoryFromNewCode relocationDirectoryFromNewCode,
            Dictionary<ulong, ulong> jumpInstructionsMap)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (code.AssemblyInstructions == null || code.AssemblyInstructions.Count == 0) throw new ArgumentException("code.AssemblyInstructions");
            if (InstructionWithAddressOperandDecider == null) throw new ArgumentNullException(nameof(InstructionWithAddressOperandDecider));
            if (InstructionWithAddressOperandTransform == null) throw new ArgumentNullException(nameof(InstructionWithAddressOperandTransform));
            if (codeInMemoryLayoutFactory == null) throw new ArgumentNullException(nameof(codeInMemoryLayoutFactory));
            if (relocationDirectoryFromNewCode == null) throw new ArgumentNullException(nameof(relocationDirectoryFromNewCode));

            //this is used map for the new and old addresses
            var oldToNewAddressDictionary = jumpInstructionsMap;

            //PASS 1:
            //map old instruction address to new address, 
            //because new instructions are going to be added in between, and other instructions
            //are going to be expanded. 
            ulong newOffset = 0;
            foreach (var instruction in code.AssemblyInstructions)
            {
                //look for each byte in instruction, because the addresses in operand 
                //can be an instruction or somewhere INSIDE the instruction
                //for example: movzx eax, byte [edx+0x411d8c]
                //where edx = 0xbc, and 0x411d8c is no instruction
                for (ulong i = 0; i < (ulong)instruction.Bytes.Length; i++)
                {
                    if (oldToNewAddressDictionary.ContainsKey(instruction.Offset + i))
                    {
                        oldToNewAddressDictionary[instruction.Offset + i] = newOffset + i;
                    }
                }

                newOffset = newOffset + (ulong)instruction.Bytes.Length;
            }


            //PASS 2:Create the new relocation directory
            var newRelocationDirectoryInfo =
                relocationDirectoryFromNewCode.CreateNewRelocationDirectoryInfo(code);

            //PASS 3:
            //set the target jump address to the new address and then change the instruction offset
            newOffset = 0;
            ulong newProgramCounter = 0;

            TryTransformInstructionDelegate tryTransformDelegate = (IAssemblyInstructionForTransformation instruction, IBasicBlock basicBlock,
                IFunction function, out List<IAssemblyInstructionForTransformation> transformedInstructions) =>
            {
                transformedInstructions = null;
                var retVal = false;
                //old jump target address or old address in operand
                ulong oldAddress;

                newProgramCounter = newProgramCounter + (ulong)instruction.Bytes.Length;

                bool isJumpTargetOperand =
                    InstructionWithAddressOperandDecider.IsJumpInstructionWithRelativeAddressOperand(instruction) ||
                    InstructionWithAddressOperandDecider.IsCallInstructionWithAddressOperand(instruction);

                bool isAbsoluteAddressInOperand =
                    InstructionWithAddressOperandDecider.IsInstructionWithAbsoluteAddressOperand(instruction,
                    code.CodeInMemoryLayout, out oldAddress);


                if (isJumpTargetOperand)
                {
                    transformedInstructions = new List<IAssemblyInstructionForTransformation>();

                    oldAddress = instruction.GetAbsoluteAddressFromRelativeAddress();
                    if (!oldToNewAddressDictionary.TryGetValue(oldAddress, out ulong newTargetAddress))
                    {
                        throw new ApplicationException("jump instruction target should exist on map");
                    }

                    var transformedInstruction = InstructionWithAddressOperandTransform.
                            CreateJumpInstructionWithNewTargetAddress(instruction, newProgramCounter, newOffset,
                                                                                                newTargetAddress);

                    transformedInstructions.Add(transformedInstruction);
                    retVal = true;
                }
                else if (isAbsoluteAddressInOperand)
                {
                    transformedInstructions = new List<IAssemblyInstructionForTransformation>();

                    ulong virtualOldAddress = (oldAddress - code.CodeInMemoryLayout.ImageBaseAddress) -
                        code.CodeInMemoryLayout.CodeVirtualAddress;

                    if (!oldToNewAddressDictionary.TryGetValue(virtualOldAddress, out ulong newAddressInOperand))
                    {
                        throw new ApplicationException("jump instruction target should exist on map");
                    }

                    newAddressInOperand += (code.CodeInMemoryLayout.ImageBaseAddress +
                                         code.CodeInMemoryLayout.CodeVirtualAddress);

                    var transformedInstruction = InstructionWithAddressOperandTransform.
                            CreateInstructionWithNewAddress(code, instruction, newProgramCounter, newOffset,
                                                       oldAddress, newAddressInOperand);

                    transformedInstructions.Add(transformedInstruction);
                    retVal = true;
                }
                else
                {
                    instruction.SetPC(newProgramCounter);
                    instruction.SetOffset(newOffset);
                }

                newOffset = newProgramCounter;
                return retVal;
            };

            //take care for updating the new entry point address, and new relocation diretory
            //define a factory to create the new code in memory layout instance
            Func<ICodeInMemoryLayout> codeInMemoryLayoutFactoryDelegate = () =>
                            codeInMemoryLayoutFactory.Create(
                            code.CodeInMemoryLayout.ImageBaseAddress,
                            code.CodeInMemoryLayout.OffsetOfCodeInBytes,
                            newProgramCounter,
                            code.CodeInMemoryLayout.CodePhysicalSizeInBytes,
                            oldToNewAddressDictionary[code.CodeInMemoryLayout.EntryPointOffset],
                            code.CodeInMemoryLayout.CodeVirtualAddress,
                            newRelocationDirectoryInfo);

            var newCode = codeTransform.Transform(code, tryTransformDelegate,
                codeInMemoryLayoutFactoryDelegate);

            return newCode;
        }


        private void ValidateCodeJumpInstructions(ICode code, IInstructionWithAddressOperandDecider InstructionWithAddressOperandDecider)
        {
            var map = new Dictionary<ulong, IAssemblyInstructionForTransformation>();
            foreach (var instruction in code.AssemblyInstructions)
            {
                map.Add(instruction.Offset, instruction);
            }

            foreach (var instruction in code.AssemblyInstructions)
            {
                if (InstructionWithAddressOperandDecider.IsJumpInstructionWithRelativeAddressOperand(instruction))
                {
                    ulong jumpTarget = instruction.GetAbsoluteAddressFromRelativeAddress();
                    if (!map.ContainsKey(jumpTarget))
                    {
                        throw new ApplicationException("code is incorrect, jump target is to no instruction");
                    }
                }
            }

        }

        
    }
}
