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



    public class Unlock
    {

        protected readonly IZennoPosterProjectModel _project;
        protected readonly bool _logShow;
        protected readonly Sql _sql;
        protected readonly string _jsonRpc;
        protected readonly Blockchain _blockchain;
        protected readonly string _abi = @"[
                        {
                            ""inputs"": [
                            {
                                ""internalType"": ""uint256"",
                                ""name"": ""_tokenId"",
                                ""type"": ""uint256""
                            }
                            ],
                            ""name"": ""keyExpirationTimestampFor"",
                            ""outputs"": [
                            {
                                ""internalType"": ""uint256"",
                                ""name"": """",
                                ""type"": ""uint256""
                            }
                            ],
                            ""stateMutability"": ""view"",
                            ""type"": ""function""
                        },
                        {
                            ""inputs"": [
                            {
                                ""internalType"": ""uint256"",
                                ""name"": ""_tokenId"",
                                ""type"": ""uint256""
                            }
                            ],
                            ""name"": ""ownerOf"",
                            ""outputs"": [
                            {
                                ""internalType"": ""address"",
                                ""name"": """",
                                ""type"": ""address""
                            }
                            ],
                            ""stateMutability"": ""view"",
                            ""type"": ""function""
                        }
                    ]";


        public Unlock(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project);
            _logShow = log;
            _jsonRpc = new W3b(project).Rpc("optimism");
            _blockchain = new Blockchain(_jsonRpc);
        }

        public string keyExpirationTimestampFor(string addressTo, int tokenId, bool decode = true)
        {
            try
            {
                string[] types = { "uint256" };
                object[] values = { tokenId };

                string result = _blockchain.ReadContract(addressTo, "keyExpirationTimestampFor", _abi, values).Result;
                if (decode) result = ProcessExpirationResult(result);
                //if (decode) result = Decode(result, "keyExpirationTimestampFor");
                return result;
            }
            catch (Exception ex)
            {
                _project.L0g(ex.InnerException?.Message ?? ex.Message);
                throw;
            }
        }

        public string ownerOf(string addressTo, int tokenId, bool decode = true)
        {
            try
            {
                string[] types = { "uint256" };
                object[] values = { tokenId };
                string result = _blockchain.ReadContract(addressTo, "ownerOf", _abi, values).Result;
                if (decode) result = Decode(result, "ownerOf");
                return result;
            }
            catch (Exception ex)
            {
                _project.L0g(ex.InnerException?.Message ?? ex.Message);
                throw;
            }
        }

        public string Decode(string toDecode, string function)
        {
            if (string.IsNullOrEmpty(toDecode))
            {
                _project.L0g("Result is empty, nothing to decode");
                return string.Empty;
            }

            if (toDecode.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) toDecode = toDecode.Substring(2);
            if (toDecode.Length < 64) toDecode = toDecode.PadLeft(64, '0');


            var decodedDataExpire = ZBSolutions.Decoder.AbiDataDecode(_abi, function, "0x" + toDecode);
            string decodedResultExpire = decodedDataExpire.Count == 1
                ? decodedDataExpire.First().Value
                : string.Join("\n", decodedDataExpire.Select(item => $"{item.Key};{item.Value}"));

            return decodedResultExpire;
        }

        string ProcessExpirationResult(string resultExpire)
        {
            if (string.IsNullOrEmpty(resultExpire))
            {
                _project.SendToLog("Result is empty, nothing to decode", LogType.Warning, true, LogColor.Yellow);
                return string.Empty;
            }

            if (resultExpire.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                resultExpire = resultExpire.Substring(2);
            }

            if (resultExpire.Length < 64)
            {
                resultExpire = resultExpire.PadLeft(64, '0');
            }

            var decodedDataExpire = ZBSolutions.Decoder.AbiDataDecode(_abi, "keyExpirationTimestampFor", "0x" + resultExpire);
            string decodedResultExpire = decodedDataExpire.Count == 1
                ? decodedDataExpire.First().Value
                : string.Join("\n", decodedDataExpire.Select(item => $"{item.Key};{item.Value}"));

            // project.Variables["lastTimeStamp"].Value = decodedResultExpire;
            // project.Variables["blockchainDecodedResult"].Value = decodedResultExpire;
            // project.Variables["a0debug"].Value = decodedResultExpire;
            // project.Variables["resultExpire"].Value = decodedResultExpire;

            return decodedResultExpire;
        }

        public Dictionary<string, string> Holders(string contract)
        {
            var result = new Dictionary<string, string>();
            int i = 0;
            while (true)
            {
                i++;
                var owner = ownerOf(contract, i);
                if (owner == "0x0000000000000000000000000000000000000000") break;
                var exp = keyExpirationTimestampFor(contract, i);
                result.Add(owner.ToLower(), exp.ToLower());
            }
            return result;


        }

    }



    public class ZerionWallet2 : Wlt
    {
        protected readonly string _extId = "klghhnkeealcohjjanjjdaeeggmfmlpl";
        protected readonly string _popupUrl = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#";
        protected readonly string _sidepanelUrl = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#";
        protected readonly string _importPage = "/get-started/import";
        protected readonly string _selectPage = "/wallet-select";
        protected readonly string _historyPage = "/overview/history";
        protected readonly string _fileName;
        protected readonly string _publicFromKey;
        protected readonly string _publicFromSeed;

        public ZerionWallet2(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string seed = null)
            : base(project, instance, log)
        {
            _fileName = "Zerion1.21.3.crx";
            _key = KeyCheck(key);
            _seed = SeedCheck(seed);
            _publicFromKey = _key.ToPubEvm();
            _publicFromSeed = _seed.ToPubEvm();
        }

        private string KeyCheck(string key)
        {
            if (string.IsNullOrEmpty(key))
                key = Decrypt(KeyT.secp256k1);
            if (string.IsNullOrEmpty(key))
                key =string.Empty;
                Log("!Key is empty");
            return key;
        }
        private string SeedCheck(string seed)
        {
            if (string.IsNullOrEmpty(seed))
                seed = Decrypt(KeyT.bip39);
            if (string.IsNullOrEmpty(seed))
                seed =string.Empty;
                Log("!Seed is empty");
                //throw new Exception("emptykey");
            return seed;
        }


        public void Go(string page = null, string mode = "sidepanel", bool newtab = false)
        {
            if (newtab) { Tab tab2 = _instance.NewTab("zw"); }


            string sourseLink;
            string method;
            if (mode == "sidepanel") sourseLink = _sidepanelUrl;
            else sourseLink = _popupUrl;

            switch (page)
            {
                case "import":
                    method = _importPage;
                    break;
                case "select":
                    method = _selectPage;
                    break;
                case "history":
                    method = _historyPage;
                    break;
                default:
                    method = null;
                    break;
            }

            _instance.ActiveTab.Navigate(sourseLink + method, "");
        }
        public void Add(string source = "seed", bool log = false)
        {

            if (!_instance.ActiveTab.URL.Contains(_importPage)) Go("import");

            if (source == "pkey") source = _key;
            else if (source == "seed") source = _seed;

            _instance.HeSet(("seedOrPrivateKey", "name"), source);
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
        public void SwitchSource(string addressToUse = "key")
        {

            _project.Deadline();

            if (addressToUse == "key") addressToUse = _publicFromKey;
            else if (addressToUse == "seed") addressToUse = _publicFromSeed;
            else throw new Exception("supports \"key\" | \"seed\" only");
            go:
            Go("select");
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

                Log($"[{masked}]{masked.ChkAddress(addressToUse)}[{addressToUse}]");

                if (masked.ChkAddress(addressToUse))
                {
                    _instance.HeClick(wallet);
                    return;
                }
            }
            Log("address not found");
            Add("seed");

            _instance.CloseExtraTabs(true);
            goto go;


        }
        public bool Sign(bool log = false)
        {
            var urlNow = _instance.ActiveTab.URL;
            try {

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
                Log($"Sending {datastring} to {recipient}, gas: {gasGwei}");

            }
            catch { }

            try
            {
                var button = _instance.HeGet(("button", "class", "_primary", "regexp", 0));
                Log(button, log: log);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                return true;
            }
            catch (Exception ex)
            {
                Log($"!W {ex.Message}", log: log);
                throw;
            }
        }
        public void Launch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            if (Install(_extId, fileName)) ZerionImport(log: log);
            else
            {
                ZerionUnlock(log: false);
                ZerionCheck(log: log);
            }
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }
        public void TestnetMode(bool testMode = false)
        {
            bool current;

            string testmode = _instance.HeGet(("input:checkbox", "fulltagname", "input:checkbox", "text", 0),atr:"value");

            if (testmode == "True") 
                current = true;
            else 
                current = false;
                
            if (testMode != current)
                _instance.HeClick(("input:checkbox", "fulltagname", "input:checkbox", "text", 0));

        }








        public void ZerionLnch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            if (Install(_extId, fileName)) ZerionImport(log: log);
            else
            {
                ZerionUnlock(log: false);
                ZerionCheck(log: log);
            }
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }

        public bool ZerionImport(string source = "pkey", string refCode = null, bool log = false)
        {
            if (string.IsNullOrWhiteSpace(refCode))
            {

                refCode = _sql.DbQ(@"SELECT referralCode
                FROM projects.zerion
                WHERE referralCode != '_' 
                AND TRIM(referralCode) != ''
                ORDER BY RANDOM()
                LIMIT 1;");
            }

            var inputRef = true;
            _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import", "regexp", 0));
            if (source == "pkey")
            {
                _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/private-key", "regexp", 0));
                string key = _key;
                _instance.ActiveTab.FindElementByName("key").SetValue(key, "Full", false);
            }
            else if (source == "seed")
            {
                _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/mnemonic", "regexp", 0));
                string seedPhrase = _seed;
                int index = 0;
                foreach (string word in seedPhrase.Split(' '))
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
            return true;
        }

        public void ZerionUnlock(bool log = false)
        {
            Go();
            //_instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview", "");

            string active = null;
            try
            {
                active = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0),deadline:2);
            }
            catch
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                active = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0));
            }
            Log(active, log: log);
        }

        public string ZerionCheck(bool log = false)
        {
            if (_instance.ActiveTab.URL != "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview")
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview", "");

            var active = _instance.HeGet(("div", "class", "_uitext_", "regexp", 0));
            var balance = _instance.HeGet(("div", "class", "_uitext_", "regexp", 1));
            var pnl = _instance.HeGet(("div", "class", "_uitext_", "regexp", 2));

            Log($"{active} {balance} {pnl}", log: log);
            return active;
        }

        public bool ZerionApprove(bool log = false)
        {

            try
            {
                var button = _instance.HeGet(("button", "class", "_primary", "regexp", 0));
                Log(button, log: log);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                return true;
            }
            catch (Exception ex)
            {
                Log($"!W {ex.Message}", log: log);
                throw;
            }
        }
        public void ZerionConnect(bool log = false)
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
        public bool ZerionWaitTx(int deadline = 60, bool log = false)
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
                Log($"unknown status {status}");
                goto check;
            }
            _instance.CloseExtraTabs();
            return result;

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

        string state = null;

        if (!connectedButton.FindChildByAttribute("img", "alt", "Zerion", "regexp", 0).IsVoid) connected.Add("Zerion");//state += "Zerion";
        if (!connectedButton.FindChildByAttribute("img", "alt", "Backpack", "regexp", 0).IsVoid) connected.Add("Backpack");
        else if (unconnectedButton.InnerText == "Connect Wallet") state = "Connect";


            if (connected.Contains(wallet))
            {
                _project.L0g($"{connectedButton.InnerText} connected with {wallet}");
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

 }


}
