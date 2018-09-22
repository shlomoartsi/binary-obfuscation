using ObfuscationTransform.Container;
using ObfuscationTransform.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDisasm;
using SharpDisasm.Factory;
using SharpDisasm.Udis86;
using PeNet;
using ObfuscationTransform.Transformation;
using PeNet.Structures;
using ObfuscationTransform.PeExtensions;

namespace ObfuscationTransform.Core.Factory
{
    public class Factory<T> : IFactory<T>
    {
        public T Create()
        {
            return Container.Container.Resolve<T>();
        }

        protected T Create(params Parameter[] parameters)
        {
            return Container.Container.Resolve<T>(parameters);
        }
    }

    public class AssemblyInstructionForTransformationFactory :
        Factory<IAssemblyInstructionForTransformation>,
        IAssemblyInstructionForTransformationFactory
    {
        public IAssemblyInstructionForTransformation Create(ref SharpDisasm.Udis86.ud u, bool keepBinary)
        {
            return Create(
                new Parameter(nameof(u), u),
                new Parameter(nameof(keepBinary), keepBinary));
        }

        IInstruction IInstructionFactory.Create(ref ud u, bool keepBinary)
        {
            return Create(ref u, keepBinary);
        }
    }

    public class BasicBlockFactory : Factory<IBasicBlock>, IBasicBlockFactory
    {
        public IBasicBlock Create(IReadOnlyList<IAssemblyInstructionForTransformation> assemblyInstructions)
        {
            return Create(new Parameter(nameof(assemblyInstructions), assemblyInstructions));
        }
    }

    public class CodeFactory : Factory<ICode>, ICodeFactory
    {

        public ICode Create(IReadOnlyList<IAssemblyInstructionForTransformation> assemblyInstructions,
            IReadOnlyList<IFunction> functions,
            ICodeInMemoryLayout codeInMemoryLayout)
        {
            return Container.Container.Resolve<ICode>(
                new Parameter(nameof(assemblyInstructions), assemblyInstructions),
                new Parameter(nameof(functions), functions),
                new Parameter(nameof(codeInMemoryLayout), codeInMemoryLayout));
        }

        public ICode Create(IAssemblyInstructionForTransformation firstInstruction,
            IReadOnlyList<IFunction> functions,
            ICodeInMemoryLayout codeInMemoryLayout)
        {
            if (firstInstruction == null) throw new ArgumentNullException(nameof(firstInstruction));
            var instruction = firstInstruction;
            var instructionsList = new List<IAssemblyInstructionForTransformation>();

            return Create(instructionsList, functions, codeInMemoryLayout);
        }
    }

    public class CodeInMemoryLayoutFactory : ICodeInMemoryLayoutFactory
    {


        public ICodeInMemoryLayout Create(ulong imageBaseAddress,
            ulong offsetOfCodeInBytes, ulong codeActualSizeInBytes,
            ulong codePhysicalSizeInBytes, ulong entryPointOffset,
            ulong codeVirtualAddress, IRelocationDirectoryInfo relocationDirectoryInfo)
        {
            return Container.Container.Resolve<ICodeInMemoryLayout>(
                new Parameter(nameof(imageBaseAddress),imageBaseAddress),
                new Parameter(nameof(relocationDirectoryInfo), relocationDirectoryInfo),
                new Parameter(nameof(offsetOfCodeInBytes), offsetOfCodeInBytes),
                new Parameter(nameof(codeActualSizeInBytes), codeActualSizeInBytes),
                new Parameter(nameof(codePhysicalSizeInBytes), codePhysicalSizeInBytes),
                new Parameter(nameof(entryPointOffset), entryPointOffset),
                new Parameter(nameof(codeVirtualAddress), codeVirtualAddress));
        }
    }

    public class FunctionFactory : IFunctionFactory
    {
        public IFunction Create(IAssemblyInstructionForTransformation startInstruction,
            IAssemblyInstructionForTransformation endInstruction,
            IReadOnlyList<IBasicBlock> basicBlocks)
        {
            return Container.Container.Resolve<IFunction>(new Parameter(nameof(startInstruction), startInstruction),
                new Parameter(nameof(endInstruction), endInstruction),
                new Parameter(nameof(basicBlocks), basicBlocks));
        }
    }

    public class InstructionWithAddressOperandDeciderFactory : Factory<IInstructionWithAddressOperandDecider>, IInstructionWithAddressOperandDeciderFactory { };

    public class CodeObfuscatorFactory : ICodeObfuscatorFactory
    {
        public ICodeObfuscator Create(ICode code, PeFile peFile)
        {
            return Container.Container.Resolve<ICodeObfuscator>(new Parameter(nameof(code), code),
                                                                new Parameter(nameof(peFile), peFile));
        }
    };

    public class DisassemblerFactory : IDisassemblerFactory
    {
        public IDisassembler Create(byte[] code)
        {
            return Container.Container.Resolve<IDisassembler>(new Parameter(nameof(code), code));
        }

        public IDisassembler Create(byte[] code, bool copyBinaryToInstruction)
        {
            return Container.Container.Resolve<IDisassembler>(new Parameter(nameof(code), code),
                new Parameter("architecture", ArchitectureMode.x86_32), new Parameter("address", 0x0),
                new Parameter(nameof(copyBinaryToInstruction), false));
        }
    }


    public class RelocationDirctoryInfoFactory : IRelocationDirectoryInfoFactory
    {
        public IRelocationDirectoryInfo Create(IMAGE_BASE_RELOCATION[] imageRelolcationDirectory,
            uint relocationDirectorySize,
            byte[] buffer,
            ulong offsetInBuffer,
            List<RelocationTypeOffsetItem> addressesOfCodeInDataSection)
        {
            return Container.Container.Resolve<IRelocationDirectoryInfo>(
                new Parameter(nameof(imageRelolcationDirectory), imageRelolcationDirectory),
                new Parameter(nameof(buffer), buffer),
                new Parameter(nameof(relocationDirectorySize), relocationDirectorySize),
                new Parameter(nameof(offsetInBuffer), offsetInBuffer),
                new Parameter(nameof(addressesOfCodeInDataSection), addressesOfCodeInDataSection));
        }
    }
}
