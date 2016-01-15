using System.Collections.Generic;

namespace Quantum.Generator
{
    public class DatabaseTableInfo
    {
        public string Name { get; set; }

        public IList<DatabaseFieldInfo> Fields { get; set; }

        public string ClassName => Tools.ToCamelCase(Name);
    }
}
