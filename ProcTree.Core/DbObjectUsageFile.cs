using System.Collections.Generic;

namespace ProcTree.Core
{
    public class DbObjectUsageFile
    {
        public DbObject DbObject { get; set; }
        public string PathToFile { get; set; }
        public int LineNumber { get; set; }
        public FileUsage FileUsage { get; set; }

        public override string ToString()
        {
            return string.Format("File: {0}, line: {1}, object: {2}", PathToFile, LineNumber, DbObject);
        }
    }

    public class FileUsage
    {
        public string PathToFile { get; set; }
        public List<int> LineNumbers { get; set; }

        public override string ToString()
        {
            return string.Format("File: {0}, lines: {1}", PathToFile, string.Join(", ", LineNumbers));
        }
    }
}
