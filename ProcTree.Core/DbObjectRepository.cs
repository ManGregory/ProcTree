using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

namespace ProcTree.Core
{
    public class DbObjectRepository : IDbObjectRepository
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

        protected FbConnection GetConnection()
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

        protected virtual string GetSqlQuery()
        {
            return string.Format(
                "select P.RDB$PROCEDURE_NAME ONAME, 'Procedure' OTYPE, P.RDB$PROCEDURE_SOURCE OSOURCE " +
                "from RDB$PROCEDURES P " +
                "where coalesce(p.RDB$PROCEDURE_SOURCE, '') <> '' " +
                "union all " +
                "select T.RDB$TRIGGER_NAME ONAME, 'Trigger' OTYPE, T.RDB$TRIGGER_SOURCE OSOURCE " +
                "from RDB$TRIGGERS T " +
                "where coalesce(T.RDB$TRIGGER_SOURCE, '') <> ''"
            );
        }

        IEnumerable<DbObject> IDbObjectRepository.GetDbObjects()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new FbCommand(GetSqlQuery(), conn);
                var reader = cmd.ExecuteReader();
                var table = new DataTable();
                table.Load(reader);
                var dbObjects = (from DataRow row in table.Rows
                    select new DbObject
                    {
                        Name = row["oname"].ToString().Trim().ToLower(CultureInfo.InvariantCulture),
                        Source = row["osource"].ToString().Trim().ToLower(CultureInfo.InvariantCulture),
                        Type = (DbObjectType) Enum.Parse(typeof(DbObjectType), row["otype"].ToString().Trim())
                    }).ToList();
                MakeLinks(dbObjects);
                return dbObjects;
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

        protected virtual void MakeLinks(List<DbObject> dbObjects)
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

    public class DbObjectRepositoryDependencies : DbObjectRepository, IDbObjectRepository
    {
        public DbObjectRepositoryDependencies(string userName, string userPass, string dataSource, string dbName)
            : base(userName, userPass, dataSource, dbName)
        {

        }

        protected virtual void MakeLinks(List<DbObject> dbObjects, FbConnection conn)
        {
            foreach (var dbObject in dbObjects)
            {
                var dependencies = GetDependencies(dbObject, conn).ToList();
                dbObject.LinkedDbOjbects.AddRange(
                    dependencies.Where(d => d.Type == DbObjectType.Procedure)
                        .Select(d => dbObjects.First(d1 => d1.Name == d.Name)));
                dbObject.LinkedDbOjbects.AddRange(dependencies.Where(d => d.Type != DbObjectType.Procedure));
            }
        }

        private IEnumerable<DbObject> GetDependencies(DbObject dbObject, FbConnection conn)
        {
            var cmd = new FbCommand("select distinct rd.RDB$DEPENDED_ON_NAME as name, rd.RDB$DEPENDED_ON_TYPE as type from RDB$DEPENDENCIES rd where rd.RDB$DEPENDENT_NAME = @object_name", conn);
            cmd.Parameters.Add("@object_name", FbDbType.VarChar, 100).Value = dbObject.Name.ToUpperInvariant();
            var table = new DataTable();
            table.Load(cmd.ExecuteReader());
            return (from DataRow row in table.Rows
                select new DbObject
                {
                    Name = row["name"].ToString().ToLowerInvariant().Trim(),
                    Type = (DbObjectType) Enum.ToObject(typeof(DbObjectType), row["type"])
                });
        }

        IEnumerable<DbObject> IDbObjectRepository.GetDbObjects()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new FbCommand(GetSqlQuery(), conn);
                var reader = cmd.ExecuteReader();
                var table = new DataTable();
                table.Load(reader);
                var dbObjects = (from DataRow row in table.Rows
                    select new DbObject
                    {
                        Name = row["oname"].ToString().Trim().ToLower(CultureInfo.InvariantCulture),
                        Source = row["osource"].ToString().Trim().ToLower(CultureInfo.InvariantCulture),
                        Type = (DbObjectType)Enum.Parse(typeof(DbObjectType), row["otype"].ToString().Trim())
                    }).ToList();
                MakeLinks(dbObjects, conn);
                return dbObjects;
            }            
        }
    }
}
