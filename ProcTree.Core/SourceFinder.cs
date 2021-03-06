﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcTree.Core
{
    public class SourceFinder
    {
        private object _syncRoot = new object();
        public List<DbObjectUsageFile> DbObjectUsageFiles { get; private set; }

        public SourceFinder()
        {
            DbObjectUsageFiles = new List<DbObjectUsageFile>();
        }

        public SourceFinder(List<DbObjectUsageFile> dbObjectUsageFiles)
        {
            DbObjectUsageFiles = dbObjectUsageFiles;
        }

        public void FindObjectUsages(IEnumerable<string> baseDirs, IList<string> searchPatterns, IList<LinkedDbObject> objectsToFind)
        {
            foreach (var baseDir in baseDirs)
            {
                FindObjectUsage(baseDir, searchPatterns, objectsToFind);
            }
        }

        private void FindObjectUsage(string baseDir, IList<string> searchPatterns,
            IList<LinkedDbObject> objectsToFind)
        {
            var dirInfo = new DirectoryInfo(baseDir);
            const bool forceNonParallel = false;
            var options = new ParallelOptions { MaxDegreeOfParallelism = forceNonParallel ? 1 : -1 };
            Parallel.ForEach(
                dirInfo.GetFilesByPatterns(searchPatterns), options, info => ProcessFile(info.FullName, objectsToFind)
            );
            Parallel.ForEach(
                dirInfo.GetDirectories(), options, info => FindObjectUsage(info.FullName, searchPatterns, objectsToFind)
            );
        }

        private void ProcessFile(string file, IList<LinkedDbObject> valuesToFind)
        {
            const string blockComments = @"{(.*?)}";
            const string lineComments = @"//(.*?)\r?\n";
            const string strings = @"'((\\[^\n]|[^""\n])*)'";
            const string verbatimStrings = @"@('[^""]*')+";
            if (File.Exists(file))
            {
                var text = File.ReadAllText(file);
                var stringMatches = Regex.Matches(text,
                    blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings, RegexOptions.Singleline);
                foreach (Match stringMatch in stringMatches)
                {                    
                    if (!(stringMatch.Value.StartsWith("//") || stringMatch.Value.StartsWith("{")))
                    {
                        var lines = stringMatch.Value.Split(new[] { "\r\n" }, StringSplitOptions.None);
                        FindUsages(file, valuesToFind, lines, text.LineFromPos(stringMatch.Index));
                    }
                }
            }
        }

        private void FindUsages(string file, IList<LinkedDbObject> valuesToFind, string[] lines, int beginLine)
        {
            for (var lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                var line = lines[lineNumber].ToLower(CultureInfo.InvariantCulture);
                var words = line.Words();
                foreach (var valueToFind in valuesToFind)
                {
                    if (words.Contains(valueToFind.Name))
                    {
                        var dbObjectUsageFile = DbObjectUsageFiles.FirstOrDefault(d => d.DbObject == valueToFind);
                        if (dbObjectUsageFile != null)
                        {
                            var fileUsage = dbObjectUsageFile.FileUsages.FirstOrDefault(f => f.PathToFile == file);
                            if (fileUsage != null)
                            {
                                if (fileUsage.LineNumbers == null)
                                {
                                    fileUsage.LineNumbers = new List<FileUsageLine>
                                    {
                                        new FileUsageLine {FileUsage = fileUsage, LineNumber = beginLine + lineNumber}
                                    };
                                }
                            }
                            else
                            {
                                if (dbObjectUsageFile.FileUsages == null)
                                {
                                    dbObjectUsageFile.FileUsages = new List<FileUsage>();
                                }
                                fileUsage = new FileUsage
                                {
                                    PathToFile = file
                                };
                                fileUsage.LineNumbers = new List<FileUsageLine>
                                {
                                    new FileUsageLine {FileUsage = fileUsage, LineNumber = beginLine + lineNumber}
                                };
                                dbObjectUsageFile.FileUsages.Add(fileUsage);
                            }
                        }
                        else
                        {
                            var fileUsage = new FileUsage {PathToFile = file};
                            fileUsage.LineNumbers = new List<FileUsageLine>
                            {
                                new FileUsageLine {FileUsage = fileUsage, LineNumber = beginLine + lineNumber}
                            };
                            DbObjectUsageFiles.Add(new DbObjectUsageFile
                            {
                                DbObject = valueToFind,
                                FileUsages = new List<FileUsage> {fileUsage}
                            });
                        }
                    }
                }
            }
        }
    }

    public static class DirUtils
    {
        public static IEnumerable<FileInfo> GetFilesByPatterns(this DirectoryInfo dir, IList<string> searchPatterns)
        {
            if (searchPatterns == null)
                throw new ArgumentNullException("searchPatterns");
            var files = dir.EnumerateFiles();
            return files.Where(f => searchPatterns.Contains(f.Extension));
        }
    }

    public static class StringExt
    {
        public static int LineFromPos(this String s, int pos)
        {
            int res = 1;
            for (int i = 0; i <= pos - 1; i++)
                if (s[i] == '\n') res++;
            return res;
        }

        public static string[] Words(this string s)
        {
            //
            // Split on all non-word characters.
            // ... Returns an array of all the words.
            //
            return s.Split(new[] { '*', ' ', ';', '(', ')', '%', '[', ']', '=', '/', '+', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);
            // @      special verbatim string syntax
            // \W+    one or more non-word characters together
        }
    }
}
