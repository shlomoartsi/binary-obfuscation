using System.Collections.Generic;
using ObfuscationTransform.Core;

namespace ObfuscationTransform.Parser
{
    public interface IFunctionParser
    {
        IBasicBlockParser BasicBlockParser { get; }
        IFunctionEpilogParser EpilogParser { get; }
        IFunctionPrologParser PrologParser { get; }

        IReadOnlyList<IFunction> Parse(IAssemblyInstructionForTransformation firstAssemblyInstruction, 
            AddressesRange addressesRangePermittedForParsing,Dictionary<ulong, ulong> jumpTargetAddresses);
    }
}