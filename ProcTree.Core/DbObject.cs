using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcTree.Core
{
    public enum DbObjectType
    {
        None,
        Procedure,
        Trigger
    }

    public class DbOjbect
    {
        public string Name { get; set; }
        public DbObjectType Type { get; set; }
        public string Source { get; set; }

        public static DbObjectType GetDbObjectType(string type)
        {
            return type == "P"
                ? DbObjectType.Procedure
                : type == "T"
                    ? DbObjectType.Trigger
                    : DbObjectType.None;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Type, Name);
        }
    }
}
