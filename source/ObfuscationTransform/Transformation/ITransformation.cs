using ObfuscationTransform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    public interface ITransformation
    {
        ICode Transform(ICode code);
    }
}
