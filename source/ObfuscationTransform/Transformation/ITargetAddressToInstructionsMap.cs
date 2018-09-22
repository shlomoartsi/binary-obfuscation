using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    public interface ITargetAddressToInstructionsMap : IReadOnlyDictionary<ulong,IReadOnlyList<IInstruction>>
    {
        void AddInstructionToTargetAddress(IInstruction instruction, ulong targetAddress);
        bool RemoveInstructionToTargetAddress(IInstruction instruction, ulong targetAddress);
        bool ExistInstructionsThatJumpsToTargetAddress(ulong targetAddress);
        bool TryReplaceInstructionThatJumpsToTargetAddress(ulong targetAddress, IInstruction oldInstruction, IInstruction newInstruction);
    }
}
