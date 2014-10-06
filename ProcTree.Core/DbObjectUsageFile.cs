﻿using System.Collections.Generic;

namespace ProcTree.Core
{
    public class DbObjectUsageFile
    {
        public DbObject DbObject { get; set; }
        public List<FileUsage> FileUsages { get; set; }

        public override string ToString()
        {
            return string.Format("Object: {0}, {1}", DbObject, string.Join(", ", FileUsages));
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
