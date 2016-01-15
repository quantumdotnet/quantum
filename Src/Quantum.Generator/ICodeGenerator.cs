using System.Collections.Generic;

namespace Quantum.Generator
{
    interface ICodeGenerator
    {
        void GenerateClasses(IList<DatabaseTableInfo> tables);
    }
}
