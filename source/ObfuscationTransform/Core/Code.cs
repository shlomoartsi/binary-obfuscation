using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public class Code : ICode
    {
        public IReadOnlyList<IAssemblyInstructionForTransformation> AssemblyInstructions { get; }
        public IReadOnlyList<IFunction> Functions { get; }
        public ICodeInMemoryLayout CodeInMemoryLayout { get; }

        public Code(IReadOnlyList<IAssemblyInstructionForTransformation> assemblyInstructions,
                    IReadOnlyList<IFunction> functions,
                    ICodeInMemoryLayout codeInMemoryLayout)
        {
            AssemblyInstructions = assemblyInstructions ?? throw new ArgumentNullException(nameof(assemblyInstructions));
            Functions = functions ?? throw new ArgumentNullException(nameof(functions));
            CodeInMemoryLayout = codeInMemoryLayout ?? throw new ArgumentNullException(nameof(codeInMemoryLayout));
        }

    }
}
