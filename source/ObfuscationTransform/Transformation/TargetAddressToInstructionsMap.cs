using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDisasm;

namespace ObfuscationTransform.Transformation
{
    public class TargetAddressToInstructionsMap : ITargetAddressToInstructionsMap
    {
        private readonly Dictionary<ulong, List<IInstruction>> m_map;

        public TargetAddressToInstructionsMap()
        {
            m_map = new Dictionary<ulong, List<IInstruction>>();
        }

        public IReadOnlyList<IInstruction> this[ulong key]
        {
            get
            {
                return m_map[key];
            }
        }

        public int Count
        {
            get
            {
                return m_map.Count;
            }
        }

        public IEnumerable<ulong> Keys
        {
            get
            {
                return m_map.Keys;
            }
        }

        public IEnumerable<IReadOnlyList<IInstruction>> Values
        {
            get
            {
                return m_map.Values;
            }
        }

        public void AddInstructionToTargetAddress(IInstruction instruction, ulong targetAddress)
        {
            List<IInstruction> instructionList = null;
            if (m_map.ContainsKey(targetAddress))
            {
                instructionList = m_map[targetAddress];
            }
            else
            {
                instructionList = new List<IInstruction>();
                m_map[targetAddress] = instructionList;
            }

            instructionList.Add(instruction);
        }

        public bool ContainsKey(ulong key)
        {
            return m_map.ContainsKey(key);
        }

        public bool ExistInstructionsThatJumpsToTargetAddress(ulong targetAddress)
        {
            return m_map.ContainsKey(targetAddress);
        }

        public IEnumerator<KeyValuePair<ulong, IReadOnlyList<IInstruction>>> GetEnumerator()
        {
            var enumerator = m_map.AsEnumerable();
            IEnumerable<KeyValuePair<ulong, IReadOnlyList<IInstruction>>> newEnumerator = (IEnumerable<KeyValuePair<ulong, IReadOnlyList<IInstruction>>>)enumerator;
            return newEnumerator.GetEnumerator();
        }

        public bool RemoveInstructionToTargetAddress(IInstruction instruction, ulong targetAddress)
        {
            if (m_map.ContainsKey(targetAddress))
            {
                return m_map[targetAddress].Remove(instruction);
            }
            return false;
        }

        public bool TryGetValue(ulong key, out IReadOnlyList<IInstruction> value)
        {
            List<IInstruction> newValue;
            var result= m_map.TryGetValue(key, out newValue);
            if (result)
            {
                value = newValue;
            }
            else
            {
                value = null;
            }
            return result;
        }

        public bool TryReplaceInstructionThatJumpsToTargetAddress(ulong targetAddress, IInstruction oldInstruction,IInstruction newInstruction)
        {
            List<IInstruction> instructionsList;
            var result = m_map.TryGetValue(targetAddress, out instructionsList);
            if (result && instructionsList!=null)
            {
                result = instructionsList.Remove(oldInstruction);
                if (result)
                {
                    instructionsList.Add(newInstruction);
                }
            }
           
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_map).GetEnumerator();
        }
    }
}
