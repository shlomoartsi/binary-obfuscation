using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public struct AddressesRange
    {
        public AddressesRange(ulong startAddress, ulong endAddress)
        {
            if (startAddress >= endAddress) throw new ArgumentException("end address must be bigger than start address");
            StartAddress = startAddress;
            EndAddress = endAddress;

        }

        public ulong StartAddress { get; }
        public ulong EndAddress { get; }
    }
}
