using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using SharpDisasm.Udis86;
using SharpDisasm;
using SharpDisasm.Translators;
using ObfuscationTransform.Extensions;

namespace ObfuscationTransform.Transformation
{
    public class InstructionWithAddressOperandTransform : IInstructionWithAddressOperandTransform
    {
        private IDisassemblerFactory m_disasmFactory;
        private IInstructionWithAddressOperandDecider m_InstructionWithAddressOperandDecider;
        private Dictionary<ud_mnemonic_code, byte[]> m_dictionaryJumpInstructionBytes;
        private Dictionary<ud_mnemonic_code, ud_mnemonic_code> m_dictionaryOfJumpToNegativeJump;

        public InstructionWithAddressOperandTransform(IInstructionWithAddressOperandDecider InstructionWithAddressOperandDecider,
            IDisassemblerFactory disasmFactory)
        {
            m_InstructionWithAddressOperandDecider = InstructionWithAddressOperandDecider ?? throw new ArgumentNullException(nameof(InstructionWithAddressOperandDecider));
            m_disasmFactory = disasmFactory ?? throw new ArgumentNullException(nameof(disasmFactory));
            m_dictionaryJumpInstructionBytes = FillDictionaryJumpInstructionBytes();
            m_dictionaryOfJumpToNegativeJump = FillDictionaryOfJumpToNegativeJump();
        }


        public bool TryTransformJumpInstructionTo4BytesOperandSize(IAssemblyInstructionForTransformation instruction,
                                                                   out IAssemblyInstructionForTransformation transformedInstruction)
        {
            transformedInstruction = instruction;

            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (!m_InstructionWithAddressOperandDecider.IsJumpInstruction(instruction.Mnemonic)) throw new ArgumentException("instruction is not a jump instruction");
            if (!m_InstructionWithAddressOperandDecider.IsJumpInstructionWithRelativeAddressOperand(instruction)) return false;
            if (!m_dictionaryJumpInstructionBytes.ContainsKey(instruction.Mnemonic)) return false;
            if (instruction.Operands[0].Size == 32) return false;//because the size of operand is already 4 bytes
            var operand = instruction.Operands[0];
            if (operand == null) throw new ApplicationException("Jump instruction transformation didn't find an operand with JIMM type. program doesnt support other types.");

            //get an expanded jump instruction with 4 bytes, which have empty (or incorrect operand bytes).
            var newInstructionBytes = GetEmptyExapndedJumpInstructionWith4Bytes(instruction);

            //fill the target address with the new operand bytes
            var newInstructionBytesWithTargetAddress = FillInstructionWithOperandBytes(instruction, newInstructionBytes);

            //create new insruction from the bytes of new instruction
            transformedInstruction = m_disasmFactory.Create(newInstructionBytes, false).Disassemble().First();
            transformedInstruction.SetOffset(instruction.Offset);
            transformedInstruction.SetPC(instruction.PC);

            return true;
        }

        public IAssemblyInstructionForTransformation CreateUnconditionalJumpInstruction(int instructionOffset)
        {
            //get instrution with 4 bytes operand
            var jumpInstructionBytes = (byte[])m_dictionaryJumpInstructionBytes[ud_mnemonic_code.UD_Ijmp].Clone();

            //set the required offset
            var bytesWithAddressShift = BitConverter.GetBytes(instructionOffset);

            var addressIdx = 1;
            replaceBytes(jumpInstructionBytes, addressIdx, bytesWithAddressShift);


            //create new instruction based on the new bytes
            var disasm = m_disasmFactory.Create(jumpInstructionBytes);
            var newInstruction = disasm.Disassemble().First();
            return newInstruction;
        }



