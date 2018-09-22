using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObfuscationTransform.PeExtensions;
using PeNet.Structures;
using System.Collections.Generic;

namespace ObfuscationTransform.Test
{
    [TestClass]
    public class RelocationDirectorySerializationTests
    {
        [TestMethod]
        public void SerializeTypeOffset_Correct()
        {
            //arrange
            TypeOffsetSerializer serializer = new TypeOffsetSerializer();
            byte[] buffer = new byte[2];
            ulong i = 0;

            //act
            serializer.Serialize(buffer, ref i, new RelocationTypeOffset { Type = 0x3, Offset = 0x1912 });

            //assert
            Assert.AreEqual(buffer[0], 0x12);
            Assert.AreEqual(buffer[1], 0x39);
        }

        [TestMethod]
        public void SerializeDirectoryInfo_Correct()
        {
            //arrange
            var imageBaseRelocation = new ImageBaseRelocationSerializer(new TypeOffsetSerializer());
            var buffer = new byte[100];
            ulong bufferOffset = 0;
            var relocationOffsetList = new List<RelocationTypeOffset>()
            {
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1000},
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1020},
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1123},
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1300},
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1300},
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1411},
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1a1f},
                new RelocationTypeOffset(){ Type=0x3,Offset = 0x1FFF},
            };

            //act
            imageBaseRelocation.Serialize(buffer, ref bufferOffset, relocationOffsetList);

            //assert
            try
            {
                var relocationDirectory = new IMAGE_BASE_RELOCATION(buffer, 0, (uint)relocationOffsetList.Count*2+8);
                Assert.AreEqual((int)relocationDirectory.SizeOfBlock, 0x18);
                Assert.AreEqual((int)relocationDirectory.VirtualAddress, 0x1000);
                Assert.AreEqual(relocationDirectory.TypeOffsets.Length, 8);
                Assert.AreEqual(relocationDirectory.TypeOffsets[0].Type, 0x3);
                Assert.AreEqual(relocationDirectory.TypeOffsets[0].Offset,0x000);
                Assert.AreEqual(relocationDirectory.TypeOffsets[1].Type, 0x3);
                Assert.AreEqual(relocationDirectory.TypeOffsets[1].Offset, 0x020);
                Assert.AreEqual(relocationDirectory.TypeOffsets[7].Type, 0x3);
                Assert.AreEqual(relocationDirectory.TypeOffsets[7].Offset, 0xFFF);

            }
            catch (Exception ex)
            {
                Assert.Fail("failed to serialize relocation directory: exception", ex.ToString());
            }

        }

    }
}
