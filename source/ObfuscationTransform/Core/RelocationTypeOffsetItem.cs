using ObfuscationTransform.PeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public struct RelocationTypeOffsetItem
    {
        public RelocationTypeOffsetItem(ulong address,
            RelocationTypeOffset relocationTypeOffset,
            object tag)
        {
            Address = address;
            RelocationTypeOffset = relocationTypeOffset;
            Tag = tag;
        }
        
        public ulong Address { get; private set; }
        public RelocationTypeOffset RelocationTypeOffset { get; private set; }
        public object Tag { get; private set; }
    }
}
