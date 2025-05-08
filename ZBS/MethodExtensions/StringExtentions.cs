

using System;

namespace ZBSolutions
{
    public static class StringExtensions
    {
        public static string EscapeMarkdown(this string text)
        {
            string[] specialChars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
            foreach (var ch in specialChars)
            {
                text = text.Replace(ch, "\\" + ch);
            }
            return text;
        }

        public static string GetTxHash(this string link)
        {
            string hash;

            if (!string.IsNullOrEmpty(link))
            {
                int lastSlashIndex = link.LastIndexOf('/');
                if (lastSlashIndex == -1) hash = link;

                else if (lastSlashIndex == link.Length - 1) hash = string.Empty;
                else hash = link.Substring(lastSlashIndex + 1);
            }
            else throw new Exception("empty Element");

            return hash;
        }


    }
}
