using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObfuscationTransform.Parser;
using SharpDisasm;
using SharpDisasm.Udis86;

namespace ObfuscationTransform.Test
{
    [TestClass]
    public class FunctionPrologInstructionTest
    {
        [TestMethod]
        public void FunctionProlog_ParseWithNullAssemblyToStartFrom_ThrowsNullArgumentException()
        {
            //arrange
            var functionPrologParser = new FunctionPrologParser();

            //act
            Exception ex=null;
            var isExceptionThrown = ExceptionTesterUtil.CheckIfExceptionIsThrown<ArgumentNullException>(
                () => functionPrologParser.Parse(null, 0),
                ref ex) ;

            //assert
            Assert.IsTrue(isExceptionThrown, "Parsing ");
        }

        [TestMethod]
        public void FunctionProlog_ParseWithAssemblyOutOfRange_ThrowsNullArgumentException()
        {
            //arrange
            var functionPrologParser = new FunctionPrologParser();

            //act
            Exception ex = null;
            var isExceptionThrown = ExceptionTesterUtil.CheckIfExceptionIsThrown<ArgumentOutOfRangeException>(
                () => functionPrologParser.Parse(new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iaddsd) { Offset = 1 },
                                                 0),
                ref ex);

            //assert
            Assert.IsTrue(isExceptionThrown, "Parsing prolog shoulf throw ArgumentNullException");
        }

        [TestMethod]
        public void FunctionProlog_ParserWithNoFunctionProlog_ReturnsNull()
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
            var parser = new FunctionPrologParser();
            var lastPrologAssembly = parser.Parse(instruction1, 2);

            //assert
            Assert.IsNull(lastPrologAssembly, "last prolog assembly should be null");

        }


      

        [TestMethod]
        public void FunctionProlog_ParserWithFunctionPrologAtFirstInstruction_ReturnsNonNull()
        {
            //arrange
            DummyInstruction instruction1 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Ipush);
            DummyInstruction instruction2 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Imov);
            DummyInstruction instruction3 = new DummyInstruction(SharpDisasm.Udis86.ud_mnemonic_code.UD_Iadc);

            //set instruction 1
            instruction1.NextInstruction = instruction2;
            instruction1.Offset = 0;
            ud_operand ud_operand1 = new ud_operand();
            ud_operand1.@base = ud_type.UD_R_EBP;
            var operand1 = new Operand(ud_operand1);
            instruction1.Operands = new Operand[] { operand1 };

            //set instruction 2
            instruction2.NextInstruction = instruction3;
            ud_operand ud_operand2_1= new ud_operand();
            ud_operand2_1.@base = SharpDisasm.Udis86.ud_type.UD_R_EBP;
            var operand2_1 = new Operand(ud_operand2_1);

            ud_operand ud_operand2_2 = new ud_operand();
            ud_operand2_2.@base = SharpDisasm.Udis86.ud_type.UD_R_ESP;
            var operand2_2 = new Operand(ud_operand2_2);
            instruction2.Operands = new Operand[] { operand2_1, operand2_2 };
            
            instruction3.Offset = 2;

            //act
            var parser = new FunctionPrologParser();
            var lastPrologAssembly = parser.Parse(instruction1, 2);

            //assert
            Assert.IsNotNull(lastPrologAssembly, "last Prolog assembly should not  be null");

        }

      
    }   

    
}
