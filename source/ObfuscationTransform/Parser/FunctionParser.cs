using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Parser
{
    public class FunctionParser : IFunctionParser
    {
        public IFunctionPrologParser PrologParser { get; private set; }
        public IFunctionEpilogParser EpilogParser { get; private set; }
        public IBasicBlockParser BasicBlockParser { get; private set; }
        public IFunctionFactory FunctionFactory { get; private set; }

        public FunctionParser(IFunctionPrologParser prologParser, IFunctionEpilogParser epilogParser,
                              IBasicBlockParser basicBlockParser,IFunctionFactory functionFactory)
        {
            PrologParser = prologParser ?? throw new NullReferenceException(nameof(prologParser));
            EpilogParser = epilogParser ?? throw new NullReferenceException(nameof(epilogParser));
            BasicBlockParser = basicBlockParser ?? throw new NullReferenceException(nameof(basicBlockParser));
            FunctionFactory = functionFactory ?? throw new NullReferenceException(nameof(functionFactory));
        }


        public IReadOnlyList<IFunction> Parse(IAssemblyInstructionForTransformation firstAssemblyInstruction, 
            AddressesRange addressesRangePermittedForParsing, Dictionary<ulong, ulong> jumpTargetAddresses)
        {
            if (firstAssemblyInstruction == null) throw new ArgumentNullException("firstAssemblyInstruction");

            List<IFunction> functionsList = new List<IFunction>();
            var instruction = firstAssemblyInstruction;

            while (instruction != null && instruction.Offset <= addressesRangePermittedForParsing.EndAddress)
            {
                var prologInstruction = PrologParser.Parse(instruction, addressesRangePermittedForParsing.EndAddress);
                if (prologInstruction == null) break;

                var epilogInstruction = EpilogParser.Parse(prologInstruction, addressesRangePermittedForParsing.EndAddress);
                if (epilogInstruction == null) break;

                IReadOnlyList<IBasicBlock> basicBlocks = BasicBlockParser.Parse(prologInstruction,
                    epilogInstruction, jumpTargetAddresses);
                functionsList.Add(FunctionFactory.Create(prologInstruction, epilogInstruction, basicBlocks));

                //set instruction to the instruction after the epilog instruction
                instruction = epilogInstruction.NextInstruction;
            }

            return functionsList;
        }
    }
}
