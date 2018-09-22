using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{

    public interface ITargetAddressToInstructionsMapProvider
    {
        ITargetAddressToInstructionsMap GetMap();
    }
}
