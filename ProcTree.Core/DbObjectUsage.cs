using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcTree.Core
{
    public class DbObjectUsage
    {
        public DbObject DbObject { get; set; }
        public IEnumerable<DbObject> DbUsages { get; set; }
        public IEnumerable<DbObjectUsageFile> SourceFileUsages { get; set; }
    }
}
