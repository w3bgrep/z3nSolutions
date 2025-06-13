using System;
using System.Collections.Generic;

using System.Linq;

using System.Net.Http;
using System.Net;

using System.Text;
using System.Text.RegularExpressions;

using System.Threading;
using Newtonsoft.Json;
using ZennoLab.InterfacesLibrary.Enums.Browser;

using System.Globalization;
using System.Runtime.CompilerServices;

using Leaf.xNet;

using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Security.Policy;

#region using
using System;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary;
using ZBSolutions;
using NBitcoin;
using Nethereum.Model;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;


using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Numerics;

using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Nethereum.Model;

using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Reflection;
using System.Security.Policy;
using ZBSolutions;

#endregion

namespace w3tools //by @w3bgrep
{

    public  static class TestStatic
    {

        public static string UnixToHuman(this IZennoPosterProjectModel project, string decodedResultExpire = null)
        {
            var _log = new Logger(project, classEmoji: "☻");
            if (string.IsNullOrEmpty(decodedResultExpire)) decodedResultExpire = project.Var("varSessionId");
            if (!string.IsNullOrEmpty(decodedResultExpire))
            {
                int intEpoch = int.Parse(decodedResultExpire);
                string converted = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(intEpoch).ToShortDateString();
                _log.Send(converted);
                return converted;

                
            }
            return string.Empty;
        }
        public static decimal Math(this IZennoPosterProjectModel project, string varA, string operation, string varB, string varRslt = "a_")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            decimal a = decimal.Parse(project.Var(varA));
            decimal b = decimal.Parse(project.Var(varB));
            decimal result;
            switch (operation)
            {
                case "+":

                    result = a + b;
                    break;
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    break;
                default:
                    throw new Exception($"unsuppoted operation {operation}");
            }
            try { project.Var(varRslt, $"{result}"); } catch { }
            return result;
        }
        public static string CookiesToJson(string cookies)
        {
            try
            {
                if (string.IsNullOrEmpty(cookies))
                {
                    return "[]";
                }

                var result = new List<Dictionary<string, string>>();
                var cookiePairs = cookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in cookiePairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                        continue;

                    var keyValue = trimmedPair.Split(new[] { '=' }, 2);
                    if (keyValue.Length != 2)
                    {
                        continue;
                    }

                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        result.Add(new Dictionary<string, string>
                    {
                        { "name", key },
                        { "value", value }
                    });
                    }
                }

                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                return json;
            }
            catch (Exception ex)
            {
                return "[]";
            }
        }

    }

    public class Balance : W3b
    {
        public readonly string _defRpc;


        public Balance(IZennoPosterProjectModel project, bool log = false, string adrEvm = null, string key = null)
        : base(project, log)
        {
            if (string.IsNullOrEmpty(adrEvm) && (!string.IsNullOrEmpty(_acc0)))
            {
                _adrEvm = ApplyKey(key).ToPubEvm();
            }
            _defRpc = project.Variables["blockchainRPC"].Value;

        }

        private string ChekAdr(string address)
        {
            if (string.IsNullOrEmpty(address)) address = _adrEvm;
            if (string.IsNullOrEmpty(address)) throw new ArgumentException("!W address is nullOrEmpty");
            return address;
        }

        //evm
        public T ChainId<T>(string rpc = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            string jsonBody = @"{""jsonrpc"": ""2.0"",""method"": ""eth_chainId"",""params"": [],""id"": 1}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexResult = json["result"]?.ToString() ?? "0x0";
            if (hexResult == "0x0")
                return (T)Convert.ChangeType("0", typeof(T));

            int chainId = Convert.ToInt32(hexResult.TrimStart('0', 'x'), 16);

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(chainId.ToString(), typeof(T));

            return (T)Convert.ChangeType(chainId, typeof(T));
        }
        public T GasPrice<T>(string rpc = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            string jsonBody = @"{""jsonrpc"":""2.0"",""method"":""eth_gasPrice"",""params"":[],""id"":1}";
            string response;

            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    try
                    {
                        string[] proxyArray = proxy.Split(':');
                        string username = proxyArray[1];
                        string password = proxyArray[2];
                        string host = proxyArray[3];
                        int port = int.Parse(proxyArray[4]);
                        request.Proxy = new HttpProxyClient(host, port, username, password);
                    }
                    catch (Exception ex)
                    {
                        _project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
                        throw;
                    }
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexResultGas = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
            BigInteger gasWei = BigInteger.Parse("0" + hexResultGas, NumberStyles.AllowHexSpecifier);
            decimal gasGwei = (decimal)gasWei / 1000000000m;

            Log(rpc, gasGwei.ToString(), "", log: log);
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(gasGwei.ToString("0.######", CultureInfo.InvariantCulture), typeof(T));
            return (T)Convert.ChangeType(gasGwei, typeof(T));
        }
        public T NonceEVM<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            address = ChekAdr(address);
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            string jsonBody = $@"{{""jsonrpc"": ""2.0"",""method"": ""eth_getTransactionCount"",""params"": [""{address}"", ""latest""],""id"": 1}}";
            string response;

            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
            string hexResultNonce = match.Success ? match.Groups[1].Value : "0x0";

            if (hexResultNonce == "0x0")
                return (T)Convert.ChangeType("0", typeof(T));

            int transactionCount = Convert.ToInt32(hexResultNonce.TrimStart('0', 'x'), 16);
            if (log) Log($"{address} nonce now {transactionCount}");
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(transactionCount.ToString(), typeof(T));
            return (T)Convert.ChangeType(transactionCount, typeof(T));
        }

        public T ERC20<T>(string tokenContract, string rpc = null, string address = null, string tokenDecimal = "18", string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            address = ChekAdr(address);
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            string data = "0x70a08231000000000000000000000000" + address.Replace("0x", "");
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexBalance = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
            BigInteger balanceWei = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);
            decimal decimals = (decimal)Math.Pow(10, double.Parse(tokenDecimal));
            decimal balance = (decimal)balanceWei / decimals;

            string balanceString = FloorDecimal<string>(balance, int.Parse(tokenDecimal));
            Log(address, balanceString, rpc, tokenContract, log: log);
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));

        }
        public T ERC721<T>(string tokenContract, string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            address = ChekAdr(address);
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            string functionSelector = "0x70a08231";
            string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');
            string data = functionSelector + paddedAddress;
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexBalance = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
            BigInteger balance = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);
            Log(address, balance.ToString(), rpc, tokenContract, log: log);

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(balance.ToString(), typeof(T));

            return (T)Convert.ChangeType(balance, typeof(T));
        }




        public T ERC1155<T>(string tokenContract, string tokenId, string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            address = ChekAdr(address);
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            string functionSelector = "0x00fdd58e";
            string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');
            string paddedTokenId = BigInteger.Parse(tokenId).ToString("x").PadLeft(64, '0');
            string data = functionSelector + paddedAddress + paddedTokenId;
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexBalance = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
            BigInteger balance = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);
            Log(address, balance.ToString(), rpc, $"[{tokenContract}:id({tokenId})]", log: log);

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(balance.ToString(), typeof(T));
            else if (typeof(T) == typeof(int))
                return (T)(object)(int)balance;
            else if (typeof(T) == typeof(BigInteger))
                return (T)(object)balance;
            else
                throw new InvalidOperationException($"!W unsupported type {typeof(T)}");
        }

        public T SPL<T>(string tokenMint, string address = null, int floor = 0, string rpc = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _sql.Address("sol");
            if (string.IsNullOrEmpty(rpc)) rpc = "https://api.mainnet-beta.solana.com";

            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getTokenAccountsByOwner"", ""params"": [""{address}"", {{""mint"": ""{tokenMint}""}}, {{""encoding"": ""jsonParsed""}}], ""id"": 1 }}";
            string response;

            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }
            var json = JObject.Parse(response);
            var tokenAccounts = json["result"]?["value"] as JArray;
            string lamports = tokenAccounts != null && tokenAccounts.Count > 0
                ? tokenAccounts[0]?["account"]?["data"]?["parsed"]?["info"]?["tokenAmount"]?["amount"]?.ToString() ?? "0"
                : "0";

            int decimals = tokenAccounts != null && tokenAccounts.Count > 0
                ? int.Parse(tokenAccounts[0]?["account"]?["data"]?["parsed"]?["info"]?["tokenAmount"]?["decimals"]?.ToString() ?? "0")
                : 0;
            decimal balance = decimal.Parse(lamports) / (decimal)Math.Pow(10, decimals);

            string balanceString = FloorDecimal<string>(balance, decimals);
            Log(address, balanceString, rpc, tokenMint);

            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));

        }
        public T SUIt<T>(string coinType, string address = null, string rpc = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address))
            {
                string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";
                address = _sql.DbQ($"SELECT sui FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            }
            if (string.IsNullOrEmpty(rpc)) rpc = "https://fullnode.mainnet.sui.io";

            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""suix_getBalance"", ""params"": [""{address}"", ""{coinType}""], ""id"": 1 }}";
            string response;

            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string mist = json["result"]?["totalBalance"]?.ToString() ?? "0";
            decimal balance = decimal.Parse(mist) / 1000000m;
            if (log) Log($"{address}: {balance} TOKEN ({coinType})");


            if (typeof(T) == typeof(string)) return FloorDecimal<T>(balance, int.Parse(mist));
            return (T)Convert.ChangeType(balance, typeof(T));

        }
        public T APTt<T>(string coinType, string address = null, string rpc = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address))
            {
                string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";
                address = _sql.DbQ($"SELECT apt FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            }
            if (string.IsNullOrEmpty(rpc)) rpc = "https://fullnode.mainnet.aptoslabs.com/v1";

            string url = $"{rpc}/accounts/{address}/resource/0x1::coin::CoinStore<{coinType}>";
            string response;

            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Get(url);
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string octas = json["data"]?["coin"]?["value"]?.ToString() ?? "0";
            decimal balance = decimal.Parse(octas) / 1000000m; // Предполагаем 6 decimals, как для USDC
            if (log) Log($"{address}: {balance} TOKEN ({coinType})");
            if (typeof(T) == typeof(string)) return FloorDecimal<T>(balance, int.Parse(octas));
            return (T)Convert.ChangeType(balance, typeof(T));
        }
        public T INITt<T>(string address = null, string chain = "interwoven-1", string token = "uinit", bool log = false)
        {
            if (string.IsNullOrEmpty(address))
                try
                {
                    address = _project.Variables["addressInitia"].Value;
                }
                catch
                {
                    Log("no Address provided");
                    throw;
                }



            string url = $"https://celatone-api-prod.alleslabs.dev/v1/initia/{chain}/accounts/{address}/balances";

            string jsonString = _project.GET(url);

            _project.L0g(jsonString, show: log);
            _project.Json.FromString(jsonString);
            try
            {
                JArray balances = JArray.Parse(jsonString);
                List<string> balanceList = new List<string>();
                foreach (JObject balance in balances)
                {
                    string denom = balance["denom"].ToString();
                    string amount = balance["amount"].ToString();
                    if (double.TryParse(amount, out double amountValue))
                    {
                        double amountInMillions = amountValue / 1000000;
                        balanceList.Add($"{denom}:{amountInMillions.ToString("0.########", CultureInfo.InvariantCulture)}");
                    }
                    else
                    {
                        balanceList.Add($"{denom}:{amount}");
                    }
                }
                string balanceToken = balanceList.FirstOrDefault(entry => entry.StartsWith(token + ":"))?.Split(':')[1] ?? "";
                if (typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(balanceToken, typeof(T));
                else if (double.TryParse(balanceToken, NumberStyles.Float, CultureInfo.InvariantCulture, out double balanceValue))
                    return (T)Convert.ChangeType(balanceValue, typeof(T));
                else
                    return default(T);

            }
            catch (Exception ex)
            {
                _project.L0g(ex.Message);
                return default(T);
            }

        }

        public Dictionary<string, decimal> DicToken(string[] chainsToUse = null, bool log = false, string tokenEVM = null, string tokenSPL = null) //usde fallback
        {
            if (chainsToUse == null) chainsToUse = _project.Var("cfgChains").Split(',');
            if (tokenEVM == null) tokenEVM = "0x5d3a1Ff2b6BAb83b63cd9AD0787074081a52ef34";//usde fallback
            if (tokenSPL == null) tokenSPL = "DEkqHyPN7GMRJ5cArtQFAWefqbZb33Hyf6s5iCwjEonT";//usde fallback

            var blsUsde = new Dictionary<string, decimal>();
            var _w3b = new W3bRead(_project, log);
            foreach (string chain in chainsToUse)
            {
                decimal bal;
                if (!chain.Contains("solana"))
                    bal = _w3b.BalERC20<decimal>(tokenEVM, _w3b.Rpc(chain));
                else
                    bal = _w3b.TokenSPL<decimal>(tokenSPL);
                blsUsde.Add(chain, bal);
            }
            return blsUsde;
        }

        public List<BigInteger> ERC721TokenIds(string tokenContract, string rpc, string address, string proxy =null, bool log= false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            address = ChekAdr(address);
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            // Проверка поддержки интерфейса ERC721Enumerable (0x780e9d63)
            string supportsInterfaceSelector = "0x01ffc9a7";
            string interfaceId = "780e9d63"; // ERC721Enumerable interface ID
            string supportsInterfaceData = supportsInterfaceSelector + interfaceId.PadLeft(64, '0');
            string supportsInterfaceJsonBody = "{\"jsonrpc\":\"2.0\",\"method\":\"eth_call\",\"params\":[{\"to\":\"" + tokenContract + "\",\"data\":\"" + supportsInterfaceData + "\"},\"latest\"],\"id\":1}";

            string supportsInterfaceResponse;
            using (HttpRequest request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1];
                    string password = proxyArray[2];
                    string host = proxyArray[3];
                    int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, supportsInterfaceJsonBody, "application/json");
                    supportsInterfaceResponse = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog("Err HTTPreq: " + ex.Message + ", Status: " + ex.Status);
                    throw;
                }
            }

            JObject supportsInterfaceJson = JObject.Parse(supportsInterfaceResponse);
            string hexSupportsInterface = supportsInterfaceJson["result"] != null ? supportsInterfaceJson["result"].ToString().TrimStart('0', 'x') : "0";
            bool supportsEnumerable = hexSupportsInterface != "0" && BigInteger.Parse("0" + hexSupportsInterface, NumberStyles.AllowHexSpecifier) != BigInteger.Zero;

            if (!supportsEnumerable)
            {
                _project.SendErrorToLog("Контракт не поддерживает ERC721Enumerable");
                return new List<BigInteger>(); // Возвращаем пустой список, если интерфейс не поддерживается
            }

            // Шаг 1: Получаем баланс токенов для адреса
            string balanceFunctionSelector = "0x70a08231";
            string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');
            string balanceData = balanceFunctionSelector + paddedAddress;
            string balanceJsonBody = "{\"jsonrpc\":\"2.0\",\"method\":\"eth_call\",\"params\":[{\"to\":\"" + tokenContract + "\",\"data\":\"" + balanceData + "\"},\"latest\"],\"id\":1}";

            string balanceResponse;
            using (HttpRequest request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1];
                    string password = proxyArray[2];
                    string host = proxyArray[3];
                    int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, balanceJsonBody, "application/json");
                    balanceResponse = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog("Err HTTPreq: " + ex.Message + ", Status: " + ex.Status);
                    throw;
                }
            }

            JObject balanceJson = JObject.Parse(balanceResponse);
            string hexBalance = balanceJson["result"] != null ? balanceJson["result"].ToString().TrimStart('0', 'x') : "0";
            BigInteger balance = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);

            // Шаг 2: Получаем ID токенов через tokenOfOwnerByIndex
            List<BigInteger> tokenIds = new List<BigInteger>();
            string tokenIdFunctionSelector = "0x4f6ccce7"; // Правильный селектор для tokenOfOwnerByIndex(address, uint256)

            for (BigInteger i = BigInteger.Zero; i < balance; i = i + BigInteger.One)
            {
                string paddedIndex = i.ToString("x").PadLeft(64, '0');
                string tokenIdData = tokenIdFunctionSelector + paddedAddress + paddedIndex;
                string tokenIdJsonBody = "{\"jsonrpc\":\"2.0\",\"method\":\"eth_call\",\"params\":[{\"to\":\"" + tokenContract + "\",\"data\":\"" + tokenIdData + "\"},\"latest\"],\"id\":1}";

                string tokenIdResponse;
                using (HttpRequest request = new HttpRequest())
                {
                    request.UserAgent = "Mozilla/5.0";
                    request.IgnoreProtocolErrors = true;
                    request.ConnectTimeout = 5000;

                    if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                    if (!string.IsNullOrEmpty(proxy))
                    {
                        string[] proxyArray = proxy.Split(':');
                        string username = proxyArray[1];
                        string password = proxyArray[2];
                        string host = proxyArray[3];
                        int port = int.Parse(proxyArray[4]);
                        request.Proxy = new HttpProxyClient(host, port, username, password);
                    }

                    try
                    {
                        HttpResponse httpResponse = request.Post(rpc, tokenIdJsonBody, "application/json");
                        tokenIdResponse = httpResponse.ToString();
                    }
                    catch (HttpException ex)
                    {
                        _project.SendErrorToLog("Err HTTPreq: " + ex.Message + ", Status: " + ex.Status);
                        throw;
                    }
                }

                JObject tokenIdJson = JObject.Parse(tokenIdResponse);
                string hexTokenId = tokenIdJson["result"] != null ? tokenIdJson["result"].ToString().TrimStart('0', 'x') : "0";
                BigInteger tokenId = BigInteger.Parse("0" + hexTokenId, NumberStyles.AllowHexSpecifier);
                tokenIds.Add(tokenId);
            }

            // Шаг 3: Логируем и возвращаем список ID токенов
            //Log(address, string.Join(", ", tokenIds), rpc, tokenContract, log);
            return tokenIds;
        }

        public List<BigInteger> ERC721TokenIds2(string tokenContract, string rpc, string address, string proxy = null, bool log = false, long startBlock = 21219959)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            address = ChekAdr(address);
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            // Событие Transfer(address,address,uint256)
            string transferTopic = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
            string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');

            // Храним токены, полученные и отправленные
            Dictionary<BigInteger, bool> tokenOwnership = new Dictionary<BigInteger, bool>(); // true = владеет, false = не владеет

            // Получаем текущий блок, чтобы ограничить диапазон
            string getBlockNumberJsonBody = "{\"jsonrpc\":\"2.0\",\"method\":\"eth_blockNumber\",\"params\":[],\"id\":1}";
            string blockNumberResponse;
            using (HttpRequest request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1];
                    string password = proxyArray[2];
                    string host = proxyArray[3];
                    int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(rpc, getBlockNumberJsonBody, "application/json");
                    blockNumberResponse = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog("Err HTTPreq: " + ex.Message + ", Status: " + ex.Status);
                    throw;
                }
            }

            JObject blockNumberJson = JObject.Parse(blockNumberResponse);
            string hexBlockNumber = blockNumberJson["result"] != null ? blockNumberJson["result"].ToString().TrimStart('0', 'x') : "0";
            long latestBlock = (long)BigInteger.Parse("0" + hexBlockNumber, NumberStyles.AllowHexSpecifier);

            // Итеративно запрашиваем логи с шагом, чтобы уложиться в лимиты RPC
            long blockStep = 10000; // Размер шага (может быть меньше, зависит от RPC)
            for (long fromBlock = startBlock; fromBlock <= latestBlock; fromBlock += blockStep)
            {
                long toBlock = Math.Min(fromBlock + blockStep - 1, latestBlock);
                string fromBlockHex = "0x" + fromBlock.ToString("x");
                string toBlockHex = "0x" + toBlock.ToString("x");

                // Запрашиваем события Transfer, где to = address
                string getLogsJsonBody = "{\"jsonrpc\":\"2.0\",\"method\":\"eth_getLogs\",\"params\":[{\"fromBlock\":\"" + fromBlockHex + "\",\"toBlock\":\"" + toBlockHex + "\",\"address\":\"" + tokenContract + "\",\"topics\":[\"" + transferTopic + "\",null,\"0x" + paddedAddress + "\"]}],\"id\":1}";
                string logsResponse;
                using (HttpRequest request = new HttpRequest())
                {
                    request.UserAgent = "Mozilla/5.0";
                    request.IgnoreProtocolErrors = true;
                    request.ConnectTimeout = 5000;

                    if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                    if (!string.IsNullOrEmpty(proxy))
                    {
                        string[] proxyArray = proxy.Split(':');
                        string username = proxyArray[1];
                        string password = proxyArray[2];
                        string host = proxyArray[3];
                        int port = int.Parse(proxyArray[4]);
                        request.Proxy = new HttpProxyClient(host, port, username, password);
                    }

                    try
                    {
                        HttpResponse httpResponse = request.Post(rpc, getLogsJsonBody, "application/json");
                        logsResponse = httpResponse.ToString();
                    }
                    catch (HttpException ex)
                    {
                        _project.SendErrorToLog("Err HTTPreq: " + ex.Message + ", Status: " + ex.Status);
                        throw;
                    }
                }

                JObject logsJson = JObject.Parse(logsResponse);
                JArray logs = logsJson["result"] != null ? (JArray)logsJson["result"] : new JArray();
                foreach (JObject logg in logs)
                {
                    string tokenIdHex = logg["data"] != null ? logg["data"].ToString().TrimStart('0', 'x') : "0";
                    BigInteger tokenId = BigInteger.Parse("0" + tokenIdHex, NumberStyles.AllowHexSpecifier);
                    tokenOwnership[tokenId] = true; // Получен
                }

                // Запрашиваем события Transfer, где from = address
                getLogsJsonBody = "{\"jsonrpc\":\"2.0\",\"method\":\"eth_getLogs\",\"params\":[{\"fromBlock\":\"" + fromBlockHex + "\",\"toBlock\":\"" + toBlockHex + "\",\"address\":\"" + tokenContract + "\",\"topics\":[\"" + transferTopic + "\",\"0x" + paddedAddress + "\",null]}],\"id\":1}";
                using (HttpRequest request = new HttpRequest())
                {
                    request.UserAgent = "Mozilla/5.0";
                    request.IgnoreProtocolErrors = true;
                    request.ConnectTimeout = 5000;

                    if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                    if (!string.IsNullOrEmpty(proxy))
                    {
                        string[] proxyArray = proxy.Split(':');
                        string username = proxyArray[1];
                        string password = proxyArray[2];
                        string host = proxyArray[3];
                        int port = int.Parse(proxyArray[4]);
                        request.Proxy = new HttpProxyClient(host, port, username, password);
                    }

                    try
                    {
                        HttpResponse httpResponse = request.Post(rpc, getLogsJsonBody, "application/json");
                        logsResponse = httpResponse.ToString();
                    }
                    catch (HttpException ex)
                    {
                        _project.SendErrorToLog("Err HTTPreq: " + ex.Message + ", Status: " + ex.Status);
                        throw;
                    }
                }

                logsJson = JObject.Parse(logsResponse);
                logs = logsJson["result"] != null ? (JArray)logsJson["result"] : new JArray();
                foreach (JObject logg in logs)
                {
                    string tokenIdHex = logg["data"] != null ? logg["data"].ToString().TrimStart('0', 'x') : "0";
                    BigInteger tokenId = BigInteger.Parse("0" + tokenIdHex, NumberStyles.AllowHexSpecifier);
                    tokenOwnership[tokenId] = false; // Отправлен
                }
            }

            // Собираем токены, которыми адрес владеет
            List<BigInteger> tokenIds = new List<BigInteger>();
            foreach (KeyValuePair<BigInteger, bool> entry in tokenOwnership)
            {
                if (entry.Value)
                {
                    tokenIds.Add(entry.Key);
                }
            }

            // Логируем и возвращаем
            //Log(address, string.Join(", ", tokenIds), rpc, tokenContract, log);
            return tokenIds;
        }


    }


    public class Starter2
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;


        public Starter2(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project, true);
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _instance = instance;
        }
        public Starter2(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _sql = new Sql(_project,true);
        }
        public void StartBrowser(bool strictProxy = true)
        {
            if (string.IsNullOrEmpty(_project.Var("acc0")))
               throw new Exception("!EmptyVar: acc0");
            
            _project.Variables["instancePort"].Value = _instance.Port.ToString();
            _logger.Send($"init browser in port: {_instance.Port}");

            string webGlData = _sql.Get("webgl", "private_profile");
            _instance.SetDisplay(webGlData, _project);

            bool goodProxy = new NetHttp(_project, true).ProxySet(_instance);
            if (strictProxy && !goodProxy) throw new Exception($"!E bad proxy");

            string cookiePath = $"{_project.Variables["profiles_folder"].Value}accounts\\cookies\\{_project.Variables["acc0"].Value}.json";
            _project.Variables["pathCookies"].Value = cookiePath;

            try
            {
                string cookies = File.ReadAllText(cookiePath);
                _instance.SetCookie(cookies);
            }
            catch
            {
                _logger.Send($"!W Fail to set cookies from file {cookiePath}");
                try
                {
                    string cookies = _sql.Get("cookies", "private_profile");
                    _instance.SetCookie(cookies);
                }
                catch (Exception Ex)
                {
                    _logger.Send($"!E Fail to set cookies from db Err. {Ex.Message}");
                }

            }
            if (_project.Var("skipBrowserScan") != "True")
            {
                var bs = new BrowserScan(_project, _instance);
                if (bs.GetScore().Contains("timezone")) bs.FixTime();
            }

        }
        public void InitVariables(string author = "")
        {
            new Sys(_project).DisableLogs();

            string sessionId = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();
            string projectName = _project.ExecuteMacro(_project.Name).Split('.')[0];
            string version = Assembly.GetExecutingAssembly()
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
               ?.InformationalVersion ?? "Unknown";
            string dllTitle = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyTitleAttribute>()
                ?.Title ?? "Unknown";


            _project.Variables["projectName"].Value = projectName;
            _project.Variables["varSessionId"].Value = sessionId;
            try { _project.Variables["nameSpace"].Value = dllTitle; } catch { }

            string[] vars = { "cfgPin", "DBsqltPath" };
            CheckVars(vars);

            _project.Variables["projectTable"].Value = "projects_" + projectName;

            _project.Range();
            SAFU.Initialize(_project);
            Logo(author, dllTitle);

        }
        private void Logo(string author, string dllTitle)
        {
            string version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";

            string name = _project.ExecuteMacro(_project.Name).Split('.')[0];
            if (author != "") author = $" script author: @{author}";
            string logo = $@"using {dllTitle} v{version};
            ┌by─┐					
            │    w3bgrep			
            └─→┘
                        ► init {name} ░▒▓█  {author}";
            _project.SendInfoToLog(logo, true);
        }
        private void CheckVars(string[] vars)
        {
            foreach (string var in vars)
            {
                try
                {
                    if (string.IsNullOrEmpty(_project.Variables[var].Value))
                    {
                        throw new Exception($"!E {var} is null or empty");
                    }
                }
                catch (Exception ex)
                {
                    _project.L0g(ex.Message);
                    throw;
                }
            }
        }
        public bool ChooseSingleAcc()
        {
            var listAccounts = _project.Lists["accs"];

        check:
            if (listAccounts.Count == 0)
            {
                _project.Variables["noAccsToDo"].Value = "True";
                _project.SendToLog($"♻ noAccoutsAvaliable", LogType.Info, true, LogColor.Turquoise);
                _project.Variables["acc0"].Value = "";
                return false;
                throw new Exception($"TimeToChill");
            }

            int randomAccount = new Random().Next(0, listAccounts.Count);
            _project.Variables["acc0"].Value = listAccounts[randomAccount];
            listAccounts.RemoveAt(randomAccount);
            if (!_project.GlobalSet())
                goto check;
            _project.Var("pathProfileFolder", $"{_project.Var("profiles_folder")}accounts\\profilesFolder\\{_project.Var("acc0")}");
            _project.L0g($"`working with: [acc{_project.Var("acc0")}] accs left: [{listAccounts.Count}]");
            return true;

        }

    }



    public static class HanaGarden
    {
        private static readonly string GRAPHQL_URL = "https://hanafuda-backend-app-520478841386.us-central1.run.app/graphql";
        private static readonly string API_KEY = "AIzaSyDipzN0VRfTPnMGhQ5PSzO27Cxm3DohJGY";

        private static string ExecuteGraphQLQuery(IZennoPosterProjectModel project, string query, string variables = null)
        {
            // Получаем токен и проверяем его
            string token = project.Variables["TOKEN_CURRENT"].Value.Trim();

            if (string.IsNullOrEmpty(token))
            {
                project.SendErrorToLog("Token is empty or null");
                return null;
            }

            // Форматируем заголовки, убедившись что токен передается корректно
            string[] headers = new string[] {
                "Content-Type: application/json",
                $"Authorization: Bearer {token.Trim()}"
            };

            // Форматируем GraphQL запрос, удаляя лишние пробелы и табуляции
            query = query.Replace("\t", "").Replace("\n", " ").Replace("\r", "").Trim();

            //string jsonBody = JsonConvert.SerializeObject(new { query = query });
            string jsonBody;
            if (variables != null)
            {
                jsonBody = JsonConvert.SerializeObject(new { query = query, variables = JsonConvert.DeserializeObject(variables) });
            }
            else
            {
                jsonBody = JsonConvert.SerializeObject(new { query = query });
            }




            try
            {
                string response = ZennoPoster.HttpPost(
                    GRAPHQL_URL,
                    Encoding.UTF8.GetBytes(jsonBody),
                    "application/json",
                    project.Variables["proxy"].Value,
                    "UTF-8",
                    ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                    30000,
                    "",
                    "HANA/v1",
                    true,
                    5,
                    headers,
                    "",
                    true
                );

                return response;
            }
            catch (Exception ex)
            {
                project.SendErrorToLog($"GraphQL request failed: {ex.Message}");
                return null;
            }
        }
        public static string RefreshToken(IZennoPosterProjectModel project, string currentToken)
        {
            string url = $"https://securetoken.googleapis.com/v1/token?key={API_KEY}";

            string jsonBody = JsonConvert.SerializeObject(new
            {
                grant_type = "refresh_token",
                refresh_token = currentToken
            });


            string[] headers = new string[] {
                "Content-Type: application/json"
            };

            try
            {
                string response = ZennoPoster.HttpPost(
                    url,
                    Encoding.UTF8.GetBytes(jsonBody),
                    "application/json",
                    project.Variables["proxy"].Value,
                    "UTF-8",
                    ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                    30000,
                    "",
                    "Firebase/v1",
                    true,
                    5,
                    headers,
                    "",
                    true
                );


                dynamic tokenData = JObject.Parse(response);
                string newToken = tokenData.access_token;

                // Сохраняем новый токен в переменную проекта
                project.Variables["TOKEN_CURRENT"].Value = newToken;

                return newToken;
            }
            catch (Exception ex)
            {
                project.SendErrorToLog($"Failed to refresh token: {ex.Message}");
                return null;
            }
        }




        private static dynamic GetUserInfo(IZennoPosterProjectModel project)
        {
            string query = @"
			query CurrentUser {
				currentUser {
					id
					sub
					name
					totalPoint
					evmAddress {
						userId
						address
					}
				}
			}";

            string response = ExecuteGraphQLQuery(project, query);
            return JObject.Parse(response);
        }// Получение информации о картах пользователя
        public static string GetUserYakuInfo(IZennoPosterProjectModel project)
        {
            string query = @"
			query GetYakuList {
				getYakuListForCurrentUser {
					cardId
					group
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }
        public static string GetUserYakuInfo2(IZennoPosterProjectModel project)
        {
            string query = @"
			query GetMasterData {
				masterData {
					yaku {
					cardId
					group
					}
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        // Получение информации о саде
        public static string GetGardenInfo(IZennoPosterProjectModel project)
        {
            project.SendInfoToLog("Getting garden info...");
            string query = @"
			query GetGardenForCurrentUser {
				getGardenForCurrentUser {
					id
					inviteCode
					gardenDepositCount
					gardenStatus {
						id
						activeEpoch
						growActionCount
						gardenRewardActionCount
					}
					gardenMembers {
						id
						sub
						name
						iconPath
						depositCount
					}
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        public static void ProcessGarden(IZennoPosterProjectModel project)
        {
            try
            {
                // Получаем и обновляем токен
                string currentToken = project.Variables["TOKEN_CURRENT"].Value;
                project.SendInfoToLog($"Initial token: {currentToken}");

                string refreshedToken = RefreshToken(project, currentToken);
                if (string.IsNullOrEmpty(refreshedToken))
                {
                    project.SendErrorToLog("Failed to refresh token");
                    return;
                }

                project.SendInfoToLog($"Successfully refreshed token: {refreshedToken}");

                // Получаем информацию о саде
                project.SendInfoToLog("Getting garden info...");
                string gardenResponse = ExecuteGraphQLQuery(project, @"
					query GetGardenForCurrentUser {
						getGardenForCurrentUser {
							id
							inviteCode
							gardenDepositCount
							gardenStatus {
								id
								activeEpoch
								growActionCount
								gardenRewardActionCount
							}
							gardenMembers {
								id
								sub
								name
								iconPath
								depositCount
							}
						}
					}");

                project.SendInfoToLog($"Garden response received: {gardenResponse.Substring(0, Math.Min(100, gardenResponse.Length))}...");

                if (string.IsNullOrEmpty(gardenResponse))
                {
                    project.SendErrorToLog("Garden response is empty!");
                    return;
                }

                dynamic gardenData = JObject.Parse(gardenResponse);

                if (gardenData.data == null || gardenData.data.getGardenForCurrentUser == null)
                {
                    project.SendErrorToLog($"Invalid garden data structure: {gardenResponse}");
                    return;
                }

                dynamic gardenStatus = gardenData.data.getGardenForCurrentUser.gardenStatus;
                dynamic gardenMembers = gardenData.data.getGardenForCurrentUser.gardenMembers;

                // Проверяем наличие необходимых данных
                if (gardenStatus == null)
                {
                    project.SendErrorToLog("Garden status is null!");
                    return;
                }

                int totalGrows = (int)gardenStatus.growActionCount;
                int totalRewards = (int)gardenStatus.gardenRewardActionCount;

                project.SendInfoToLog($"Found actions - Grows: {totalGrows}, Rewards: {totalRewards}");

                string accountName = "Unknown";
                string accountId = "Unknown";

                if (gardenMembers != null && gardenMembers.Count > 0)
                {
                    accountName = gardenMembers[0].name;
                    accountId = gardenMembers[0].id;
                }

                project.SendInfoToLog($"Processing account: {accountName} (ID: {accountId})");



                //grow
                string growQuery = @"
				mutation {
					executeGrowAction(withAll: true) {
						baseValue
						leveragedValue
						totalValue
						multiplyRate
						limit
					}
				}";

                project.SendInfoToLog($"Executing grow all action");
                string growResponse = ExecuteGraphQLQuery(project, growQuery);
                project.SendInfoToLog($"Grow response: {growResponse}");

                dynamic growData = JObject.Parse(growResponse);
                if (growData.data != null && growData.data.executeGrowAction != null)
                {
                    var result = growData.data.executeGrowAction;
                    project.SendInfoToLog($"Grow results: Base={result.baseValue}, " +
                                        $"Leveraged={result.leveragedValue}, " +
                                        $"Total={result.totalValue}, " +
                                        $"Rate={result.multiplyRate}, " +
                                        $"Limit={result.limit}");
                }


                // Получаем обновленные очки
                string userInfoResponse = ExecuteGraphQLQuery(project, @"
					query CurrentUser {
						currentUser {
							totalPoint
						}
					}");

                dynamic userInfo = JObject.Parse(userInfoResponse);
                int totalPoints = (int)userInfo.data.currentUser.totalPoint;

                project.SendInfoToLog($"Grow action completed. Current Total Points: {totalPoints}");

                int delay = new Random().Next(1000, 5000);
                project.SendInfoToLog($"Waiting for {delay}ms before next action");
                Thread.Sleep(delay);


                // Получение наград
                if (totalRewards > 0)
                {
                    project.SendInfoToLog($"Starting reward collection. Total rewards: {totalRewards}");

                    string rewardQuery = @"
					mutation executeGardenRewardAction($limit: Int!) {
						executeGardenRewardAction(limit: $limit) {
							data { cardId, group }
							isNew
						}
					}";

                    int steps = (int)Math.Ceiling(totalRewards / 10.0);
                    project.SendInfoToLog($"Will process rewards in {steps} steps");

                    for (int i = 0; i < steps; i++)
                    {
                        try
                        {
                            project.SendInfoToLog($"Processing rewards step {i + 1} of {steps}");
                            string variables = @"{""limit"": 10}";
                            string rewardResponse = ExecuteGraphQLQuery(project, rewardQuery, variables);
                            project.SendInfoToLog($"Reward response: {rewardResponse}");

                            dynamic rewardData = JObject.Parse(rewardResponse);

                            foreach (var reward in rewardData.data.executeGardenRewardAction)
                            {
                                if ((bool)reward.isNew)
                                {
                                    project.SendInfoToLog($"New card received: ID {reward.data.cardId}, Group: {reward.data.group}");
                                }
                            }

                            delay = new Random().Next(1000, 5000);
                            project.SendInfoToLog($"Waiting for {delay}ms before next reward collection");
                            Thread.Sleep(delay);
                        }
                        catch (Exception ex)
                        {
                            project.SendErrorToLog($"Error during reward collection: {ex.Message}\nStack trace: {ex.StackTrace}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                project.SendErrorToLog($"Major error in garden processing: {ex.Message}\nStack trace: {ex.StackTrace}");
            }
        }


        // Выполнение всех доступных действий роста
        public static string ExecuteGrowAll(IZennoPosterProjectModel project)
        {
            string query = @"
			mutation {
				executeGrowAction(withAll: true) {
					baseValue
					leveragedValue
					totalValue
					multiplyRate
					limit
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        // Получение текущих очков пользователя
        public static string GetUserPoints(IZennoPosterProjectModel project)
        {
            string query = @"
			query CurrentUser {
				currentUser {
					totalPoint
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        // Получение наград с указанным лимитом
        public static string CollectRewards(IZennoPosterProjectModel project, int limit)
        {
            string query = @"
			mutation executeGardenRewardAction($limit: Int!) {
				executeGardenRewardAction(limit: $limit) {
					data { 
						cardId
						group 
					}
					isNew
				}
			}";

            string variables = $"{{\"limit\": {limit}}}";
            return ExecuteGraphQLQuery(project, query, variables);
        }








    }
    public static class HanaAPI
    {
        private static readonly string GRAPHQL_URL = "https://hanafuda-backend-app-520478841386.us-central1.run.app/graphql";

        public static string GetSchemaInfo(IZennoPosterProjectModel project)
        {
            string introspectionQuery = @"
			query {
				__schema {
					types {
						name
						fields {
							name
							type {
								name
								kind
							}
						}
					}
					mutationType {
						fields {
							name
							type {
								name
							}
							args {
								name
								type {
									name
								}
							}
						}
					}
				}
			}";

            string[] headers = new string[] {
                "Content-Type: application/json",
                $"Authorization: Bearer {project.Variables["TOKEN_CURRENT"].Value}"
            };

            string jsonBody = JsonConvert.SerializeObject(new { query = introspectionQuery });

            return ZennoPoster.HttpPost(
                GRAPHQL_URL,
                Encoding.UTF8.GetBytes(jsonBody),
                "application/json",
                "",
                "UTF-8",
                ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                30000,
                "",
                "HANA/v1",
                true,
                5,
                headers,
                "",
                true
            );
        }
    }




}
