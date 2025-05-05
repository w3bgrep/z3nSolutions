using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using ZennoLab.InterfacesLibrary.ProjectModel;



namespace ZBSolutions
{
    public class OnChain
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly L0g _log;
        private readonly bool _logShow;
        private readonly Sql _sql;
        private readonly string _adrEvm;
        private readonly string _adrSol;
        private readonly string _adrApt;
        private readonly string _adrSui;
        private readonly string _defRpc;

        public OnChain(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project;
            _log = new L0g(_project);
            _sql = new Sql(_project);
            _logShow = log;
            _adrEvm = _sql.AdrEvm();
            _adrSol = _sql.AdrSol();
            _defRpc = project.Variables["blockchainRPC"].Value;

        }
        public string HexToString(string hexValue, string convert = "")
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
        public void BalLog(string address, string balance, string rpc, string contract = null, [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _log.Send($"[ ⛽  {callerName}] [{address}] balance {contract} is\n		  [{balance}] by [{rpc}]");
        }
        public string Rpc(string chain)
        {
            chain = chain.ToLower();
            switch (chain)
            {
                //ethNative
                case "ethereum": return "https://ethereum-rpc.publicnode.com";
                case "arbitrum": return "https://arbitrum-one.publicnode.com";
                case "base": return "https://base-rpc.publicnode.com";
                case "blast": return "https://rpc.blast.io";
                case "linea": return "https://rpc.linea.build";
                case "manta": return "https://pacific-rpc.manta.network/http";
                case "optimism": return "https://optimism-rpc.publicnode.com";
                case "scroll": return "https://rpc.scroll.io";
                case "soneium": return "https://rpc.soneium.org";
                case "taiko": return "https://rpc.mainnet.taiko.xyz";
                case "zksync": return "https://mainnet.era.zksync.io";
                case "zora": return "https://rpc.zora.energy";
                //nonEthEvm
                case "avalanche": return "https://avalanche-c-chain.publicnode.com";
                case "bsc": return "https://bsc-rpc.publicnode.com";
                case "gravity": return "https://rpc.gravity.xyz";
                case "fantom": return "https://rpc.fantom.network";
                case "opbnb": return "https://opbnb-mainnet-rpc.bnbchain.org";
                case "polygon": return "https://polygon-rpc.com";
                //Testnets
                case "sepolia": return "https://ethereum-sepolia-rpc.publicnode.com";
                //nonEvm
                case "aptos": return "https://fullnode.mainnet.aptoslabs.com/v1";
                case "movement": return "https://mainnet.movementnetwork.xyz/v1";

                default:
                    throw new ArgumentException("No RPC for: " + chain);
            }
        }
        public string[] RpcArr(string chains)
        {
            string rpcs = null;
            string[] toAdd = chains.Split(',');
            foreach (string chain in toAdd) rpcs += Rpc(chain.Trim()) + "\n";
            return rpcs.Trim().Split('\n');
        }      
        public T FloorDecimal<T>(decimal value, int? decimalPlaces = null)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            int effectiveDecimalPlaces = decimalPlaces ?? 18;

            if (effectiveDecimalPlaces < 0)
                throw new ArgumentException("Decimal places must be non-negative", nameof(decimalPlaces));

            try
            {
                string valueStr = value.ToString(CultureInfo.InvariantCulture);
                int actualDecimalPlaces = 0;
                if (valueStr.Contains("."))
                {
                    actualDecimalPlaces = valueStr.Split('.')[1].Length;
                }

                effectiveDecimalPlaces = Math.Min(effectiveDecimalPlaces, actualDecimalPlaces);

                if (effectiveDecimalPlaces > 28) // decimal type supports up to 28-29 digits
                {
                    _project.SendWarningToLog($"Requested decimal places ({effectiveDecimalPlaces}) exceeds decimal type limit. Adjusting to 28.");
                    effectiveDecimalPlaces = 28;
                }

                decimal multiplier = (decimal)Math.Pow(10, effectiveDecimalPlaces);
                decimal flooredValue = Math.Floor(value * multiplier) / multiplier;

                if (typeof(T) == typeof(string))
                {
                    string format = "0." + new string('#', effectiveDecimalPlaces);
                    return (T)Convert.ChangeType(flooredValue.ToString(format, CultureInfo.InvariantCulture), typeof(T));
                }
                if (typeof(T) == typeof(int))
                    return (T)Convert.ChangeType((int)flooredValue, typeof(T));
                if (typeof(T) == typeof(double))
                    return (T)Convert.ChangeType((double)flooredValue, typeof(T));
                return (T)Convert.ChangeType(flooredValue, typeof(T));
            }
            catch (OverflowException ex)
            {
                _project.SendWarningToLog($"Overflow error while flooring {value} to {effectiveDecimalPlaces} decimal places: {ex.Message}");
                return (T)Convert.ChangeType(value, typeof(T)); // Return original value as fallback
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog($"Error while flooring {value} to {effectiveDecimalPlaces} decimal places: {ex.Message}");
                return (T)Convert.ChangeType(value, typeof(T)); // Return original value as fallback
            }
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
            
            BalLog(rpc, gasGwei.ToString(),"", log: log);
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(gasGwei.ToString("0.######", CultureInfo.InvariantCulture), typeof(T));
            return (T)Convert.ChangeType(gasGwei, typeof(T));
        }
              
