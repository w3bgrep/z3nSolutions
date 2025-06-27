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
using ZennoLab.InterfacesLibrary;
using z3n;
using NBitcoin;

using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Numerics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;
using Newtonsoft.Json.Linq;
using System.Dynamic;

using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;

using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI;
using Nethereum.Util;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;


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

        public static void Sleep(this IZennoPosterProjectModel project,int min, int max)
        {
            Thread.Sleep(new Random().Next(min, min) * 1000);

        }

    }




    public class ZerionWallet2
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


        public ZerionWallet2(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string fileName = "Zerion1.21.3.crx")
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
                    string current = GetActive();
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
        public bool Sign(bool log = false)
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

            try
            {
                var button = _instance.HeGet(("button", "class", "_primary", "regexp", 0));
                _logger.Send(button);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
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

            if (string.IsNullOrWhiteSpace(refCode))
            {
                refCode = new Sql(_project).DbQ(@"SELECT referralCode
                FROM projects.zerion
                WHERE referralCode != '_' 
                AND TRIM(referralCode) != ''
                ORDER BY RANDOM()
                LIMIT 1;");
            }
            if (string.IsNullOrWhiteSpace(refCode)) refCode = "JZA87YJDS";

            _logger.Send(keyType);
            var inputRef = true;

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
            if (inputRef)
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
                _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass, deadline:3);
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
            _instance.Go(_urlWalletSelect);
            Thread.Sleep(1000);

        waitWallets:
            _project.Deadline(60);
            if (_instance.ActiveTab.FindElementByAttribute("button", "class", "_wallet", "regexp", 0).IsVoid) goto waitWallets;

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

            string testmode = _instance.HeGet(("input:checkbox", "fulltagname", "input:checkbox", "text", 0), deadline:1, atr: "value");

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

    }


    public class Traffic2
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;


        public Traffic2(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "🌎");
        }

        /// <summary>
        /// Получает данные сетевого трафика для указанного URL.
        /// </summary>
        /// <param name="url">Часть URL для фильтрации трафика.</param>
        /// <param name="parametr">Конкретный параметр трафика для возврата. Доступные параметры: Method, ResultCode, Url, ResponseContentType, RequestHeaders, RequestCookies, RequestBody, ResponseHeaders, ResponseCookies, ResponseBody. Если null или пустая строка, возвращаются все параметры в формате "ключ-значение".</param>
        /// <param name="reload">Если true, выполняется перезагрузка страницы (по умолчанию false).</param>
        /// <param name="method">Не используется в текущей реализации (по умолчанию null).</param>
        /// <returns>Значение указанного параметра трафика или строка со всеми параметрами, если parametr не указан.</returns>
        public string Get(string url, string parametr, bool reload = false)
        {
            string param;
            Dictionary<string, string> data = Get(url, reload);
            if (string.IsNullOrEmpty(parametr))
                return string.Join("\n", data.Select(kvp => $"{kvp.Key}-{kvp.Value}"));
            else data.TryGetValue(parametr, out param);
            return param;
        }
        public Dictionary<string, string> Get(string url, bool reload = false)
        {
            _project.Deadline();
            _instance.UseTrafficMonitoring = true;
            if (reload) _instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");

            get:
            _project.Deadline(10);
            Thread.Sleep(1000);
            var traffic = _instance.ActiveTab.GetTraffic();
            var data = new Dictionary<string, string>();
            string param;
            foreach (var t in traffic)
            {
                if (t.Url.Contains(url))
                {
                    var Method = t.Method;
                    var ResultCode = t.ResultCode.ToString();
                    var Url = t.Url;
                    var ResponseContentType = t.ResponseContentType;
                    var RequestHeaders = t.RequestHeaders;
                    var RequestCookies = t.RequestCookies;
                    var RequestBody = t.RequestBody;
                    var ResponseHeaders = t.ResponseHeaders;
                    var ResponseCookies = t.ResponseCookies;
                    var ResponseBody = t.ResponseBody == null ? "" : Encoding.UTF8.GetString(t.ResponseBody, 0, t.ResponseBody.Length);

                    if (Method == "OPTIONS") continue;
                    data.Add("Method", Method);
                    data.Add("ResultCode", ResultCode);
                    data.Add("Url", Url);
                    data.Add("ResponseContentType", ResponseContentType);
                    data.Add("RequestHeaders", RequestHeaders);
                    data.Add("RequestCookies", RequestCookies);
                    data.Add("RequestBody", RequestBody);
                    data.Add("ResponseHeaders", ResponseHeaders);
                    data.Add("ResponseCookies", ResponseCookies);
                    data.Add("ResponseBody", ResponseBody);
                    break;
                }
            }
            if (data.Count == 0) goto get;
            return data;
        }
        public string GetHeader(string url, string headerToGet = "Authorization", bool trim = true, bool reload = false)
        {
            Dictionary<string, string> data = Get(url, reload);
            data.TryGetValue("RequestHeaders", out string headersString);

            var headers = headersString.Split('\n');

            foreach (string header in headers)
            {
                string headerName = header.Split(':')[0];
                string headerValue = header.Split(':')[1];
                data.Add(headerName, headerValue);
            }

            data.TryGetValue(headerToGet, out string Value);

            if (trim) Value = Value.Replace("Bearer", "").Trim();
            return Value;

        }

    }


    public class ChainOperaAI 
    {

        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        public ChainOperaAI(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "ChainOpera");

        }
        public void GetAuthData()
        {
            var url = "https://chat.chainopera.ai/userCenter/api/v1/";

            _project.Deadline();
            _instance.UseTrafficMonitoring = true;

            _instance.HeClick(("button", "class", "inline-flex\\ items-center\\ justify-center\\ whitespace-nowrap\\ text-sm\\ font-medium\\ transition-colors\\ focus-visible:outline-hidden\\ focus-visible:ring-1\\ focus-visible:ring-primary\\ disabled:pointer-events-none\\ disabled:opacity-50\\ border\\ bg-background\\ hover:border-primary/50\\ shadow-xs\\ hover:bg-primary\\ hover:text-primary-foreground\\ hover:ring-primary/60\\ hover:ring-2\\ active:bg-background\\ active:text-primary\\ active:ring-0\\ px-4\\ py-2\\ h-10\\ truncate\\ rounded-full\\ pr-1\\ w-auto\\ gap-4", "regexp", 0));


        get:
            _project.Deadline(10);
            Thread.Sleep(1000);
            var traffic = _instance.ActiveTab.GetTraffic();
            var data = new Dictionary<string, string>();
            //string param;
            foreach (var t in traffic)
            {
                if (t.Url.Contains(url))
                {

                    var Method = t.Method;
                    var ResultCode = t.ResultCode.ToString();
                    var Url = t.Url;
                    var ResponseContentType = t.ResponseContentType;
                    var RequestHeaders = t.RequestHeaders;
                    var RequestCookies = t.RequestCookies;
                    var RequestBody = t.RequestBody;
                    var ResponseHeaders = t.ResponseHeaders;
                    var ResponseCookies = t.ResponseCookies;
                    var ResponseBody = t.ResponseBody == null ? "" : Encoding.UTF8.GetString(t.ResponseBody, 0, t.ResponseBody.Length);

                    if (Method == "OPTIONS") continue;
                    data.Add("Method", Method);
                    data.Add("ResultCode", ResultCode);
                    data.Add("Url", Url);
                    data.Add("ResponseContentType", ResponseContentType);
                    data.Add("RequestHeaders", RequestHeaders);
                    data.Add("RequestCookies", RequestCookies);
                    data.Add("RequestBody", RequestBody);
                    data.Add("ResponseHeaders", ResponseHeaders);
                    data.Add("ResponseCookies", ResponseCookies);
                    data.Add("ResponseBody", ResponseBody);
                    break;
                }

            }
            if (data.Count == 0) goto get;


            string headersString = data["RequestHeaders"].Trim();//RequestCookies
            _project.L0g(headersString);

            string headerToGet = "authorization";
            var headers = headersString.Split('\n');

            foreach (string header in headers)
            {
                if (header.ToLower().Contains("authorization"))
                {
                    _project.Var("token", header.Split(':')[1]);
                }
                if (header.ToLower().Contains("cookie"))
                {
                    _project.Var("cookie", header.Split(':')[1]);
                }

            }
            _instance.HeClick(("button", "class", "inline-flex\\ items-center\\ justify-center\\ whitespace-nowrap\\ text-sm\\ font-medium\\ transition-colors\\ focus-visible:outline-hidden\\ focus-visible:ring-1\\ focus-visible:ring-primary\\ disabled:pointer-events-none\\ disabled:opacity-50\\ border\\ bg-background\\ hover:border-primary/50\\ shadow-xs\\ hover:bg-primary\\ hover:text-primary-foreground\\ hover:ring-primary/60\\ hover:ring-2\\ active:bg-background\\ active:text-primary\\ active:ring-0\\ px-4\\ py-2\\ h-10\\ truncate\\ rounded-full\\ pr-1\\ w-auto\\ gap-4", "regexp", 0), emu: 1);

        }

        public string ReqGet(string path, bool parse = false , bool log = false)
        {

            string token = _project.Variables["token"].Value;
            string cookie = _project.Variables["cookie"].Value;

            var headers = new Dictionary<string, string>
            {
                { "authority", "chat.chainopera.ai" },
                { "authorization", $"{token}" },
                { "method", "GET" },
                { "path", path },
                { "accept", "application/json, text/plain, */*" },
                { "accept-encoding", "gzip, deflate, br" },
                { "accept-language", "en-US,en;q=0.9" },
                { "content-type", "application/json" },
                { "origin", "https://chat.chainopera.ai" },
                { "priority", "u=1, i" },

                { "sec-ch-ua", "\"Chromium\";v=\"134\", \"Not:A-Brand\";v=\"24\", \"Google Chrome\";v=\"134\"" },
                { "sec-ch-ua-mobile", "?0" },
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "sec-fetch-dest", "empty" },
                { "sec-fetch-mode", "cors" },
                { "sec-fetch-site", "same-site" },

                { "cookie", $"{cookie}" },
            };



            string[] headerArray = headers.Select(header => $"{header.Key}:{header.Value}").ToArray();
            string url = $"https://chat.chainopera.ai{path}";
            string response;

            try
            {
                response = _project.GET(url, "+", headerArray, log: false, parse);
                _logger.Send(response);
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Err HTTPreq: {ex.Message}");
                throw;
            }
            
            return response;






        }


    }









}


