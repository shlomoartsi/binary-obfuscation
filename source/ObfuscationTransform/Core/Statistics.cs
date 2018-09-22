using ObfuscationTransform.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core
{
    public class Statistics : IStatistics
    {
        public ulong MisinterpretedInstructions { get; private set; }
        public ulong AddedInstructions { get; private set; }
        public ulong AddedBytes { get; private set ; }
        public ulong ExpandedInstructions { get; private set; }
        public ulong AddedJunkBytes { get; private set; }
        public ulong AddedJunkInstructions { get; private set; }
        public ulong NumberOfInstructions { get; private set; }
        public ulong EffectiveInstructions { get; private set; }

        public event EventHandler<StatisiticsEventArgs> NewStatistics;

        public void SetNumberOfInstructions(ICode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            EffectiveInstructions = GetNumberOfEffectiveInstructions(code);
            NumberOfInstructions = (ulong)code.AssemblyInstructions.Count;
        }

        public void IncrementAddedInstructions(uint addedInstructions, uint addedBytes)
        {
            AddedInstructions += addedInstructions;
            AddedBytes += addedBytes;
            UpdateStatisitics();
        }

        

        public void IncrementInstructionExpanded(uint addedBytes)
        {
            ExpandedInstructions++;
            AddedBytes += addedBytes;
            UpdateStatisitics();
        }

        
        public void IncrementJunkInstructions(uint addedinstructions, uint addedJunkBytes)
        {
            AddedJunkBytes += addedJunkBytes;
            AddedInstructions++;
            UpdateStatisitics();

        }

        public void IncrementMissinterpretedInstructions(uint misinterpretedInstructions)
        {
            MisinterpretedInstructions += misinterpretedInstructions;
            UpdateStatisitics();
        }

        
        private void UpdateStatisitics()
        {
            NewStatistics?.Invoke(this, new StatisiticsEventArgs(AddedInstructions, MisinterpretedInstructions,
                ExpandedInstructions, AddedJunkBytes, AddedJunkInstructions,
                AddedBytes,NumberOfInstructions,EffectiveInstructions));
        }

        private static uint GetNumberOfEffectiveInstructions(ICode code)
        {
            uint effectiveInstructions = 0;
            foreach (var instruction in code.AssemblyInstructions)
            {
                if (!instruction.IsNopInstruction()) effectiveInstructions++;
            }
            return effectiveInstructions;
        }

    }
}
