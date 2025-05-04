using System;
using System.Security.Cryptography;
using System.Text;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Runtime.CompilerServices;
using ZennoLab.InterfacesLibrary.Enums.Http;


namespace ZBSolutions
{
    public  class BinanceApi
    {


        private readonly IZennoPosterProjectModel _project;
        private readonly string[] _apiKeys;

        private readonly L0g _log;
        private readonly bool _logShow;
        private readonly Sql _sql;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _proxy;
        public BinanceApi(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project);
            _log = new L0g(_project);
            _logShow = log;
            _apiKeys = BinanceKeys();
            _apiKey = _apiKeys[0];
            _secretKey = _apiKeys[1];
            _proxy = _apiKeys[2];
        }
        public void CexLog(string toSend = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _log.Send($"[ 💸  {callerName}] {toSend} ");
        }
        public string[] BinanceKeys()
        {

            string[] keys = _sql.BinanceApiKeys().Split(';');
            var apiKey = keys[0];
            var secretKey = keys[1];
            var proxy = keys[2];

            string[] result = new string[] { apiKey, secretKey, proxy };
            return result;
        }
        private string MapNetwork(string chain, bool log)
        {
            CexLog("Mapping network: " + chain, log: log);
            //if (log) Loggers.l0g(_project, "Mapping network: " + chain);
            chain = chain.ToLower();
            switch (chain)
            {
                case "arbitrum": return "Arbitrum One";
                case "ethereum": return "ERC20";
                case "base": return "Base";
                case "bsc": return "BSC";
                case "avalanche": return "Avalanche C-Chain";
                case "polygon": return "Polygon";
                case "optimism": return "Optimism";
                case "trc20": return "TRC20";
                case "zksync": return "zkSync Era";
                case "aptos": return "Aptos";
                default:
                    CexLog("Unsupported network: " + chain, log: log);
                    throw new ArgumentException("Unsupported network: " + chain);
            }
        }
 
        private string MkSign(string parameters)
        {
            byte[] secretkeyBytes = Encoding.UTF8.GetBytes(_secretKey);

            using (HMACSHA256 hmacsha256 = new HMACSHA256(secretkeyBytes))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(parameters);
                byte[] hashValue = hmacsha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }

        private static string TimeStamp() 
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        private string BinancePOST(string method, string body, bool log = false)
        {
 
            string url = $"https://api.binance.com{method}";
            var result = ZennoPoster.HTTP.Request(HttpMethod.POST, url, body, "application/x-www-form-urlencoded; charset=utf-8", _proxy, "UTF-8", ResponceType.BodyOnly, 30000, "", _project.Profile.UserAgent, true, 5,
                new string[] {
                    "X-MBX-APIKEY: "+_apiKey,
                    "Content-Type: application/x-www-form-urlencoded; charset=utf-8"
                    },
                "",
                false, false, _project.Profile.CookieContainer);
            _project.Json.FromString(result);
            CexLog($"json received: [{result}]");
            return result;
        }
        private string BinanceGET(string method, string parameters, bool log = false)
        {

            string url = $"https://api.binance.com{method}?{parameters}";
            string result = ZennoPoster.HttpGet(
                            url,
                            _proxy,
                            "UTF-8",
                            ResponceType.BodyOnly,
                            30000,
                            "",
                            "Mozilla/4.0",
                            true,
                            5,
                            new string[] {
                            "X-MBX-APIKEY: "+_apiKey,
                            "Content-Type: application/x-www-form-urlencoded; charset=utf-8"
                            },
                            "",
                            true
                        );
            _project.Json.FromString(result);
            CexLog($"json received: [{result}]");
            return result;
        }

        public  string GetUserAsset(string coin = "")
        {

            var method = "/sapi/v3/asset/getUserAsset";

            string parameters = $"timestamp={TimeStamp()}";
            string hash = MkSign(parameters);
            string jsonBody = $@"{parameters}&signature={hash}";

            var result = BinancePOST(method, jsonBody);

            _project.Json.FromString(result);

            var balanceList = "";
            foreach (var item in _project.Json)
            {
                string asset = item.asset;
                string free = item.free;
                balanceList += $"{asset}:{free}\n";
            }

            balanceList.Trim();

            if (coin == "") return $"{balanceList}";
            if (!balanceList.Contains(coin)) return $"NoCoinFound: {coin}";

            string tiker = "", balance = "";
            foreach (string asset in balanceList.Split('\n'))
            {
                tiker = asset.Split(':')[0];
                balance = asset.Split(':')[1];
                if (tiker == coin) break;
            }

            return $"{balance}";

        }
        public string Withdraw( string amount, string network, string coin = "ETH", string address = "")
        {

            var method = "/sapi/v1/capital/withdraw/apply";

            string parameters = $"timestamp={TimeStamp()}&coin={coin}&network={network}&address={address}&amount={amount}";
            string hash = MkSign(parameters);

            string jsonBody = $@"{parameters}&signature={hash}";
            var result = BinancePOST(method, jsonBody);
            
            _project.SendInfoToLog(jsonBody);
            return result;
        }

        public  string GetWithdrawHistory(IZennoPosterProjectModel project, string searchId = "")
        {
            var method = "/sapi/v1/capital/withdraw/history";
            string parameters = $"timestamp={TimeStamp()}";
            string hash = MkSign(parameters);
            string signed = $"{parameters}&signature={hash}";

            string response = BinanceGET(method, signed, _logShow);


            project.Json.FromString(response);

            var historyList = "";
            foreach (var item in project.Json)
            {
                string id = item.id;
                string amount = item.amount;
                string coin = item.coin;
                string status = item.status.ToString(); // явное преобразование числового status в строку
                historyList += $"{id}:{amount}:{coin}:{status}\n";
            }

            historyList = historyList.Trim();

            if (searchId == "") return historyList;
            if (!historyList.Contains(searchId)) return $"NoIdFound: {searchId}";

            string foundId = "", foundAmount = "", foundCoin = "", foundStatus = "";
            foreach (string withdrawal in historyList.Split('\n'))
            {
                var parts = withdrawal.Split(':');
                foundId = parts[0];
                foundAmount = parts[1];
                foundCoin = parts[2];
                foundStatus = parts[3];
                if (foundId == searchId) break;
            }

            return $"{foundAmount}:{foundCoin}:{foundStatus}";
        }
    }
}
