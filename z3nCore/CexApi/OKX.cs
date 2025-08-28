﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json;

namespace z3nCore
{
    public class OKX
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;
        //private readonly string[] _apiKeys;
        private readonly Sql _sql;

        private readonly bool _logShow;
        private protected string _apiKey;
        private protected string _secretKey;
        private protected string _passphrase;

        public OKX(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project);
            _logger = new Logger(project, log: log, classEmoji: "OKX");
            LoadKeys();
        }
        public void CexLog(string toSend = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 💸  {callerName}] {toSend} ");
        }

        private void LoadKeys()
        {
            var creds = _project.SqlGet("apikey, apisecret, passphrase", "_api", where: "id = 'okx'").Split('|');


            _apiKey = creds[0];
            _secretKey = creds[1];
            _passphrase = creds[2];


        }

        private string MapNetwork(string chain, bool log)
        {

            _logger.Send("Mapping network: " + chain);
            string chainTo = chain.ToLower();
            switch (chainTo)
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
                case "linea": return "Linea";
                default:
                    _logger.Send($"Unrecognized network: {chain} returne as it is" + chain);
                    return chain;
            }
        }
        private string CalculateHmacSha256ToBaseSignature(string message, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacSha256 = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmacSha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private string OKXPost(string url, object body, string proxy = null, bool log = false)
        {
            var jsonBody = JsonConvert.SerializeObject(body);
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            _project.SendInfoToLog(jsonBody);

            string message = timestamp + "POST" + url + jsonBody;
            string signature = CalculateHmacSha256ToBaseSignature(message, _secretKey);

            // Send HTTP request
            var result = ZennoPoster.HttpPost(
                "https://www.okx.com" + url,
                jsonBody,
                "application/json",
                proxy,
                "UTF-8",
                ResponceType.BodyOnly,
                10000,
                "",
                _project.Profile.UserAgent,
                true,
                5,
                new string[]
                {
                    "Content-Type: application/json",
                    "OK-ACCESS-KEY: " + _apiKey,
                    "OK-ACCESS-SIGN: " + signature,
                    "OK-ACCESS-TIMESTAMP: " + timestamp,
                    "OK-ACCESS-PASSPHRASE: " + _passphrase
                },
                "",
                false//,
                     //false,
                     //_project.Profile.CookieContainer
            );
            _project.Json.FromString(result);
            CexLog($"json received: [{result}]");
            return result;
        }
        private string OKXGet(string url, string proxy = null, bool log = false)
        {

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            string message = timestamp + "GET" + url;
            string signature = CalculateHmacSha256ToBaseSignature(message, _secretKey);

            var jsonResponse = ZennoPoster.HttpGet(
                "https://www.okx.com" + url,
                proxy,
                "UTF-8",
                ResponceType.BodyOnly,
                10000,
                "",
                _project.Profile.UserAgent,
                true,
                5,
                new string[]
                {
                    "Content-Type: application/json",
                    "OK-ACCESS-KEY: " + _apiKey,
                    "OK-ACCESS-SIGN: " + signature,
                    "OK-ACCESS-TIMESTAMP: " + timestamp,
                    "OK-ACCESS-PASSPHRASE: " + _passphrase
                },
                "",
                false
            );

            CexLog($"json received: [{jsonResponse}]");
            _project.Json.FromString(jsonResponse);
            return jsonResponse;
        }

        public List<string> OKXGetSubAccs(string proxy = null, bool log = false)
        {
            var jsonResponse = OKXGet("/api/v5/users/subaccount/list", log: log);

            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;
            var subsList = new List<string>();

            if (code != "0") throw new Exception("OKXGetSubMax: Err [{code}]; Сообщение [{msg}]");
            else
            {
                var dataArray = response.data;
                if (dataArray != null)
                {
                    foreach (var item in dataArray)
                    {
                        string subAcct = item.subAcct;                   // "subName"
                        string label = item.label;
                        subsList.Add($"{subAcct}");
                        CexLog($"found: {subAcct}:{label}");
                    }
                }

            }
            return subsList;
        }
        public List<string> OKXGetSubMax(string accName, string proxy = null, bool log = false)
        {
            var jsonResponse = OKXGet($"/api/v5/account/subaccount/max-withdrawal?subAcct={accName}", log: log);

            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;
            var balanceList = new List<string>();

            if (code != "0") throw new Exception("OKXGetSubMax: Err [{code}]; Сообщение [{msg}]");
            else
            {
                var dataArray = response.data;
                if (dataArray != null)
                {
                    foreach (var item in dataArray)
                    {
                        string ccy = item.ccy;                   // "EGLD"
                        string maxWd = item.maxWd;               // "0.22193226"
                        balanceList.Add($"{ccy}:{maxWd}");
                        CexLog($"Currency: {ccy}, Max Withdrawal: {maxWd}");
                    }
                }
            }
            return balanceList;
        }
        public List<string> OKXGetSubTrading(string accName, string proxy = null, bool log = false)
        {
            var jsonResponse = OKXGet($"/api/v5/account/subaccount/balances?subAcct={accName}", log: log);

            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;
            var balanceList = new List<string>();

            if (code != "0") throw new Exception("OKXGetSubMax: Err [{code}]; Сообщение [{msg}]");
            else
            {
                var dataArray = response.data;
                if (dataArray != null)
                {
                    foreach (var item in dataArray)
                    {
                        string adjEq = item.adjEq;                   // "EGLD"

                        balanceList.Add($"{adjEq}");
                        CexLog($"adjEq: {adjEq}");
                    }
                }
            }
            return balanceList;
        }
        public List<string> OKXGetSubFunding(string accName, string proxy = null, bool log = false)
        {
            var jsonResponse = OKXGet($"/api/v5/asset/subaccount/balances?subAcct={accName}", log: log);

            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;
            var balanceList = new List<string>();

            if (code != "0") throw new Exception("Err [{code}]; Сообщение [{msg}]");
            else
            {
                var dataArray = response.data;
                if (dataArray != null)
                {
                    foreach (var item in dataArray)
                    {
                        string ccy = item.ccy;
                        string availBal = item.availBal;                    // "EGLD"
                        balanceList.Add($"{ccy}:{availBal}");
                        CexLog($"{ccy}:{availBal}");
                        //Loggers.l0g(_project, $"{ccy}:{availBal}");
                    }
                }
            }
            return balanceList;
        }
        public List<string> OKXGetSubsBal(string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var subs = OKXGetSubAccs();
            _project.SendInfoToLog(subs.Count.ToString());

            var balanceList = new List<string>();

            foreach (string sub in subs)
            {

                var balsFunding = OKXGetSubFunding(sub, log: true);
                foreach (string bal in balsFunding)
                {
                    if (string.IsNullOrEmpty(bal)) continue;
                    _project.SendInfoToLog($"balsFunding [{bal}]");
                    string ccy = bal.Split(':')[0]?.ToString();
                    string maxWd = bal.Split(':')[1]?.ToString();
                    if (!string.IsNullOrEmpty(maxWd))
                        try
                        {
                            if (double.Parse(maxWd) > 0)
                            {
                                balanceList.Add($"{sub}:{ccy}:{maxWd}");
                                Thread.Sleep(1000);
                            }
                        }
                        catch
                        {
                            CexLog($"!W failed to add [{maxWd}]$[{ccy}] from [{sub}] to main");
                        }
                }

                var balsTrading = OKXGetSubMax(sub, log: true);
                foreach (string bal in balsTrading)
                {
                    _project.SendInfoToLog($"balsTrading [{bal}]");
                    string ccy = bal.Split(':')[0]?.ToString();
                    string maxWd = bal.Split(':')[1]?.ToString();
                    if (!string.IsNullOrEmpty(maxWd))
                        try
                        {
                            if (double.Parse(maxWd) > 0)
                            {
                                balanceList.Add($"{sub}:{ccy}:{maxWd}");
                                Thread.Sleep(1000);
                            }
                        }
                        catch
                        {
                            CexLog($"!W failed to add [{maxWd}]$[{ccy}] from [{sub}] to main");
                        }
                }
            }
            return balanceList;
        }

        public void OKXWithdraw(string toAddress, string currency, string chain, decimal amount, decimal fee, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string network = MapNetwork(chain, log);
            var body = new
            {
                amt = amount.ToString("G", CultureInfo.InvariantCulture),
                fee = fee.ToString("G", CultureInfo.InvariantCulture),
                dest = "4",
                ccy = currency,
                chain = currency + "-" + network,
                toAddr = toAddress
            };
            var jsonResponse = OKXPost("/api/v5/asset/withdrawal", body, proxy, log);
            CexLog($"raw response: {jsonResponse}");

            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;

            if (code != "0") throw new Exception($"Err [{code}]; Сообщение [{msg}]");
            else
            {
                CexLog($"Refueled {toAddress} for {amount} `b");
            }
            _project.Json.FromString(jsonResponse);
        }
        private void OKXSubToMain(string fromSubName, string currency, decimal amount, string accountType = "6", string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            string strAmount = amount.ToString("G", CultureInfo.InvariantCulture);

            var body = new
            {
                ccy = currency,
                type = "2",
                amt = strAmount,
                from = accountType, //18 tradinng |6 funding
                to = "6",
                subAcct = fromSubName
            };
            var jsonResponse = OKXPost("/api/v5/asset/transfer", body, proxy, log);

            CexLog($"raw response: {jsonResponse}");

            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;

            if (code != "0") throw new Exception("Err [{code}]; Сообщение [{msg}] amt:[{strAmount}] ccy:[{currency}]");
            else
            {
                CexLog($"raw response: {jsonResponse}");
            }

        }
        public void OKXCreateSub(string subName, string accountType = "1", string proxy = null, bool log = false)
        {
            var body = new
            {
                subAcct = subName,
                type = accountType
            };
            var jsonResponse = OKXPost("/api/v5/users/subaccount/create-subaccount", body, proxy, log);

            CexLog($"raw response: {jsonResponse}");

            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;

            if (code != "0") throw new Exception($"Err [{code}]; Сообщение [{msg}]");
            else
            {
                CexLog($"raw response: {jsonResponse}");
            }

        }

        public void OKXDrainSubs()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var subs = OKXGetSubAccs();
            _project.SendInfoToLog(subs.Count.ToString());

            foreach (string sub in subs)
            {

                var balsFunding = OKXGetSubFunding(sub, log: true);
                foreach (string bal in balsFunding)
                {
                    if (string.IsNullOrEmpty(bal)) continue;
                    _project.SendInfoToLog($"balsFunding [{bal}]");
                    string ccy = bal.Split(':')[0]?.ToString();
                    string maxWd = bal.Split(':')[1]?.ToString();
                    if (!string.IsNullOrEmpty(maxWd))
                        try
                        {
                            if (decimal.Parse(maxWd) > 0)
                            {
                                decimal amount = decimal.Parse(maxWd);
                                _project.SendInfoToLog($"sending {maxWd}${ccy} from {sub} to main");
                                OKXSubToMain(sub, ccy, amount, "6", log: true);
                                Thread.Sleep(500);
                            }
                        }
                        catch
                        {
                            _project.SendInfoToLog($"failed to send [{maxWd}]$[{ccy}] from [{sub}] to main");
                        }
                }

                var balsTrading = OKXGetSubMax(sub, log: true);
                foreach (string bal in balsTrading)
                {
                    _project.SendInfoToLog($"balsTrading [{bal}]");
                    string ccy = bal.Split(':')[0]?.ToString();
                    string maxWd = bal.Split(':')[1]?.ToString();
                    if (!string.IsNullOrEmpty(maxWd))
                        try
                        {
                            if (decimal.Parse(maxWd) > 0)
                            {
                                decimal amount = decimal.Parse(maxWd);
                                _project.SendInfoToLog($"sending {maxWd}${ccy} from {sub} to main");
                                OKXSubToMain(sub, ccy, amount, "18", log: true);
                            }
                        }
                        catch
                        {
                            _project.SendInfoToLog($"failed to send [{maxWd}]$[{ccy}] from [{sub}] to main");
                        }
                }
            }


        }
        public void OKXAddMaxSubs()
        {
            int i = 1;
            while (true)
            {

                try
                {
                    OKXCreateSub($"sub{i}t{Time.Now("unix")}");
                    i++;
                    Thread.Sleep(1500);
                }
                catch
                {
                    _project.SendInfoToLog($"{i} subs added");
                    break;
                }
            }
        }
        public T OKXPrice<T>(string pair, string proxy = null, bool log = false)
        {
            var jsonResponse = OKXGet($"/api/v5/market/ticker?instId={pair}", log: log);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string msg = response.msg;
            string code = response.code;
            string last = null;

            if (code != "0") throw new Exception("Err [{code}]; Сообщение [{msg}]");
            else
            {
                var dataArray = response.data;
                if (dataArray != null)
                {
                    foreach (var item in dataArray)
                    {
                        string lastPrice = item.last;
                        if (!string.IsNullOrEmpty(lastPrice))
                        {
                            last = lastPrice;
                            CexLog($"{pair}:{lastPrice}");
                            break;
                        }
                    }
                }
            }
            decimal price = decimal.Parse(last);
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(price.ToString("0.##################"), typeof(T));
            return (T)Convert.ChangeType(price, typeof(T));
        }
    }
}
