﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

        public void FindObjectUsages(IEnumerable<string> baseDirs, IList<string> searchPatterns, IList<DbObject> objectsToFind)
        {
            foreach (var baseDir in baseDirs)
            {
                FindObjectUsage(baseDir, searchPatterns, objectsToFind);
            }
        }

        private void FindObjectUsage(string baseDir, IList<string> searchPatterns,
            IList<DbObject> objectsToFind)
        {
            var dirInfo = new DirectoryInfo(baseDir);
            Parallel.ForEach(
                dirInfo.GetFilesByPatterns(searchPatterns), info => ProcessFile(info.FullName, objectsToFind)
            );
            Parallel.ForEach(
                dirInfo.GetDirectories(), info => FindObjectUsage(info.FullName, searchPatterns, objectsToFind)
            );
        }

        public static string GetTextWithoutComments(string text)
        {
            const string blockComments = @"{(.*?)}";
            const string lineComments = @"//(.*?)\r?\n";
            const string strings = @"'((\\[^\n]|[^""\n])*)'";
            const string verbatimStrings = @"@('[^""]*')+";
            return Regex.Replace(text,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("//"))
                        return Environment.NewLine;
                    if (me.Value.StartsWith("{"))
                    {
                        int count = me.Value.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).Count() - 1;
                        return string.Concat(Enumerable.Repeat(Environment.NewLine, count));
                    }
                    return me.Value;
                },
                RegexOptions.Singleline);
        }

        private void ProcessFile(string file, IList<DbObject> valuesToFind)
        {
            if (File.Exists(file))
            {
                var text = File.ReadAllText(file);
                if (Path.GetExtension(file) == ".pas")
                {
                    text = GetTextWithoutComments(text);
                }
                var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                for (var lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                {
                    var line = lines[lineNumber].ToLower(CultureInfo.InvariantCulture);
                    foreach (var valueToFind in valuesToFind)
                    {
                        if (line.Contains(valueToFind.Name))
                        {
                            var dbObjectUsageFile = DbObjectUsageFiles.FirstOrDefault(d => d.DbObject == valueToFind);
                            if (dbObjectUsageFile != null)
                            {
                                var fileUsage = dbObjectUsageFile.FileUsages.FirstOrDefault(f => f.PathToFile == file);
                                if (fileUsage != null)
                                {
                                    if (fileUsage.LineNumbers == null)
                                    {
                                        fileUsage.LineNumbers = new List<FileUsageLine>{new FileUsageLine{FileUsage = fileUsage, LineNumber = lineNumber + 1}};
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
                                        new FileUsageLine {FileUsage = fileUsage, LineNumber = lineNumber + 1}
                                    };
                                    dbObjectUsageFile.FileUsages.Add(fileUsage);
                                }
                            }
                            else
                            {
                                var fileUsage = new FileUsage {PathToFile = file};
                                fileUsage.LineNumbers = new List<FileUsageLine>
                                {
                                    new FileUsageLine {FileUsage = fileUsage, LineNumber = lineNumber + 1}
                                };
                                DbObjectUsageFiles.Add(new DbObjectUsageFile
                                {
                                    DbObject = valueToFind,
                                    FileUsages = new List<FileUsage> { fileUsage }
                                });
                            }
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
}
