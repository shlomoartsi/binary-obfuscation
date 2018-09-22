using ObfuscationTransform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Parser
{
    public interface IBasicBlockEpilogParser
    {
        IAssemblyInstructionForTransformation Parse(IAssemblyInstructionForTransformation assemblyToStartFrom,
            ulong lastAddress, Dictionary<ulong, ulong> jumpTargetAddresses);
    }
}
