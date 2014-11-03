using System.Collections.Generic;

namespace ProcTree.Core
{
    public interface IDbObjectRepository
    {
        IEnumerable<LinkedDbObject> GetDbObjects();
    }
}