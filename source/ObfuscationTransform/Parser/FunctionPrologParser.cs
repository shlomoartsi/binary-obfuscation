using ObfuscationTransform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Parser
{
    /// <summary>
    /// parse a prolog of a function
    /// </summary>
    public class FunctionPrologParser : IFunctionPrologParser
    {
        /// <summary>
        /// Parse a function prolog given an assembly instruction to start from.
        /// Searches for the pattern of : 
        /// push ebp
        /// mov  ebp, esp
        /// </summary>
        /// <param name="assemblyToStartFrom">Assembly instruction to start from </param>
        /// <param name="lastAddress">last address to try parsing</param>
        /// <returns>the address where the function starts,null otherwise</returns>
        public IAssemblyInstructionForTransformation Parse(IAssemblyInstructionForTransformation assemblyToStartFrom,ulong lastAddress )
        {
            if (assemblyToStartFrom == null) throw new ArgumentNullException("assemblyToStartFrom");
            if (assemblyToStartFrom.Offset >= lastAddress) throw new ArgumentOutOfRangeException("lastAddress", "assembly start address can not be greater nor equal to the last address param");
            IAssemblyInstructionForTransformation currentInstruction = assemblyToStartFrom;
            while (currentInstruction!=null && currentInstruction.Offset <= lastAddress)
            {
                //if current examined instruction is PUSH EBP
                if (InstructionIsPush_EBP(currentInstruction) && 
                    currentInstruction.NextInstruction !=null &&
                    //and next instruction is MOV EBP ESP
                    InstructionIsMov_EBP_ESP(currentInstruction.NextInstruction))
                {
                    return currentInstruction;
                }
                currentInstruction = currentInstruction.NextInstruction;
            }

            return null;
        }

        private bool InstructionIsPush_EBP(IAssemblyInstructionForTransformation instruction)
        {
            return instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Ipush &&
                    instruction.Operands != null &&
                    instruction.Operands.Count() == 1 &&
                    instruction.Operands[0].Base == SharpDisasm.Udis86.ud_type.UD_R_EBP;
        }

        private bool InstructionIsMov_EBP_ESP(IAssemblyInstructionForTransformation instruction)
        {
            return instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Imov &&
                instruction.Operands != null &&
                instruction.Operands.Count() == 2 &&
                instruction.Operands[0].Base == SharpDisasm.Udis86.ud_type.UD_R_EBP &&
                instruction.Operands[1].Base == SharpDisasm.Udis86.ud_type.UD_R_ESP;
        }

       
    }
}
