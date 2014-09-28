using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProcTree.Core
{
    public static class SourceFinder
    {
        public static IEnumerable<DbObjectUsageFile> GetObjectUsages(IEnumerable<string> baseDirs, IList<string> searchPatterns, IList<DbObject> objectsToFind)
        {
            var result = new List<DbObjectUsageFile>();
            foreach (var baseDir in baseDirs)
            {
                result.AddRange(GetObjectUsage(baseDir, searchPatterns, objectsToFind));
            }
            return result;
        }

        private static IEnumerable<DbObjectUsageFile> GetObjectUsage(string baseDir, IList<string> searchPatterns,
            IList<DbObject> objectsToFind)
        {
            var result = new List<DbObjectUsageFile>();
            var dirInfo = new DirectoryInfo(baseDir);
            foreach (var file in dirInfo.GetFilesByPatterns(searchPatterns))
            {
                result.AddRange(ProcessFile(file.FullName, objectsToFind));
            }
            foreach (var dir in dirInfo.GetDirectories())
            {
                result.AddRange(GetObjectUsage(dir.FullName, searchPatterns, objectsToFind));
            }
            return result;
        }

        private static string GetTextWithoutComments(string text)
        {
            const string blockComments = @"{(.*?)}";
            const string lineComments = @"//(.*?)\r?\n";
            const string strings = @"""((\\[^\n]|[^""\n])*)""";
            const string verbatimStrings = @"@(""[^""]*"")+";            
            return Regex.Replace(text,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("{") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    return me.Value;
                },
                RegexOptions.Singleline);            
        }

        private static IEnumerable<DbObjectUsageFile> ProcessFile(string file, IList<DbObject> valuesToFind)
        {
            var result = new List<DbObjectUsageFile>();
            if (File.Exists(file))
            {
                var lines = GetTextWithoutComments(File.ReadAllText(file)).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (var lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                {
                    var line = lines[lineNumber].ToLower(CultureInfo.InvariantCulture);
                    result.AddRange(from valueToFind in valuesToFind
                        where line.Contains(valueToFind.Name)
                        select new DbObjectUsageFile
                        {
                            DbObject = valueToFind, LineNumber = lineNumber, PathToFile = file
                        });
                }
            }
            return result;
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
