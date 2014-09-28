﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProcTree.Core;

namespace pt
{
    internal class Program
    {
        private static void Main(string[] args)
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
            var objectUsages = SourceFinder.GetObjectUsages(
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
    }
}
