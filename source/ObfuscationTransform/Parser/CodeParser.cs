using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.Extensions;
using ObfuscationTransform.Transformation;
using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Parser
{
    public class CodeParser : ICodeParser
    {
        private IFunctionParser m_functionParser; 
        private ICodeFactory m_codeFactory;
        private IDisassemblerFactory m_disassemblerFactory;
        private readonly IInstructionWithAddressOperandDecider m_jumpTargetAddressDecider;


        public CodeParser(IFunctionParser functionParser,ICodeFactory codeFactory,
            IDisassemblerFactory disassemblerFactory,
            IInstructionWithAddressOperandDecider jumpTargetAddressDecider)
        {
            m_functionParser = functionParser ?? throw new ArgumentNullException(nameof(functionParser));
            m_codeFactory = codeFactory ?? throw new ArgumentNullException(nameof(codeFactory));
            m_disassemblerFactory = disassemblerFactory ?? throw new ArgumentNullException(nameof(disassemblerFactory));
            m_jumpTargetAddressDecider = jumpTargetAddressDecider ?? throw new ArgumentNullException(nameof(jumpTargetAddressDecider));
        }

        public ICode ParseCode(byte[] code, ICodeInMemoryLayout codeInMemoryLayout)
        {
            if (code == null || code.Length == 0) throw new ArgumentNullException(nameof(code),"code buffer can not be empty nor null");
            if (codeInMemoryLayout==null) throw new ArgumentNullException(nameof(codeInMemoryLayout));
            IReadOnlyList<IAssemblyInstructionForTransformation> listOfInstructions = ParseInstructions(code);
            IReadOnlyList<IFunction> listOfFunctions = parseFunctions(m_jumpTargetAddressDecider,listOfInstructions);

            return m_codeFactory.Create(listOfInstructions, listOfFunctions,codeInMemoryLayout);
        }

        public ICode ParseCode(IReadOnlyList<IAssemblyInstructionForTransformation> listOfInstructions,
            ICodeInMemoryLayout codeInMemoryLayout)
        {
            if (listOfInstructions == null) throw new ArgumentNullException(nameof(listOfInstructions));
            if (codeInMemoryLayout == null) throw new ArgumentNullException(nameof(codeInMemoryLayout));
            IReadOnlyList<IFunction> listOfFunctions = parseFunctions(m_jumpTargetAddressDecider, listOfInstructions);

            return m_codeFactory.Create(listOfInstructions, listOfFunctions, codeInMemoryLayout);
        }


        private List<IAssemblyInstructionForTransformation> ParseInstructions(byte[] code)
        {
            var disasm = m_disassemblerFactory.Create(code);
            if (disasm == null) throw new NullReferenceException(nameof(disasm));

            // Disassemble each instruction and output to console
            IAssemblyInstructionForTransformation lastInstruction = null;
            var listOfInstruction = new List<IAssemblyInstructionForTransformation>();

            foreach (IAssemblyInstructionForTransformation instruction in disasm.Disassemble())
            {
                if (instruction==null)
                {
                    StringBuilder errorMessage = new StringBuilder("can not disassemble instruction after ");
                    if (lastInstruction != null) errorMessage.Append(lastInstruction.Offset.ToString());
                    else errorMessage.Append("Begining of program");
                    throw new ApplicationException(errorMessage.ToString());
                }
                if (lastInstruction != null)
                {
                    lastInstruction.NextInstruction = instruction;
                    instruction.PreviousInstruction = lastInstruction;
                }
                listOfInstruction.Add(instruction);
                lastInstruction = instruction;
            }
                       
            return listOfInstruction;
        }

        
        private IReadOnlyList<IFunction> parseFunctions(
            IInstructionWithAddressOperandDecider instructionWithAddressOperandDecider,
            IReadOnlyList<IAssemblyInstructionForTransformation> listOfInstructions)
        {
            AddressesRange addressesRange = new AddressesRange(listOfInstructions[0].Offset, listOfInstructions.Last().Offset);
            var jumpTargetAddresses = GetJumpTargetAddresses(instructionWithAddressOperandDecider,
                                                             listOfInstructions);
            return m_functionParser.Parse(listOfInstructions[0], addressesRange,jumpTargetAddresses);
        }

        private Dictionary<ulong,ulong> GetJumpTargetAddresses(
            IInstructionWithAddressOperandDecider instructionWithAddressOperandDecider,
            IReadOnlyList<IAssemblyInstructionForTransformation> listOfInstructions)
        {
            var addressesInInstructionsDictionary = new Dictionary<ulong, ulong>();

            //create the map, and create entry for each jump target
            foreach (var instruction in listOfInstructions)
            {
                ulong instructionTarget = ulong.MaxValue;

                bool isJumpTargetOperand =
                    instructionWithAddressOperandDecider.IsJumpInstructionWithRelativeAddressOperand(instruction) ||
                    instructionWithAddressOperandDecider.IsUnconditionalJumpInstruction(instruction.Mnemonic) ||
                    instructionWithAddressOperandDecider.IsCallInstructionWithAddressOperand(instruction);

                if (!isJumpTargetOperand) continue;
                bool result = false;
                result = instruction.TryGetAbsoluteAddressFromRelativeAddress(out instructionTarget);
                if (!result)
                {
                    result = instruction.TryGetAbsoluteAddress(out instructionTarget);
                }
                if (!result) continue;

                addressesInInstructionsDictionary[instructionTarget] = 0;


            }

            return addressesInInstructionsDictionary;
        }
    }
}
