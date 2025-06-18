using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z3n.MethodExtensions
{
    public static class DictionaryExtentions
    {
        public static string GetValue(this Dictionary<string, string> msgData, string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    return msgData[key].ToString();
                }
                catch { }
            }
            return string.Empty;
        }


    }
}
