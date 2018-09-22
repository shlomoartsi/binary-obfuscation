using System;
using System.Collections.Generic;

namespace ObfuscationTransform.Core
{
    public class BasicBlock : IBasicBlock
    {
        public AddressesRange AddressesRange { get;private set; }
        public uint NumberOfBytes { get;private set; }
        public IBasicBlock NewTansformedBasicBlock { get; set; }
        public IReadOnlyList<IAssemblyInstructionForTransformation> AssemblyInstructions { get; private set; }
        private byte[] m_assemblyBytes;

        public BasicBlock(IReadOnlyList<IAssemblyInstructionForTransformation> assemblyInstructions)
        {
            if (assemblyInstructions == null) throw new ArgumentNullException("assemblyInstructions");
            if (assemblyInstructions.Count < 1) throw new ArgumentException("assemblyInstructions must contain at least one instruction");

            AssemblyInstructions = assemblyInstructions;
            NumberOfBytes = CountBytes(assemblyInstructions);

        }


        private uint CountBytes(IReadOnlyList<IAssemblyInstructionForTransformation> assemblyInstructions)
        {
            uint numberOfBytes = 0;
            foreach (var instruction in assemblyInstructions)
            {
                numberOfBytes += (uint)instruction.Bytes.Length;
            }

            return numberOfBytes;
        }

        public byte[] GetAllBytes()
        {
            if (AssemblyInstructions == null) return new byte[0];

            if (m_assemblyBytes == null)
            {
                m_assemblyBytes = new byte[NumberOfBytes];
                int indexInAssemblyBytes = 0;
                foreach (var assemblyInstruction in AssemblyInstructions)
                {
                    assemblyInstruction.Bytes.CopyTo(m_assemblyBytes, indexInAssemblyBytes);
                    indexInAssemblyBytes += assemblyInstruction.Bytes.Length;
                }
            }

            return m_assemblyBytes;
        }

        
    }
}