using ObfuscationTransform.Core;
using PeNet;
using System.IO;

namespace ObfuscationTransform.Transformation
{
    public interface IPeTransform
    {
        ICode Code { get; }
        ICode TransformedCode { get; }
        PeFile PeFile { get; }
        void Transform(string filePath);
    }
}