using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObfuscationTransform.Parser;

namespace ObfuscationTransform.Test
{
    [TestClass]
    public class FunctionEpilogInstructionTest
    {
        [TestMethod]
        public void FunctionEpilog_ParseWithNullAssemblyToStartFrom_ThrowsNullArgumesntException()
        {
            //arrange
            var functionEpilogParser = new FunctionEpilogParser();

            //act
            Exception ex=null;
            var isExceptionThrown = ExceptionTesterUtil.CheckIfExceptionIsThrown<ArgumentNullException>(
                () => functionEpilogParser.Parse(null, 0),
                ref ex) ;

            //assert
            Assert.IsTrue(isExceptionThrown, "Parsing ");
        }

        [TestMethod]
        public void FunctionEpilog_ParseWithAssemblyOutOfRange_ThrowsNullArgumentException()
        {
            //arrange
            var functionEpilogParser = new FunctionEpilogParser();

            //act
            Exception ex = null;
            var isExceptionThrown = ExceptionTesterUtil.CheckIfExceptionIsThrown<ArgumentOutOfRangeException>(
                () => functionEpilogParser.Parse(new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaddsd) { Offset = 1 },
                                                 0),
                ref ex);

            //assert
            Assert.IsTrue(isExceptionThrown, "Parsing ");
        }

        [TestMethod]
        public void FunctionEpilog_ParserWithNoFunctionEpilog_ReturnsNull()
        {
            //arrange
            DummyInstruction instruction1 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaaa);
            DummyInstruction instruction2 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaad);
            DummyInstruction instruction3 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iadc);
            instruction1.NextInstruction = instruction2;
            instruction1.Offset = 0;
            instruction2.NextInstruction = instruction3;
            instruction2.Offset = 1;
            instruction3.Offset = 2;

            //act
            var parser = new FunctionEpilogParser();
            var lastEpilogAssembly = parser.Parse(instruction1, 2);

            //assert
            Assert.IsNull(lastEpilogAssembly, "last epilog assembly should be null");

        }


        [TestMethod]
        public void FunctionEpilog_ParserWithFunctionEpilogAtFirstInstruction_ReturnsNonNull()
        {
            //arrange
            DummyInstruction instruction1 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iret);
            DummyInstruction instruction2 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaad);
            DummyInstruction instruction3 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iadc);
            instruction1.NextInstruction = instruction2;
            instruction1.Offset = 0;
            instruction2.NextInstruction = instruction3;
            instruction2.Offset = 1;
            instruction3.Offset = 2;

            //act
            var parser = new FunctionEpilogParser();
            var lastEpilogAssembly = parser.Parse(instruction1, 2);

            //assert
            Assert.IsNotNull(lastEpilogAssembly, "last epilog assembly should not  be null");

        }

        [TestMethod]
        public void FunctionEpilog_ParserWithFunctionEpilogAtSecondInstruction_ReturnsNonNull()
        {
            //arrange
            DummyInstruction instruction1 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaaa);
            DummyInstruction instruction2 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iretf);
            DummyInstruction instruction3 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iadc);
            instruction1.NextInstruction = instruction2;
            instruction1.Offset = 0;
            instruction2.NextInstruction = instruction3;
            instruction2.Offset = 1;
            instruction3.Offset = 2;

            //act
            var parser = new FunctionEpilogParser();
            var lastEpilogAssembly = parser.Parse(instruction1, 2);

            //assert
            Assert.IsNotNull(lastEpilogAssembly, "last epilog assembly should not  be null");

        }

        [TestMethod]
        public void FunctionEpilog_ParserWithFunctionEpilogAtThirdInstruction_ReturnsNonNull()
        {
            //arrange
            DummyInstruction instruction1 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaad);
            DummyInstruction instruction2 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaad);
            DummyInstruction instruction3 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iretf);
            instruction1.NextInstruction = instruction2;
            instruction1.Offset = 0;
            instruction2.NextInstruction = instruction3;
            instruction2.Offset = 1;
            instruction3.Offset = 2;

            //act
            var parser = new FunctionEpilogParser();
            var lastEpilogAssembly = parser.Parse(instruction1, 2);

            //assert
            Assert.IsNotNull(lastEpilogAssembly, "last epilog assembly should not  be null");

        }
    }   

    
}
