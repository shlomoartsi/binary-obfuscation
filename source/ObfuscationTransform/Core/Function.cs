using System.Collections.Generic;
using System;

namespace ObfuscationTransform.Core
{
    public class Function : IFunction
    {
        public IAssemblyInstructionForTransformation EndInstruction { get; private set; }
        public IAssemblyInstructionForTransformation StartInstruction { get; private set; }
        /// <summary>
        /// The start and end instruction addresses
        /// </summary>
        public AddressesRange AddressesRange { get; private set; }
        public IReadOnlyList<IBasicBlock> BasicBlocks { get; private set; }


        public Function(IAssemblyInstructionForTransformation startInstruction,
                        IAssemblyInstructionForTransformation endInstruction,
                        IReadOnlyList<IBasicBlock> basicBlocks)
        {
            StartInstruction = startInstruction ?? throw new ArgumentNullException("startInstrucion");
            EndInstruction = endInstruction ?? throw new ArgumentNullException("endInstruction");
            AddressesRange = new AddressesRange(startInstruction.Offset,endInstruction.Offset);
            BasicBlocks = basicBlocks ?? throw new ArgumentNullException("basicBlocks");

        }
    }
}