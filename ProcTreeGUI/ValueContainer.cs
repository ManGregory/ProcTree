using System.Collections.Generic;
using ProcTree.Core;

namespace ProcTreeGUI
{
    public static class ValueContainer
    {
        public static class DbConnectionValues
        {
            public static string UserName { get; set; }
            public static string UserPassword { get; set; }
            public static string ServerName { get; set; }
            public static string DbName { get; set; }
            public static DbObjectRepository Repository { get; set; }
        }

        public static class ScriptValues
        {
            public static string Script { get; set; }
            public static IList<LinkedDbObject> UnusedDbObjects { get; set; }
        }
    }
}
