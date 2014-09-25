using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql;
using FirebirdSql.Data;
using FirebirdSql.Data.FirebirdClient;
using ProcTree.Core;

namespace pt
{
    class Program
    {
        private static FbConnection GetConnection(string userName, string userPass, string dataSource, string db)
        {
            var connBuilder = new FbConnectionStringBuilder();
            connBuilder.UserID = userName;
            connBuilder.Password = userPass;
            connBuilder.DataSource = dataSource;
            connBuilder.Database = db;
            var conn = new FbConnection(connBuilder.ToString());
            return conn;
        }

        private static string GetSqlQuery()
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

        private static List<DbOjbect> GetDbObjects(DataTable table)
        {
            var dbObjects = new List<DbOjbect>();
            foreach (DataRow row in table.Rows)
            {
                dbObjects.Add(
                    new DbOjbect
                    {
                        Name = row["oname"].ToString(),
                        Source = row["osource"].ToString(),
                        Type = DbOjbect.GetDbObjectType(row["otype"].ToString())
                    }
                );
            }
            return dbObjects;
        }

        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: pt <username> <password> <servername> <dbname>");
                Console.ReadLine();
                return;
            }
            using (var conn = GetConnection(args[0], args[1], args[2], args[3]))
            {
                conn.Open();
                var cmd = new FbCommand(GetSqlQuery(), conn);
                var reader = cmd.ExecuteReader();
                var table = new DataTable();
                table.Load(reader);
                var dbObjects = GetDbObjects(table);
                foreach (var dbObj in dbObjects)
                {
                    Console.WriteLine(dbObj.ToString());
                }
            }
            Console.ReadLine();
        }
    }
}
