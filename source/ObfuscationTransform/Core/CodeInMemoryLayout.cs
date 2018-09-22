using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeNet.Structures;

namespace ObfuscationTransform.Core
{
    public class CodeInMemoryLayout : ICodeInMemoryLayout
    {
        public CodeInMemoryLayout(ulong imageBaseAddress,ulong offsetOfCodeInBytes, ulong codeActualSizeInBytes,
            ulong codePhysicalSizeInBytes,ulong entryPointOffset, ulong codeVirtualAddress,
            IRelocationDirectoryInfo relocationDirectoryInfo)
        {
            ImageBaseAddress = imageBaseAddress;
            OffsetOfCodeInBytes = offsetOfCodeInBytes;
            CodeActualSizeInBytes = codeActualSizeInBytes;
            CodePhysicalSizeInBytes = codePhysicalSizeInBytes;
            EntryPointOffset = entryPointOffset;
            CodeVirtualAddress = codeVirtualAddress;
            RelocationDirectoryInfo = relocationDirectoryInfo;
        }

        public ulong ImageBaseAddress  { get;protected set; }

        public ulong OffsetOfCodeInBytes { get; protected set; }

        public ulong CodeActualSizeInBytes { get; protected set; }

        public ulong CodePhysicalSizeInBytes { get; protected set; }

        public ulong EntryPointOffset { get; protected set; }

        public ulong CodeVirtualAddress { get; protected set; }

        public IRelocationDirectoryInfo RelocationDirectoryInfo { get; protected set; }

    }
}
