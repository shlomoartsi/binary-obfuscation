using PeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public interface ICodeObfuscator
    {
        ICode NewCode { get; }
        void Obfuscate(string newFileName,PeFile peFile,ICode code);
    }
}
