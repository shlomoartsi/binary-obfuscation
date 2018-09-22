using System;

namespace ObfuscationTransform.Core
{
    public class StatisiticsEventArgs : EventArgs
    {
        public ulong AddedInstructions { get; }
        public ulong MisinterpretedInstructions { get; }
        public ulong ExpandedInstructions { get; }
        public ulong AddedJunkBytes { get; }
        public ulong AddedJunkInstructions { get; }
        public ulong AddedBytes { get; }
        public ulong NumberOfInstructions { get; }
        public ulong EffectiveInstructions { get; }

        public StatisiticsEventArgs(ulong addedInstructions,ulong misintrepretedInstructions,
            ulong expandedInstructions,ulong addedJunkBytes,
            ulong addedJunkInstructions,ulong addedBytes,
            ulong numberOfInstructions,ulong effectiveInstructions)
        {
            AddedInstructions = addedInstructions;
            MisinterpretedInstructions = misintrepretedInstructions;
            ExpandedInstructions = expandedInstructions;
            AddedJunkBytes = addedJunkBytes;
            AddedJunkInstructions = addedJunkInstructions;
            AddedBytes = addedBytes;
            NumberOfInstructions = numberOfInstructions;
            EffectiveInstructions = effectiveInstructions;
        }
    }
}