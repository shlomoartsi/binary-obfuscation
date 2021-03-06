// <copyright file="ImageResourceDirectoryParserTest.cs">Copyright ©  2016</copyright>

using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeNet.Parser.Tests
{
    [TestClass]
    [PexClass(typeof(ImageResourceDirectoryParser))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class ImageResourceDirectoryParserTest
    {
        [PexMethod]
        internal ImageResourceDirectoryParser Constructor(byte[] buff, uint offset)
        {
            var target = new ImageResourceDirectoryParser(buff, offset);
            return target;
            // TODO: add assertions to method ImageResourceDirectoryParserTest.Constructor(Byte[], UInt32)
        }
    }
}