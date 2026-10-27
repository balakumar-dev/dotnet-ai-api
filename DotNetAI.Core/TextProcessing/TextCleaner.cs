using System.Text.RegularExpressions;

namespace DotNetAI.Core.TextProcessing;

public static class TextCleaner
{
    private static readonly Regex ExcessNewlines = new(@"\n{3,}", RegexOptions.Compiled);
    private static readonly Regex ExcessSpaces = new(@"[ \t]+", RegexOptions.Compiled);

    public static string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string normalized = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Replace('\u0000', ' ');

        normalized = ExcessSpaces.Replace(normalized, " ");
        normalized = ExcessNewlines.Replace(normalized, "\n\n");

        return normalized.Trim();
    }
}
