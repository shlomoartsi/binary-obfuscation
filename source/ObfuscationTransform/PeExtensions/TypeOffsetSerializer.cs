using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.PeExtensions
{
    public class TypeOffsetSerializer : ITypeOffsetSerializer
    {
        public void Serialize(byte[] buffer,ref ulong bufferOffset,RelocationTypeOffset relocatiomOffset)
        {
            //type occupies the 4 most left bit (ranges from 0-3)
            var typeBytes =  BitConverter.GetBytes((ushort)(relocatiomOffset.Type << 12));
            
            //the offset is represented on one byte and next 4 bits (ranges from 0-0x999)
            UInt16 offsetValue0To999 = (UInt16)(relocatiomOffset.Offset % 0x1000);
            var offsetByte = BitConverter.GetBytes(offsetValue0To999);

            buffer[bufferOffset] = offsetByte[0];
            buffer[bufferOffset + 1] = offsetByte[1];
            buffer[bufferOffset + 1] |= typeBytes[1];

            bufferOffset += 2;
        }
    }
}
