using ObfuscationTransform.Core;

namespace ObfuscationTransform.Parser
{
    public interface IFunctionPrologParser
    {
        IAssemblyInstructionForTransformation Parse(IAssemblyInstructionForTransformation assemblyToStartFrom, ulong lastAddress);
    }
}