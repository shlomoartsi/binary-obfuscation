using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDisasm;
using ObfuscationTransform.Core.Factory;

namespace ObfuscationTransform.Core
{
    public class Disassembler : IDisassembler
    {
        private byte[] m_code;
        private SharpDisasm.Disassembler m_disassembler;

        public Disassembler(byte[] code)
        {
            m_code = code ?? throw new ArgumentNullException(nameof(code));
            m_disassembler = createDisassembler(code);
        }

        private SharpDisasm.Disassembler createDisassembler(byte[] code)
        {
            SharpDisasm.ArchitectureMode mode = SharpDisasm.ArchitectureMode.x86_32;
            // Configure the translator to output instruction addresses and instruction binary as hex
            SharpDisasm.Disassembler.Translator.IncludeAddress = true;
            SharpDisasm.Disassembler.Translator.IncludeBinary = true;
            // Create the disassembler
            return new SharpDisasm.Disassembler(
                code,
                mode, 0, true, instructionFactory: new AssemblyInstructionForTransformationFactory());
        }

        public IEnumerable<IAssemblyInstructionForTransformation> Disassemble()
        {
            return m_disassembler.Disassemble().Cast<IAssemblyInstructionForTransformation>();
        }
    }
}
