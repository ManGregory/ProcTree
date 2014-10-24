using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcTree.Core.ConvertSql
{
    public class DelphiConverter : ISqlTextConverter
    {
        private const string LineSeparator = "\r\n";
        private const string OpenQuote = "'";
        private const string CloseQuote = "'";
        private const string ConcatenationSymbol = " + ";
        private const string TerminationSymbol = ";";

        public int BeginMarginCount { get; private set; }
        public string BeginMarginSymbol { get; private set; }

        private static readonly Dictionary<string, string> EscapeCharacters = new Dictionary<string, string>
            {
                {"'", "''"}
            };

        private string EscapeText(string text)
        {
            return EscapeCharacters.Keys.Aggregate(text, (current, escChar) => current.Replace(escChar, EscapeCharacters[escChar]));
        }

        public DelphiConverter(int beginMarginCount, string beginMarginSymbol)
        {
            BeginMarginCount = beginMarginCount;
            BeginMarginSymbol = beginMarginSymbol;
        }

        public string ConvertToProgrammingLanguage(string sqlText)
        {
            if (sqlText == null)
            {
                throw new ArgumentNullException("sqlText");
            }
            var builder = new StringBuilder();
            var lines = sqlText.Split(new string[] {LineSeparator}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                builder.Append(CreateBeginMargin() + OpenQuote + EscapeText(line) + ' ' + CloseQuote + ConcatenationSymbol + Environment.NewLine);
            }
            var result = builder.ToString();
            return result.Remove(result.Length - 5, 5) + TerminationSymbol;
        }

        private string CreateBeginMargin()
        {
            return String.Concat(Enumerable.Repeat(BeginMarginSymbol, BeginMarginCount));
        }

        public string ConvertFromProgrammingLangeuage(string sqlText)
        {
            throw new NotImplementedException();
        }
    }
}
