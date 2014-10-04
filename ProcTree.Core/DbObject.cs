using System.Collections.Generic;
using FirebirdSql.Data.Isql;

namespace ProcTree.Core
{
    public enum DbObjectType
    {
        None,
        Procedure,
        Trigger,
        Table
    }

    public enum DbFirebirdBaseFieldType
    {
        Unknown = -1,
        SmallInt = 7,
        Integer = 8,
        Int64 = 16,
        Quad = 9,
        Float = 10,
        DFloat = 11,
        Boolean = 17,
        Double = 27,
        Date = 12,
        Time = 13,
        Timestamp = 35,
        Blob = 261,
        Varchar = 37,
        Char = 14,
        CString = 40,
        BlobId = 45
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

    public class DbFieldType
    {
        public DbFirebirdBaseFieldType BaseType { get; set; }
        public DbFirebirdBaseFieldSubType BasedSubType { get; set; }
        public int Length { get; set; }
        public int Scale { get; set; }
        public int Precision { get; set; }
        public int CharacterLength { get; set; }
        public string CollationName { get; set; }
        public string CharacterSetName { get; set; }
    }

    public class DbField
    {
        
    }

    public class DbObject
    {
        private static readonly Dictionary<string, DbObjectType> DbObjectTypesDictionary = new Dictionary<string, DbObjectType>
        {
            {"P", DbObjectType.Procedure},
            {"T", DbObjectType.Trigger},
            {"Table", DbObjectType.Table}
        };

        public string Name { get; set; }
        public DbObjectType Type { get; set; }
        public string Source { get; set; }
        public List<DbObject> LinkedDbOjbects { get; set; }

        public DbObject()
        {
            LinkedDbOjbects = new List<DbObject>();
        }

        public static DbObjectType GetDbObjectType(string type)
        {
            return DbObjectTypesDictionary.ContainsKey(type)
                ? DbObjectTypesDictionary[type]
                : DbObjectType.None;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Type, Name);            
        }
    }

    public class DbProcedureParameter
    {
        public string Name { get; set; }
    }

    public class DbProcedure : DbObject
    {
        
    }
}
