using System.Collections.Generic;

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
        public string Name { get; set; }
        public DbObjectType Type { get; set; }
        public string Source { get; set; }
        public List<DbObject> LinkedDbOjbects { get; set; }

        public DbObject()
        {
            LinkedDbOjbects = new List<DbObject>();
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
