using PeNet.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public interface ICodeInMemoryLayout
    {
        ulong ImageBaseAddress { get; }
        ulong OffsetOfCodeInBytes { get; }
        ulong CodeActualSizeInBytes { get; }
        ulong CodePhysicalSizeInBytes { get; }
        ulong EntryPointOffset { get; }
        ulong CodeVirtualAddress { get; }
        IRelocationDirectoryInfo RelocationDirectoryInfo{get;}
    }
}
