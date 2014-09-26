using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FirebirdSql;
using FirebirdSql.Data;
using FirebirdSql.Data.FirebirdClient;
using ProcTree.Core;

namespace pt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: pt <username> <password> <servername> <dbname> <<sourcedirs> ...>");
                Console.ReadLine();
                return;
            }

            var repo = new DbObjectRepository(args[0], args[1], args[2], args[3]);
            var dbObjects = repo.GetDbObjects().OrderBy(d => d.Name).ToList();
            DbObjectRepository.MakeLinks(dbObjects);
            var unusedDbObjects = DbObjectRepository.GetUnusedDbObjects(dbObjects).ToList();
            var objectUsages = GetObjectUsageFilesDirs(
                args.Skip(4).Take(args.Length - 4), 
                new List<string>(new[] {".pas", ".dfm"}), 
                unusedDbObjects).ToList();
            var usedInSource = objectUsages.Select(u => u.DbObject).GroupBy(d => d.Name).Select(gr => gr.First());
            unusedDbObjects = unusedDbObjects.Except(usedInSource).ToList();
            File.WriteAllLines("unused_procs.txt", unusedDbObjects.Select(d => d.ToString()));
            foreach (var unusedDbObject in unusedDbObjects)
            {
                Console.WriteLine(unusedDbObject);
            }
            Console.ReadLine();
        }

        private static IEnumerable<DbObjectUsageFile> GetObjectUsageFilesDirs(IEnumerable<string> baseDirs, IList<string> extensions, IList<DbObject> objectsToFind)
        {
            var result = new List<DbObjectUsageFile>();
            foreach (var baseDir in baseDirs)
            {
                result.AddRange(GetObjectUsageFilesDir(baseDir, extensions, objectsToFind));
            }
            return result;
        }

        private static IEnumerable<DbObjectUsageFile> GetObjectUsageFilesDir(string baseDir, IList<string> extensions , IList<DbObject> objectsToFind)
        {
            var result = new List<DbObjectUsageFile>();
            var dirInfo = new DirectoryInfo(baseDir);
            foreach (var file in dirInfo.GetFilesByExtensions(extensions))
            {
                result.AddRange(ProcessFile(file.FullName, objectsToFind));
            }
            foreach (var dir in dirInfo.GetDirectories())
            {
                result.AddRange(GetObjectUsageFilesDir(dir.FullName, extensions, objectsToFind));
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
                var lines = GetTextWithoutComments(File.ReadAllText(file)).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                {
                    var line = lines[lineNumber].ToLower(CultureInfo.InvariantCulture);                    
                    foreach (var valueToFind in valuesToFind)
                    {
                        if (line.Contains(valueToFind.Name))
                        {
                            result.Add(new DbObjectUsageFile
                            {
                                DbObject = valueToFind,
                                LineNumber = lineNumber,
                                PathToFile = file
                            });
                        }
                    }
                }
            }
            return result;
        }
    }

    public static class DirUtils
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, IList<string> extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }
    }
}
