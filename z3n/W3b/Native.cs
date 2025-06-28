using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using Newtonsoft.Json.Linq;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Text.RegularExpressions;
using static NBitcoin.Scripting.OutputDescriptor;

namespace z3n
{
    public class Native : W3b
    {
        public readonly string _defRpc;


        public Native(IZennoPosterProjectModel project, bool log = false, string adrEvm = null, string key = null)
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
       

    }
}
