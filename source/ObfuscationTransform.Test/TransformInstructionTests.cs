using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObfuscationTransform.Transformation;
using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using SharpDisasm.Translators;
using SharpDisasm;
using System.Linq;

namespace ObfuscationTransform.Test
{


    [TestClass]
    public class TransformInstructionTests
    {
        static InstructionWithAddressOperandTransform m_jumpTransform;

        [ClassInitialize]
        public static void InitTestClass(TestContext context)
        {
            m_jumpTransform = new InstructionWithAddressOperandTransform(new Core.InstructionWithAddressOperandDecider(),
             new DisassemblerFactory());
        }

      
      
        [TestMethod]
        public void Test_FindSubbarray2()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3 };
            byte[] bytesToFind = { 2, 3 };

            //act
            var idx = m_jumpTransform.FindOriginalBytesIndex(byteArray, bytesToFind);

            //assert
            Assert.AreEqual(idx, 1);
        }

        [TestMethod]
        public void Test_FindSubbarray3()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3 };
            byte[] bytesToFind = { 3, 2, 3 };

            //act
            var idx = m_jumpTransform.FindOriginalBytesIndex(byteArray, bytesToFind);

            //assert
            Assert.AreEqual(idx, 2);
        }

        [TestMethod]
        public void Test_FindSubbarray4()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3 };
            byte[] bytesToFind = { 1, 2, 3, 2, 3 };

            //act
            var idx = m_jumpTransform.FindOriginalBytesIndex(byteArray, bytesToFind);

            //assert
            Assert.AreEqual(idx, 0);
        }

        [TestMethod]
        public void Test_FindSubbarray5()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3, 4 };
            byte[] bytesToFind = { 4 };

            //act
            var idx = m_jumpTransform.FindOriginalBytesIndex(byteArray, bytesToFind);

            //assert
            Assert.AreEqual(idx, 5);
        }


        [TestMethod]
        public void Test_FindSubbarray6()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3 };
            byte[] bytesToFind = { 1, 2, 7 };

            //act
            var idx = m_jumpTransform.FindOriginalBytesIndex(byteArray, bytesToFind);

            //assert
            Assert.AreEqual(idx, -1);
        }


        [TestMethod]
        public void Test_ReplaceSubbarray1()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3 };
            byte[] bytesToReplace = { 8, 9 };

            //act
            m_jumpTransform.replaceBytes(byteArray, 0, bytesToReplace);

            //assert
            Assert.IsTrue(ArrayEquals(byteArray, (new byte[] { 8, 9, 3, 2, 3 })));
        }



        [TestMethod]
        public void Test_ReplaceSubbarray2()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3 };
            byte[] bytesToReplace = { 3, 4 };

            //act
            m_jumpTransform.replaceBytes(byteArray, 3, bytesToReplace);

            //assert
            Assert.IsTrue(ArrayEquals(byteArray, (new byte[] { 1, 2, 3, 3, 4 })));
        }


        [TestMethod]
        public void Test_ReplaceSubbarray5()
        {
            //arrange
            byte[] byteArray = { 1, 2, 3, 2, 3 };
            byte[] bytesToReplace = { 3, 4, 7, 8, 2 };

            //act
            m_jumpTransform.replaceBytes(byteArray, 0, bytesToReplace);

            //assert
            Assert.IsTrue(ArrayEquals(byteArray, (new byte[] { 3, 4, 7, 8, 2 })));
        }

        private bool ArrayEquals(byte[] array1, byte[] array2)
        {
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            };

            return true;
        }



        [TestMethod]
        public void IncrementInstructionAddressTest()
        {
            //arrange
            byte[] code = new byte[] { 0xeb, 0xe9 }; //jmp 0xe9
            var instruction = new DisassemblerFactory().Create(code).Disassemble().First();
            IAssemblyInstructionForTransformation newInstruction=null;

            //act
            try
            {
                newInstruction = m_jumpTransform.IncrementJumpInstructionTargetAddress(instruction, 5);
            }
            catch (Exception ex)
            {
                Assert.Fail("IncrementInstructionAddressTest. Details:" + ex.ToString());
            }


            //assert
            Assert.IsTrue(ArrayEquals(newInstruction.Bytes, new byte[] { 0xeb, 0xee }));
        }


        [TestMethod]
        public void IncrementInstructionAddressTest2()
        {
            //arrange
            byte[] code = new byte[] { 0xE9, 0xE9, 0xEB, 0x00, 0x00 };
            var instruction = new DisassemblerFactory().Create(code).Disassemble().First();
            IAssemblyInstructionForTransformation newInstruction = null;


            //act
            try
            {
                newInstruction = m_jumpTransform.IncrementJumpInstructionTargetAddress(instruction, 5);
            }
            catch (Exception ex)
            {
                Assert.Fail("IncrementInstructionAddressTest. Details:" + ex.ToString());
            }


            //assert
            Assert.IsTrue(ArrayEquals(newInstruction.Bytes, new byte[] { 0xE9, 0xEE, 0xEB, 0x00, 0x00 }));
        }

        
    }
}
