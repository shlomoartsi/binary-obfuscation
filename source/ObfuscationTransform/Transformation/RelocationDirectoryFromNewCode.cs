using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.PeExtensions;
using PeNet.Structures;
using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    public class RelocationDirectoryFromNewCode : IRelocationDirectoryFromNewCode
    {
        private readonly IImageBaseRelocationSerializer m_ImageBaseRelocationSerializer;
        private readonly IRelocationDirectoryInfoFactory m_relocationDirectoryInfoFactory;

        public RelocationDirectoryFromNewCode(IImageBaseRelocationSerializer imageBaseRelcationSerializer,
            IRelocationDirectoryInfoFactory relocationDirectoryInfoFactory)
        {
            m_ImageBaseRelocationSerializer = imageBaseRelcationSerializer ?? 
                                                throw new ArgumentNullException(nameof(imageBaseRelcationSerializer));
            m_relocationDirectoryInfoFactory = relocationDirectoryInfoFactory ?? 
                                                throw new ArgumentNullException(nameof(relocationDirectoryInfoFactory));
            
        }

        public IRelocationDirectoryInfo CreateNewRelocationDirectoryInfo(ICode code)
        {
            //in case there is not directory info
            if (code.CodeInMemoryLayout.RelocationDirectoryInfo == null) return null;

            List<RelocationTypeOffset> relocationTypeOffetList = CreateNewRelocationList(code);

            IRelocationDirectoryInfo directoryInfo = CreateDirectoryInfoFromList(
                relocationTypeOffetList, code,m_ImageBaseRelocationSerializer,m_relocationDirectoryInfoFactory);
            return directoryInfo;
        }

        private IRelocationDirectoryInfo CreateDirectoryInfoFromList(
            List<RelocationTypeOffset> relocationTypeOffetList, ICode code,
            IImageBaseRelocationSerializer imageBaseRelocationSerializer,
            IRelocationDirectoryInfoFactory relocationDirecroryInfoFactory)
        {
            var relocationInfo = code.CodeInMemoryLayout.RelocationDirectoryInfo;
            
            //the relocation directory might grow up to twice. the obfuscation algorithm might add jump instruction for each existing
            //jump instruction which doubles the number of addresses
            var newBufferSize = relocationInfo.RelocationDirectorySize * 2;
            byte[] buffer = new byte[newBufferSize];

            List<IMAGE_BASE_RELOCATION> imageBaseRelocationList = new List<IMAGE_BASE_RELOCATION>();
            //contains reloction offset list of virtual addresses from 0 to 999 after performing
            //virtual address%1000
            IMAGE_BASE_RELOCATION relocations0To999;
            ulong bufferIndex = 0;
            int listIndex = 0;
            while (listIndex < relocationTypeOffetList.Count)
            {
                relocations0To999 = CreateRelocationsFrom0To999(relocationTypeOffetList,
                            ref listIndex, buffer, ref bufferIndex, imageBaseRelocationSerializer);
                imageBaseRelocationList.Add(relocations0To999);
            }

            List<RelocationTypeOffsetItem> newCodeAddressesInData = GetNewAddressesInData(code);

            return relocationDirecroryInfoFactory.Create(imageBaseRelocationList.ToArray(),
                (uint)bufferIndex, buffer, 0, newCodeAddressesInData);
        }

  

        private IMAGE_BASE_RELOCATION CreateRelocationsFrom0To999(
            List<RelocationTypeOffset> relocationTypeOffetList, ref int listIndex,
            byte[] buffer, ref ulong bufferIndex,
            IImageBaseRelocationSerializer imageBaseRelocationSerialiazer)
        {
            //get the first item virtual relocation address
            if (relocationTypeOffetList.Count < listIndex) return null;
            if ((uint)bufferIndex >= buffer.Length) return null;
            var firstRelocationAddress = relocationTypeOffetList[listIndex].Offset;
            //if for example the first virtual address to check is 0x3034, than max address is 0x4000
            var maxVirtualAddressToCheck = ((firstRelocationAddress / 0x1000) + 1) * 0x1000;
            var relocationsList0To999 = new List<RelocationTypeOffset>();
            var oldBufferIndex = bufferIndex;

            while (relocationTypeOffetList.Count > listIndex &&
                   relocationTypeOffetList[listIndex].Offset < maxVirtualAddressToCheck)
            {
                relocationsList0To999.Add(relocationTypeOffetList[listIndex]);
                listIndex++;
            }

            if (relocationsList0To999.Count % 2 != 0)
            {
                relocationsList0To999.Add(RelocationTypeOffset.PaddingRelocationOffset);
            }

            imageBaseRelocationSerialiazer.Serialize(buffer, ref bufferIndex, relocationsList0To999);
            return new IMAGE_BASE_RELOCATION(buffer, (uint)oldBufferIndex, (uint)buffer.Length);
        }


        private List<RelocationTypeOffset> CreateNewRelocationList(ICode code)
        {
            List<RelocationTypeOffset> oldRelocationTypeOffsetList = 
                                                CreateOldRelocationTypeOffsetList(code);
            

            //create the new relocation list from the old relocation entries
            ulong newOffset = 0;
            ulong newProgramCounter = 0;
            var instructionEnum = code.AssemblyInstructions.GetEnumerator();
            List<RelocationTypeOffset> newRelocationTypeOffsetList = new List<RelocationTypeOffset>();

            instructionEnum.MoveNext(); //move to first instruction
            newProgramCounter = (ulong)instructionEnum.Current.Length;

            foreach (var relocationTypeOffsetInfo in oldRelocationTypeOffsetList)
            {
                // iterate from last instruction that was related to an offset.
                // the same instruction can have few relocation entries.
                while (instructionEnum.Current != null)
                {
                    var instruction = instructionEnum.Current;
                    if (IsInstructionBeforeOffset(instruction, relocationTypeOffsetInfo,
                                                 code.CodeInMemoryLayout.CodeVirtualAddress))
                    {
                        if (instructionEnum.MoveNext())
                        {
                            newOffset = newProgramCounter;
                            newProgramCounter += (ulong)instructionEnum.Current.Bytes.Length;
                        }
                    }
                    else if (IsOffsetInInstruction(instruction, relocationTypeOffsetInfo,
                             code.CodeInMemoryLayout.CodeVirtualAddress))
                    {
                        var offsetFromBeginingOfInstruction =
                            (relocationTypeOffsetInfo.Offset - instruction.Offset) % 0x1000;

                        newRelocationTypeOffsetList.Add(new RelocationTypeOffset()
                        {
                            Type = relocationTypeOffsetInfo.Type,
                            Offset = newOffset +
                                     code.CodeInMemoryLayout.CodeVirtualAddress +
                                     offsetFromBeginingOfInstruction
                        });
                        break;
                    }
                }

                //add the rest of relocation info. the remaining relocation entries are of
                //the next section
                if (instructionEnum.Current == null)
                {
                    newRelocationTypeOffsetList.Add(relocationTypeOffsetInfo);
                }
            }

            if (newRelocationTypeOffsetList.Count != oldRelocationTypeOffsetList.Count)
            {
                throw new ApplicationException("some of relocation entries were not transfomed!");
            }
            return newRelocationTypeOffsetList;
        }


        private static List<RelocationTypeOffset> CreateOldRelocationTypeOffsetList(ICode code)
        {
            List<RelocationTypeOffset> oldRelocationTypeOffsetList = new List<RelocationTypeOffset>();

            //collect old relocation entries into a list
            foreach (var relocationDirectory in code.CodeInMemoryLayout.RelocationDirectoryInfo.ImageRelocationDirectory)
            {
                foreach (var relocationInfo in relocationDirectory.TypeOffsets)
                {
                    //this is just an alignment block
                    if (relocationInfo.Type == 0)
                    {
                        continue;
                    }
                    oldRelocationTypeOffsetList.Add(new RelocationTypeOffset
                    {
                        Offset = relocationInfo.Offset + relocationDirectory.VirtualAddress,
                        Type = relocationInfo.Type
                    });
                }
            }

            return oldRelocationTypeOffsetList;
        }


        /// <summary>
        /// is instruction before relocation offset
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="relocationOffset"></param>
        /// <param name="codeVirtualAddress"></param>
        /// <returns></returns>
        private static bool IsInstructionBeforeOffset(IInstruction instruction,
            RelocationTypeOffset relocationOffset, ulong codeVirtualAddress)
        {
            return (instruction.PC + codeVirtualAddress <= relocationOffset.Offset);
        }

        private static bool IsOffsetInInstruction(IInstruction instruction,
            RelocationTypeOffset relocationOffset,ulong codeVirtualAddress)
        {
            return (instruction.Offset + codeVirtualAddress <= relocationOffset.Offset &&
                    instruction.PC + codeVirtualAddress >  relocationOffset.Offset);
        }

        private List<RelocationTypeOffsetItem> GetNewAddressesInData(ICode code)
        {
            //create a hashtable with all addresses in data
            //create a dictionary from old address to all addresses in code 
            var oldAdressesDictionary = CreateOldAddressesDictionary(
                code.CodeInMemoryLayout.RelocationDirectoryInfo.AddressesOfCodeInDataSection);
            var newAddressesInDataList = new List<RelocationTypeOffsetItem>();
            ulong newOffset = 0;

            foreach (var instruction in code.AssemblyInstructions)
            {
                List<RelocationTypeOffsetItem> relocationTypeOffsetList = null;
                if (oldAdressesDictionary.TryGetValue(instruction.Offset,
                                                      out relocationTypeOffsetList))
                {
                    //Add to the list the relocation type offset with new updated address
                    foreach (var item in relocationTypeOffsetList)
                    {
                        newAddressesInDataList.Add(new RelocationTypeOffsetItem(newOffset,
                            item.RelocationTypeOffset,item.Tag));
                    }
                }

                newOffset += (ulong)instruction.Length;
            }

            return newAddressesInDataList;
        }

        private Dictionary<ulong, List<RelocationTypeOffsetItem>> CreateOldAddressesDictionary(
            List<RelocationTypeOffsetItem> addressesOfCodeInDataSection)
        {
            var dictionary = new Dictionary<ulong, List<RelocationTypeOffsetItem>>();
            foreach (var item in addressesOfCodeInDataSection)
            {
                List<RelocationTypeOffsetItem> relocationTypeOffsetList = null;
                if (!dictionary.TryGetValue(item.Address,out relocationTypeOffsetList))
                {
                    relocationTypeOffsetList = new List<RelocationTypeOffsetItem>();
                    dictionary[item.Address] = relocationTypeOffsetList;
                }
                relocationTypeOffsetList.Add(item);
            }

            return dictionary;
        }
    }
}
