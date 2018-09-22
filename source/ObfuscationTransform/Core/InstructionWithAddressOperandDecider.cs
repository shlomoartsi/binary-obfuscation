using SharpDisasm.Udis86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public class InstructionWithAddressOperandDecider : IInstructionWithAddressOperandDecider
    {
        public bool IsJumpInstruction(ud_mnemonic_code mnemonicCode)
        {
            return mnemonicCode >= ud_mnemonic_code.UD_Ija && mnemonicCode <= ud_mnemonic_code.UD_Ijz;
        }

        
        public bool IsJumpInstructionWithRegisterOperand(IAssemblyInstructionForTransformation instruction)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null || instruction.Operands.Length == 0) return false;
            return IsJumpInstruction(instruction.Mnemonic) &&
                instruction.Operands[0].Type == ud_type.UD_OP_REG;
        }

        public bool IsJumpInstructionWithRelativeAddressOperand(IAssemblyInstructionForTransformation instruction)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null || instruction.Operands.Length == 0) return false;
            return IsJumpInstruction(instruction.Mnemonic) &&
                instruction.Operands[0].Type == ud_type.UD_OP_JIMM;
        }


        public bool IsUnconditionalJumpInstruction(ud_mnemonic_code mnemonicCode)
        {
            return mnemonicCode == ud_mnemonic_code.UD_Ijmp;
        }

        public bool IsConditionalJumpInstruction(ud_mnemonic_code mnemonicCode)
        {
            return mnemonicCode >= ud_mnemonic_code.UD_Ija && 
                   mnemonicCode <= ud_mnemonic_code.UD_Ijz &&
                   mnemonicCode != ud_mnemonic_code.UD_Ijmp;
        }

        public bool IsCallInstruction(IAssemblyInstructionForTransformation instruction)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Mnemonic != ud_mnemonic_code.UD_Icall) return false;
            return  (instruction.Operands != null && instruction.Operands.Length == 1) ;
        }

        public bool IsCallInstructionWithAddressOperand(IAssemblyInstructionForTransformation instruction)
        {
            return IsCallInstruction(instruction) &&
                     instruction.Operands[0].Type == ud_type.UD_OP_JIMM;

        }

        public bool IsReturnInstruction(IAssemblyInstructionForTransformation instruction)
        {
            return instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iiretw ||
                instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iiretd ||
                instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iiretq ||
                instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iret ||
                instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iretf;
        }

        
        public bool IsInstructionWithAbsoluteAddressOperand(
            IAssemblyInstructionForTransformation instruction, 
            ICodeInMemoryLayout codeInMemoryLayout,out ulong addressOperand)
        {
            var codeBeginAddress = codeInMemoryLayout.CodeVirtualAddress +
                    codeInMemoryLayout.ImageBaseAddress;
            var codeEndAddress = codeInMemoryLayout.CodeVirtualAddress +
                codeInMemoryLayout.CodePhysicalSizeInBytes +
                codeInMemoryLayout.ImageBaseAddress;

            addressOperand = ulong.MaxValue;

            if (instruction.Operands == null || instruction.Operands.Length == 0) return false;
            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (instruction.Operands[i].SignedValue >= (long)codeBeginAddress &&
                    instruction.Operands[i].SignedValue < (long)codeEndAddress)
                {
                    if (instruction.Mnemonic == ud_mnemonic_code.UD_Ijmp) System.Diagnostics.Debugger.Break();
                    addressOperand = (ulong)instruction.Operands[i].SignedValue;
                    return true;
                }
            }

            return false;
        }
    }
}
