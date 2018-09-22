using ObfuscationTransform.Core;
using System;
using PeNet.Structures;
using ObfuscationTransform.PeExtensions;
using System.Collections.Generic;

public struct RelocationDirectoryInfo : IRelocationDirectoryInfo
{
	public  RelocationDirectoryInfo(IMAGE_BASE_RELOCATION[] imageRelolcationDirectory, byte[] buffer,
        uint relocationDirectorySize,ulong offsetInBuffer,
        List<RelocationTypeOffsetItem> addressesOfCodeInDataSection)
	{
        ImageRelocationDirectory = imageRelolcationDirectory ?? throw new ArgumentNullException(nameof(imageRelolcationDirectory));
        Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        RelocationDirectorySize = relocationDirectorySize;
        OffsetInBuffer = offsetInBuffer;
        AddressesOfCodeInDataSection = addressesOfCodeInDataSection;
	}

    public IMAGE_BASE_RELOCATION[] ImageRelocationDirectory { get; private set; }

    public byte[] Buffer { get; private set; }

    public uint RelocationDirectorySize { get; private set; }
    
    public ulong OffsetInBuffer { get; private set; }

    public List<RelocationTypeOffsetItem> AddressesOfCodeInDataSection { get; private set; }
}
