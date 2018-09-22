using System.Collections.Generic;

namespace ObfuscationTransform.Core
{
    public interface IBasicBlock
    {
        AddressesRange AddressesRange { get; }
        IReadOnlyList<IAssemblyInstructionForTransformation> AssemblyInstructions { get; }
        IBasicBlock NewTansformedBasicBlock { get; set; }
        uint NumberOfBytes { get; }
        byte[] GetAllBytes();
    }
}