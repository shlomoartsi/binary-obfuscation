using SharpDisasm.Udis86;

namespace ObfuscationTransform.Core
{
    public interface IInstructionWithAddressOperandDecider
    {
        bool IsJumpInstruction(ud_mnemonic_code mnemonicCode);

        bool IsUnconditionalJumpInstruction(ud_mnemonic_code mnemonicCode);

        bool IsConditionalJumpInstruction(ud_mnemonic_code mnemonicCode);

        bool IsJumpInstructionWithRegisterOperand(
            IAssemblyInstructionForTransformation instruction);

        bool IsJumpInstructionWithRelativeAddressOperand(
            IAssemblyInstructionForTransformation instruction);

        bool IsCallInstruction(
            IAssemblyInstructionForTransformation instruction);

        bool IsCallInstructionWithAddressOperand(
            IAssemblyInstructionForTransformation instruction);

        bool IsReturnInstruction(
            IAssemblyInstructionForTransformation instruction);

        bool IsInstructionWithAbsoluteAddressOperand(
            IAssemblyInstructionForTransformation instruction,
            ICodeInMemoryLayout codeInMemoryLayout, out ulong addressOperand);
    }
}