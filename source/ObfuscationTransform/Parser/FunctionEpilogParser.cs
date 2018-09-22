using ObfuscationTransform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Parser
{
    /// <summary>
    /// Class that parses the function epilog
    /// </summary>
    public class FunctionEpilogParser : IFunctionEpilogParser
    {
        public IAssemblyInstructionForTransformation Parse(IAssemblyInstructionForTransformation assemblyToStartFrom, ulong lastAddress)
        {
            if (assemblyToStartFrom == null) throw new ArgumentNullException("assemblyToStartFrom");
            if (assemblyToStartFrom.Offset >= lastAddress) throw new ArgumentOutOfRangeException("lastAddress", "assembly start address can not be greater nor equal to the last address param");
            IAssemblyInstructionForTransformation currentInstruction = assemblyToStartFrom;
            while (currentInstruction != null && currentInstruction.Offset <= lastAddress)
            {
                if (InstructionIsRet(currentInstruction)) return currentInstruction;
                currentInstruction = currentInstruction.NextInstruction;
            }

            return null;
        }

        /// <summary>
        /// searches for the epilogue pattern 
        /// </summary>
        /// <param name="currentInstruction"></param>
        /// <returns></returns>
        private bool InstructionIsRet(IAssemblyInstructionForTransformation currentInstruction)
        {
            return (currentInstruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iret ||
                currentInstruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iretf);
        }
    }
}