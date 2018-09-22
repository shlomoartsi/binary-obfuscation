using ObfuscationTransform.Core;

namespace ObfuscationTransform.Parser
{
    public interface IFunctionEpilogParser
    {
        IAssemblyInstructionForTransformation Parse(IAssemblyInstructionForTransformation assemblyToStartFrom, ulong lastAddress);
    }
}