using System;

namespace ObfuscationTransform.Core
{
    public interface IStatistics
    {
        event EventHandler<StatisiticsEventArgs> NewStatistics;
        ulong AddedInstructions { get; }
        ulong MisinterpretedInstructions { get; }
        ulong ExpandedInstructions { get; }
        ulong AddedJunkBytes { get; }
        ulong AddedJunkInstructions { get; }
        ulong AddedBytes { get; }
        ulong NumberOfInstructions { get; }
        ulong EffectiveInstructions { get; }

        void SetNumberOfInstructions(ICode code);
        void IncrementAddedInstructions(uint addedInstruction,uint addedBytes);
        void IncrementMissinterpretedInstructions(uint misinterpretedInstruction);
        void IncrementInstructionExpanded(uint addedBytes);
        void IncrementJunkInstructions(uint addedinstructions,uint addedJunkBytes);
    }
}