        public T NativeEVM<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _adrEvm;
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

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
            BalLog(address, balanceString, rpc, log: log);
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));
        }
        public T BalERC20<T>(string tokenContract, string rpc = null, string address = null, string tokenDecimal = "18", string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _adrEvm;
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
            BalLog(address, balanceString, rpc, tokenContract, log: log);
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));

        }
        public T BalERC721<T>(string tokenContract, string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _adrEvm;
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

            if (log) _log.Send($"[Leaf.xNet] Баланс токенов ERC-721 для адреса {address} в контракте {tokenContract}: {balance}");

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(balance.ToString(), typeof(T));

            return (T)Convert.ChangeType(balance, typeof(T));
        }
        public T BalERC1155<T>(string tokenContract, string tokenId, string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _adrEvm;
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

            if (log) _log.Send($"[Leaf.xNet ⇌] balance of ERC-1155 [{tokenContract}:id({tokenId})] on {address}: [{balance}]");

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(balance.ToString(), typeof(T));
            else if (typeof(T) == typeof(int))
                return (T)(object)(int)balance;
            else if (typeof(T) == typeof(BigInteger))
                return (T)(object)balance;
            else
                throw new InvalidOperationException($"!W unsupported type {typeof(T)}");
        }
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
        public T NonceEVM<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            if (string.IsNullOrEmpty(address)) address = _adrEvm;
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
            if (log) _log.Send($"{address} nonce now {transactionCount}");
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(transactionCount.ToString(), typeof(T));
            return (T)Convert.ChangeType(transactionCount, typeof(T));
        }
        public T NativeSOL<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _adrSol;
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


            decimal balance = decimal.Parse(tokenDecimal) / 1000000000m;

            string balanceString = FloorDecimal<string>(balance, int.Parse(tokenDecimal));
            BalLog(address, balanceString, rpc, log: log);

            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));

        }
        public T TokenSPL<T>(string tokenMint, string address = null, int floor = 0, string rpc = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) address = _adrSol;
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
            BalLog(address, balanceString, rpc, tokenMint);

            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));

        }
        public T NativeSUI<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address))
            {
                string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";
                address = _sql.DbQ($"SELECT sui FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            }
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

            var json = JObject.Parse(response);
            string mist = json["result"]?["totalBalance"]?.ToString() ?? "0";
            decimal balanceSui = decimal.Parse(mist) / 1000000000m;
            if (log) _log.Send($"{address}: {balanceSui} SUI");

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(balanceSui.ToString("0.##################"), typeof(T));
            return (T)Convert.ChangeType(balanceSui, typeof(T));
        }
        public T TokenSUI<T>(string coinType, string address = null, string rpc = null, string proxy = null, bool log = false)
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
            if (log) _log.Send($"{address}: {balance} TOKEN ({coinType})");


            if (typeof(T) == typeof(string)) return FloorDecimal<T>(balance, int.Parse(mist));
            return (T)Convert.ChangeType(balance, typeof(T));

        }
        public T NativeAPT<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (string.IsNullOrEmpty(address))
            {
                string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";
                address = _sql.DbQ($"SELECT apt FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            }

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

                // Настройка прокси, если указан
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
                if (typeof(T) == typeof(string)) return (T)(object)"0";
                return (T)(object)0m;
            }

            string octas = json[0]?.ToString() ?? "0";
            decimal balance;
            try
            {
                balance = decimal.Parse(octas) / 100000000m; // 8 decimals
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Failed to parse balance: {ex.Message}");
                if (typeof(T) == typeof(string)) return (T)(object)"0";
                return (T)(object)0m;
            }

            string balanceString = FloorDecimal<string>(balance, 8);
            BalLog(address, balanceString, rpc, log: log);

            if (typeof(T) == typeof(string)) return (T)(object)balanceString;
            return (T)Convert.ChangeType(balance, typeof(T));
        }
        public T TokenAPT<T>(string coinType, string address = null, string rpc = null, string proxy = null, bool log = false)
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
            if (log) _log.Send($"{address}: {balance} TOKEN ({coinType})");
            if (typeof(T) == typeof(string)) return FloorDecimal<T>(balance, int.Parse(octas));
            return (T)Convert.ChangeType(balance, typeof(T));
        }
        public T TokenInitia<T>(string address = null, string chain = "interwoven-1", string token = "uinit", bool log = false) 
        {
            if (string.IsNullOrEmpty(address))
                try
                {
                    address = _project.Variables["addressInitia"].Value;                  
                }
                catch {
                    _log.Send("no Address provided");
                    throw;
                }

            

            string url = $"https://celatone-api-prod.alleslabs.dev/v1/initia/{chain}/accounts/{address}/balances";

            string jsonString = Requests.GET(url);

            _project.L0g(jsonString , show:log);
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

        public bool WaitTransaction(string rpc = null, string hash = null, int deadline = 60, string proxy = "", bool log = false)
        {
            if (string.IsNullOrEmpty(hash)) hash = _project.Variables["blockchainHash"].Value;
            if (string.IsNullOrEmpty(rpc)) rpc = _project.Variables["blockchainRPC"].Value;

            string jsonReceipt = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionReceipt"",""params"":[""{hash}""],""id"":1}}";
            string jsonRaw = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionByHash"",""params"":[""{hash}""],""id"":1}}";

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

                DateTime startTime = DateTime.Now;
                TimeSpan timeout = TimeSpan.FromSeconds(deadline);


                while (true)
                {
                    if (DateTime.Now - startTime > timeout)
                        throw new Exception($"timeout {deadline}s");

                    string logString = "";

                    try
                    {
                        HttpResponse httpResponse = request.Post(rpc, jsonReceipt, "application/json");
                        response = httpResponse.ToString();

                        if (httpResponse.StatusCode != HttpStatusCode.OK)
                        {
                            _project.SendErrorToLog($"Ошибка сервера (receipt): {httpResponse.StatusCode}");
                            Thread.Sleep(2000);
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(response) || response.Contains("\"result\":null"))
                        {
                            _project.Variables["txStatus"].Value = "noStatus";
                        }
                        else
                        {
                            _project.Json.FromString(response);
                            try
                            {
                                string gasUsed = HexToString(_project.Json.result.gasUsed, "gwei");
                                string gasPrice = HexToString(_project.Json.result.effectiveGasPrice, "gwei");
                                string status = HexToString(_project.Json.result.status);

                                _project.Variables["txStatus"].Value = status == "1" ? "SUCCSESS" : "!W FAIL";
                                bool res = status == "1" ? true : false;
                                string result = $"{rpc} {hash} [{_project.Variables["txStatus"].Value}] gasUsed: {gasUsed}";
                                _log.Send($"[ TX state:  {result}");
                                return res;
                            }
                            catch
                            {
                                _project.Variables["txStatus"].Value = "noStatus";
                            }
                        }
                    }
                    catch (HttpException ex)
                    {
                        _project.SendErrorToLog($"Ошибка запроса (receipt): {ex.Message}");
                        Thread.Sleep(2000);
                        continue;
                    }

                    try
                    {
                        HttpResponse httpResponse = request.Post(rpc, jsonRaw, "application/json");
                        response = httpResponse.ToString();

                        if (httpResponse.StatusCode != HttpStatusCode.OK)
                        {
                            _project.SendErrorToLog($"Ошибка сервера (raw): {httpResponse.StatusCode}");
                            Thread.Sleep(2000);
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(response) || response.Contains("\"result\":null"))
                        {
                            _project.Variables["txStatus"].Value = "";
                            logString = $"[{rpc} {hash}] not found";
                        }
                        else
                        {
                            _project.Json.FromString(response);
                            try
                            {
                                string gas = HexToString(_project.Json.result.maxFeePerGas, "gwei");
                                string gasPrice = HexToString(_project.Json.result.gasPrice, "gwei");
                                string nonce = HexToString(_project.Json.result.nonce);
                                string value = HexToString(_project.Json.result.value, "eth");
                                _project.Variables["txStatus"].Value = "PENDING";

                                logString = $"[{rpc} {hash}] pending  gasLimit:[{gas}] gasNow:[{gasPrice}] nonce:[{nonce}] value:[{value}]";
                            }
                            catch
                            {
                                _project.Variables["txStatus"].Value = "";
                                logString = $"[{rpc} {hash}] not found";
                            }
                        }
                    }
                    catch (HttpException ex)
                    {
                        _project.SendErrorToLog($"Ошибка запроса (raw): {ex.Message}");
                        Thread.Sleep(2000);
                        continue;
                    }
                    _log.Send($"[ TX state:  {logString}");
                    Thread.Sleep(3000);
                }
            }
        }

    }

}
