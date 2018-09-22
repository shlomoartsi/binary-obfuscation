using System;
using PeNet;
using System.IO;
using System.Text;
using PeNet.Structures;
using SharpDisasm;

namespace Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var file = @"C:\programming\WindowsInternals\By Sasha\PrimeNumberCalculation\Debug\PrimeNumberCalculation.exe";
            var peFile = new PeFile(file);
            var imageBase = peFile.ImageNtHeaders.OptionalHeader.ImageBase;
            var baseOfCode = peFile.ImageNtHeaders.OptionalHeader.BaseOfCode;

            Console.WriteLine(peFile.MetaDataStreamTablesHeader);
            if (peFile.MetaDataStreamTablesHeader != null)
            {
                foreach (var tableDefinition in peFile.MetaDataStreamTablesHeader.TableDefinitions)
                {
                    Console.WriteLine(tableDefinition);
                }

                Console.WriteLine(peFile.MetaDataStreamTablesHeader.MetaDataTables.ModuleTable);
            }
            else Console.WriteLine("No export functions");

            foreach (var sectionHeader in peFile.ImageSectionHeaders)
            {
                StringBuilder sb = new StringBuilder();
             
                sb.Append("\n\nName: ");
                sb.AppendLine(System.Text.Encoding.Default.GetString(sectionHeader.Name));
                sb.Append("Section type: ");
                if ((sectionHeader.Characteristics & (uint)PeNet.Constants.SectionFlags.IMAGE_SCN_CNT_CODE) != 0)
                {
                    sb.Append("Code.");
                }
                else
                {
                    sb.Append("Other.");
                }


                sb.AppendFormat("\nNumber of relocations: {0}", sectionHeader.NumberOfRelocations);
                sb.AppendFormat("\nPhysicalAddress: 0x{0:X8}", sectionHeader.PhysicalAddress);
                sb.AppendFormat("\nPointer To Line Number 0x{0:X8}", sectionHeader.PointerToLinenumbers);
                sb.AppendFormat("\nPointer To Raw Data: 0x{0:X8}", sectionHeader.PointerToRawData);
                sb.AppendFormat("\nPointer To Relocation: 0x{0:X8}", sectionHeader.PointerToRelocations);
                sb.AppendFormat("\nSize Of Raw Data: {0}", sectionHeader.SizeOfRawData);
                sb.AppendFormat("\nVirtual Address: 0x{0:X8}", sectionHeader.VirtualAddress);
                sb.AppendFormat("\nVirtual Size: {0}", sectionHeader.VirtualSize);
                
                Console.WriteLine(sb.ToString());
                if ((sectionHeader.Characteristics & (uint)PeNet.Constants.SectionFlags.IMAGE_SCN_CNT_CODE) != 0 && 
                    sectionHeader.SizeOfRawData>0)
                {
                    byte[] codeBuffer = CopyBytesOfCode(sectionHeader.PointerToRawData, sectionHeader.SizeOfRawData, peFile.Buff);
                    OutputAssembly(codeBuffer,peFile);
                }
            }


            Console.WriteLine("Finished!!!");
            Console.ReadKey();
        }

       

        private static byte[] CopyBytesOfCode(long bufferPosition, long length, byte[] buff)
        {
            byte[] codeBuffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                codeBuffer[i] = buff[bufferPosition + i];
            }
            return codeBuffer;
        }


        static int fileCounter = 0;

        private static void OutputAssembly(byte[] codeBuffer,PeFile peFile)
        {
            fileCounter++;
            SharpDisasm.ArchitectureMode mode = SharpDisasm.ArchitectureMode.x86_32;

            // Configure the translator to output instruction addresses and instruction binary as hex
            SharpDisasm.Disassembler.Translator.IncludeAddress = true;
            SharpDisasm.Disassembler.Translator.IncludeBinary = true;
            // Create the disassembler
            var disasm = new SharpDisasm.Disassembler(
                codeBuffer,
                mode, 0, true);


            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter("AssemblerOutput" + fileCounter.ToString() + ".txt"))
            {
                SharpDisasm.Instruction instruction = null;
                var instructionEnum =  disasm.Disassemble().GetEnumerator();

                foreach (var relocationDirectory in peFile.ImageRelocationDirectory)
                {
                    foreach (var relocationOffset in relocationDirectory.TypeOffsets)
                    {
                        // Disassemble each instruction and output to console
                        while (instructionEnum.MoveNext() && 
                               (instruction = instructionEnum.Current) !=null &&
                               !IsOffsetInInstruction(instruction, relocationOffset))
                        {
                            file.WriteLine(instruction.ToString());
                        }
                        if (instruction != null)
                        {
                            file.WriteLine(instruction.ToString() + " Relocation Address: " + relocationOffset.Offset.ToString("X"));
                        }
                           
                    }
                }
                
                
                
            }
        }

        private static bool IsOffsetInInstruction(Instruction instruction, 
            IMAGE_BASE_RELOCATION.TypeOffset relocationOffset)
        {
            return ((instruction.Offset % 0x1000) <= relocationOffset.Offset &&
                    (instruction.PC % 0x1000) > relocationOffset.Offset);
        }
    }
}