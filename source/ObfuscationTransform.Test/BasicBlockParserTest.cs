using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SharpDisasm.Udis86;
using System.Linq;
using ObfuscationTransform.Core;
using SharpDisasm;

namespace ObfuscationTransform.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class BasicBlockParserTest
    {
        public BasicBlockParserTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Test()
        {
            //Arrange
            

            //Act

            //Assert
            
        }
    }


    class DummyInstruction : IAssemblyInstructionForTransformation
    {
        private ud_mnemonic_code mnemonic;

        public DummyInstruction(ud_mnemonic_code _mnemonic)
        {
            mnemonic = _mnemonic;
        }

        public byte[] Bytes
        {
            get
            {
                return new byte[0];
            }
        }

        public bool Error
        {
            get
            {
                return false;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return string.Empty;
            }
        }

        public bool IsNew
        {
            get
            {
                return false;
            }

            set
            {
                
            }
        }

        public bool IsPaddingInstruction
        {
            get
            {
                return false;
            }
        }

        public int Length
        {
            get
            {
                return 2;
            }
        }

        public ud_mnemonic_code Mnemonic
        {
            get
            {
                return mnemonic;
            }
        }

        public IAssemblyInstructionForTransformation NewInstruction
        {
            get
            {
                return null;
            }

            set
            {

            }
        }

        public IAssemblyInstructionForTransformation NextInstruction { get; set; }

        public ulong Offset
        {
            get;
            set;
        }

        public Operand[] Operands { get; set; }

        public ulong PC
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAssemblyInstructionForTransformation PreviousInstruction
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetOffset(ulong offset)
        {
            throw new NotImplementedException();
        }

        public void SetPC(ulong programCounter)
        {
            throw new NotImplementedException();
        }

        public string ToString(bool includeAddress, bool includeBinary)
        {
            throw new NotImplementedException();
        }
    }
}
