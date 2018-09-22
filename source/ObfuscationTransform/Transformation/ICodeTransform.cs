using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    /// <summary>
    /// Transformation method that try to transform an instruction to other/list of instructions
    /// </summary>
    /// <param name="instruction">instruction to transform</param>
    /// <param name="basicBlock">the basic block which the instruction belongs to</param>
    /// <param name="function">the function which the basic block belongs to</param>
    /// <param name = "transformedInstructions">transformed instructions list in case transformation succeeded</param>
    /// <returns>true if transformation is performed</returns>
    public delegate bool TryTransformInstructionDelegate(IAssemblyInstructionForTransformation instruction,
        IBasicBlock basicBlock, IFunction function,
        out List<IAssemblyInstructionForTransformation> transformedInstruction);

    public interface ICodeTransform
    {
        ICode Transform(ICode code,TryTransformInstructionDelegate transformInstructionDelegate,
            Func<ICodeInMemoryLayout> codeInMemoryLayoutFactoryDeleate = null);
    }
}