        private byte[] FillInstructionWithOperandBytes(IAssemblyInstructionForTransformation instruction,
                                                                      byte[] instructionBytes)
        {
            long targetAddressShiftAsLong;

            bool succeededGettingAddress = instruction.TryParseInstructionTargetAddress(InstructionToStringTargetAddressMode.AddressShift,
                                                                                        out targetAddressShiftAsLong);
            if (!succeededGettingAddress) throw new ApplicationException("jump instruction should have target address");

            byte[] targetAddressBytes = BitConverter.GetBytes((int)targetAddressShiftAsLong);
            int operandBytesOffset = instructionBytes.Length - 4;
            for (int j = 0; j < 4; j++)
            {
                instructionBytes[j + operandBytesOffset] = targetAddressBytes[j];
            }
            return instructionBytes;
        }

        private byte[] GetEmptyExapndedJumpInstructionWith4Bytes(IAssemblyInstructionForTransformation instruction)
        {
            byte[] newInstructionBytes;
            //get the instruction's bytes from dictionary
            newInstructionBytes = m_dictionaryJumpInstructionBytes[instruction.Mnemonic];
            ////we can not replace this instruction, (jcxz,jecxz instructions) because of different parameters size
            if (newInstructionBytes.Length <= 3) throw new NotSupportedException("transformation doesn not support jcxz,jecxz instruction for now :(");

            //copy the original target address to the new bytes of the instruction
            //get a copy of the same instruction with 4 bytes operand size.
            return (byte[])m_dictionaryJumpInstructionBytes[instruction.Mnemonic].Clone();
        }

        /// <summary>
        /// Increment the jump instruction target
        /// </summary>
        /// <param name="instruction">the jump instruction</param>
        /// <param name="addressShift">the address shift</param>
        /// <returns>new instruction that its target address is shifted by the parameter</returns>
        public IAssemblyInstructionForTransformation IncrementJumpInstructionTargetAddress(IAssemblyInstructionForTransformation instruction,
            int addressShift)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null) throw new ArgumentException("parameters operands can be 0", "instruction.Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can not be 0");
            if (instruction.Operands[0].Type != ud_type.UD_OP_JIMM) throw new ArgumentException("parameters operand must be of type 'JIMM'");

            byte[] bytesWithAddressShift = null;
            byte[] bytesOriginal = null;
            byte[] newInstructionBytes = instruction.Bytes.Clone() as byte[];

            var opr = instruction.Operands[0];
            switch (opr.Size)
            {
                case 8:
                    if (opr.LvalSByte + addressShift > sbyte.MaxValue) throw new ApplicationException("operand value is more than 128 which is the max value. operand size have to be extended!");
                    if (opr.LvalSByte + addressShift < sbyte.MinValue) throw new ApplicationException("operand value is less than -127 which is the min value. operand size have to be extended!");

                    bytesWithAddressShift = new byte[] { (byte)(opr.LvalSByte + addressShift) };
                    bytesOriginal = new byte[] { (byte)(opr.LvalSByte) };
                    break;
                case 16:
                    if (opr.LvalSWord + addressShift > short.MaxValue) throw new ApplicationException("operand value is more than 32,768 which is the max value. operand size have to be extended!");
                    if (opr.LvalSWord + addressShift < short.MinValue) throw new ApplicationException("operand value is more than -32,767 which is the max value. operand size have to be extended!");

                    bytesWithAddressShift = BitConverter.GetBytes((short)(opr.LvalSWord + addressShift));
                    bytesOriginal = BitConverter.GetBytes(opr.LvalSWord);
                    break;
                case 32:
                    if (opr.LvalSDWord + addressShift > int.MaxValue) throw new ApplicationException("operand value is more than 2,147,483,648 which is the max value.operand size have to be extended!");
                    if (opr.LvalSDWord + addressShift < int.MinValue) throw new ApplicationException("operand value is less than -2,147,483,647 which is the max value.operand size have to be extended!");

                    bytesWithAddressShift = BitConverter.GetBytes(opr.LvalSDWord + addressShift);
                    break;
            }

            int idx = instruction.Bytes.Length - (opr.Size / 8);
            if (idx > -1) replaceBytes(newInstructionBytes, idx, bytesWithAddressShift);
            else throw new ApplicationException("Could not replace target address in instruction");

