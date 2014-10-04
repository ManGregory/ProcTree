using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcTree.Core
{
    public class ScriptCreator
    {
        private const string DropProcedureTemplate = "DROP PROCEDURE {0};";


        public IEnumerable<string> CreateDropProcedureScript(IList<DbObject> dbObjects)
        {
            return (from dbObject in dbObjects select string.Format(DropProcedureTemplate, dbObject.Name));
        }

        public IEnumerable<string> CreateAlterProcedureScript(IList<DbObject> dbObjects)
        {
            throw new NotImplementedException();
        }
    }
}
