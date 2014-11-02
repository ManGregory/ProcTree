using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SqlServer.Server;

namespace ProcTree.Core
{
    public enum DbObjectType
    {
        Unknown = -1,
        Table = 0,
        View = 1,
        Trigger = 2,
        Computed = 3,
        Validation = 4,
        Procedure = 5,
        ExpressionIndex = 6,
        Exception = 7,
        User = 8,
        Field = 9,
        Index = 10,
        Generator = 14,
        Udf = 15
    }

    public enum DbFirebirdBaseFieldType
    {
        Unknown = -1,
        Short = 7,
        Long = 8,
        Quad = 9,
        Float = 10,
        Date = 12,
        Time = 13,
        Text = 14, // char
        Int64 = 16,
        Double = 27,
        Timestamp = 35,
        Varying = 37, // varchar
        CString = 40,
        BlobId = 45,
        Blob = 261
    }

    public enum DbFirebirdBaseFieldSubType
    {
        Unknown,
        Numeric,
        Decimal,
        BigInt,
        Unspecified,
        Binary,
        Acl,
        Text,
        Blr,
        Reserved,
        EncodedMetaData,
        IrregularFinishedMultiDbTx,
        TransactionalDescription,
        ExternalFileDescription
    }

    public enum ParameterType
    {
        Input = 0,
        Output = 1
    }

    public enum ParameterMechanism
    {
        Normal = 0,
        TypeOf = 1
    }

    public class DbObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: {1}, Description: {2}", Id, Name, Description);
        }

        public virtual string GetDdl()
        {
            return string.Empty;
        }
    }

    public class LinkedDbObject : DbObject
    {
        public DbObjectType Type { get; set; }
        public string Source { get; set; }
        public List<LinkedDbObject> LinkedDbOjbects { get; set; }

        public LinkedDbObject()
        {
            LinkedDbOjbects = new List<LinkedDbObject>();
            Id = -1;
            Type = DbObjectType.Unknown;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Type, Name);            
        }
    }

    public class CharacterSet : DbObject
    {
        public Collation DefaultCollation { get; set; }
        public int BytesPerCharacter { get; set; }
    }

    public class Collation : DbObject
    {
        public CharacterSet CharacterSet { get; set; }
        public int CollationAttributes { get; set; }
        public Collation BaseCollation { get; set; }
        public string SpecificAttributes { get; set; }
    }

    public class DbField : LinkedDbObject
    {
        public DbFirebirdBaseFieldType BaseType { get; set; }
        public DbFirebirdBaseFieldSubType SubType { get; set; }
        public int Length { get; set; }
        public int Scale { get; set; }
        public int Precision { get; set; }
        public int CharacterLength { get; set; }
        public Collation Collation { get; set; }
        public CharacterSet CharacterSet { get; set; }
        public bool IsNull { get; set; }
        public string DefaultSource { get; set; }
        public string ComputedSource { get; set; }

        public static Dictionary<DbFirebirdBaseFieldType, string> TypeNames = new Dictionary
            <DbFirebirdBaseFieldType, string>
        {
            {DbFirebirdBaseFieldType.Short, "SMALLINT"},
            {DbFirebirdBaseFieldType.Long, "INTEGER"},
            {DbFirebirdBaseFieldType.Quad, "BIGINT"},
            {DbFirebirdBaseFieldType.Int64, "BIGINT"},
            {DbFirebirdBaseFieldType.Varying, "VARCHAR"},
            {DbFirebirdBaseFieldType.Text, "CHAR"}
        };

        public override string GetDdl()
        {
            var res = BaseType.ToString().ToUpper();
            if ((BaseType == DbFirebirdBaseFieldType.Short) ||
                (BaseType == DbFirebirdBaseFieldType.Long) ||
                (BaseType == DbFirebirdBaseFieldType.Quad) ||
                (BaseType == DbFirebirdBaseFieldType.Int64))
            {
                if (SubType == DbFirebirdBaseFieldSubType.Unknown)
                {
                    res = TypeNames.ContainsKey(BaseType) ? TypeNames[BaseType] : BaseType.ToString();
                }
                else if ((SubType == DbFirebirdBaseFieldSubType.Numeric) || (SubType == DbFirebirdBaseFieldSubType.Decimal))
                {
                    res = string.Format("{0}({1},{2})", SubType, Precision, Scale);
                }
            } 
            else if ((BaseType == DbFirebirdBaseFieldType.Text) ||
                     (BaseType == DbFirebirdBaseFieldType.Varying))
            {
                res = string.Format("{0}({1}) {2}", TypeNames[BaseType], CharacterLength, 
                    string.Format("CHARACTER SET {0}", CharacterSet.Name));
            }
            return res;
        }
    }

    public class DbProcedureParameter : DbObject
    {
        public int OrderNumber { get; set; }
        public ParameterType ParameterType { get; set; }
        public DbField Field { get; set; }
        public Collation Collation { get; set; }
        public bool IsNull { get; set; }
        public ParameterMechanism ParameterMechanism { get; set; }
        public string FieldName { get; set; }
        public string RelationName { get; set; }
        public string DefaultSource { get; set; }

        public override string GetDdl()
        {
            return string.Format("{0} {1} {2} {3} {4}", 
                Name, 
                GetSqlType(), 
                IsNull ? string.Empty : "NOT NULL",
                Collation == null 
                    ? Field.Collation == null ? string.Empty : Field.Collation.Name 
                    : string.Format("COLLATE {0}", Collation.Name),
                GetDefaultSql());
        }

        private string GetDefaultSql()
        {
            return ParameterType == ParameterType.Input
                ? DefaultSource ?? string.Empty
                : string.Empty;
        }

        private string GetSqlType()
        {
            return ParameterMechanism == ParameterMechanism.Normal
                ? Field.GetDdl()
                : string.Format("TYPE OF COLUMN {0}.{1}", RelationName, FieldName);
        }
    }

    public class DbProcedure : LinkedDbObject
    {
        public string OwnerName { get; set; }
        public List<DbProcedureParameter> Parameters { get; set; }

        public DbProcedure()
        {
            Parameters = new List<DbProcedureParameter>();
        }

        public override string GetDdl()
        {
            var sb = new StringBuilder();
            sb.Append(DdlCreationUtils.SetTerm(true));
            sb.Append(DdlCreationUtils.CreateOrAlter(this));
            sb.Append(string.Format("({0}) returns ({1})", 
                DdlCreationUtils.CreateParameteresList(Parameters.Where(p => p.ParameterType == ParameterType.Input)),
                DdlCreationUtils.CreateParameteresList(Parameters.Where(p => p.ParameterType == ParameterType.Output))));
            sb.Append(DdlCreationUtils.SetTerm(false));
            return sb.ToString();
        }
    }

    public static class DdlCreationUtils
    {
        public static string SetTerm(bool isBegin)
        {
            return isBegin ? "SET TERM ^ ;" : "SET TERM ; ^";
        }

        public static string CreateOrAlter(LinkedDbObject dbObject)
        {
            return string.Format("CREATE OR ALTER {0} {1}", dbObject.Type.ToString().ToUpper(), dbObject.Name.ToUpper());
        }

        public static string CreateParameteresList(IEnumerable<DbProcedureParameter> parameters)
        {
            return string.Join(", " + Environment.NewLine, parameters.Select(p => p.GetDdl()));
        }
    }
}
