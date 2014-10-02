using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ProcTreeGUI
{
    internal enum SyntaxHighlighting
    {
        Sql,
        Pascal
    }

    internal  static class GuiUtils
    {
        private const string DefaultSyntaxFolder = @"AvalonEdit\SyntaxHighlighting";
        private static readonly Dictionary<SyntaxHighlighting, string> SyntaxFileName = new Dictionary<SyntaxHighlighting, string>
        {
            {SyntaxHighlighting.Pascal, "Pascal.xshd"},
            {SyntaxHighlighting.Sql, "sql.xshd"}
        };

        private static string GetPathToSyntaxFile(SyntaxHighlighting syntax)
        {
            return Path.Combine(DefaultSyntaxFolder, SyntaxFileName[syntax]);
        }

        public static void LoadAvalonSyntax(string pathToFile, ICSharpCode.AvalonEdit.TextEditor editor)
        {
            using (var reader = new XmlTextReader(pathToFile))
                editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        public static void LoadAvalonSyntax(SyntaxHighlighting syntax, ICSharpCode.AvalonEdit.TextEditor editor)
        {
            LoadAvalonSyntax(GetPathToSyntaxFile(syntax), editor);
        }
    }
}
