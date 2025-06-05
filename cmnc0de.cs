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

using Newtonsoft.Json.Linq;

#endregion

namespace w3tools //by @w3bgrep
{

    public static class TestStatic
    {
   



        public static string UnixToHuman(this string decodedResultExpire)
        {
            if (!string.IsNullOrEmpty(decodedResultExpire))
            {
                int intEpoch = int.Parse(decodedResultExpire);
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(intEpoch).ToShortDateString();
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



        private static readonly object FileLock = new object();

        public static string GetNewCreds(this IZennoPosterProjectModel project, string dataType)
        {
            string pathFresh = $"{project.Path}.data\\fresh\\{dataType}.txt";
            string pathUsed = $"{project.Path}.data\\used\\{dataType}.txt";

            lock (FileLock)
            {
                try
                {
                    if (!File.Exists(pathFresh))
                    {
                        project.SendWarningToLog($"File not found: {pathFresh}");
                        return null;
                    }

                    var freshAccs = File.ReadAllLines(pathFresh).ToList();
                    project.SendInfoToLog($"Loaded {freshAccs.Count} accounts from {pathFresh}");

                    if (freshAccs.Count == 0)
                    {
                        project.SendInfoToLog($"No accounts available in {pathFresh}");
                        return string.Empty;
                    }

                    string creds = freshAccs[0];
                    freshAccs.RemoveAt(0);

                    File.WriteAllLines(pathFresh, freshAccs);
                    File.AppendAllText(pathUsed, creds + Environment.NewLine);

                    return creds;
                }
                catch (Exception ex)
                {
                    project.SendWarningToLog($"Error processing files for {dataType}: {ex.Message}");
                    return null;
                }
            }

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

        public static string NewNickName()
        {
            // Списки слов для комбинации
            string[] adjectives = {
        "Sunny", "Mystic", "Wild", "Cosmic", "Shadow", "Lunar", "Blaze", "Dream", "Star", "Vivid",
        "Frost", "Neon", "Gloomy", "Swift", "Silent", "Fierce", "Radiant", "Dusk", "Nova", "Spark",
        "Crimson", "Azure", "Golden", "Midnight", "Velvet", "Stormy", "Echo", "Vortex", "Phantom", "Bright",
        "Chill", "Rogue", "Daring", "Lush", "Savage", "Twilight", "Crystal", "Zesty", "Bold", "Hazy",
        "Vibrant", "Gleam", "Frosty", "Wicked", "Serene", "Bliss", "Rusty", "Hollow", "Sleek", "Pale"
        };

            // Список существительных (50 элементов)
            string[] nouns = {
            "Wolf", "Viper", "Falcon", "Spark", "Catcher", "Rider", "Echo", "Flame", "Voyage", "Knight",
            "Raven", "Hawk", "Storm", "Tide", "Drift", "Shade", "Quest", "Blaze", "Wraith", "Comet",
            "Lion", "Phantom", "Star", "Cobra", "Dawn", "Arrow", "Ghost", "Sky", "Vortex", "Wave",
            "Tiger", "Ninja", "Dreamer", "Seeker", "Glider", "Rebel", "Spirit", "Hunter", "Flash", "Beacon",
            "Jaguar", "Drake", "Scout", "Path", "Glow", "Riser", "Shadow", "Bolt", "Zephyr", "Forge"
        };

            // Список суффиксов (10 элементов, как расширение)
            string[] suffixes = { "", "", "", "", "", "X", "Z", "Vibe", "Glow", "Rush", "Peak", "Core", "Wave", "Zap" };

            // Потокобезопасный генератор случайных чисел
            Random random = new Random(Guid.NewGuid().GetHashCode());

            // Выбираем случайные слова
            string adjective = adjectives[random.Next(adjectives.Length)];
            string noun = nouns[random.Next(nouns.Length)];
            string suffix = suffixes[random.Next(suffixes.Length)];

            // Комбинируем никнейм
            string nickname = $"{adjective}{noun}{suffix}";

            // Убедимся, что никнейм не слишком длинный (например, до 15 символов, как на TikTok)
            if (nickname.Length > 15)
            {
                nickname = nickname.Substring(0, 15);
            }

            return nickname;
        }


    }


    public class Tiktok
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _token;
        protected string _login;
        protected string _pass;
        protected string _2fa;
        protected string _email;
        protected string _email_pass;

        public Tiktok(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logShow = log;

            //LoadCreds();

        }

        protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 💠  {callerName}] [{tolog}] ");
        }

