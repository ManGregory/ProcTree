namespace ProcTree.Core.ConvertSql
{
    public enum SqlConversionDirection
    {
        SqlToCode = 0,
        CodeToSql = 1
    }

    public interface ISqlTextConverter
    {
        string ConvertToProgrammingLanguage(string sqlText);
        string ConvertFromProgrammingLanguage(string sqlText);
        SqlConversionDirection GetSqlConversionDirection(string sqlText);
    }
}