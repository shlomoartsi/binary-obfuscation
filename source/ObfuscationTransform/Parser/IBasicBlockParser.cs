using System.Collections.Generic;
using ObfuscationTransform.Core;

namespace ObfuscationTransform.Parser
{
    public interface IBasicBlockParser
    {
        IReadOnlyList<IBasicBlock> Parse(IAssemblyInstructionForTransformation firstInstruction,
            IAssemblyInstructionForTransformation lastInstruction, Dictionary<ulong, ulong> jumpTargetAddresses);
    }
}