using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZBS
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
    }
}
