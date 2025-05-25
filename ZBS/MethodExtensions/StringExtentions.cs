

using NBitcoin;
using Newtonsoft.Json;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
        private static string DetectKeyType(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (Regex.IsMatch(input, @"^[0-9a-fA-F]{64}$"))
                return "key";

            var words = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 12)
                return "seed";
            if (words.Length == 24)
                return "seed";

            return null;
        }
        public static string ToPubEvm(this string key)
        {
            string keyType = key.DetectKeyType();
            var blockchain = new Blockchain();

            if (keyType == "seed")
            {
                var mnemonicObj = new Mnemonic(key);
                var hdRoot = mnemonicObj.DeriveExtKey();
                var derivationPath = new NBitcoin.KeyPath("m/44'/60'/0'/0/0");
                key = hdRoot.Derive(derivationPath).PrivateKey.ToHex();

            }
            return blockchain.GetAddressFromPrivateKey(key);
        }

        public static string[] TxToString(this string txJson)
        {
            dynamic txData = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(txJson);

            string gas = $"{txData.gas}";
            string value = $"{txData.value}";
            string sender = $"{txData.from}";
            string recipient = $"{txData.to}";
            string data = $"{txData.data}";
           
            BigInteger gasWei = BigInteger.Parse("0" + gas.TrimStart('0', 'x'), NumberStyles.AllowHexSpecifier);
            decimal gasGwei = (decimal)gasWei / 1000000000m;
            string gwei = gasGwei.ToString().Replace(',','.');

            return new string[] { gas, value, sender, data, recipient, gwei };


        }




        public static bool ChkAddress(this string shortAddress, string fullAddress)
        {
            if (string.IsNullOrEmpty(shortAddress) || string.IsNullOrEmpty(fullAddress))
                return false;

            if (!shortAddress.Contains("…") || shortAddress.Count(c => c == '…') != 1)
                return false;

            var parts = shortAddress.Split('…');
            if (parts.Length != 2)
                return false;

            string prefix = parts[0];
            string suffix = parts[1];

            if (prefix.Length < 4 || suffix.Length < 2)
                return false;

            if (fullAddress.Length < prefix.Length + suffix.Length)
                return false;

            bool prefixMatch = fullAddress.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            bool suffixMatch = fullAddress.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);

            return prefixMatch && suffixMatch;
        }

        public static Dictionary<string, string> ParseCreds(this string data, string format)
        {
            var parsedData = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(data))
                return parsedData;

            string[] formatParts = format.Split(':');
            string[] dataParts = data.Split(':');

            for (int i = 0; i < formatParts.Length && i < dataParts.Length; i++)
            {
                string key = formatParts[i].Trim('{', '}').Trim();
                if (!string.IsNullOrEmpty(key))
                    parsedData[key] = dataParts[i].Trim();
            }
            return parsedData;
        }

        public static string StringToHex(this string value, string convert = "")
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return "0x0";

                value = value?.Trim();
                if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal number))
                    return "0x0";

                BigInteger result;
                switch (convert.ToLower())
                {
                    case "gwei":
                        result = (BigInteger)(number * 1000000000m);
                        break;
                    case "eth":
                        result = (BigInteger)(number * 1000000000000000000m);
                        break;
                    default:
                        result = (BigInteger)number;
                        break;
                }

                // Convert to hexadecimal and ensure it starts with 0x
                string hex = result.ToString("X").TrimStart('0');
                return string.IsNullOrEmpty(hex) ? "0x0" : "0x" + hex;
            }
            catch
            {
                return "0x0";
            }
        }

        public static string HexToString(this string hexValue, string convert = "")
        {
            try
            {
                hexValue = hexValue?.Replace("0x", "").Trim();
                if (string.IsNullOrEmpty(hexValue)) return "0";
                BigInteger number = BigInteger.Parse("0" + hexValue, NumberStyles.AllowHexSpecifier);
                switch (convert.ToLower())
                {
                    case "gwei":
                        decimal gweiValue = (decimal)number / 1000000000m;
                        return gweiValue.ToString("0.#########", CultureInfo.InvariantCulture);
                    case "eth":
                        decimal ethValue = (decimal)number / 1000000000000000000m;
                        return ethValue.ToString("0.##################", CultureInfo.InvariantCulture);
                    default:
                        return number.ToString();
                }
            }
            catch
            {
                return "0";
            }
        }



    }
}
