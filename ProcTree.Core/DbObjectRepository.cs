using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

namespace ProcTree.Core
{
    public class DbObjectRepository
    {
        public readonly string UserName;
        public readonly string UserPassword;
        public readonly string DataSource;
        public readonly string DbName;

        public DbObjectRepository() 
        {            
        }

        public DbObjectRepository(string userName, string userPass, string dataSource, string dbName)
        {
            UserName = userName;
            UserPassword = userPass;
            DataSource = dataSource;
            DbName = dbName;
        }

        private FbConnection GetConnection()
        {
            var connBuilder = new FbConnectionStringBuilder
            {
                UserID = UserName,
                Password = UserPassword,
                DataSource = DataSource,
                Database = DbName
            };
            var conn = new FbConnection(connBuilder.ToString());
            return conn;
        }

        private string GetSqlQuery()
        {
            return string.Format(
                "select P.RDB$PROCEDURE_NAME ONAME, 'P' OTYPE, P.RDB$PROCEDURE_SOURCE OSOURCE " +
                "from RDB$PROCEDURES P " +
                "where coalesce(p.RDB$PROCEDURE_SOURCE, '') <> '' " +
                "union all " +
                "select T.RDB$TRIGGER_NAME ONAME, 'T' OTYPE, T.RDB$TRIGGER_SOURCE OSOURCE " +
                "from RDB$TRIGGERS T " +
                "where coalesce(T.RDB$TRIGGER_SOURCE, '') <> ''"
            );
        }

        public IEnumerable<DbObject> GetDbObjects()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new FbCommand(GetSqlQuery(), conn);
                var reader = cmd.ExecuteReader();
                var table = new DataTable();
                table.Load(reader);
                return (from DataRow row in table.Rows
                    select new DbObject
                    {
                        Name = row["oname"].ToString().Trim().ToLower(CultureInfo.InvariantCulture),
                        Source = row["osource"].ToString().Trim().ToLower(CultureInfo.InvariantCulture),
                        Type = DbObject.GetDbObjectType(row["otype"].ToString().Trim())
                    }).ToList();
            }
        }

        public void ExecuteScript(string scriptText)
        {
            using (var conn = GetConnection())
            {
                var fbScript = new FbScript(scriptText);
                fbScript.Parse();
                var batch = new FbBatchExecution(conn, fbScript);
                batch.Execute(true);
            }
        }

        public static void MakeLinks(List<DbObject> dbObjects)
        {
            if (dbObjects == null)
            {
                throw new ArgumentNullException("dbObjects");
            }
            foreach (var dbObj in dbObjects)
            {
                foreach (var dbObj1 in dbObjects)
                {
                    if (dbObj1.Type == DbObjectType.Procedure)
                    {
                        if (dbObj.Source.IndexOf(dbObj1.Name, StringComparison.Ordinal) > -1)
                        {
                            dbObj.LinkedDbOjbects.Add(dbObj1);
                        }
                    }
                }
            }
        }

        public static IEnumerable<DbObject> GetUnusedDbObjects(List<DbObject> dbObjects)
        {
            return (from dbObj in dbObjects.Where(d => d.Type == DbObjectType.Procedure)
                let isUsed = dbObjects.Any(dbObj1 => dbObj1.LinkedDbOjbects.Contains(dbObj))
                where !isUsed
                select dbObj).ToList();
        }
    }
}
