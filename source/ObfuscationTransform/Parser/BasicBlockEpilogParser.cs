using System;
using ObfuscationTransform.Core;
using System.Collections.Generic;

namespace ObfuscationTransform.Parser
{
    public class BasicBlockEpilogParser : IBasicBlockEpilogParser
    {
        private IInstructionWithAddressOperandDecider m_jumpInstructionDecider { get; set; }

        public BasicBlockEpilogParser(IInstructionWithAddressOperandDecider jumpInstructionDecider)
        {
            m_jumpInstructionDecider = jumpInstructionDecider ?? throw new ArgumentNullException("jumpInstructionDecider");
        }

        public IAssemblyInstructionForTransformation Parse(
                               IAssemblyInstructionForTransformation assemblyToStartFrom, ulong lastAddress,
                               Dictionary<ulong, ulong> jumpTargetAddresses)
        {
            if (assemblyToStartFrom == null) throw new ArgumentNullException(nameof(assemblyToStartFrom));
            if (jumpTargetAddresses == null) throw new ArgumentNullException(nameof(jumpTargetAddresses));
            var currentInstruction = assemblyToStartFrom;
            var epilogFound = false;
            do
            {
                if (m_jumpInstructionDecider.IsJumpInstruction(currentInstruction.Mnemonic) ||
                    m_jumpInstructionDecider.IsReturnInstruction(currentInstruction) ||
                    m_jumpInstructionDecider.IsCallInstruction(currentInstruction) ||
                    (currentInstruction.NextInstruction !=null && 
                    InstructionIsJumpTarget(currentInstruction.NextInstruction,jumpTargetAddresses)))
               {
                    //epilog instruction is found
                    epilogFound = true;
                    break;
                }
                currentInstruction = currentInstruction.NextInstruction;
                
            } while (currentInstruction.Offset<=lastAddress);

            var returnedInstruction = currentInstruction;
            if (!epilogFound) returnedInstruction = null;
            return returnedInstruction;
        }

        private bool InstructionIsJumpTarget(IAssemblyInstructionForTransformation instruction,
            Dictionary<ulong,ulong> jumpTargetAddresses)
        {
            return jumpTargetAddresses.ContainsKey(instruction.Offset);
        }

        
    }
}
