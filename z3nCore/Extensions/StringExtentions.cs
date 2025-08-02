

using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
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

        public static string ToSepc256k1(this string seed, int path = 0)
        {
            var blockchain = new Blockchain();
            var mnemonicObj = new Mnemonic(seed);
            var hdRoot = mnemonicObj.DeriveExtKey();
            var derivationPath = new NBitcoin.KeyPath($"m/44'/60'/0'/0/{path}");
            var key = hdRoot.Derive(derivationPath).PrivateKey.ToHex();
            return key;
        }

        public static string ToEvmPrivateKey(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder hex = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    hex.AppendFormat("{0:x2}", b);
                }

                return hex.ToString();
            }
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

        public static Dictionary<string, string> ParseCreds(this string data, string format, char devider = ':')
        {
            var parsedData = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(data))
                return parsedData;

            string[] formatParts = format.Split(devider);
            string[] dataParts = data.Split(devider);

            for (int i = 0; i < formatParts.Length && i < dataParts.Length; i++)
            {
                string key = formatParts[i].Trim('{', '}').Trim();
                if (!string.IsNullOrEmpty(key))
                    parsedData[key] = dataParts[i].Trim();
            }
            return parsedData;
        }

        public static Dictionary<string, string> ParseByMask(this string input, string mask)
        {
            input = input?.Trim();
            mask = mask?.Trim();

            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(mask))
            {
                return new Dictionary<string, string>();
            }

            var variableNames = new List<string>();
            var regex = new Regex(@"\{([^{}]+)\}");
            foreach (Match mtch in regex.Matches(mask))
            {
                variableNames.Add(mtch.Groups[1].Value);
            }

            if (variableNames.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            string pattern = Regex.Escape(mask);
            foreach (var varName in variableNames)
            {
                string escapedVar = Regex.Escape("{" + varName + "}");
                pattern = pattern.Replace(escapedVar, "(.*?)");
            }
            pattern += "$";


            var match = Regex.Match(input, pattern);
            if (!match.Success)
            {
                return new Dictionary<string, string>();
            }

            var result = new Dictionary<string, string>();
            for (int i = 0; i < variableNames.Count; i++)
            {
                result[variableNames[i]] = match.Groups[i + 1].Value;
            }

            return result;
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

        public static string[] Range(this string accRange)
        {
            if (string.IsNullOrEmpty(accRange))  
                throw new Exception("range cannot be empty");
            if (accRange.Contains(","))
                return accRange.Split(',');
            else if (accRange.Contains("-"))
            {
                var rangeParts = accRange.Split('-').Select(int.Parse).ToArray();
                int rangeS = rangeParts[0];
                int rangeE = rangeParts[1];
                accRange = string.Join(",", Enumerable.Range(rangeS, rangeE - rangeS + 1));
                return accRange.Split(',');
            }
            else
            {
                int rangeS = 1;
                int rangeE = int.Parse(accRange);
                accRange = string.Join(",", Enumerable.Range(rangeS, rangeE - rangeS + 1));
                return accRange.Split(',');
            }
        }

        public static string KeyType(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new Exception($"input isNullOrEmpty");

            input = input.Trim().StartsWith("0x") ? input.Substring(2) : input;
            
            if (Regex.IsMatch(input, @"^[0-9a-fA-F]{64}$"))
                return "keyEvm";

            if (Regex.IsMatch(input, @"^[123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz]{87,88}$"))
                return "keySol";

            var words = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 12 || words.Length == 24)
                return "seed";
            
            throw new Exception ($"not recognized as any key or seed {input}");
        }

        public static string GetLink(this string text)
        {
            int startIndex = text.IndexOf("https://");
            if (startIndex == -1) startIndex = text.IndexOf("http://");
            if (startIndex == -1) throw new Exception($"No Link found in message {text}");

            string potentialLink = text.Substring(startIndex);
            int endIndex = potentialLink.IndexOfAny(new[] { ' ', '\n', '\r', '\t', '"' });
            if (endIndex != -1)
                potentialLink = potentialLink.Substring(0, endIndex);

            return Uri.TryCreate(potentialLink, UriKind.Absolute, out _)
                ? potentialLink
                : throw new Exception($"No Link found in message {text}");
        }

        public static string GetOTP(this string text)
        {
            Match match = Regex.Match(text, @"\b\d{6}\b");
            if (match.Success)
                return match.Value;
            else
                throw new Exception($"Fmail: OTP not found in [{text}]");
        }

        public static string CleanFilePath(this string text)
        {

            if (string.IsNullOrEmpty(text))
                return text;

            char[] invalidChars = Path.GetInvalidFileNameChars();

            string cleaned = text;
            foreach (char c in invalidChars)
            {
                cleaned = cleaned.Replace(c.ToString(), "");
            }
            return cleaned;

        }
    
    
    
    }
}
