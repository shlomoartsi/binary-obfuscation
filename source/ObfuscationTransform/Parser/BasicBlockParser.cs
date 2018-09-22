using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Parser
{
    public class BasicBlockParser : IBasicBlockParser
    {

        public IBasicBlockEpilogParser BasicBlockEpilogParser { get; set; }
        public IBasicBlockFactory BasicBlockFactory { get; set; }

        public BasicBlockParser(IBasicBlockEpilogParser basicBlockEpilogParser,
            IBasicBlockFactory basicBlockFactory)
        {
            BasicBlockEpilogParser = basicBlockEpilogParser ?? throw new ArgumentNullException("basicBlockEpilogParser");
            BasicBlockFactory = basicBlockFactory ?? throw new ArgumentNullException("basicBlockFactory");
        }


        public IReadOnlyList<IBasicBlock> Parse(IAssemblyInstructionForTransformation firstInstruction,
            IAssemblyInstructionForTransformation lastInstruction, Dictionary<ulong, ulong> jumpTargetAddresses)
        {
            if (firstInstruction == null) throw new ArgumentNullException(nameof(firstInstruction));
            if (lastInstruction == null) throw new ArgumentNullException(nameof(lastInstruction));
            if (jumpTargetAddresses == null) throw new ArgumentNullException(nameof(jumpTargetAddresses));
            if (firstInstruction.Offset >= lastInstruction.Offset) throw new ArgumentException("first argument address should be less then last argument address", "firstInstruction");

            List<IBasicBlock> basicBlocks = new List<IBasicBlock>();
            var currrentInstruction = firstInstruction;

            while (currrentInstruction.Offset <= lastInstruction.Offset)
            {
                //find the basic block epilog
                var basicBlockEpilog = BasicBlockEpilogParser.Parse(currrentInstruction, 
                    lastInstruction.Offset,jumpTargetAddresses);

                //no epilog was found, for example because the code examined is not a function or ends when the whole code section ends...
                if (basicBlockEpilog == null) break;

                //initialize the assembly instructions of the current basic block being parsed.
                var assemblyInstructions = new List<IAssemblyInstructionForTransformation>() { currrentInstruction };

                //Add all instruction in between the current and epilog instruction of the basic block
                if (currrentInstruction != basicBlockEpilog)
                {
                    currrentInstruction = currrentInstruction.NextInstruction;
                    while (currrentInstruction != null &&  //may not be possible to happen
                           currrentInstruction != basicBlockEpilog)
                    {
                        assemblyInstructions.Add(currrentInstruction);
                        currrentInstruction = currrentInstruction.NextInstruction;
                    }

                    //add epilog instruction
                    assemblyInstructions.Add(basicBlockEpilog);
                }

                IBasicBlock basicBlock = BasicBlockFactory.Create(assemblyInstructions);
                basicBlocks.Add(basicBlock);
                currrentInstruction = currrentInstruction.NextInstruction;

            }
            return basicBlocks;
        }
    }
}
