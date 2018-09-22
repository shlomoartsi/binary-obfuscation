using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.PeExtensions;
using PeNet;
using PeNet.Structures;
using PeNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Extensions
{
    public static class PeFileExtensions
    {

        public static byte[] GetBytesOfCode(this PeFile peFile)
        {
            if (peFile == null) throw new ArgumentNullException(nameof(peFile));

            var codeInMemoryLayout = peFile.GetCodeInMemoryLayout();

            var codeBytes = ExtractBytesOfCode(codeInMemoryLayout.OffsetOfCodeInBytes,
                codeInMemoryLayout.CodeActualSizeInBytes, peFile.Buff);

            return codeBytes;
        }

        public static ICodeInMemoryLayout GetCodeInMemoryLayout(this PeFile peFile)
        {
            if (peFile == null) throw new ArgumentNullException(nameof(peFile));
            ICodeInMemoryLayout codeInMemoryLayout = null;

            var optionalHeader = peFile.ImageNtHeaders.OptionalHeader;
            IMAGE_SECTION_HEADER codeSectionHeader = null;
            foreach (var sectionHeader in peFile.ImageSectionHeaders)
            {
                if ((sectionHeader.Characteristics & (uint)PeNet.Constants.SectionFlags.IMAGE_SCN_CNT_CODE) != 0 &&
                    sectionHeader.SizeOfRawData > 0)
                {
                    codeSectionHeader = sectionHeader;

                    var relocationDirectorySize = peFile.ImageNtHeaders.OptionalHeader.
                                                    DataDirectory[(int)Constants.DataDirectoryIndex.BaseReloc].Size;
                    var relocationDirectoryOffset = peFile.ImageNtHeaders.OptionalHeader.
                                                    DataDirectory[(int)Constants.DataDirectoryIndex.BaseReloc].VirtualAddress.
                                                    RVAtoFileMapping(peFile.ImageSectionHeaders);
                    var codeInMemoryLayoutFactory = Container.Container.Resolve<ICodeInMemoryLayoutFactory>();
                    var addressesOfCodeInData = peFile.GetAddressesOfCodeInData(sectionHeader);

                    if (peFile.ImageRelocationDirectory != null)
                    {
                        var relocationInfo = Container.Container.Resolve<IRelocationDirectoryInfoFactory>().Create(
                            peFile.ImageRelocationDirectory,
                            relocationDirectorySize,
                            peFile.Buff,
                            relocationDirectoryOffset,
                            addressesOfCodeInData);

                        codeInMemoryLayout = codeInMemoryLayoutFactory.Create(
                            peFile.ImageNtHeaders.OptionalHeader.ImageBase,
                            codeSectionHeader.PointerToRawData,
                            codeSectionHeader.VirtualSize,
                            codeSectionHeader.SizeOfRawData,
                            codeSectionHeader.PointerToRawData +
                            optionalHeader.AddressOfEntryPoint - sectionHeader.VirtualAddress,
                            codeSectionHeader.VirtualAddress,
                            relocationInfo);
                    }
                    else
                    {
                        codeInMemoryLayout = codeInMemoryLayoutFactory.Create(
                            peFile.ImageNtHeaders.OptionalHeader.ImageBase,
                            codeSectionHeader.PointerToRawData,
                            codeSectionHeader.VirtualSize,
                            codeSectionHeader.SizeOfRawData,
                            codeSectionHeader.PointerToRawData +
                            optionalHeader.AddressOfEntryPoint - sectionHeader.VirtualAddress,
                            codeSectionHeader.VirtualAddress,
                            null);
                    }


                    break;
                }
            }

            return codeInMemoryLayout;
        }

        public static IMAGE_SECTION_HEADER GetCodeSectionHeader(this PeFile peFile)
        {
            foreach (var sectionHeader in peFile.ImageSectionHeaders)
            {
                if ((sectionHeader.Characteristics & (uint)PeNet.Constants.SectionFlags.IMAGE_SCN_CNT_CODE) != 0 &&
                    sectionHeader.SizeOfRawData > 0)
                {
                    return sectionHeader;
                }
            }
            return null;
        }

        public static IReadOnlyList<IMAGE_SECTION_HEADER> GetSectionsOrderedByPhysicalLayout(this PeFile peFile)
        {
            return peFile.ImageSectionHeaders.OrderBy(sectionHeader => 
                sectionHeader.VirtualAddress.RVAtoFileMapping(peFile.ImageSectionHeaders)).ToList();
        }

        public static IReadOnlyList<IMAGE_SECTION_HEADER> GetSectionsOrderedByVirtualAddress(this PeFile peFile)
        {
            return peFile.ImageSectionHeaders.OrderBy(sectionHeader =>
                sectionHeader.VirtualAddress).ToList();
        }

        public static IMAGE_SECTION_HEADER FindRelocationSection(this PeFile peFile)
        {
            var dataDirectoryAddress = peFile.ImageNtHeaders.OptionalHeader.
                DataDirectory[(int)Constants.DataDirectoryIndex.BaseReloc].VirtualAddress;

            var sectionsOrderedByAddress = GetSectionsOrderedByVirtualAddress(peFile);

            //relocation section is usually the last
            for (int i = sectionsOrderedByAddress.Count-1; i >= 0; i--)
            {
                if (dataDirectoryAddress>= sectionsOrderedByAddress[i].VirtualAddress)
                {
                    if (i == sectionsOrderedByAddress.Count - 1) return sectionsOrderedByAddress[i];
                    else if (dataDirectoryAddress<sectionsOrderedByAddress[i+1].VirtualAddress) return sectionsOrderedByAddress[i];
                }
            }
            return null;

        }

        public static List<RelocationTypeOffsetItem> GetAddressesOfCodeInData(
                this PeFile peFile,
                IMAGE_SECTION_HEADER codeSectionHeader)
        {
            ulong startOfCodeAddress = codeSectionHeader.VirtualAddress;
            ulong endOfCodeAddress = startOfCodeAddress + codeSectionHeader.VirtualSize;
            List<RelocationTypeOffsetItem> relocationTypeOffsetList = new List<RelocationTypeOffsetItem>();
            if (peFile.ImageRelocationDirectory == null) return relocationTypeOffsetList;

            foreach (var relocationDirectoryInfo in peFile.ImageRelocationDirectory)
            {
                //for code section do not check for addresses in data.
                if (relocationDirectoryInfo.VirtualAddress >= startOfCodeAddress &&
                    relocationDirectoryInfo.VirtualAddress <= endOfCodeAddress) continue;

                foreach (var sectionHeader in peFile.ImageSectionHeaders)
                {
                    ulong endOfSection = sectionHeader.VirtualAddress + sectionHeader.VirtualSize;

                    //this section header contains the whole relocation directory entry
                    if (relocationDirectoryInfo.VirtualAddress >= sectionHeader.VirtualAddress &&
                        relocationDirectoryInfo.VirtualAddress < endOfSection)
                    {
                        foreach (var typeOffset in relocationDirectoryInfo.TypeOffsets)
                        {
                            if (typeOffset.Type == 0) continue;

                            //calculate the offset of the relocation directory info from the begining of section
                            var reloationDirectroryOffsetFromSectionBegining =
                                relocationDirectoryInfo.VirtualAddress - sectionHeader.VirtualAddress;

                            //read the address from buffer
                            uint bufferOffset = sectionHeader.PointerToRawData +
                                  typeOffset.Offset + reloationDirectroryOffsetFromSectionBegining;

                            //the address that appears in the data section - in term of RVA address 
                            //that sums also the image base. i.e. 0x411212
                            ulong addressInData = peFile.Buff.BytesToUInt32(bufferOffset);

                            //in case relocation info is 0
                            if (addressInData == 0) continue;

                            //the relative address in term of virtual address like 0x11212
                            ulong relativeAddressInData = addressInData - peFile.ImageNtHeaders.OptionalHeader.ImageBase;

                            //the address of code in data as intruction offset (like 0x212)
                            ulong addressInDataAsInstructionOffset = relativeAddressInData - codeSectionHeader.VirtualAddress;

                            //if the address in the data section is in code section, 
                            //then create an item and add it to list
                            if (relativeAddressInData >= startOfCodeAddress &&
                                relativeAddressInData <= endOfCodeAddress)
                            {
                                var relocationOffsetType = new RelocationTypeOffset()
                                {
                                    Offset = relocationDirectoryInfo.VirtualAddress +
                                         typeOffset.Offset,
                                    Type = typeOffset.Type,
                                };

                                relocationTypeOffsetList.Add(new RelocationTypeOffsetItem(
                                    addressInDataAsInstructionOffset,
                                    relocationOffsetType, sectionHeader));

                            }
                        }

                        //continue examine next relocation directory info entry
                        break;
                    }
                }
            }
            return relocationTypeOffsetList;
        }


        private static byte[] ExtractBytesOfCode(ulong bufferPosition, ulong length, byte[] buff)
        {
            byte[] codeBuffer = new byte[length];
            for (ulong i = 0; i < length; i++)
            {
                codeBuffer[i] = buff[bufferPosition + i];
            }
            return codeBuffer;
        }

        

    }
}
