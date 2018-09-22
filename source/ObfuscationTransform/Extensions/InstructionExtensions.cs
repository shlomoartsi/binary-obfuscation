using ObfuscationTransform.Core;
using SharpDisasm;
using SharpDisasm.Udis86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Extensions
{
    /// <summary>
    /// Target address mode - getting the address shift or absolute jump target address
    /// </summary>
    public enum InstructionToStringTargetAddressMode
    {
        /// <summary>
        /// Address shift
        /// </summary>
        AddressShift,
        /// <summary>
        /// Absolute address
        /// </summary>
        AbsoluteAddress
    };

    public static class InstructionExtensions
    {
        public static bool BytesEquals(this IInstruction instruction1, IInstruction instruction2)
        {
            if (instruction1 == null) throw new ArgumentNullException(nameof(instruction1));
            if (instruction2 == null) throw new ArgumentNullException(nameof(instruction2));
            return instruction1.Bytes.SequenceEqual(instruction2.Bytes);
        }


        public static bool TryParseInstructionTargetAddress(this IInstruction instruction, InstructionToStringTargetAddressMode mode, out long targetAddress)
        {
            targetAddress = 0;
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null) throw new ArgumentNullException(nameof(instruction) + "Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can be 0");
            if (instruction.Operands[0].Type != ud_type.UD_OP_JIMM) return false;

            int i = 0;
            while (instruction.Operands[i] != null && instruction.Operands[i].Type != ud_type.UD_OP_JIMM && instruction.Operands.Count() > i) { i++; }

            if (instruction.Operands[i] == null) return false;

            if (mode == InstructionToStringTargetAddressMode.AbsoluteAddress) targetAddress = instruction.Operands[i].SignedValue + (long)instruction.PC;
            else targetAddress = instruction.Operands[i].SignedValue;

            return true;

        }

        public static bool TryGetAbsoluteAddressFromRelativeAddress(this IInstruction instruction,
            out ulong absoluteAddress)
        {
            absoluteAddress = 0;
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null) throw new ArgumentNullException(nameof(instruction) + "Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can be 0");
            if (instruction.Operands[0].Type != ud_type.UD_OP_JIMM) return false;

            int i = 0;
            while (instruction.Operands[i] != null && instruction.Operands[i].Type != ud_type.UD_OP_JIMM && instruction.Operands.Count() > i) { i++; }

            if (instruction.Operands[i] == null) return false;


            absoluteAddress = instruction.Operands[i].SignedValue > 0 ?
                instruction.PC + (ulong)instruction.Operands[i].SignedValue :
                instruction.PC - (ulong)(instruction.Operands[i].SignedValue * -1);

            return true;

        }

        public static ulong GetAbsoluteAddressFromRelativeAddress(this IInstruction instruction,
            ulong instructionProgramCounter)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null) throw new ArgumentNullException(nameof(instruction) + "Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can be 0");

            int i = 0;
            while (instruction.Operands[i] != null && instruction.Operands[i].Type != ud_type.UD_OP_JIMM && instruction.Operands.Count() > i) { i++; }

            if (instruction.Operands[i] == null) throw new ApplicationException("There is not operand for absolute target address");

            ulong absoluteAddress = instruction.Operands[i].SignedValue > 0 ?
                instructionProgramCounter + (ulong)instruction.Operands[i].SignedValue :
                instructionProgramCounter - (ulong)(instruction.Operands[i].SignedValue * -1);

            return absoluteAddress;

        }

        public static ulong GetAbsoluteAddressFromRelativeAddress(this IInstruction instruction)
        {
            return GetAbsoluteAddressFromRelativeAddress(instruction, instruction.PC);
        }

        public static int GetRelativeAddress(this IInstruction instruction)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null) throw new ArgumentNullException(nameof(instruction) + "Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can be 0");

            return (int)instruction.Operands[0].SignedValue;

        }

        public static bool TryGetAbsoluteAddressFromAnyOperand(this IInstruction instruction,
            ICodeInMemoryLayout codeInMemoryLayout,
            out ulong targetAddress)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null) throw new ArgumentNullException(nameof(instruction) + "Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can be 0");
            if (codeInMemoryLayout == null) throw new ArgumentNullException(nameof(codeInMemoryLayout), "code can not be null");

            targetAddress = ulong.MaxValue;
            var startOfCodeAddress = codeInMemoryLayout.ImageBaseAddress +
                 codeInMemoryLayout.CodeVirtualAddress;
            var endOfCodeAddress = startOfCodeAddress +
                codeInMemoryLayout.CodeActualSizeInBytes;

            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if ((ulong)instruction.Operands[i].SignedValue >= startOfCodeAddress && 
                    (ulong)instruction.Operands[i].SignedValue < endOfCodeAddress)
                {
                    targetAddress = (ulong)instruction.Operands[i].SignedValue;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetAbsoluteAddress(this IInstruction instruction,out ulong targetAddress)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            targetAddress = ulong.MaxValue;

            if (instruction.Operands == null ||
                instruction.Operands.Count() == 0 ||
                instruction.Mnemonic != ud_mnemonic_code.UD_Ijmp ||
                instruction.Operands[0].Type != ud_type.UD_OP_MEM) return false;

            targetAddress = (ulong)instruction.Operands[0].Value;
            return true;
        }

        public static bool IsNopInstruction(this IInstruction instruction)
        {
            return instruction.Mnemonic == ud_mnemonic_code.UD_Iint ||
                instruction.Mnemonic == ud_mnemonic_code.UD_Iint1 ||
                instruction.Mnemonic == ud_mnemonic_code.UD_Iint3;
                

        }

        


    }
}
