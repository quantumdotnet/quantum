using System.Collections.Generic;

namespace Quantum.Generator
{
    interface IDatabaseInfoReader
    {
        IList<DatabaseTableInfo> GetSimpleTableStructure();
    }
}
