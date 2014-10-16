using System.Collections.Generic;
using System.Linq;

namespace ProcTree.Core
{
    public class DbObjectUsage
    {
        public DbObject DbObject { get; set; }
        public IEnumerable<DbObject> DbUsages { get; set; }
        public IEnumerable<DbObjectUsageFile> SourceFileUsages { get; set; }
        public bool IsUsed { get; set; }
    }
}
