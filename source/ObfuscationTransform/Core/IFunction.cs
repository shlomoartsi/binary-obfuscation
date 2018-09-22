using System.Collections.Generic;

namespace ObfuscationTransform.Core
{
    public interface IFunction
    {
        AddressesRange AddressesRange { get; }
        IReadOnlyList<IBasicBlock> BasicBlocks { get; }
        IAssemblyInstructionForTransformation EndInstruction { get; }
        IAssemblyInstructionForTransformation StartInstruction { get; }
    }
}