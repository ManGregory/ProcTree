using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        private static readonly Dictionary<string, string> UnescapeCharacters = new Dictionary<string, string>
            {
                {"''", "\""},
                {"'", ""},
                {"\"", "'"},
            };

        private string EscapeText(string text)
        {
            return EscapeCharacters.Keys.Aggregate(text, (current, escChar) => current.Replace(escChar, EscapeCharacters[escChar]));
        }

        private string UnescapeText(string text)
        {
            const string strings = @"'((\\[^\n]|[^""\n])*)'";
            var stringMatches = Regex.Matches(text, strings, RegexOptions.Singleline);
            var result = new StringBuilder();
            foreach (Match stringMatch in stringMatches)
            {
                result.Append(UnescapeCharacters.Aggregate(stringMatch.Value,
                    (current, pair) => current.Replace(pair.Key, pair.Value)));
            }
            return result.ToString();
        }

        public DelphiConverter(int beginMarginCount, string beginMarginSymbol)
        {
            BeginMarginCount = beginMarginCount;
            BeginMarginSymbol = beginMarginSymbol;
        }

        private string Convert(string sqlText, SqlConversionDirection direction)
        {
            var builder = new StringBuilder();
            var lines = sqlText.Split(new[] { LineSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var procLine = direction == SqlConversionDirection.CodeToSql
                    ? CreateBeginMargin() + OpenQuote + EscapeText(line) + ' ' + CloseQuote + ConcatenationSymbol +
                      Environment.NewLine
                    : UnescapeText(line) + Environment.NewLine;
                builder.Append(procLine);
            }
            return builder.ToString();            
        }

        public string ConvertToProgrammingLanguage(string sqlText)
        {
            if (sqlText == null)
            {
                throw new ArgumentNullException("sqlText");
            }
            var result = Convert(sqlText, SqlConversionDirection.CodeToSql); 
            return result.Length > 5 ? result.Remove(result.Length - 5, 5) + TerminationSymbol : result;
        }

        private string CreateBeginMargin()
        {
            return String.Concat(Enumerable.Repeat(BeginMarginSymbol, BeginMarginCount));
        }

        public string ConvertFromProgrammingLanguage(string sqlText)
        {
            if (sqlText == null)
            {
                throw new ArgumentNullException("sqlText");
            }
            var result = Convert(sqlText, SqlConversionDirection.SqlToCode);
            return result.TrimEnd();
        }

        public SqlConversionDirection GetSqlConversionDirection(string sqlText)
        {
            return sqlText.Trim().StartsWith(OpenQuote)
                ? SqlConversionDirection.CodeToSql
                : SqlConversionDirection.SqlToCode;
        }
    }
}
