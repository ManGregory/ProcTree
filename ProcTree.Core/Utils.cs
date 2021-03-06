﻿using System.Text.RegularExpressions;

namespace ProcTree.Core
{
    public static class Utils
    {
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
                    if (me.Value.StartsWith("{") || me.Value.StartsWith("//"))
                    {
                        return string.Empty;
                    }
                    return me.Value;
                },
                RegexOptions.Singleline);
        }
    }
}
