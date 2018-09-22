using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.PeExtensions
{
    public struct RelocationTypeOffset
    {
        internal byte Type;
        internal ulong Offset;
    }


    public class ImageBaseRelocationSerialiazer
    {
        /// <summary>
        /// Serialize the list of relocations to a buffer from offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="relocationsOffset"></param>
        public void Serialize(byte[] buffer,ulong bufferOffset,List<RelocationTypeOffset> relocationsOffset)
        {

        }
    }
}
