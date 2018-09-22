using System.Collections.Generic;

namespace ObfuscationTransform.PeExtensions
{
    public interface IImageBaseRelocationSerializer
    {
        /// <summary>
        /// Serialize the list of relocations to a buffer from offset. 
        /// Relocations list is expected to be from 0xDigit000 to 0xDigit999 because the 
        /// it serializes only to offset of thousands in each directory entry
        /// </summary>
        /// <param name="buffer">the buffer to serialize to </param>
        /// <param name="bufferOffset">buffer offset to stat from. changes to the new offset 
        /// after the serialization</param>
        /// <param name="relocationsOffsetList">all relocations that contains full virtual address to serialize</param>
        void Serialize(byte[] buffer, ref ulong bufferOffset,
            List<RelocationTypeOffset> bufferOffsetList);
    }
}