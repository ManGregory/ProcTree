using System.Collections.Generic;

namespace ProcTree.Core
{
    public class DbObjectUsage
    {
        public DbObject DbObject { get; set; }
        public IEnumerable<DbObject> DbUsages { get; set; }
        public IEnumerable<DbObjectUsageFile> SourceFileUsages { get; set; }
    }
}
