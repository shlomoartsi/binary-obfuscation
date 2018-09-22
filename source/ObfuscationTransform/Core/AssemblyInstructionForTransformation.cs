using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    /// <summary>
    /// Class that represent the assembly instruction with more info for transformation purposes
    /// </summary>
    public class AssemblyInstructionForTransformation : SharpDisasm.Instruction, IAssemblyInstructionForTransformation
    {
        /// <summary>
        /// ctor for real assembly instruction
        /// </summary>
        /// <param name="u"></param>
        /// <param name="keepBinary"></param>
        public AssemblyInstructionForTransformation( SharpDisasm.Udis86.ud u, bool keepBinary) :
            base( u, keepBinary)
        { }

        /// <summary>
        /// ctor for dummy inserted instruction
        /// </summary>
        public AssemblyInstructionForTransformation(byte[] bytecodes)
        {
            base.Bytes = bytecodes;
        }

        /// <summary>
        /// The next instruction in the code
        /// </summary>
        public IAssemblyInstructionForTransformation NextInstruction { get; set; }

        /// <summary>
        /// The previous instruction in the code
        /// </summary>
        public IAssemblyInstructionForTransformation PreviousInstruction { get; set; }

        /// <summary>
        /// Is this instruction was added in the transformation
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// set new instruction offset
        /// </summary>
        /// <param name="offset">new instruction offset</param>
        public void SetOffset(ulong offset)
        {
            Offset = offset;
            ClearToStringCache();
        }

        /// <summary>
        /// set new program counter value
        /// </summary>
        /// <param name="programCounter">new program counter value</param>
        public void SetPC(ulong programCounter)
        {
            PC = programCounter;
            ClearToStringCache();
        }

    }

}

