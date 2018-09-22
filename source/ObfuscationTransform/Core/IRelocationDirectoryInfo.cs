using ObfuscationTransform.PeExtensions;
using PeNet.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public interface IRelocationDirectoryInfo
    {
        /// <summary>
        /// the program relocation directory
        /// </summary>
        IMAGE_BASE_RELOCATION[] ImageRelocationDirectory { get; }

        /// <summary>
        /// the buffer where the relocation directory is serialized
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// the relocation directory size (of serialization) in bytes
        /// </summary>
        uint RelocationDirectorySize { get;}

        /// <summary>
        /// the offset of relocation directory serialization 
        /// </summary>
        ulong OffsetInBuffer { get; }
        
        /// <summary>
        /// list of addresses of code that appears in the code 
        /// </summary>
        /// <remarks>each address have a relocation offset that describes it</remarks>
        List<RelocationTypeOffsetItem> AddressesOfCodeInDataSection { get; }
    }
}
