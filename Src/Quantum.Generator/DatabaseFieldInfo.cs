using System;

namespace Quantum.Generator
{
    public class DatabaseFieldInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public string AttributeName => Tools.ToCamelCase(Name);        

        public string FieldName => Tools.ToPrivateName(AttributeName);
    }
}
