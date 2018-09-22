using ObfuscationTransform.Core;
using ObfuscationTransform.Extensions;
using ObfuscationTransform.Parser;
using PeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    /// <summary>
    /// Create a new portable executable file using the previous PE
    /// the old code and the new code
    /// </summary>
    public class PeTransform : IPeTransform
    {
        private readonly ICodeObfuscator m_codeObfuscator;
        private readonly ICodeParser m_codeParser;
        private IStatistics m_statistics;

        public ICode Code { get; private set; }

        public ICode TransformedCode { get; private set; }

        public PeFile PeFile { get; private set; }

        public PeTransform(ICodeObfuscator codeObfuscator,ICodeParser codeParser,IStatistics statistics)
        {
            m_codeObfuscator = codeObfuscator ?? throw new ArgumentNullException(nameof(codeObfuscator));
            m_codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
            m_statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        }

        public void Transform(string filePath)
        {
            var peFile = new PeNet.PeFile(filePath);

            Code = ParseCode(peFile);

            UpdateStatistics(Code,m_statistics);

            PrintOriginalAssembly(peFile, filePath);

            ObfuscateAssembly(filePath,peFile,Code);

            PrintNewAssembly(filePath);

            PrintNewAssemblyMisinterpreted(filePath, peFile);
        }

        

        private void ObfuscateAssembly(string filePath,PeFile peFile,ICode code)
        {
            var fileObfuscated = Path.GetFileNameWithoutExtension(filePath) +
                "Obf" + Path.GetExtension(filePath);
            m_codeObfuscator.Obfuscate(fileObfuscated,peFile,code);
        }

        private ICode ParseCode(PeFile peFile)
        {
            return  m_codeParser.ParseCode(peFile.GetBytesOfCode(), 
                peFile.GetCodeInMemoryLayout());
        }

        private void UpdateStatistics(ICode code, IStatistics m_statistics)
        {
            m_statistics.SetNumberOfInstructions(code);
        }

        private void PrintOriginalAssembly(PeFile peFile, string filePath)
        {
            var fileAssemblyOutput = Path.GetFileNameWithoutExtension(filePath) +
                "Orig.txt";
            OutputAssembly(peFile.GetBytesOfCode(), fileAssemblyOutput);
        }


        private void OutputAssembly(ICode code,string fileName)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
            {
                foreach (var assemblyInstruction in code.AssemblyInstructions)
                {
                    file.WriteLine(assemblyInstruction.ToString(true, true));
                }
            }
        }

        private static void OutputAssembly(byte[] codeBuffer,string fileName)
        {
            SharpDisasm.ArchitectureMode mode = SharpDisasm.ArchitectureMode.x86_32;

            // Configure the translator to output instruction addresses and instruction binary as hex
            SharpDisasm.Disassembler.Translator.IncludeAddress = true;
            SharpDisasm.Disassembler.Translator.IncludeBinary = true;
            // Create the disassembler
            var disasm = new SharpDisasm.Disassembler(
                codeBuffer,
                mode, 0, true);
            int instructions = 0;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
            {
                List<string> jumpInstructions = new List<string>();
                // Disassemble each instruction and output to console
                foreach (var insn in disasm.Disassemble())
                {
                    file.WriteLine(insn.ToString());
                    instructions++;
                }
            }
        }

        private static void PrintNewAssemblyMisinterpreted(string filePath, PeFile peFile)
        {
            var fileMisinterpreted = Path.GetFileNameWithoutExtension(filePath) + "MisInterpreted.txt";
            OutputAssembly(peFile.GetBytesOfCode(), fileMisinterpreted);
        }

        private void PrintNewAssembly(string filePath)
        {
            var fileObfuscatedOutput = Path.GetFileNameWithoutExtension(filePath) + "Obf.txt";
            OutputAssembly(m_codeObfuscator.NewCode, fileObfuscatedOutput);
        }

    }
}