using ObfuscationTransform.Core;
using System.Collections.Generic;

namespace ObfuscationTransform.Parser
{
    public interface ICodeParser
    {
        ICode ParseCode(byte[] code,ICodeInMemoryLayout codeInMemoryLayout);
        ICode ParseCode(IReadOnlyList<IAssemblyInstructionForTransformation> listOfInstructions, ICodeInMemoryLayout codeInMemoryLayout);
    }
}