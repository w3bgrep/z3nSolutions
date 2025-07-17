using Leaf.xNet;
using NBitcoin;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.Enums.Browser;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using z3n;
//using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;


namespace w3tools //by @w3bgrep
{
    public class W3B
    {
        public async Task<decimal> GetEvmBalance(string rpc, string address)
        {
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_getBalance"", ""params"": [""{address}"", ""latest""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string hexBalance = json["result"]?.ToString().Replace("0x", "") ?? "0";
                    BigInteger balanceWei = BigInteger.Parse(hexBalance, NumberStyles.AllowHexSpecifier);
                    decimal balance = (decimal)balanceWei / 1000000000000000000m;
                    return balance;
                }
            }
        }

        public static decimal EvmNative(string rpc, string address)
        {
            return new W3B().GetEvmBalance(rpc, address).GetAwaiter().GetResult();
        }


        public async Task<decimal> GetErc20Balance(string tokenContract, string rpc, string address, string tokenDecimal = "18")
        {
            string data = "0x70a08231000000000000000000000000" + address.Replace("0x", "");
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string hexBalance = json["result"]?.ToString().Replace("0x", "") ?? "0";
                    BigInteger balanceRaw = BigInteger.Parse(hexBalance, NumberStyles.AllowHexSpecifier);
                    decimal decimals = (decimal)Math.Pow(10, double.Parse(tokenDecimal, CultureInfo.InvariantCulture));
                    decimal balance = (decimal)balanceRaw / decimals;
                    return balance;
                }
            }
        }

        public static decimal ERC20(string tokenContract, string rpc, string address, string tokenDecimal = "18")
        {
            return new W3B().GetErc20Balance(tokenContract, rpc, address, tokenDecimal).GetAwaiter().GetResult();
        }

        public async Task<decimal> GetSolanaBalance(string rpc, string address)
        {
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getBalance"", ""params"": [""{address}""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    BigInteger balanceLamports = json["result"]?["value"]?.ToObject<BigInteger>() ?? 0;
                    decimal balance = (decimal)balanceLamports / 1_000_000_000m; // Convert lamports to SOL
                    return balance;
                }
            }
        }

        public static decimal SolNative( string address, string rpc = "https://api.mainnet-beta.solana.com")
        {
            return new W3B().GetSolanaBalance(rpc, address).GetAwaiter().GetResult();
        }

        public async Task<decimal> SolFeeByTx(string transactionHash, string rpc = null, string tokenDecimal = "9")
        {
            if (string.IsNullOrEmpty(rpc)) rpc = "https://api.mainnet-beta.solana.com";

            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getTransaction"", ""params"": [""{transactionHash}"", {{""encoding"": ""jsonParsed"", ""maxSupportedTransactionVersion"": 0}}], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string feeLamports = json["result"]?["meta"]?["fee"]?.ToString() ?? "0";
                    BigInteger balanceRaw = BigInteger.Parse(feeLamports);
                    decimal decimals = (decimal)Math.Pow(10, double.Parse(tokenDecimal, CultureInfo.InvariantCulture));
                    decimal balance = (decimal)balanceRaw / decimals;
                    return balance;
                }
            }
        }

        public static decimal SolTxFee(string transactionHash, string rpc = null, string tokenDecimal = "9")
        {
            return new W3B().SolFeeByTx(transactionHash, rpc, tokenDecimal).GetAwaiter().GetResult();
        }



        public static decimal Price(string CGid = "ethereum")
        {
            try
            {
                string result = new CoinGecco().CoinInfo(CGid).GetAwaiter().GetResult();

                var json = JObject.Parse(result);
                JToken usdPriceToken = json["market_data"]?["current_price"]?["usd"];

                if (usdPriceToken == null)
                {
                    return 0m;
                }

                decimal usdPrice = usdPriceToken.Value<decimal>();
                return usdPrice;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static decimal DSPrice(string contract = "So11111111111111111111111111111111111111112", string chain = "solana")
        {
            try
            {
                string result = new DexScreener().CoinInfo(contract, chain).GetAwaiter().GetResult();

                var json = JArray.Parse(result);
                JToken priceToken = json.FirstOrDefault()?["priceNative"];

                if (priceToken == null)
                {
                    return 0m;
                }

                return priceToken.Value<decimal>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




    }

    public class CoinGecco
    {

        private readonly string _apiKey = "CG-TJ3DRjP93bTSCto6LiPbMgaV";
        
        public async Task<string> CoinInfo(string CGid = "ethereum")
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = System.Net.Http.HttpMethod.Get,
                RequestUri = new Uri($"https://api.coingecko.com/api/v3/coins/{CGid}"),
                Headers =
            {
                { "accept", "application/json" },
                { "x-cg-demo-api-key", _apiKey },
            },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }

    }

    public class DexScreener
    {

        public async Task<string> CoinInfo(string contract, string chain)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = System.Net.Http.HttpMethod.Get,
                RequestUri = new Uri($"https://api.dexscreener.com/tokens/v1/{chain}/{contract}"),
                Headers =
                {
                    { "accept", "application/json" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }
    }









    public static class TestStatic
    {
        public static string Body(this IZennoPosterProjectModel project, Instance instance, string url, string parametr = "ResponseBody", bool reload = false)
        {
            return new Traffic(project, instance).Get(url, parametr);

        }
        public static decimal Price(this IZennoPosterProjectModel project, string tiker)
        {
            tiker = tiker.ToUpper();
            return new OKXApi(project).OKXPrice<decimal>($"{tiker}-USDT");

        }
        public static string SPLcontract(this string tiker)
        {
            switch (tiker)
            {
                case "Sol":
                    tiker = "So11111111111111111111111111111111111111112";
                    break;
                case "pSol":
                    tiker = "pSo1f9nQXWgXibFtKf7NWYxb5enAM4qfP6UJSiXRQfL";
                    break;
                case "jitoSol":
                    tiker = "J1toso1uCk3RLmjorhTtrVwY9HJ7X8V9yYac6Y7kGCPn";
                    break;
                case "mSol":
                    tiker = "mSoLzYCxHdYgdzU16g5QSh3i5K3z3KZK7ytfqcJm7So";
                    break;
                case "bbSol":
                    tiker = "Bybit2vBJGhPF52GBdNaQfUJ6ZpThSgHBobjWZpLPb4B";
                    break;
                case "soLayer":
                    tiker = "sSo14endRuUbvQaJS3dq36Q829a3A6BEfoeeRGJywEh";
                    break;
                case "ezSol":
                    tiker = "ezSoL6fY1PVdJcJsUpe5CM3xkfmy3zoVCABybm5WtiC";
                    break;
                case "lotusSol":
                    tiker = "gangqfNY8fA7eQY3tHyjrevxHCLnhKRrLGRwUMBR4y6";
                    break;
                case "stepSol":
                    tiker = "StPsoHokZryePePFV8N7iXvfEmgUoJ87rivABX7gaW6";
                    break;
                case "binanceSol":
                    tiker = "BNso1VUJnh4zcfpZa6986Ea66P6TCp59hvtNJ8b1X85";
                    break;
                case "heliumSol":
                    tiker = "he1iusmfkpAdwvxLNGV8Y1iSbj4rUy6yMhEA3fotn9A";
                    break;
                case "driftSol":
                    tiker = "Dso1bDeDjCQxTrWHqUUi63oBvV7Mdm6WaobLbQ7gnPQ";
                    break;
                case "bonkSol":
                    tiker = "BonK1YhkXEGLZzwtcvRTip3gAL9nCeQD7ppZBLXhtTs";
                    break;
                    

                default:
                    throw new Exception($"unexpected {tiker}");


                    
            }
            return tiker;
        }


    }
    
    public class tNative : W3b
    {
        public readonly string _defRpc;


        public tNative(IZennoPosterProjectModel project, bool log = false, string adrEvm = null, string key = null)
        : base(project, log)
        {
            if (string.IsNullOrEmpty(adrEvm) && (!string.IsNullOrEmpty(_acc0)))
            {
                //_key = ApplyKey(key);
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
        private string CheckRpc(string rpc)
        {
            if (string.IsNullOrEmpty(rpc)) rpc = _project.Var("blockchainRPC");
            if (string.IsNullOrEmpty(rpc)) throw new ArgumentException("!W rpc is nullOrEmpty");
            return rpc;
        }
        //evm

        public string EVM(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            address = ChekAdr(address);
            rpc = CheckRpc(rpc);


            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_getBalance"", ""params"": [""{address}"", ""latest""], ""id"": 1 }}";
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
            decimal balance = (decimal)balanceWei / 1000000000000000000m;

            string balanceString = FloorDecimal<string>(balance, int.Parse("18"));
            _logger.Send($"NativeBal: [{balanceString}] by {rpc} ({address})");
            //Log(address, balanceString, rpc, log: log);
            return balanceString;

            //if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            //return (T)Convert.ChangeType(balance, typeof(T));
        }
        public T EVM<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string balanceString = EVM(rpc, address, proxy, log);//FloorDecimal<string>(balance, int.Parse("18"));
            decimal balance = decimal.Parse(balanceString);
            //_logger.Send($"NativeBal: [{balanceString}] by {rpc} ({address})");
            //Log(address, balanceString, rpc, log: log);
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));
        }

        public string SOL(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _sql.Address("sol");
            if (string.IsNullOrEmpty(rpc)) rpc = "https://api.mainnet-beta.solana.com";

            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getBalance"", ""params"": [""{address}""], ""id"": 1 }}";
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
            string tokenDecimal = json["result"]?["value"]?.ToString() ?? "0";
            decimal balance = decimal.Parse(tokenDecimal) / 1000000000m; // Solana uses 9 decimal places (lamports)

            string balanceString = FloorDecimal<string>(balance, 9); // Fixed precision to 9 for Solana
            _logger.Send($"NativeBal: [{balanceString}] by {rpc} ({address})");
            //Log(address, balanceString, rpc, log: log);
            return balanceString;
        }
        public T SOL<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string balanceString = SOL(rpc, address, proxy, log);
            decimal balance = decimal.Parse(balanceString);
            Log(address, balanceString, rpc, log: log);
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));
        }

        public string APT(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (string.IsNullOrEmpty(address)) address = _sql.Address("apt");
            if (string.IsNullOrEmpty(rpc))
                rpc = "https://fullnode.mainnet.aptoslabs.com/v1";

            string url = $"{rpc}/view";
            string coinType = "0x1::aptos_coin::AptosCoin";
            string requestBody = $@"{{
                                    ""function"": ""0x1::coin::balance"",
                                    ""type_arguments"": [""{coinType}""],
                                    ""arguments"": [""{address}""]
                                    }}";

            string response;

            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;
                request.AddHeader("Content-Type", "application/json");

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
                    HttpResponse httpResponse = request.Post(url, requestBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTpreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            JArray json;
            try
            {
                json = JArray.Parse(response);
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Failed to parse JSON response: {ex.Message}");
                return "0";
            }

            string octas = json[0]?.ToString() ?? "0";
            decimal balance;
            try
            {
                balance = decimal.Parse(octas) / 100000000m; // 8 decimals for Aptos (octas)
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Failed to parse balance: {ex.Message}");
                return "0";
            }

            string balanceString = FloorDecimal<string>(balance, 8);
            _logger.Send($"NativeBal: [{balanceString}] by {rpc} ({address})");
            //Log(address, balanceString, rpc, log: log);
            return balanceString;
        }
        public T APT<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string balanceString = APT(rpc, address, proxy, log);
            decimal balance;
            try
            {
                balance = decimal.Parse(balanceString);
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Failed to parse balance string in generic method: {ex.Message}");
                if (typeof(T) == typeof(string)) return (T)(object)"0";
                return (T)(object)0m;
            }
            //Log(address, balanceString, rpc, log: log);
            if (typeof(T) == typeof(string)) return (T)(object)balanceString;
            return (T)Convert.ChangeType(balance, typeof(T));
        }


        public string SUI(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _sql.Address("sui");
            if (string.IsNullOrEmpty(rpc)) rpc = "https://fullnode.mainnet.sui.io";

            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""suix_getBalance"", ""params"": [""{address}"", ""0x2::sui::SUI""], ""id"": 1 }}";
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

            JObject json;
            try
            {
                json = JObject.Parse(response);
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Failed to parse JSON response: {ex.Message}");
                return "0";
            }

            string mist = json["result"]?["totalBalance"]?.ToString() ?? "0";
            decimal balance;
            try
            {
                balance = decimal.Parse(mist) / 1000000000m; // 9 decimals for SUI (MIST)
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Failed to parse balance: {ex.Message}");
                return "0";
            }

            string balanceString = FloorDecimal<string>(balance, 9); // 9 decimals for consistency
            _logger.Send($"NativeBal: [{balanceString}] by {rpc} ({address})");
            //Log(address, balanceString, rpc, log: log);
            return balanceString;
        }
        public T SUI<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string balanceString = SUI(rpc, address, proxy, log);
            decimal balance;
            try
            {
                balance = decimal.Parse(balanceString);
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Failed to parse balance string in generic method: {ex.Message}");
                if (typeof(T) == typeof(string)) return (T)(object)"0";
                return (T)(object)0m;
            }
            //Log(address, balanceString, rpc, log: log);
            if (typeof(T) == typeof(string)) return (T)(object)balanceString;
            return (T)Convert.ChangeType(balance, typeof(T));
        }

        //cosmos
        public T Init<T>(string address = null, string chain = "interwoven-1", string token = "uinit", bool log = false)
        {
            if (string.IsNullOrEmpty(address))
                try
                {
                    address = _project.Variables["addressInitia"].Value;
                }
                catch
                {
                    _logger.Send("no Address provided");
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


        public Dictionary<string, decimal> DicNative(string[] chainsToUse = null, bool log = false)
        {
            if (chainsToUse == null) chainsToUse = _project.Var("cfgChains").Split(',');

            var bls = new Dictionary<string, decimal>();
            var _w3b = new W3bRead(_project, log);
            foreach (string chain in chainsToUse)
            {
                decimal native;
                if (!chain.Contains("solana"))
                    native = _w3b.NativeSOL<decimal>();
                else
                    native = _w3b.NativeEVM<decimal>(_w3b.Rpc(chain));
                bls.Add(chain, native);
            }
            return bls;
        }


        public string SolFeeByTx(string transactionHash, string rpc = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = "https://api.mainnet-beta.solana.com";

            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getTransaction"", ""params"": [""{transactionHash}"", {{""encoding"": ""jsonParsed"", ""maxSupportedTransactionVersion"": 0}}], ""id"": 1 }}";
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
            string feeLamports = json["result"]?["meta"]?["fee"]?.ToString() ?? "0";
            decimal fee = decimal.Parse(feeLamports) / 1000000000m; // Convert lamports to SOL

            string feeString = FloorDecimal<string>(fee, 9); // Fixed precision to 9 for Solana
            _logger.Send($"TransactionFee: [{feeString}] for tx {transactionHash} by {rpc}");
            //Log(transactionHash, feeString, rpc, log: log);
            return feeString;
        }

    }

    public class tBackpackWallet
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        private readonly string _key;
        protected readonly string _pass;
        protected readonly string _fileName;

        protected readonly string _extId = "aflkmfhebedbjioipglgcbcmnbpgliof";
        protected readonly string _popout = $"chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html";
        protected readonly string _urlImport = $"chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/options.html?onboarding=true";


        public tBackpackWallet(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string fileName = "Backpack0.10.94.crx")

        {
            _project = project;
            _instance = instance;
            _fileName = fileName;
            _key = KeyLoad(key);
            _pass = SAFU.HWPass(_project);
            _logger = new Logger(project, log: log, classEmoji: "🎒");
        }

        private string KeyLoad(string key)
        {
            if (string.IsNullOrEmpty(key)) key = "key";

            switch (key)
            {
                case "key":
                    key = new Sql(_project).Key("sol");
                    break;
                case "seed":
                    key = new Sql(_project).Key("seed");
                    break;
                default:
                    return key;
            }
            return key;
        }

        private string KeyType(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (Regex.IsMatch(input, @"^[0-9a-fA-F]{64}$"))
                return "keyEvm";

            if (Regex.IsMatch(input, @"^[123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz]{87,88}$"))
                return "keySol";

            var words = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 12 || words.Length == 24)
                return "seed";

            return null;
        }


        public string Launch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            _logger.Send($"Launching Backpack ({fileName})");
            if (new ChromeExt(_project, _instance).Install(_extId, fileName, log))
                Import(log: log);
            else
                Unlock(log: log);
            _logger.Send($"checking");
            var adr = ActiveAddress(log: log);
            _logger.Send($"using [{adr}]");
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
            return adr;
        }

        public bool Import(bool log = false)
        {
            var key = _key;
            var password = _pass;
            var keyType = KeyType(_key);
            _logger.Send($"Importing Backpack wallet with {keyType}");

            var type = "Solana";
            var source = "key";

            if (keyType == "keyEvm") type = "Ethereum";
            if (!keyType.Contains("key")) source = "phrase";

            _instance.CloseExtraTabs();
            _instance.Go(_urlImport);
            _logger.Send($"keytype is {keyType}");
        check:
            Thread.Sleep(1000);
            string state = null;
            if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Select\\ one\\ or\\ more \\wallets", "regexp", 0).IsVoid) state = "NoFundedWallets";

            else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ Wallet", "regexp", 0).IsVoid) state = "importButton";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Backpack\\ supports\\ multiple\\ blockchains.\\nWhich\\ do\\ you\\ want\\ to\\ use\\?\\ You\\ can\\ add\\ more\\ later.", "regexp", 0).IsVoid) state = "chooseChain";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Choose\\ a\\ method\\ to\\ import\\ your\\ wallet.", "regexp", 0).IsVoid) state = "chooseSource";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Enter private key", "text", 0).IsVoid) state = "enterKey";
            else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Open\\ Backpack", "regexp", 0).IsVoid) state = "open";
            else if (!_instance.ActiveTab.FindElementByAttribute("p", "innertext", "Already\\ setup", "regexp", 0).IsVoid) state = "alreadySetup";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Enter\\ or\\ paste\\ your\\ 12\\ or\\ 24-word\\ phrase.", "regexp", 0).IsVoid) state = "enterSeed";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Create\\ a\\ Password", "regexp", 0).IsVoid) state = "inputPass";


            _logger.Send(state);
            switch (state)
            {
                case null:
                    _logger.Send("...");
                    Thread.Sleep(2000);
                    goto check;
                case "importButton":
                    _instance.HeClick(("button", "innertext", "Import\\ Wallet", "regexp", 0));
                    goto check;

                case "chooseChain":
                    _instance.HeClick(("button", "innertext", type, "regexp", 0));
                    goto check;

                case "chooseSource":
                    _instance.HeClick(("button", "innertext", source, "text", 0));
                    goto check;

                case "enterKey":
                    _instance.HeSet(("textarea", "fulltagname", "textarea", "regexp", 0), key);
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    goto check;

                case "open":
                    _instance.HeClick(("button", "innertext", "Open\\ Backpack", "regexp", 0));
                    _instance.CloseExtraTabs();
                    return true;

                case "alreadySetup":
                    _instance.CloseExtraTabs();
                    return false;

                case "enterSeed":
                    string[] seed = key.Split(' ');
                    int i = 0;
                    foreach (string word in seed)
                    {
                        _instance.HeSet(("input:text", "fulltagname", "input:text", "regexp", i), word, delay: 0);
                        i++;
                    }
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    goto check;

                case "inputPass":
                    _instance.HeSet(("input:password", "placeholder", "Password", "regexp", 0), password);
                    _instance.HeSet(("input:password", "placeholder", "Confirm\\ Password", "regexp", 0), password);
                    _instance.HeClick(("input:checkbox", "class", "PrivateSwitchBase-input\\ ", "regexp", 0));
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;

                case "NoFundedWallets":
                    _instance.HeClick(("button", "class", "is_Button\\ ", "regexp", 0));
                    _instance.HeClick(("div", "class", "is_SelectItem\\ _bg-0active-744986709\\ _btc-0active-1163467620\\ _brc-0active-1163467620\\ _bbc-0active-1163467620\\ _blc-0active-1163467620\\ _bg-0hover-1067792163\\ _btc-0hover-1394778429\\ _brc-0hover-1394778429\\ _bbc-0hover-1394778429\\ _blc-0hover-1394778429\\ _bg-0focus-455866976\\ _btc-0focus-1452587353\\ _brc-0focus-1452587353\\ _bbc-0focus-1452587353\\ _blc-0focus-1452587353\\ _outlineWidth-0focus-visible-1px\\ _outlineStyle-0focus-visible-solid\\ _dsp-flex\\ _ai-center\\ _fd-row\\ _fb-auto\\ _bxs-border-box\\ _pos-relative\\ _mih-1611762906\\ _miw-0px\\ _fs-0\\ _pr-1316332129\\ _pl-1316332129\\ _pt-1316333028\\ _pb-1316333028\\ _jc-441309761\\ _fw-nowrap\\ _w-10037\\ _btc-2122800589\\ _brc-2122800589\\ _bbc-2122800589\\ _blc-2122800589\\ _maw-10037\\ _ox-hidden\\ _oy-hidden\\ _bg-1067792132\\ _cur-default\\ _outlineOffset--0d0t5px46", "regexp", 3));
                    _instance.HeClick(("div", "class", "is_Circle\\ ", "regexp", 0));
                    _instance.HeClick(("button", "innertext", "Import\\ Wallet", "regexp", 0));

                    goto check;

                default:
                    goto check;

            }
        }

        public void Unlock(bool log = false)
        {
            _logger.Send("Unlocking Backpack wallet");
            var password = _pass;
            _project.Deadline();

            if (!_instance.ActiveTab.URL.Contains(_popout))
                _instance.ActiveTab.Navigate(_popout, "");

            check:
            Thread.Sleep(1000);
            string state = null;
            _project.Deadline(30);
            if (!_instance.ActiveTab.FindElementByAttribute("path", "d", "M12 5v14", "text", 0).IsVoid) state = "unlocked";
            else if (!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "regexp", 0).IsVoid) state = "unlock";


            switch (state)
            {
                case null:
                    _logger.Send("...");
                    Thread.Sleep(1000);
                    goto check;
                case "unlocked":
                    return;
                case "unlock":
                    _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), password);
                    _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                    Thread.Sleep(2000);
                    goto check;
            }

        }

        public string ActiveAddress(bool log = false)
        {
            _logger.Send("Checking Backpack wallet address");
            if (_instance.ActiveTab.URL != _popout)
                _instance.ActiveTab.Navigate(_popout, "");
            _instance.CloseExtraTabs();

            try
            {
                while (_instance.ActiveTab.FindElementByAttribute("button", "class", "is_Button\\ ", "regexp", 0).IsVoid)
                    _instance.HeClick(("path", "d", "M12 5v14", "text", 0));

                var address = _instance.HeGet(("p", "class", "MuiTypography-root\\ MuiTypography-body1", "regexp", 0), "last");
                _instance.HeClick(("button", "aria-label", "TabsNavigator,\\ back", "regexp", 0));
                return address;
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to check address: {ex.Message}");
                throw;
            }
        }
        public string CurrentChain(bool log = true)
        {
            string modeNow = null;
        ifNow:
            var mode = _instance.HeGet(("div", "aria-haspopup", "dialog", "regexp", 0), atr: "innerhtml");

            if (mode.Contains("solana.png")) modeNow = "mainnet";
            if (mode.Contains("devnet.png")) modeNow = "devnet";
            if (mode.Contains("testnet.png")) modeNow = "testnet";
            if (mode.Contains("ethereum.png")) modeNow = "ethereum";
            switch (modeNow)
            {
                case "devnet":
                case "mainnet":
                case "testnet":
                case "ethereum":
                    _project.L0g(modeNow);
                    break;

                default:
                    Thread.Sleep(1000);
                    _project.L0g("unknown");
                    goto ifNow;
            }
            return modeNow;
        }

        public void Approve(bool log = false)
        {
            _logger.Send("Approving Backpack wallet action");

            _project.Deadline();

        checkTab:
            if (!_instance.ActiveTab.URL.Contains(_extId))
                try { _project.Deadline(10); goto checkTab; }
                catch { _logger.Send("!W no tab in 10s"); throw; }


        approve:
            try
            {
                _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last");
                _instance.CloseExtraTabs();
                _logger.Send("Action approved successfully");
                return;
            }
            catch (Exception ex){ _logger.Send($"!W {ex.Message}");}

        unlock:
            try
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), _pass, deadline: 0);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last");
                _instance.CloseExtraTabs();
                _logger.Send("Action approved after unlocking");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Approve"))
                {
                    _instance.CloseExtraTabs(true);
                    _logger.Send("no Approve after unlock. Removing");
                    _instance.UninstallExtension(_extId);
                    throw new Exception("Removed");
                }
            }
        }

        public void Connect(bool log = false)
        {
            _project.Deadline();

            string action = null;
        getState:
            _project.Deadline(30);
            try
            {
                action = _instance.HeGet(("div", "innertext", "Approve", "regexp", 0), "last");
            }
            catch (Exception ex)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "regexp", 0).IsVoid)
                {
                    _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), _pass);
                    _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                    Thread.Sleep(2000);
                    goto getState;
                }

                if (!_instance.ActiveTab.URL.Contains(_extId))
                {
                    _logger.Send($"No Wallet tab found. 0");
                    return;
                }
                else
                {
                    _logger.Send($"wallet tab detected. {action}");
                }

            }

            _project.L0g(action);

            switch (action)
            {
                case "Approve":
                    _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last", emu: 1);
                    goto getState;

                default:
                    goto getState;

            }


        }

        public void Devmode(bool enable = true)
        {
            _instance.Go(_popout);

        ifswitch:
            try
            {
            switchBox:
                bool DevModeNow = false;
                if (_instance.HeGet(("input:checkbox", "class", "css-1m9pwf3", "regexp", 0), deadline: 1, atr: "value") == "True") DevModeNow = true;

                if (enable != DevModeNow)
                {
                    _instance.HeClick(("input:checkbox", "class", "css-1m9pwf3", "regexp", 0));
                    goto switchBox;
                }

            }
            catch
            {
                _instance.HeClick(("button", "class", "css-xxmhpt\\ css-yt63r3", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Settings", "regexp", 0));
                _instance.HeClick(("div", "innertext", "Preferences", "regexp", 0), "last");
                goto ifswitch;
            }


        }


        public void DevChain(string reqmode = "devnet")
        {
            Switch("Solana");
            var chain = CurrentChain();
        check:
            if (chain != reqmode)
            {
                _instance.HeClick(("div", "aria-haspopup", "dialog", "regexp", 0));
                _instance.HeClick(("span", "innertext", "Add\\ Network", "regexp", 0), "last");

                try
                {
                    _instance.HeGet(("span", "innertext", "Test\\ Networks", "regexp", 0));
                }
                catch
                {
                    _instance.HeClick(("button", "aria-label", "TabsNavigator,\\ back", "regexp", 0));
                    Devmode();
                    goto check;
                }

                _instance.HeClick(("img", "src", $"{reqmode}.png", "regexp", 0));
                _instance.HeClick(("span", "innertext", "From\\ Solana", "regexp", 0), "last", deadline: 3, thr0w: false);
                _instance.HeClick(("button", "class", "is_Button\\ ", "regexp", 0), deadline: 3, thr0w: false);

            }

        }


        public void Add(string type = "Ethereum", string source = "key")//"Solana" | "Ethereum" //"key" | "phrase"
        {
            string _urlAdd = "chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/options.html?add-user-account=true";
            string key;
            if (type == "Ethereum") key = new Sql(_project).Key("evm");
            else key = new Sql(_project).Key("sol");
            _instance.Go(_urlAdd, true);

        check:

            string state = null;

            if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ Wallet", "regexp", 0).IsVoid) state = "importButton";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Backpack\\ supports\\ multiple\\ blockchains.\\nWhich\\ do\\ you\\ want\\ to\\ use\\?\\ You\\ can\\ add\\ more\\ later.", "regexp", 0).IsVoid) state = "chooseChain";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Choose\\ a\\ method\\ to\\ import\\ your\\ wallet.", "regexp", 0).IsVoid) state = "chooseSource";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Enter private key", "text", 0).IsVoid) state = "enterKey";
            else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Open\\ Backpack", "regexp", 0).IsVoid) state = "open";

            switch (state)
            {
                case "importButton":
                    _instance.HeClick(("button", "innertext", "Import\\ Wallet", "regexp", 0));
                    goto check;

                case "chooseChain":
                    _instance.HeClick(("button", "innertext", type, "regexp", 0));
                    goto check;

                case "chooseSource":
                    _instance.HeClick(("button", "innertext", source, "text", 0));
                    goto check;

                case "enterKey":
                    _instance.HeSet(("textarea", "fulltagname", "textarea", "regexp", 0), key);
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    goto check;
                case "open":
                    _instance.HeClick(("button", "innertext", "Open\\ Backpack", "regexp", 0));
                    _instance.CloseExtraTabs();
                    return;
                default:
                    goto check;

            }





        }

        public void Switch(string type)//"Solana" | "Ethereum" //"key" | "phrase"

        {
        start:
            if (_instance.ActiveTab.URL != _popout) _instance.ActiveTab.Navigate(_popout, "");
            //_instance.CloseExtraTabs();

            int toUse = 0;
            if (type == "Ethereum")
                toUse = 1;
            _instance.HeClick(("button", "class", "MuiButtonBase-root\\ MuiIconButton-root\\ MuiIconButton-sizeMedium\\ css-xxmhpt\\ css-yt63r3", "regexp", 0));
            int i = 0;
            while (!_instance.ActiveTab.FindElementByAttribute("button", "class", "MuiButtonBase-root\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ css-1y4j1ko", "regexp", i).InnerText.Contains("Add")) i++;


            if (i < 2)
            {
                Add();
                goto start;
            }
            _instance.HeClick(("button", "class", "MuiButtonBase-root\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ css-1y4j1ko", "regexp", toUse));

        }

        public string Current()//"Solana" | "Ethereum" //"key" | "phrase"

        {
        start:
            if (_instance.ActiveTab.URL != _popout) _instance.ActiveTab.Navigate(_popout, "");
            var chan = _instance.HeGet(("div", "aria-haspopup", "dialog", "regexp", 0), atr: "innerhtml");
            if (chan.Contains("solana")) return "Solana";
            else if (chan.Contains("ethereum")) return "Ethereum";
            else return "Undefined";
        }

    }
    public class tZerionWallet
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        private readonly string _key;
        private readonly string _pass;
        private readonly string _fileName;
        private string _expectedAddress;


        private readonly string _extId = "klghhnkeealcohjjanjjdaeeggmfmlpl";
        private readonly string _sidepanelUrl = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#";

        private readonly string _urlOnboardingTab = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html?windowType=tab&appMode=onboarding#/onboarding/import";
        private readonly string _urlPopup = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#";
        private readonly string _urlImport = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/get-started/import";
        private readonly string _urlWalletSelect = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/wallet-select";


        public tZerionWallet(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string fileName = "Zerion1.21.3.crx")
        {
            _project = project;
            _instance = instance;
            _fileName = fileName;

            _key = KeyLoad(key);
            _pass = SAFU.HWPass(_project);
            _logger = new Logger(project, log: log, classEmoji: "🇿");

        }


        private string KeyLoad(string key)
        {
            if (string.IsNullOrEmpty(key)) key = "key";

            switch (key)
            {
                case "key":
                    key = new Sql(_project).Key("evm");
                    break;
                case "seed":
                    key = new Sql(_project).Key("seed");
                    break;
                default:
                    return key;
            }
            if (string.IsNullOrEmpty(key)) throw new Exception("keyIsEmpy");

            _expectedAddress = key.ToPubEvm();
            return key;
        }

        public string Launch(string fileName = null, bool log = false, string source = null, string refCode = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = _fileName;
            if (string.IsNullOrEmpty(source))
                source = "key";
            string active = null;
            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            new ChromeExt(_project, _instance).Install(_extId, fileName, log);

        check:
            string state = GetState();
            _logger.Send(state);
            switch (state)
            {
                case "onboarding":
                    Import(source, refCode, log: log);
                    goto check;
                case "noTab":
                    _instance.Go(_urlPopup);
                    goto check;
                case "unlock":
                    Unlock();
                    goto check;
                case "overview":
                    //string current = GetActive();
                    SwitchSource(source);
                    break;
                default:
                    goto check;
            }

            try { TestnetMode(false); } catch { }
            GetActive();
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
            return _expectedAddress;
        }


        private void Add(string source = null, bool log = false)
        {
            string key = KeyLoad(source);
            _instance.Go(_urlImport);

            _instance.HeSet(("seedOrPrivateKey", "name"), key);
            _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
            _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
            _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            try
            {
                _instance.HeClick(("button", "class", "_option", "regexp", 0));
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            }
            catch { }

        }
        public bool Sign(bool log = false, int deadline = 10)
        {
            parseURL();
            try
            {
                int i = 0;
            scan:

                var button = _instance.HeGet(("button", "class", "_primary", "regexp", i));
                if (_instance.GetHe(("button", "class", "_primary", "regexp", i)).Width == -1)
                { i++; goto scan; }
                //if (button.Width == -1) { i++; goto scan; }
                _logger.Send(button);
                _instance.HeClick(("button", "class", "_primary", "regexp", i));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Send($"!W {ex.Message}");
                throw;
            }
        }

        public void Connect(bool log = false)
        {

            string action = null;
        getState:

            try
            {
                action = _instance.HeGet(("button", "class", "_primary", "regexp", 0));
            }
            catch (Exception ex)
            {
                _project.L0g($"No Wallet tab found. 0");
                return;
            }

            _project.L0g(action);
            _project.L0g(_instance.ActiveTab.URL);

            switch (action)
            {
                case "Add":
                    _project.L0g($"adding {_instance.HeGet(("input:url", "fulltagname", "input:url", "text", 0), atr: "value")}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                    goto getState;
                case "Close":
                    _project.L0g($"added {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                    goto getState;
                case "Connect":
                    _project.L0g($"connecting {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                    goto getState;
                case "Sign":
                    _project.L0g($"sign {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                    goto getState;
                case "Sign In":
                    _project.L0g($"sign {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                    goto getState;

                default:
                    goto getState;

            }


        }

        private void Import(string source = null, string refCode = null, bool log = false)
        {
            string key = KeyLoad(source);
            _logger.Send(key);
            key = key.Trim().StartsWith("0x") ? key.Substring(2) : key;
            string keyType = key.KeyType();
            _instance.Go(_urlOnboardingTab);


            _logger.Send(keyType);
            

            _logger.Send(keyType);
            if (keyType == "keyEvm")
            {
                _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/private-key", "regexp", 0));
                _instance.ActiveTab.FindElementByName("key").SetValue(key, "Full", false);
            }
            else if (keyType == "seed")
            {
                _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/mnemonic", "regexp", 0));
                int index = 0;
                foreach (string word in key.Split(' '))
                {
                    _instance.ActiveTab.FindElementById($"word-{index}").SetValue(word, "Full", false);
                    index++;
                }
            }

            _instance.HeClick(("button", "innertext", "Import\\ wallet", "regexp", 0));
            _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
            _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
            _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            if (!string.IsNullOrEmpty(refCode))
            {
                _instance.HeClick(("button", "innertext", "Enter\\ Referral\\ Code", "regexp", 0));
                _instance.HeSet((("referralCode", "name")), refCode);
                _instance.HeClick(("button", "class", "_regular", "regexp", 0));
            }
            _instance.CloseExtraTabs(true);
            _instance.Go(_urlPopup);
        }

        private void Unlock(bool log = false)
        {
            //Go();
            //string active = null;
            try
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass, deadline: 3);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            }
            catch (Exception ex)
            {
                _logger.Send(ex.Message);
            }
        }

        public void SwitchSource(string addressToUse = "key")
        {

            _project.Deadline();

            if (addressToUse == "key") addressToUse = new Sql(_project).Key("evm").ToPubEvm();
            else if (addressToUse == "seed") addressToUse = new Sql(_project).Key("seed").ToPubEvm();
            else throw new Exception("supports \"key\" | \"seed\" only");
            _expectedAddress = addressToUse;

        go:
            //_instance.Go(_urlWalletSelect);
            _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\#/wallet-select", "regexp", 0));
            Thread.Sleep(1000);

        waitWallets:
            _project.Deadline(60);
            if (_instance.ActiveTab.FindElementByAttribute("button", "class", "_wallet", "regexp", 0).IsVoid)
                goto waitWallets;

            var wallets = _instance.ActiveTab.FindElementsByAttribute("button", "class", "_wallet", "regexp").ToList();

            foreach (HtmlElement wallet in wallets)
            {
                string masked = "";
                string balance = "";
                string ens = "";

                if (wallet.InnerHtml.Contains("M18 21a2.9 2.9 0 0 1-2.125-.875A2.9 2.9 0 0 1 15 18q0-1.25.875-2.125A2.9 2.9 0 0 1 18 15a3.1 3.1 0 0 1 .896.127 1.5 1.5 0 1 0 1.977 1.977Q21 17.525 21 18q0 1.25-.875 2.125A2.9 2.9 0 0 1 18 21")) continue;
                if (wallet.InnerText.Contains("·"))
                {
                    ens = wallet.InnerText.Split('\n')[0].Split('·')[0];
                    masked = wallet.InnerText.Split('\n')[0].Split('·')[1];
                    balance = wallet.InnerText.Split('\n')[1].Trim();

                }
                else
                {
                    masked = wallet.InnerText.Split('\n')[0];
                    balance = wallet.InnerText.Split('\n')[1];
                }
                masked = masked.Trim();

                _logger.Send($"[{masked}]{masked.ChkAddress(addressToUse)}[{addressToUse}]");

                if (masked.ChkAddress(addressToUse))
                {
                    _instance.HeClick(wallet);
                    return;
                }
            }
            _logger.Send("address not found");
            Add("seed");

            _instance.CloseExtraTabs(true);
            goto go;


        }

        private void TestnetMode(bool testMode = false)
        {
            bool current;

            string testmode = _instance.HeGet(("input:checkbox", "fulltagname", "input:checkbox", "text", 0), deadline: 1, atr: "value");

            if (testmode == "True")
                current = true;
            else
                current = false;

            if (testMode != current)
                _instance.HeClick(("input:checkbox", "fulltagname", "input:checkbox", "text", 0));

        }

        public bool WaitTx(int deadline = 60, bool log = false)
        {
            DateTime functionStart = DateTime.Now;
        check:
            bool result;
            if ((DateTime.Now - functionStart).TotalSeconds > deadline) throw new Exception($"!W Deadline [{deadline}]s exeeded");


            if (!_instance.ActiveTab.URL.Contains("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview/history"))
            {
                Tab tab = _instance.NewTab("zw");
                if (tab.IsBusy) tab.WaitDownloading();
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview/history", "");

            }
            Thread.Sleep(2000);

            var status = _instance.HeGet(("div", "style", "padding: 0px 16px;", "regexp", 0));



            if (status.Contains("Pending")) goto check;
            else if (status.Contains("Failed")) result = false;
            else if (status.Contains("Execute")) result = true;
            else
            {
                _logger.Send($"unknown status {status}");
                goto check;
            }
            _instance.CloseExtraTabs();
            return result;

        }

        public List<string> Claimable(string address)
        {
            var res = new List<string>();
            var _h = new NetHttp(_project);
            address = address.ToLower();

            string url = $@"https://dna.zerion.io/api/v1/memberships/{address}/quests";

            var headers = new Dictionary<string, string>
            {
                { "Accept", "*/*" },
                { "Accept-Language", "en-US,en;q=0.9" },
                { "Origin", "https://app.zerion.io" },
                { "Referer", "https://app.zerion.io" },
                { "Priority", "u=1, i" }
            };

            string response = _h.GET(
                url: url,
                proxyString: "+",
                headers: headers,
                parse: false
            );


            int i = 0;
            try
            {
                JArray jArr = JArray.Parse(response);
                while (true)
                {
                    var id = "";
                    var kind = "";
                    var link = "";
                    var reward = "";
                    var kickoff = "";
                    var deadline = "";
                    var recurring = "";
                    var claimable = "";

                    try
                    {

                        id = jArr[i]["id"].ToString();
                        kind = jArr[i]["kind"].ToString();
                        recurring = jArr[i]["recurring"].ToString();
                        reward = jArr[i]["reward"].ToString();
                        kickoff = jArr[i]["kickoff"].ToString();
                        deadline = jArr[i]["deadline"].ToString();
                        claimable = jArr[i]["claimable"].ToString();
                        try { link = jArr[i]["meta"]["mint"]["link"]["url"].ToString(); } catch { }
                        try { link = jArr[i]["meta"]["call"]["link"]["url"].ToString(); } catch { }
                        var toLog = $"Unclaimed [{claimable}]Exp on Zerion  [{kind}]  [{id}]";
                        if (claimable != "0")
                        {
                            res.Add(id);
                            _project.L0g(toLog);
                        }
                        i++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch
            {
                _project.L0g($"!W failed to parse : [{response}] ");
            }
            return res;

        }

        private string GetState()
        {
        check:
            string state = null;
            //Thread.Sleep(1000);
            if (!_instance.ActiveTab.URL.Contains(_extId))
                state = "noTab";
            else if (_instance.ActiveTab.URL.Contains("onboarding"))
                state = "onboarding";
            else if (_instance.ActiveTab.URL.Contains("login"))
                state = "unlock";
            else if (_instance.ActiveTab.URL.Contains("overview"))
                state = "overview";

            else
                goto check;
            return state;
        }

        private string GetActive()
        {
            string activeWallet = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\#/wallet-select", "regexp", 0));
            string total = _instance.HeGet(("div", "style", "display:\\ grid;\\ gap:\\ 0px;\\ grid-template-columns:\\ minmax\\(0px,\\ auto\\);\\ align-items:\\ start;", "regexp", 0)).Split('\n')[0];
            _logger.Send($"wallet Now {activeWallet}  [{total}]");
            return activeWallet;
        }

        private void parseURL()
        {
            var urlNow = _instance.ActiveTab.URL;
            try
            {

                var type = "null";
                var data = "null";
                var origin = "null";

                var parts = urlNow.Split('?').ToList();

                foreach (string part in parts)
                {
                    //project.SendInfoToLog(part);
                    if (part.StartsWith("windowType"))
                    {
                        type = part.Split('=')[1];
                    }
                    if (part.StartsWith("origin"))
                    {
                        origin = part.Split('=')[1];
                        data = part.Split('=')[2];
                        data = data.Split('&')[0].Trim();
                    }

                }
                dynamic txData = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(data);
                var gas = txData.gas.ToString();
                var value = txData.value.ToString();
                var sender = txData.from.ToString();
                var recipient = txData.to.ToString();
                var datastring = $"{txData.data}";


                BigInteger gasWei = BigInteger.Parse("0" + gas.TrimStart('0', 'x'), NumberStyles.AllowHexSpecifier);
                decimal gasGwei = (decimal)gasWei / 1000000000m;
                _logger.Send($"Sending {datastring} to {recipient}, gas: {gasGwei}");

            }
            catch { }
        }

    }

}