            //validate the byte array modification    
            var disasm = m_disasmFactory.Create(newInstructionBytes);
            var newInstruction = disasm.Disassemble().First();
            if ((opr.SignedValue + addressShift) != newInstruction.Operands[0].SignedValue)
            {
                throw new ApplicationException("updating jump instruction target address failed");
            }

            return newInstruction;
        }

        public IAssemblyInstructionForTransformation CreateJumpInstructionWithNewTargetAddress(
            IAssemblyInstructionForTransformation instruction,
            ulong newProgramCounter, ulong newOffset, ulong newTargetAddress)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (instruction.Operands == null) throw new ArgumentException("parameters operands can be 0", "instruction.Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can not be 0");
            if (instruction.Operands[0].Type != ud_type.UD_OP_JIMM) throw new ArgumentException("parameters operand must be of type 'JIMM'");

            var opr = instruction.Operands[0];
            //same instruction target, no change is needed
            if (instruction.GetAbsoluteAddressFromRelativeAddress(newProgramCounter) == newTargetAddress)
            {
                instruction.SetOffset(newOffset);
                instruction.SetPC(newProgramCounter);
                return instruction;
            }

            byte[] bytesWithAddressShift = null;
            byte[] bytesOriginal = null;
            byte[] newInstructionBytes = instruction.Bytes.Clone() as byte[];

            int offsetValue = newTargetAddress > newProgramCounter ? (int)(newTargetAddress - newProgramCounter) : (int)(newProgramCounter - newTargetAddress) * -1;
            switch (opr.Size)
            {
                case 8:
                    if (offsetValue > (long)sbyte.MaxValue) throw new ApplicationException("operand value is more than 128 which is the max value. operand size have to be extended!");
                    if (offsetValue < (long)sbyte.MinValue) throw new ApplicationException("operand value is less than -127 which is the min value. operand size have to be extended!");
                    bytesWithAddressShift = new byte[] { (byte)(offsetValue) };
                    bytesOriginal = new byte[] { (byte)(opr.LvalSByte) };
                    break;
                case 16:
                    if (offsetValue > short.MaxValue) throw new ApplicationException("operand value is more than 32,768 which is the max value. operand size have to be extended!");
                    if (offsetValue < short.MinValue) throw new ApplicationException("operand value is more than -32,767 which is the max value. operand size have to be extended!");

                    bytesWithAddressShift = BitConverter.GetBytes((short)(offsetValue));
                    bytesOriginal = BitConverter.GetBytes(opr.LvalSWord);
                    break;
                case 32:
                    if (offsetValue > int.MaxValue) throw new ApplicationException("operand value is more than 2,147,483,648 which is the max value.operand size have to be extended!");
                    if (offsetValue < int.MinValue) throw new ApplicationException("operand value is less than -2,147,483,647 which is the max value.operand size have to be extended!");

                    bytesWithAddressShift = BitConverter.GetBytes(offsetValue);
                    break;
            }

            int idx = instruction.Bytes.Length - (opr.Size / 8);
            if (idx > -1) replaceBytes(newInstructionBytes, idx, bytesWithAddressShift);
            else throw new ApplicationException("Could not replace target address in instruction");

            //create new instruction based on the new bytes
            var disasm = m_disasmFactory.Create(newInstructionBytes);
            var newInstruction = disasm.Disassemble().First();
            newInstruction.SetOffset(newOffset);
            newInstruction.SetPC(newProgramCounter);

            if (newTargetAddress != newInstruction.GetAbsoluteAddressFromRelativeAddress())
            {
                throw new ApplicationException("updating jump instruction target address failed");
            }
            return newInstruction;
        }

        public IAssemblyInstructionForTransformation CreateInstructionWithNewAddress(ICode code,
            IAssemblyInstructionForTransformation instruction, ulong newProgramCounter,
            ulong newOffset, ulong oldAddressInOperand, ulong newAddressInOperand)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (instruction.Operands == null) throw new ArgumentException("parameters operands can be 0", "instruction.Operands");
            if (instruction.Operands.Count() == 0) throw new ArgumentException("parameters operands can not be 0");

            var opr = instruction.Operands[0];
            //same instruction target, no change is needed
            if (oldAddressInOperand == newAddressInOperand)
            {
                instruction.SetOffset(newOffset);
                instruction.SetPC(newProgramCounter);
                return instruction;
            }


            byte[] oldAddressBytes = BitConverter.GetBytes((uint)oldAddressInOperand);
            byte[] newAddressBytes = BitConverter.GetBytes((uint)newAddressInOperand);
            int idx = FindOriginalBytesIndex(instruction.Bytes, oldAddressBytes);
            if (idx < 0)
            {
                throw new ApplicationException("Could not replace target address in instruction");
            }

            byte[] newInstructionBytes = instruction.Bytes.Clone() as byte[];
            replaceBytes(newInstructionBytes, idx, newAddressBytes);

            //create new instruction based on the new bytes
            var disasm = m_disasmFactory.Create(newInstructionBytes);
            var newInstruction = disasm.Disassemble().First();
            newInstruction.SetOffset(newOffset);
            newInstruction.SetPC(newProgramCounter);

            //validate new instructions
            ulong parsedAddress;
            if (!newInstruction.TryGetAbsoluteAddressFromAnyOperand(code.CodeInMemoryLayout, out parsedAddress) ||
                newAddressInOperand != parsedAddress)
            {
                throw new ApplicationException("updating jump instruction target address failed");
            }
            return newInstruction;
        }

        public IAssemblyInstructionForTransformation CreateNegativeJumpInstruction(
            IAssemblyInstructionForTransformation instruction, int instructionOffset)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));
            if (!m_InstructionWithAddressOperandDecider.IsJumpInstruction(instruction.Mnemonic))
                throw new ArgumentException("instruction must be jump instruction",nameof(instruction));

            if (!m_dictionaryOfJumpToNegativeJump.ContainsKey(instruction.Mnemonic)) return null;
            var negativeJump = m_dictionaryOfJumpToNegativeJump[instruction.Mnemonic];

             //get instrution with 4 bytes operand
            var jumpInstructionBytes = (byte[])m_dictionaryJumpInstructionBytes[negativeJump].Clone();

            if (jumpInstructionBytes.Length != 6)
                throw new ApplicationException("creating new conditional jump instruction consists of instruction 6 bytes long");

            //set the required offset
            var bytesWithAddressShift = BitConverter.GetBytes(instructionOffset);

            //change the address from index 2 to 5. The size of the instruction is 6.
            var addressIdx = 2;
            replaceBytes(jumpInstructionBytes, addressIdx, bytesWithAddressShift);

            //create new instruction based on the new bytes
            var disasm = m_disasmFactory.Create(jumpInstructionBytes);
            var newInstruction = disasm.Disassemble().First();

            return newInstruction;
        }

        public bool TryCreateNegativeJumpIntruction(IAssemblyInstructionForTransformation instruction,
            int instructionOffset, out IAssemblyInstructionForTransformation newInstruction)
        {
            newInstruction = null;

            if (instruction == null ||
                !m_InstructionWithAddressOperandDecider.IsConditionalJumpInstruction(instruction.Mnemonic))
                return false;

            newInstruction = CreateNegativeJumpInstruction(instruction, instructionOffset);

            return (newInstruction != null);
        }

        //todo change to extension method
        internal void replaceBytes(byte[] byteArray, int idxToReplaceFrom, byte[] bytesToReplace)
        {
            if (byteArray == null) throw new ArgumentNullException(nameof(byteArray));
            if (bytesToReplace == null) throw new ArgumentNullException(nameof(bytesToReplace));
            if (byteArray.Length < bytesToReplace.Length + idxToReplaceFrom) throw new ArgumentNullException("bytes to replace length must be less than byte array");

            for (int i = 0; i < bytesToReplace.Length; i++)
            {
                byteArray[idxToReplaceFrom + i] = bytesToReplace[i];
            }
        }

        //todo change to extension method
        internal int FindOriginalBytesIndex(byte[] byteArray, byte[] bytesToFind)
        {
            if (byteArray == null) throw new ArgumentNullException(nameof(byteArray));
            if (bytesToFind == null) throw new ArgumentNullException(nameof(bytesToFind));
            if (byteArray.Length < bytesToFind.Length) throw new ArgumentNullException("bytes to find length must be less than byte array");

            for (int i = 0; i <= byteArray.Length - bytesToFind.Length; i++)
            {
                int bytesEqual = 0;
                if (byteArray[i] == bytesToFind[bytesEqual])
                {
                    while (++bytesEqual < bytesToFind.Length)
                    {
                        if (byteArray[i + bytesEqual] != bytesToFind[bytesEqual]) break;
                    }
                    if (bytesEqual == bytesToFind.Length) return i;
                }
            }

            return -1;
        }


        private Dictionary<ud_mnemonic_code, byte[]> FillDictionaryJumpInstructionBytes()
        {
            Dictionary<ud_mnemonic_code, byte[]> dict = new Dictionary<ud_mnemonic_code, byte[]>
            {
                [ud_mnemonic_code.UD_Ija] = new byte[] { 0x0F, 0x87, 0x04, 0x03, 0x02, 0x01 }, //ja  01020304
                [ud_mnemonic_code.UD_Ijae] = new byte[] { 0x0F, 0x83, 0x4, 0x03, 0x02, 0x01 },//jae 01020304
                [ud_mnemonic_code.UD_Ijb] = new byte[] { 0x0F, 0x82, 0xFE, 0x02, 0x02, 0x01 }, //jb 01020304
                [ud_mnemonic_code.UD_Ijbe] = new byte[] { 0x0F, 0x86, 0xFE, 0x02, 0x02, 0x01 }, // jbe 01020304
                [ud_mnemonic_code.UD_Ijcxz] = new byte[] { 0x67, 0xE3, 0xFD },//jcxz 0x00
                [ud_mnemonic_code.UD_Ijecxz] = new byte[] { 0xE3, 0xFE }, //jecxz 0x00
                [ud_mnemonic_code.UD_Ijg] = new byte[] { 0x0F, 0x8F, 0xFE, 0x02, 0x02, 0x01 }, //jg 01020304
                [ud_mnemonic_code.UD_Ijge] = new byte[] { 0x0F, 0x8D, 0xFE, 0x02, 0x02, 0x01 }, //jge 01020304
                [ud_mnemonic_code.UD_Ijl] = new byte[] { 0x0F, 0x8C, 0xFE, 0x02, 0x02, 0x01 }, //jl 01020304
                [ud_mnemonic_code.UD_Ijle] = new byte[] { 0x0F, 0x8E, 0xFE, 0x02, 0x02, 0x01 }, //jle 01020304
                [ud_mnemonic_code.UD_Ijmp] = new byte[] { 0xE9, 0x04, 0x03, 0x02, 0x01 }, //jmp 01020304
                [ud_mnemonic_code.UD_Ijno] = new byte[] { 0x0F, 0x81, 0x04, 0x03, 0x02, 0x01 },//jno 01020304
                [ud_mnemonic_code.UD_Ijnp] = new byte[] { 0x0F, 0x8B, 0x04, 0x03, 0x02, 0x01 }, //jnp 01020304
                [ud_mnemonic_code.UD_Ijns] = new byte[] { 0x0F, 0x89, 0x04, 0x03, 0x02, 0x01 }, //jns 01020304
                [ud_mnemonic_code.UD_Ijnz] = new byte[] { 0x0F, 0x85, 0x04, 0x03, 0x02, 0x01 }, //jnz 01020304
                [ud_mnemonic_code.UD_Ijo] = new byte[] { 0x0F, 0x80, 0x04, 0x03, 0x02, 0x01 }, //jo 01020304
                [ud_mnemonic_code.UD_Ijp] = new byte[] { 0x0F, 0x8A, 0x04, 0x03, 0x02, 0x01 }, //jp 01020304
               /*[ud_mnemonic_code.UD_Ijrcxz] = only supported in 64 bits*/
                [ud_mnemonic_code.UD_Ijs] = new byte[] { 0x0F, 0x88, 0x04, 0x03, 0x02, 0x01 }, //js 01020304
                [ud_mnemonic_code.UD_Ijz] = new byte[] { 0x0F, 0x84, 0x04, 0x03, 0x02, 0x01 } //jz 01020304
            };
            return dict;
        }

        private Dictionary<ud_mnemonic_code, ud_mnemonic_code> FillDictionaryOfJumpToNegativeJump()
        {
            //+--------+------------------------------+-------------+--------------------+
            //| Instr | Description | signed - ness | Flags |
            var dict = new Dictionary<ud_mnemonic_code, ud_mnemonic_code>
            {

                //| JA    | Jump if above                | unsigned    | CF = 0 and ZF = 0  |
                //| JBE   | Jump if below or equal       | unsigned    | CF = 1 or ZF = 1   |
                [ud_mnemonic_code.UD_Ija] = ud_mnemonic_code.UD_Ijbe,
                [ud_mnemonic_code.UD_Ijbe] = ud_mnemonic_code.UD_Ija,

                //| JAE   | Jump if above or equal       | unsigned    | CF = 0             |
                //| JB    | Jump if below                | unsigned    | CF = 1             |
                [ud_mnemonic_code.UD_Ijae] = ud_mnemonic_code.UD_Ijb,
                [ud_mnemonic_code.UD_Ijb] = ud_mnemonic_code.UD_Ijae,

                //| JCXZ  | Jump if CX is zero           |             | CX = 0             |
                //[ud_mnemonic_code.UD_Ijcxz] - there is not instruction for CX=1
                //[ud_mnemonic_code.UD_Ijecxz] - there is not instruction for CX=1

                //| JG    | Jump if greater              | signed     | ZF = 0 and SF = OF  |
                //| JLE   | Jump if less or equal        | signed     | ZF = 1 or SF<> OF   |
                [ud_mnemonic_code.UD_Ijg] = ud_mnemonic_code.UD_Ijle,
                [ud_mnemonic_code.UD_Ijle] = ud_mnemonic_code.UD_Ijg,

                //| JL    | Jump if less                 | signed     | SF <> OF            |
                //| JGE   | Jump if greater or equal     | signed     | SF = OF             |
                [ud_mnemonic_code.UD_Ijge] = ud_mnemonic_code.UD_Ijl,
                [ud_mnemonic_code.UD_Ijl] = ud_mnemonic_code.UD_Ijge,

                //| JO    | Jump if overflow             |             | OF = 1            |
                //| JNO   | Jump if not overflow         |             | OF = 0            |
                [ud_mnemonic_code.UD_Ijo] = ud_mnemonic_code.UD_Ijno,
                [ud_mnemonic_code.UD_Ijno] = ud_mnemonic_code.UD_Ijo,

                //| JP    | Jump if parity               |             | PF = 1            |
                //| JNP   | Jump if no parity            |             | PF = 0            |
                [ud_mnemonic_code.UD_Ijp] = ud_mnemonic_code.UD_Ijnp,
                [ud_mnemonic_code.UD_Ijnp] = ud_mnemonic_code.UD_Ijp,

                //| JS    | Jump if sign                 |             | SF = 1            |
                //| JNS   | Jump if not sign             |             | SF = 0            |
                [ud_mnemonic_code.UD_Ijs] = ud_mnemonic_code.UD_Ijns,
                [ud_mnemonic_code.UD_Ijns] = ud_mnemonic_code.UD_Ijs,

                //| JZ     | Jump if zero                |             | ZF =  0           |
                //| JNZ | Jump if not zero |             |             | ZF =  1           |
                [ud_mnemonic_code.UD_Ijz] = ud_mnemonic_code.UD_Ijnz,
                [ud_mnemonic_code.UD_Ijnz] = ud_mnemonic_code.UD_Ijz
            };

            /* dict[ud_mnemonic_code.UD_Ijrc */
            return dict;
        }


    }
}