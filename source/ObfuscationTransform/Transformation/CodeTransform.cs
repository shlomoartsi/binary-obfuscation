using System;
using System.Collections.Generic;
using System.Linq;
using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;

namespace ObfuscationTransform.Transformation
{
    public class CodeTransform : ICodeTransform
    {
        private ICodeFactory m_codeFactory;
        private IFunctionFactory m_functionFactory;
        private IBasicBlockFactory m_basicBlockFactory;

        public CodeTransform(ICodeFactory codeFactory,
            IFunctionFactory functionFactory,
            IBasicBlockFactory basicBlockFactory)
        {
            m_codeFactory = codeFactory ?? throw new ArgumentNullException(nameof(codeFactory));
            m_functionFactory = functionFactory ?? throw new ArgumentNullException(nameof(functionFactory));
            m_basicBlockFactory = basicBlockFactory ?? throw new ArgumentNullException(nameof(basicBlockFactory));
        }

        public ICode Transform(ICode code, TryTransformInstructionDelegate transformInstructionDelegate,
            Func<ICodeInMemoryLayout> codeInLayoutFactoryDelegate = null) 
        {
            var newInstructionsList = new List<IAssemblyInstructionForTransformation>();
            var newFunctionsList = new List<IFunction>();
            var instructionListIterator = code.AssemblyInstructions.GetEnumerator();
            var lastInstructionInLastBlock = code.Functions.Last().BasicBlocks.Last().AssemblyInstructions.Last();
            IAssemblyInstructionForTransformation previousInstruction = null;
            bool afterInstructionOfLastBlock = false;

            //move to the first instruction
            instructionListIterator.MoveNext(); 

            //iterate through all functions
            foreach (var function in code.Functions)
            {
                var basicBlockList = new List<IBasicBlock>();
                //iterate through all basic block
                foreach (var basicBlock in function.BasicBlocks)
                {
                    var instructionsListOfBlock = new List<IAssemblyInstructionForTransformation>();
                    foreach (var instructionInBasicBlock in basicBlock.AssemblyInstructions)
                    {
                        bool isInstructionInBasicBlock;
                        bool done = false;

                        //transform instructions from instruction list.
                        //this loop transform:
                        //1. instructions before the basic block which are left to process 
                        //2. instructions inside a basic block
                        //3. instructions after the last basic block
                        while (!done)
                        {
                            isInstructionInBasicBlock = instructionListIterator.Current == instructionInBasicBlock;
                            IAssemblyInstructionForTransformation instructionToTransform =
                                                                instructionListIterator.Current;

                            //perfom the transformation of the instruction
                            List<IAssemblyInstructionForTransformation> transformedInstructionList;
                            var wasTransformed = transformInstructionDelegate(instructionToTransform,
                                                        isInstructionInBasicBlock ? basicBlock : null,
                                                        isInstructionInBasicBlock ? function : null,
                                                        out transformedInstructionList);

                            if (wasTransformed)
                            {
                                if (transformedInstructionList.Count == 0) throw new ApplicationException("transformation should return at least one instruction");

                                if (isInstructionInBasicBlock) instructionsListOfBlock.AddRange(transformedInstructionList);
                                newInstructionsList.AddRange(transformedInstructionList);

                                if (previousInstruction != null)
                                {
                                    transformedInstructionList[0].PreviousInstruction = previousInstruction;
                                    previousInstruction.NextInstruction = transformedInstructionList[0];
                                }

                                if (transformedInstructionList.Count > 1)
                                {
                                    for (int i = 1; i < transformedInstructionList.Count; i++)
                                    {
                                        transformedInstructionList[i].PreviousInstruction = transformedInstructionList[i - 1];
                                        transformedInstructionList[i - 1].NextInstruction = transformedInstructionList[i];
                                    }
                                }

                                previousInstruction = transformedInstructionList.Last();
                            }
                            else
                            {
                                if (isInstructionInBasicBlock) instructionsListOfBlock.Add(instructionToTransform);
                                newInstructionsList.Add(instructionToTransform);
                                if (previousInstruction != null)
                                {
                                    instructionToTransform.PreviousInstruction = previousInstruction;
                                    previousInstruction.NextInstruction = instructionToTransform;
                                }
                                previousInstruction = instructionToTransform;
                            }

                            //check weather this is the last instruction in the last basic block
                            if (isInstructionInBasicBlock && !afterInstructionOfLastBlock)
                            {
                                //The transformed instruction is now in the end of program
                                //after the last basic block instruction
                                afterInstructionOfLastBlock = (instructionToTransform == lastInstructionInLastBlock);
                            }

                            instructionListIterator.MoveNext();

                            //stop transforming intructions in loop when all instruction in scope are processed
                            done = (isInstructionInBasicBlock ||
                                   instructionListIterator.Current == null);
                            
                            //keep transforming after the last basic block instruction to the end of the program
                            if (afterInstructionOfLastBlock && instructionListIterator.Current != null) done = false;


                        }
                    }

                    IBasicBlock newBasicBlock = m_basicBlockFactory.Create(instructionsListOfBlock);
                    basicBlockList.Add(newBasicBlock);
                }

                var newFunction = m_functionFactory.Create(basicBlockList.First().AssemblyInstructions.First(),
                    basicBlockList.Last().AssemblyInstructions.Last(),
                    basicBlockList);
                newFunctionsList.Add(newFunction);
            }

            
            //if there is a factory to create a new code in memory layout structure than use it, otherwise use
            //the original code layout in memrory instance
            ICodeInMemoryLayout codeInMemoryLayout = codeInLayoutFactoryDelegate == null ? code.CodeInMemoryLayout:codeInLayoutFactoryDelegate() ;
            
            //return m_codeFactory.Create(newInstructionsList, newFunctionsList,codeInMemoryLayout);
            var newcode = m_codeFactory.Create(newInstructionsList, newFunctionsList,codeInMemoryLayout);
            ValidateNewCode(newcode);
            return newcode;
        }

        private void ValidateNewCode(ICode code)
        {
            IAssemblyInstructionForTransformation lastInstruction = null;
            var instructionEnumerator = code.AssemblyInstructions.GetEnumerator();
            while(instructionEnumerator.MoveNext())
            {
                if (lastInstruction!=null && !instructionEnumerator.Current.IsNew &&
                    lastInstruction.Offset>=instructionEnumerator.Current.Offset)
                {
                    var instructionIndex = ((List < IAssemblyInstructionForTransformation>)code.AssemblyInstructions).IndexOf(instructionEnumerator.Current);
                    throw new ApplicationException($"Code structure is incorrect. instruction:{instructionEnumerator.Current.ToString()}" +
                        $"at index {instructionIndex} has higher or equal address than last instruction {lastInstruction.ToString()}");
                }
                if (!instructionEnumerator.Current.IsNew) lastInstruction = instructionEnumerator.Current;
            }
        }
    }
}