        private string LoadCreds()
        {
            string[] xCreds = _sql.Get(" status, token, login, password, otpsecret, email, emailpass", "private_tiktok").Split('|');
            _status = xCreds[0];
            _token = xCreds[1];
            _login = xCreds[2];
            _pass = xCreds[3];
            _2fa = xCreds[4];
            _email = xCreds[5];
            _email_pass = xCreds[6];
            try
            {
                _project.Variables["ttStatus"].Value = _status;
                _project.Variables["ttToken"].Value = _token;
                _project.Variables["ttLogin"].Value = _login;
                _project.Variables["ttPass"].Value = _pass;
                _project.Variables["tt2fa"].Value = _2fa;
                _project.Variables["ttEmail"].Value = _email;
                _project.Variables["ttEmailPass"].Value = _email_pass;
            }
            catch (Exception ex)
            {
                _project.SendInfoToLog(ex.Message);
            }

            return _status;

        }

        public string GetCurrent()
        {

            string acc = _instance.HeGet(("a", "href", "https://www.tiktok.com/@", "regexp", 0), atr: "href").Split('@')[1].Trim();
            Log(acc);
            return acc;

        }


    }




    public class Htt 
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly bool _logShow;


        public Htt(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _logShow = log;
        }
                protected void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;
            _project.L0g($"[ 🌍 {callerName}] [{message}]");
        }
