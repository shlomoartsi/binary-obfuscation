using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.Extensions;
using ObfuscationTransform.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    public class TransformationAddingUnconditionalJump :  TransformationBase,ITransformationAddingUnconditionalJump
    {
        ICodeParser m_codeParser;

        public TransformationAddingUnconditionalJump(IInstructionWithAddressOperandDecider instructionWithAddressOperandDecider,
            IInstructionWithAddressOperandTransform instructionWithAddressOperandTransform,
            ICodeFactory codeFactory,
            ICodeInMemoryLayoutFactory codeInMemoryLayoutFactory,
            ICodeTransform codeTransform,
            IStatistics statistics,
            IRelocationDirectoryFromNewCode relocationDirectoryFromNewCode,
            ICodeParser codeParser):base(instructionWithAddressOperandDecider,
                instructionWithAddressOperandTransform,codeFactory,codeInMemoryLayoutFactory,codeTransform,
                statistics,relocationDirectoryFromNewCode)
        {
            m_codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
        }


        public override ICode Transform(ICode code)
        {
            return base.Transform(code);

        }

        protected override ICode TransformCode(ICode code, Dictionary<ulong, 
            IAssemblyInstructionForTransformation> addressToInstructionMap, 
            Dictionary<ulong, ulong> addressesInInstructionMap)
        {
            
            //define the delegate that transform a single instruction. 
            //this delegate is passes to a code transformer that execute this delegate on each assembly instruction
            TryTransformInstructionDelegate transformInstructionDelegate =
                (IAssemblyInstructionForTransformation instruction, IBasicBlock basicBlock, IFunction function,
                    out List<IAssemblyInstructionForTransformation> listOfTransformedInstructions) =>
                {
                    listOfTransformedInstructions = null;

                    //change unconditional jumps only inside a basic block
                    if (basicBlock == null) return false;
                    //transformation made only on conditional jump intructions
                    if (!m_instructionWithAddressOperandDecider.IsConditionalJumpInstruction(instruction.Mnemonic)) return false;

                    //because the new inserted jump instruction target address is the instruction after
                    if (instruction.NextInstruction == null) return false;

                    //transform an unconditional jump instruction.
                    //jump by condition C to address A 
                    //instruction after
                    //=>
                    // jump by inverse condition to address of instruction after
                    //Jump to address A
                    //instruction after

                    //for example
                    //            instruction address    instruction    operands
                    //            000017ba               ja             0x17e7
                    //            000017c0               mov            ecx, [eax * 4 + 0x41a00c]
                    // is tranformed into
                    //            000017ba               jbe            000017c5
                    //            000017c0               jmp            0x17e7 
                    //            000017c5               mov            ecx, [eax * 4 + 0x41a00c]



                    //IGenericTransform.UpdateJumpInstructionsTargetAddress updates the target address
                    //of all instructions to the new one so, this transformation create those instructions
                    //            instruction address    instruction    operands
                    //            000017ba               jbe            000017c0 (will be updated to 000017c5)
                    //            000017ba               jmp            0x17e7 (will updated to 0x17e7 +5)
                    //            000017c0               mov            ecx, [eax * 4 + 0x41a00c]


                    //create a new negative jump to the next instruction
                    IAssemblyInstructionForTransformation newConditionalJumpInstruction;
                    if (!m_instructionWithAddressOperandTransform.TryCreateNegativeJumpIntruction(
                                            instruction, 0, out newConditionalJumpInstruction)) return false;
                    newConditionalJumpInstruction.SetOffset(instruction.Offset);
                    newConditionalJumpInstruction.SetPC(instruction.PC);

                    //create a new jump instruction to the same address as the original condition jump condition
                    var jumpAddress = instruction.GetRelativeAddress();
                    var newJumpInstruction = m_instructionWithAddressOperandTransform.
                                            CreateUnconditionalJumpInstruction(jumpAddress);
                    newJumpInstruction.IsNew = true;
                    newJumpInstruction.SetPC(instruction.PC);
                    newJumpInstruction.SetOffset(instruction.Offset);
                    
                    listOfTransformedInstructions = new List<IAssemblyInstructionForTransformation>()
                                            { newConditionalJumpInstruction,newJumpInstruction};

                    //update dictionary of target addresses that apears in program.
                    //this addresses have to be replaced later in the instructions that contain them (as operand).
                    addressesInInstructionMap[instruction.NextInstruction.Offset]=0;

                    var addedBytesForConditionalJump = instruction.Bytes.Length - 
                                                                        newConditionalJumpInstruction.Bytes.Length;
                    m_statistics.IncrementAddedInstructions(1, 
                                            (uint)(newJumpInstruction.Bytes.Length + addedBytesForConditionalJump));
                    return true;
                };

            var newCode = m_codeTransform.Transform(code,transformInstructionDelegate);
            //after this transformation there are more basic blocks, so parse them again
            return m_codeParser.ParseCode(newCode.AssemblyInstructions, newCode.CodeInMemoryLayout);
        }
    }
}
