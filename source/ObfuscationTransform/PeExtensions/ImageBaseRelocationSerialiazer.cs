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
        private static RelocationTypeOffset m_PaddingRelocationOffset = new RelocationTypeOffset();
        public static RelocationTypeOffset PaddingRelocationOffset { get { return m_PaddingRelocationOffset; } }
    }


    public class ImageBaseRelocationSerializer : IImageBaseRelocationSerializer
    {
        private readonly ITypeOffsetSerializer m_TypeOffsetSerializer;

        public ImageBaseRelocationSerializer(ITypeOffsetSerializer typeOffsetSerializer)
        {
            m_TypeOffsetSerializer = typeOffsetSerializer ?? throw new ArgumentNullException(nameof(typeOffsetSerializer));

        }
        /// <summary>
        /// Serialize the list of relocations to a buffer from offset. 
        /// Relocations list is expected to be from 0xDigit000 to 0xDigit999 because the 
        /// it serializes only to offset of thousands in each directory entry
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset">buffer offset to stat from. changes to the new offset 
        /// after the serialization</param>
        /// <param name="relocationsOffsetList"></param>
        public void Serialize(byte[] buffer,ref ulong bufferOffset,
            List<RelocationTypeOffset> relocationsOffsetList)
        {
            if (relocationsOffsetList == null) throw new ArgumentNullException(nameof(relocationsOffsetList));
            if (relocationsOffsetList.Count == 0) return;

            //serialize virtual address of the entry
            SerializeVirtualAddress(buffer, bufferOffset, relocationsOffsetList);

            //serialize size of block
            SerializeSizeOfBlock(buffer, bufferOffset, relocationsOffsetList);

            //serialize relocation offset
            bufferOffset = SerializeRelocationsOffsets(buffer, bufferOffset, relocationsOffsetList);
        }

        private static void SerializeSizeOfBlock(byte[] buffer, ulong bufferOffset, List<RelocationTypeOffset> relocationsOffsetList)
        {
            var bytesOfSizeOfBlock = BitConverter.GetBytes((UInt32)relocationsOffsetList.Count*2+8);
            bytesOfSizeOfBlock.CopyTo(buffer, (int)bufferOffset + 0x4);
        }

        private ulong SerializeRelocationsOffsets(byte[] buffer, ulong bufferOffset, List<RelocationTypeOffset> relocationsOffsetList)
        {
            //serialize relocaions offset
            bufferOffset += 0x8;
            foreach (var relocationOffset in relocationsOffsetList)
            {
                m_TypeOffsetSerializer.Serialize(buffer, ref bufferOffset, relocationOffset);
            }

            return bufferOffset;
        }

        private static void SerializeVirtualAddress(byte[] buffer, ulong bufferOffset, List<RelocationTypeOffset> relocationsOffsetList)
        {
            //get the first entry to get the virtual address base
            var firstOffset = relocationsOffsetList.First().Offset;
            var virtualAddress = firstOffset - firstOffset % 0x1000;
            var virtualAddressBytes = BitConverter.GetBytes((UInt32)virtualAddress);
            virtualAddressBytes.CopyTo(buffer, (int)bufferOffset);
        }
    }
}
