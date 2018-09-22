using Microsoft.Practices.Unity;
using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.Parser;
using ObfuscationTransform.PeExtensions;
using ObfuscationTransform.Transformation;
using ObfuscationTransform.Transformation.Factory;
using ObfuscationTransform.Transformation.Junk;
using SharpDisasm;
using SharpDisasm.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Container
{
    public static class Container
    {
        static Lazy<ContainerInternal> m_container = new Lazy<ContainerInternal>();
        
        public static T Resolve<T> ()
        {
           return  m_container.Value.Resolve<T>();
        }

        public static T Resolve<T>(params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            if (parameters.Length > 0)
            {
                ParameterOverrides paramsOverride = new ParameterOverrides();

                foreach (var parameter in parameters)
                { 
                    if (parameter.Value != null)
                    {
                        paramsOverride.Add(parameter.Name, parameter.Value);
                    }
                }

                return m_container.Value.Resolve<T>(paramsOverride);
            }
            else return m_container.Value.Resolve<T>();
        }
    }

    public class ContainerInternal:UnityContainer
    {
        public ContainerInternal()
        {
            RegisterAll();
        }

        internal void RegisterAll()
        {
            //core classes
            this.RegisterType<IInstruction, AssemblyInstructionForTransformation>();
            this.RegisterType<IAssemblyInstructionForTransformation, AssemblyInstructionForTransformation>();
            this.RegisterType<IBasicBlock, BasicBlock>();
            this.RegisterType<ICode,Code> ();
            this.RegisterType<ICodeInMemoryLayout, CodeInMemoryLayout>();
            this.RegisterType<IRelocationDirectoryInfo, RelocationDirectoryInfo>();
            this.RegisterType<IFunction,Function> ();
            (this).RegisterType<IInstructionWithAddressOperandDecider, Core.InstructionWithAddressOperandDecider> (new HierarchicalLifetimeManager());
            this.RegisterType<ICodeObfuscator, CodeObfuscator>();
            this.RegisterType<IDisassembler, Core.Disassembler>();
            this.RegisterType<IStatistics, Statistics>(new HierarchicalLifetimeManager());
            this.RegisterType<ICodeInMemoryLayout, CodeInMemoryLayout>(new InjectionConstructor(
                        new InjectionParameter(typeof(ulong), 0), 
                        new InjectionParameter(typeof(ulong),0),
                        new InjectionParameter(typeof(ulong), 0),
                        new InjectionParameter(typeof(ulong), 0),
                        new InjectionParameter(typeof(ulong), 0),
                        new InjectionParameter(typeof(ulong), 0),
                        new InjectionParameter(typeof(IRelocationDirectoryInfo), null)));



            //transformation classes
            this.RegisterType<IPeTransform, PeTransform>(new HierarchicalLifetimeManager());
            this.RegisterType<ITransformationAddingJunkBytes, TransformationAddingJunkBytes>(new HierarchicalLifetimeManager());
            this.RegisterType<ITransformationExecuter, TransformationExecuter>();
            this.RegisterType<ITransformationAddingUnconditionalJump, TransformationAddingUnconditionalJump>(new HierarchicalLifetimeManager());
            this.RegisterType<IJunkBytesProvider, JunkBytesProvider>(new HierarchicalLifetimeManager());
            this.RegisterType<IInstructionWithAddressOperandTransform, InstructionWithAddressOperandTransform>(new HierarchicalLifetimeManager());
            this.RegisterType<ICodeTransform, CodeTransform>(new HierarchicalLifetimeManager());
            this.RegisterType<IRelocationDirectoryFromNewCode, RelocationDirectoryFromNewCode>(new HierarchicalLifetimeManager());
            
            //parsers
            this.RegisterType <IBasicBlockParser,BasicBlockParser> (new HierarchicalLifetimeManager());
            this.RegisterType<IBasicBlockEpilogParser, BasicBlockEpilogParser>(new HierarchicalLifetimeManager());
            this.RegisterType <ICodeParser,CodeParser> (new HierarchicalLifetimeManager());
            this.RegisterType<IFunctionParser, FunctionParser>(new HierarchicalLifetimeManager());
            this.RegisterType <IFunctionEpilogParser,FunctionEpilogParser> (new HierarchicalLifetimeManager());
            this.RegisterType <IFunctionPrologParser,FunctionPrologParser> (new HierarchicalLifetimeManager());


            //factories
            this.RegisterType<ICodeFactory, CodeFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<IAssemblyInstructionForTransformationFactory, AssemblyInstructionForTransformationFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<IBasicBlockFactory, BasicBlockFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<IFunctionFactory, FunctionFactory>(new HierarchicalLifetimeManager());
            (this).RegisterType<IInstructionWithAddressOperandDeciderFactory, Core.Factory.InstructionWithAddressOperandDeciderFactory>();
            this.RegisterType<IRelocationDirectoryInfoFactory, RelocationDirctoryInfoFactory>();
            this.RegisterType<ICodeObfuscatorFactory, CodeObfuscatorFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<ITransformationAddingJunkBytesFactory, TransformationAddingJunkBytesFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<ITransformationToConditionalJumpFactory, TransformationToConditionalJumpFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<IDisassemblerFactory, DisassemblerFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<ITransformationExecuterFactory, TransfomationExecuterFactory>(new HierarchicalLifetimeManager());
            this.RegisterType<ICodeInMemoryLayoutFactory, CodeInMemoryLayoutFactory>(new HierarchicalLifetimeManager());
            
            //register PeExtension classes
            this.RegisterType<IImageBaseRelocationSerializer, ImageBaseRelocationSerializer>(new HierarchicalLifetimeManager());
            this.RegisterType<ITypeOffsetSerializer, TypeOffsetSerializer>(new HierarchicalLifetimeManager());
        }
    }
}
