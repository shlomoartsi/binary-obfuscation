using ObfuscationTransform.Container;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.Transformation.Junk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation.Factory
{
    public interface IPeTransformFactory : IFactory<IPeTransform> { };
    public interface ITransformationAddingJunkBytesFactory : IFactory<ITransformationAddingJunkBytes> { };
    public interface ITransformationExecuterFactory 
    {
        ITransformationExecuter Create(IReadOnlyCollection<ITransformation> transformationsCollection);
    }
    public interface ITransformationToConditionalJumpFactory : IFactory<ITransformationAddingUnconditionalJump> { };

    public class PeTransformFactory : Factory<IPeTransform> { };
    public class TransformationAddingJunkBytesFactory : Factory<ITransformationAddingJunkBytes>,ITransformationAddingJunkBytesFactory { };
    public class TransfomationExecuterFactory : ITransformationExecuterFactory
    {
        public ITransformationExecuter Create(IReadOnlyCollection<ITransformation> transformationsCollection)
        {
            return Container.Container.Resolve<ITransformationExecuter>(
                new Parameter(nameof(transformationsCollection), transformationsCollection));
        }
    };
    public class TransformationToConditionalJumpFactory : Factory<ITransformationAddingUnconditionalJump>,
        ITransformationToConditionalJumpFactory{ };
   
}
