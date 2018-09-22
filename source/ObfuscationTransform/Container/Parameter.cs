using System;

namespace ObfuscationTransform.Container
{
    public class Parameter : IParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public Parameter(string name,object value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            Name = name;
            Value = value;
                
        }
    }
}