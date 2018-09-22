using SharpDisasm;

namespace ObfuscationTransform.Core
{
    public interface IAssemblyInstructionForTransformation : IInstruction
    {
        /// <summary>
        /// Is new instruction
        /// </summary>
        bool IsNew { get; set; }

        /// <summary>
        /// Next instruction
        /// </summary>
        IAssemblyInstructionForTransformation NextInstruction { get; set; }

        /// <summary>
        /// previous instruction
        /// </summary>
        IAssemblyInstructionForTransformation PreviousInstruction { get; set; }

        /// <summary>
        /// set new instruction offset
        /// </summary>
        /// <param name="offset">new instruction offset</param>
        void SetOffset(ulong offset);

        /// <summary>
        /// set new program counter value
        /// </summary>
        /// <param name="programCounter">new program counter value</param>
        void SetPC(ulong programCounter);
    }
}