namespace ProcTree.Core
{
    public class DbObjectUsageFile
    {
        public DbObject DbObject { get; set; }
        public string PathToFile { get; set; }
        public int LineNumber { get; set; }

        public override string ToString()
        {
            return string.Format("File: {0}, line: {1}, object: {2}", PathToFile, LineNumber, DbObject);
        }
    }
}
