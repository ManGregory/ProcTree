namespace ProcTree.Core.ConvertSql
{
    public interface ISqlTextConverter
    {
        string ConvertToProgrammingLanguage(string sqlText);
        string ConvertFromProgrammingLangeuage(string sqlText);
    }
}