public string GET(
            string url,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
        {
            string debugHeaders = string.Empty;
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(15);

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent; // Same as in POST
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                    }

                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        Log("Set-Cookie found: " + cookies, callerName);
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    //if (parse) ParseJson(result);
                    Log($"{result}", callerName);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                Log($"[GET] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), Headers\n{debugHeaders.Trim()}", callerName);
                if (throwOnFail) throw;

                return string.Empty;
            }
            catch (Exception e)
            {
                Log($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) Headers\n{debugHeaders.Trim()}", callerName);
                if (throwOnFail) throw;

                return string.Empty;
            }
        }
        public WebProxy ParseProxy(string proxyString, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(proxyString))
            {
                return null;
            }
            if (proxyString == "+") proxyString = _project.Variables["proxy"].Value;
            try
            {
                WebProxy proxy = new WebProxy();

                if (proxyString.Contains("//")) proxyString = proxyString.Split('/')[2];

                if (proxyString.Contains("@")) // Прокси с авторизацией (login:pass@proxy:port)
                {
                    string[] parts = proxyString.Split('@');
                    string credentials = parts[0];
                    string proxyHost = parts[1];

                    proxy.Address = new Uri("http://" + proxyHost);
                    string[] creds = credentials.Split(':');
                    proxy.Credentials = new NetworkCredential(creds[0], creds[1]);

                    Log($"proxy set:{proxyHost}", callerName);
                }
                else // Прокси без авторизации (proxy:port)
                {
                    proxy.Address = new Uri("http://" + proxyString);
                    Log($"proxy set: ip:{proxyString}", callerName);
                }

                return proxy;
            }
            catch (Exception e)
            {
                Log($"Ошибка настройки прокси: {e.Message}", callerName, true);
                return null;
            }
        }


        public bool CheckProxy(string proxyString = null)
        {
            if (string.IsNullOrEmpty(proxyString))
                proxyString = new Sql(_project).Get("proxy", "private_profile");

            //WebProxy proxy = ParseProxy(proxyString);

            string ipLocal = GET("http://api.ipify.org/", null);
            string ipProxified = GET("http://api.ipify.org/", proxyString);

            Log($"ipLocal: {ipLocal}, ipProxified: {ipProxified}");

            // Проверяем валидность IP один раз
            bool isValidIp = System.Net.IPAddress.TryParse(ipProxified, out var ipAddress);

            if (isValidIp && ipProxified != ipLocal)
            {
                Log($"proxy validated: {ipProxified}");
                _project.Var("proxy", proxyString);
                return true;
            }
            else
            {
                throw new Exception($"!W proxy failed: ipLocal: [{ipLocal}], ipProxified: [{ipProxified}]. {(isValidIp ? "Proxy was not applied" : "Invalid IP format")}");
                //_project.L0g($"!W proxy failed: ipLocal: [{ipLocal}], ipProxified: [{ipProxified}]. {(isValidIp ? "Proxy was not applied" : "Invalid IP format")}");
            }
            //throw new Exception("");
            return false;
        }
    }

 public class Stargate2
 {

     protected readonly IZennoPosterProjectModel _project;
     protected readonly Instance _instance;
     protected readonly bool _logShow;


     public Stargate2(IZennoPosterProjectModel project, Instance instance, bool log = false)
     {
         _project = project;
         _instance = instance;
         _logShow = log;
     }

     public void Go(string srcChain, string dstChain, string srcToken = null, string dstToken = null)
     {
         var srcDefault = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE";
         if (string.IsNullOrEmpty(srcToken)) srcToken = srcDefault;
         if (string.IsNullOrEmpty(dstToken)) dstToken = srcDefault;
         string url = "https://stargate.finance/bridge?" + $"srcChain={srcChain}" + $"&srcToken={srcToken}" + $"&dstChain={dstChain}" + $"&dstToken={dstToken}";
         if (_instance.ActiveTab.URL != url) _instance.ActiveTab.Navigate(url, "");
         _instance.HeClick(("path","d","M6 9.75h12l-3.5-3.5M18 14.25H6l3.5 3.5","regexp",0));

     }


     public void Connect()
     {
         _project.Deadline();
     check:

         _project.Deadline(60); Thread.Sleep(1000);

         var connectedButton = _instance.ActiveTab.FindElementByAttribute("button", "class", "css-x1wnqh", "regexp", 0);
         var unconnectedButton = _instance.ActiveTab.FindElementByAttribute("button", "sx", "\\[object\\ Object]", "regexp", 0).ParentElement;

         string state = null;

         if (!connectedButton.FindChildByAttribute("img", "alt", "Zerion", "regexp", 0).IsVoid) state = "Zerion";
         if (!connectedButton.FindChildByAttribute("img", "alt", "Backpack", "regexp", 0).IsVoid) state = "Backpack";
         else if (unconnectedButton.InnerText == "Connect Wallet") state = "Connect";

         switch (state)
         {
             case "Connect":
                 _instance.HeClick(unconnectedButton, emu: 1);
                 _instance.HeClick(("button", "innertext", "Zerion\\nConnect", "regexp", 0));
                 new ZerionWallet(_project, _instance).ZerionConnect();
                 goto check;

             case "Zerion":
                 _project.L0g($"{connectedButton.InnerText} connected with {state}");
                 break;

             default:
                 _project.L0g($"unknown state {connectedButton.InnerText}  {unconnectedButton.InnerText}");
                 goto check;

         }
     }

     public void Connect(string wallet)
     {

        var connected = new List<string>();
        _project.Deadline();
        check:

        _project.Deadline(60); Thread.Sleep(1000);

        var connectedButton = _instance.ActiveTab.FindElementByAttribute("button", "class", "css-x1wnqh", "regexp", 0);
        var unconnectedButton = _instance.ActiveTab.FindElementByAttribute("button", "sx", "\\[object\\ Object]", "regexp", 0).ParentElement;

        _project.L0g($"checking... {connectedButton.InnerText}  {unconnectedButton.InnerText}");
        if (unconnectedButton.IsVoid && connectedButton.IsVoid) goto check;

        string state = null;

        if (!connectedButton.FindChildByAttribute("img", "alt", "Zerion", "regexp", 0).IsVoid) connected.Add("Zerion");//state += "Zerion";
        if (!connectedButton.FindChildByAttribute("img", "alt", "Backpack", "regexp", 0).IsVoid) connected.Add("Backpack");
        else if (unconnectedButton.InnerText == "Connect Wallet") state = "Connect";


            if (connected.Contains(wallet))
            {
                _project.L0g($"{connectedButton.InnerText} connected with {wallet}");
                _instance.HeClick(("button", "class", "css-1k2e1h7", "regexp", 0),deadline:1,thr0w:false);
            }

            else if (wallet == "Zerion")
            {
                _instance.HeClick(unconnectedButton, emu: 1);
                _instance.HeClick(("button", "innertext", "Zerion\\nConnect", "regexp", 0));
                new ZerionWallet(_project, _instance).ZerionConnect();
                goto check;

            }
            


            else if (wallet == "Backpack" && connected.Contains("Zerion"))
            {
                _instance.HeClick(connectedButton, emu: 1);
                _instance.HeClick(("path", "d", "M14 8H2M8 2v12", "text", 0));
                _instance.HeClick(("div", "innertext", "Connect\\ Another\\ Wallet", "regexp", 0), "last", thr0w: false);
                _instance.HeClick(("img", "alt", "Backpack", "regexp", 0));
                _instance.HeClick(("img", "alt", "Backpack", "regexp", 0));

                goto check;

            }


            else
            {
                _project.L0g($"unknown state {connectedButton.InnerText}  {unconnectedButton.InnerText}");
                goto check;
            }

        }
        


     public decimal LoadBalance()
     {
         _project.Deadline();
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

     waitForBal:
         _project.Deadline(60);
         string est = _instance.HeGet(("div", "class", "css-n2rwim", "regexp", 0));

         try
         {
             decimal bal = decimal.Parse(est.Split('\n')[1].Replace("Balance: ", ""));
             return bal;
         }
         catch
         {
             goto waitForBal;
         }

     }


     public decimal WaitExpected()
     {
         _project.Deadline();
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

     waitForBal:
         _project.Deadline(60);
         string est = _instance.HeGet(("input:text", "class", "css-109vo2x", "regexp", 1), atr: "value");

         try
         {
             decimal expected = decimal.Parse(est);
             return expected;
         }
         catch
         {
             goto waitForBal;
         }

     }

     public void SetManualAddress(string address)
     {
         _instance.HeClick(("button", "innertext", "Advanced\\ Transfer", "regexp", 0));
         _instance.HeClick(("button", "role", "switch", "regexp", 1));
         _instance.HeSet(("input:text", "fulltagname", "input:text", "regexp", 1), address);
     }

     public void GasOnDestination(string qnt, string sliperage = "0.5")
     {
         _instance.HeSet(("input:text", "class", "css-1qhcc16", "regexp", 0), qnt);
         _instance.HeSet(("input:text", "class", "css-1qhcc16", "regexp", 1), sliperage);
     }

     public Dictionary<string,decimal> dicNative(bool log = false)
     {
        var chainsToUse = _project.Var("cfgChains").Split(',');
        var bls = new Dictionary<string,decimal>();
        var _w3b = new W3bRead(_project,log);
        foreach (string chain in chainsToUse)
        {
            try{	
                decimal native = _w3b.NativeEVM<decimal>(_w3b.Rpc(chain));
                bls.Add(chain,native);
            }
            catch
            {
                decimal native = _w3b.NativeSOL<decimal>();
                bls.Add(chain,native);            
            }
        }
        return bls;
     }

     public Dictionary<string,decimal> dicToken(bool log = false)
     {
        var chainsToUse = _project.Var("cfgChains").Split(',');
        var blsUsde = new Dictionary<string,decimal>();
        var _w3b = new W3bRead(_project,log);
        foreach (string chain in chainsToUse)
        {
            try{	
                decimal usdeBal = _w3b.BalERC20<decimal>("0x5d3a1Ff2b6BAb83b63cd9AD0787074081a52ef34",_w3b.Rpc(chain));
                blsUsde.Add(chain,usdeBal);
            }
            catch
            {
                decimal usdeBal = _w3b.TokenSPL<decimal>("DEkqHyPN7GMRJ5cArtQFAWefqbZb33Hyf6s5iCwjEonT");
                blsUsde.Add(chain,usdeBal);
                
            }
        }
        return blsUsde;
     }


 }


}
