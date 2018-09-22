using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    public class TargetAddressToInstructionsMapProvider :ITargetAddressToInstructionsMapProvider
    {
        ITargetAddressToInstructionsMap  m_targetAddressToInstructionsMap;
        public TargetAddressToInstructionsMapProvider(ITargetAddressToInstructionsMap targetAddressToInstructionsMap)
        {
            if (targetAddressToInstructionsMap == null) throw new ArgumentNullException(nameof(targetAddressToInstructionsMap));

            m_targetAddressToInstructionsMap = targetAddressToInstructionsMap;
        }

        public ITargetAddressToInstructionsMap GetMap()
        {
            return m_targetAddressToInstructionsMap;
        }
    }
}
