using System.Collections.Generic;

namespace ObfuscationTransform.Core
{

    /// <summary>
    /// interface that represents a code section
    /// </summary>
    public interface ICode
    {
        /// <summary>
        /// Assembly instructions list
        /// </summary>
        IReadOnlyList<IAssemblyInstructionForTransformation> AssemblyInstructions { get; }

        /// <summary>
        /// Functions list
        /// </summary>
        IReadOnlyList<IFunction> Functions { get; }

        /// <summary>
        /// Code layout in whole program memory
        /// </summary>
        ICodeInMemoryLayout CodeInMemoryLayout { get; }

    }
}