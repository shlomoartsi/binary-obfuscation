using ObfuscationTransform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    
    public interface IInstructionWithAddressOperandTransform
    {
        bool TryTransformJumpInstructionTo4BytesOperandSize(IAssemblyInstructionForTransformation instruction,
            out IAssemblyInstructionForTransformation transformedInstruction);

        IAssemblyInstructionForTransformation IncrementJumpInstructionTargetAddress(IAssemblyInstructionForTransformation instruction,
            int addressShift);

        IAssemblyInstructionForTransformation CreateJumpInstructionWithNewTargetAddress(
            IAssemblyInstructionForTransformation instruction, 
            ulong newProgramCounter, ulong newOffset, ulong newTargetAddress);

        IAssemblyInstructionForTransformation CreateUnconditionalJumpInstruction(int instructionOffset);

        IAssemblyInstructionForTransformation CreateInstructionWithNewAddress(ICode code,
            IAssemblyInstructionForTransformation instruction, ulong newProgramCounter,
            ulong newOffset, ulong oldAddressInOperand, ulong newAddressInOperand);

        IAssemblyInstructionForTransformation CreateNegativeJumpInstruction(IAssemblyInstructionForTransformation instruction,
            int instructionOffset);

        bool TryCreateNegativeJumpIntruction(IAssemblyInstructionForTransformation instruction,
            int instructionOffset,out IAssemblyInstructionForTransformation newInstruction);

    }
}
