using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public interface IDisassembler
    {
        IEnumerable<IAssemblyInstructionForTransformation> Disassemble();

    }
}
