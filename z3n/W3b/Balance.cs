using Leaf.xNet;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;


namespace z3n
{
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
            if (log) _logger.Send($"{address} nonce now {transactionCount}");
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
        // Основная функция, возвращает баланс токенов как строку
        
        
        public string ERC721(string tokenContract, string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            address = ChekAdr(address);
            if (string.IsNullOrEmpty(rpc)) rpc = _defRpc;

            string functionSelector = "0x70a08231"; // balanceOf(address)
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
            string balanceString = balance.ToString(); // Баланс токенов как строка
            Log(address, balanceString, rpc, tokenContract, log: log);

            return balanceString;
        }
        public T ERC721<T>(string tokenContract, string rpc = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string balanceString = ERC721(tokenContract, rpc, address, proxy, log); // Вызываем основную функцию
            BigInteger balance = BigInteger.Parse(balanceString); // Парсим строку в BigInteger
            Log(address, balanceString, rpc, tokenContract, log: log);

            if (typeof(T) == typeof(string))
                return (T)(object)balanceString;
            else if (typeof(T) == typeof(int))
            {
                // Проверяем, что balance помещается в int
                if (balance > int.MaxValue || balance < int.MinValue)
                    throw new OverflowException($"Balance {balance} exceeds the range of int.");
                return (T)(object)(int)balance;
            }
            else
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");
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
            if (log) _logger.Send($"{address}: {balance} TOKEN ({coinType})");


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
            _logger.Send($"{address}: {balance} TOKEN ({coinType})");
            if (typeof(T) == typeof(string)) return FloorDecimal<T>(balance, int.Parse(octas));
            return (T)Convert.ChangeType(balance, typeof(T));
        }

        public string INITIA(string address, string chain = "interwoven-1", bool parse = false)
        {
            string url = $"https://celatone-api-prod.alleslabs.dev/v1/initia/{chain}/accounts/{address}/balances";

            string jsonString = _project.GET(url, parseJson:parse);
            return jsonString;

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
                    _logger.Send("no Address provided");
                    throw;
                }

            string jsonString = INITIA(address, chain);

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

        public List<BigInteger> ERC721TokenIds(string tokenContract, string rpc, string address, string proxy = null, bool log = false)
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

    }
}
