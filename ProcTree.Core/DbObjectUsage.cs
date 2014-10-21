using System.Collections.Generic;
using System.Linq;

namespace ProcTree.Core
{
    public class DbObjectUsage
    {
        public DbObject DbObject { get; set; }
        public IEnumerable<DbObjectUsageProcedure> DbUsages { get; set; }
        public IEnumerable<DbObjectUsageFile> SourceFileUsages { get; set; }
        public bool IsUsed { get; set; }
    }

    public class DbObjectUsageProcedure 
    {
        public DbObject DbObject { get; set; }
        public IEnumerable<ProcedureUsageLine> LineNumbers { get; set; } 

        public override string ToString()
        {
            return string.Format("Procedure: {0}, lines: {1}", DbObject.Name, string.Join(", ", LineNumbers));
        }
    }

    public class ProcedureUsageLine
    {
        public DbObjectUsageProcedure DbObjectUsageProcedure { get; set; }
        public int LineNumber { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", LineNumber);
        }
    }
}
