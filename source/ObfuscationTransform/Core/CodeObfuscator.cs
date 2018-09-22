using ObfuscationTransform.Extensions;
using ObfuscationTransform.Transformation;
using ObfuscationTransform.Transformation.Factory;
using ObfuscationTransform.Transformation.Junk;
using PeNet;
using PeNet.Structures;
using PeNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObfuscationTransform.Core
{
    public class CodeObfuscator : ICodeObfuscator
    {
        private readonly List<ITransformation> m_transformationList;
        private readonly ITransformationExecuterFactory m_transformationExecuterFactory;
        private readonly IStatistics m_statistics;

        public ICode NewCode { get; private set; }

        public CodeObfuscator(ITransformationAddingJunkBytes junkBytesTransformation,
            ITransformationAddingUnconditionalJump transformationToConditionalJump,
            ITransformationExecuterFactory transfomationExecuterFactory,
            IStatistics statistics)
        {
            m_transformationList = new List<ITransformation>()
                                   { 
                                     transformationToConditionalJump,
                                     junkBytesTransformation
                                   };
            m_transformationExecuterFactory = transfomationExecuterFactory ?? throw new ArgumentNullException(nameof(transfomationExecuterFactory));
            m_statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        }

        public void Obfuscate(string newFileName,PeFile peFile,ICode code)
        {
            var transformationExecuter = m_transformationExecuterFactory.Create(m_transformationList);
            NewCode  = transformationExecuter.Transform(code);

            GenerateNewFileFromCode(newFileName,code, NewCode, peFile,m_statistics);
        }


        private void GenerateNewFileFromCode(string newFileName, ICode oldCode,
                                             ICode newCode, 
                                             PeFile peFile, IStatistics statistics)
        {
            
            RewriteCodeSection(oldCode,newCode, peFile);

            //the program is extention (by relocation table bytes)
            byte[] programExtensionBuffer = null;
            RewriteRelocationDirectory(oldCode,newCode, peFile,out programExtensionBuffer);

            RewriteEntryPointAddress(newCode, peFile);

            RewriteCodeAdressesInDataSection(newCode, peFile);

            RewriteCodeSize(newCode, peFile, statistics);

            WriteProgramBytesToFile(newFileName, peFile.Buff, programExtensionBuffer);
                        
        }

        private void WriteProgramBytesToFile(string newFileName, byte[] programBuffer, byte[] programExtensionBuffer)
        {
            using (var file = System.IO.File.OpenWrite(newFileName))
            {
                file.Write(programBuffer, 0, programBuffer.Length);
                if (programExtensionBuffer != null) file.Write(programExtensionBuffer, 0, programExtensionBuffer.Length);
                file.Close();
            }
        }

        private void RewriteEntryPointAddress(ICode code, PeFile peFile)
        {
            //update entry point address
            peFile.ImageNtHeaders.OptionalHeader.AddressOfEntryPoint = (uint)(code.CodeInMemoryLayout.EntryPointOffset + code.CodeInMemoryLayout.CodeVirtualAddress - code.CodeInMemoryLayout.OffsetOfCodeInBytes);
        }

        private void RewriteRelocationDirectory(ICode oldCode,ICode newCode, PeFile peFile,out byte[] programExtensionBuffer)
        {
            programExtensionBuffer = null;
            if (newCode.CodeInMemoryLayout.RelocationDirectoryInfo == null) return;
            var newRelocationDirectorySize = newCode.CodeInMemoryLayout.RelocationDirectoryInfo.RelocationDirectorySize;
            
            //UPDATE THE RELOCATION DIRECTORY SIZE
            //old reloation directory size is different than the new code
            if (oldCode.CodeInMemoryLayout.RelocationDirectoryInfo.RelocationDirectorySize != newRelocationDirectorySize)
            {
                peFile.ImageNtHeaders.OptionalHeader.DataDirectory[(int)PeNet.Constants.DataDirectoryIndex.BaseReloc].Size =
                        newRelocationDirectorySize;

                //if the section of reloaction physical size is shorter than new relocation size,
                //we need to extend the buffer size (and actually the program size)
                IMAGE_SECTION_HEADER relocationSection = peFile.FindRelocationSection();
                if (relocationSection.SizeOfRawData < newRelocationDirectorySize)
                {
                    if (peFile.GetSectionsOrderedByPhysicalLayout().Last() != relocationSection)
                    {
                        throw new ApplicationException("can not extend program relocation directory, because the information about" +
                            " the relocations is not the last in the program. This program do not repack program sections, and so can not obfuscate it(-;)");
                    }

                    relocationSection.SizeOfRawData = newRelocationDirectorySize;
                    relocationSection.VirtualSize = newRelocationDirectorySize;
                    programExtensionBuffer = new byte[newRelocationDirectorySize - relocationSection.SizeOfRawData];
                }
            }
            
            //UPDATE THE RELOCATION DIRECTORY
            var offsetInBufferOfRelocationDirectory = oldCode.CodeInMemoryLayout.
                    RelocationDirectoryInfo.OffsetInBuffer;
            if (programExtensionBuffer == null)
            {
                Array.Copy(newCode.CodeInMemoryLayout.RelocationDirectoryInfo.Buffer, 0,
                         peFile.Buff, (int)offsetInBufferOfRelocationDirectory,
                         newRelocationDirectorySize);
            }
            else
            {
                //copy the relocation buffer to program buffer first
                Array.Copy(newCode.CodeInMemoryLayout.RelocationDirectoryInfo.Buffer, 0,
                         peFile.Buff, (int)offsetInBufferOfRelocationDirectory,
                         newRelocationDirectorySize - programExtensionBuffer.Length);

                //copy the rest of relocation bytes to extended buffer
                Array.Copy(newCode.CodeInMemoryLayout.RelocationDirectoryInfo.Buffer, 
                          newRelocationDirectorySize - programExtensionBuffer.Length,
                          programExtensionBuffer, 0,
                          programExtensionBuffer.Length);
            }

            
        }

        private void RewriteCodeSection(ICode oldCode,ICode code, PeFile peFile)
        {
            ulong index = oldCode.CodeInMemoryLayout.OffsetOfCodeInBytes;
            ulong maxIndexToWrite = oldCode.CodeInMemoryLayout.CodePhysicalSizeInBytes +
                oldCode.CodeInMemoryLayout.OffsetOfCodeInBytes;

            foreach (var instruction in code.AssemblyInstructions)
            {
                //stop writing when exceeding the size of code section
                ulong indexAfterWritingInstruction = index + (ulong)instruction.Bytes.Length;
                //we might stop writing the instructions before all instructions are written
                if (indexAfterWritingInstruction > maxIndexToWrite) break;
               
                Array.Copy(instruction.Bytes, 0L, peFile.Buff, (long)index, instruction.Bytes.Length);
                index = indexAfterWritingInstruction;
            }
        }

        private void RewriteCodeAdressesInDataSection(ICode code, PeFile peFile)
        {
            if (code.CodeInMemoryLayout.RelocationDirectoryInfo == null) return;
            
            foreach (var relocationOffsetItem in code.CodeInMemoryLayout.RelocationDirectoryInfo.AddressesOfCodeInDataSection)
            {
                var section = (IMAGE_SECTION_HEADER)relocationOffsetItem.Tag;
                var bufferOffsetToUpdate = section.PointerToRawData + 
                    relocationOffsetItem.RelocationTypeOffset.Offset - section.VirtualAddress;
                uint newAddress = (uint)(peFile.ImageNtHeaders.OptionalHeader.ImageBase +
                           code.CodeInMemoryLayout.CodeVirtualAddress + 
                           relocationOffsetItem.Address);

                peFile.Buff.SetUInt32((uint)bufferOffsetToUpdate,
                    newAddress);
            }
        }

        private void RewriteCodeSize(ICode code, PeFile peFile,IStatistics statistics)
        {
            var codeSectionHeader = peFile.GetCodeSectionHeader();

            if ((codeSectionHeader.VirtualSize + statistics.AddedBytes) >codeSectionHeader.SizeOfRawData)
            {
                codeSectionHeader.VirtualSize = codeSectionHeader.SizeOfRawData;
            }
            else
            {
                codeSectionHeader.VirtualSize += (uint)statistics.AddedBytes;
            }
        }


                
    }
}
