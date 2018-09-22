using ObfuscationTransform.PeExtensions;
using PeNet;
using PeNet.Structures;
using SharpDisasm.Udis86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Core.Factory
{
    public interface IFactory<T> 
    {
        T Create();
    }

    public interface IAssemblyInstructionForTransformationFactory : IFactory<IAssemblyInstructionForTransformation>,
        SharpDisasm.Factory.IInstructionFactory { }

    public interface IBasicBlockFactory
    {
        IBasicBlock Create(IReadOnlyList<IAssemblyInstructionForTransformation> assemblyInstructions);
    }

    public interface ICodeFactory 
    {
        ICode Create(IReadOnlyList<IAssemblyInstructionForTransformation> assemblyInstructions,
                    IReadOnlyList<IFunction> functions,
                    ICodeInMemoryLayout codeInMemoryLayout);

        ICode Create(IAssemblyInstructionForTransformation firstInstruction, 
            IReadOnlyList<IFunction> functions,
            ICodeInMemoryLayout codeInMemoryLayout);
    }

    public interface ICodeInMemoryLayoutFactory
    {
        ICodeInMemoryLayout Create(ulong imageBaseAddress,ulong offsetOfCodeInBytes,
            ulong codeActualSizeInBytes,ulong codePhysicalSizeInBytes,
            ulong entryPointOffset, ulong codeVirtualAddress, 
            IRelocationDirectoryInfo relocationDirectoryInfo);
    }

    public interface IFunctionFactory
    {
        IFunction Create(IAssemblyInstructionForTransformation startInstruction,
                        IAssemblyInstructionForTransformation endInstruction,
                        IReadOnlyList<IBasicBlock> basicBlocks);
    }

    public interface IInstructionWithAddressOperandDeciderFactory : IFactory<IInstructionWithAddressOperandDecider> { };

    public interface ICodeObfuscatorFactory 
    {
        ICodeObfuscator Create(ICode code,PeFile peFile);
    };


    public interface IDisassemblerFactory
    {
        IDisassembler Create(byte[] code);
        IDisassembler Create(byte[] code,bool copyBinaryToInstruction);
    }

    public interface IRelocationDirectoryInfoFactory
    {
        IRelocationDirectoryInfo Create(IMAGE_BASE_RELOCATION[] imageRelolcationDirectory,
            uint relocationDirectorySize, 
            byte[] buffer,
            ulong offsetInBuffer, 
            List<RelocationTypeOffsetItem> addressesOfCodeInDataSection);
    }

}
