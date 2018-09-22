using ObfuscationTransform.Core;
using PeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation
{
    /// <summary>
    /// Responsible for executing the transformation
    /// </summary>
    public class TransformationExecuter : ITransformationExecuter
    {
        public IReadOnlyCollection<ITransformation> m_transformations; 

        public TransformationExecuter(IReadOnlyCollection<ITransformation> transformationsCollection)
        {
            if (transformationsCollection == null || transformationsCollection.Count == 0) throw new ArgumentNullException("Transformation can not be null nor empty");

            m_transformations = transformationsCollection;
        }


        public ICode Transform(ICode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            var transformedCode = code;

            foreach (var transformation in m_transformations)
            {
                transformedCode = transformation.Transform(transformedCode);
            }

            return transformedCode;
        }

        
 
    }

}

