using System.Collections;
using System.Collections.Generic;

namespace ProcTree.Core
{
    public interface IDbObjectRepository
    {
        IEnumerable<DbObject> GetDbObjects();
    }
}