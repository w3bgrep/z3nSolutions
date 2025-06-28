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



    public class Accountant
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Sql _sql;
        private readonly Logger _logger;
        private int _offset; 

        public Accountant(IZennoPosterProjectModel project,  bool log = false)
        {
            _project = project;
  
            _logger = new Logger(project, log: log, classEmoji: "$");
            _sql = new Sql(project, log: log);
        }

        public void ShowBalanceTable(string chains = null)
        {
            var tableName = "public_native";
            var columns = new List<string>();
            
            if (string.IsNullOrEmpty(chains))
                columns = new Sql(_project).GetColumnList("public_native");

            else
                columns = chains.Split(',').ToList();


            int pageSize = 100;
            _offset = 0; // Инициализируем offset
            int totalRows = int.Parse(_project.Variables["rangeEnd"].Value);

            var form = CreateForm();
            var panel = CreatePaginationPanel(form);
            var prevButton = CreatePreviousButton(panel);
            var nextButton = CreateNextButton(panel);
            var grid = CreateDataGridView(form);

            Action loadData = () => LoadData(grid, columns, tableName, pageSize, ref _offset, totalRows, prevButton, nextButton);
            loadData();

            ConfigurePaginationEvents(prevButton, nextButton, pageSize, totalRows, loadData);
            ConfigureCellFormatting(grid, columns);

            form.ShowDialog();
        }

        private System.Windows.Forms.Form CreateForm()
        {
            var form = new System.Windows.Forms.Form
            {
                Text = "Balance Table",
                Width = 1008,
                Height = 800,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
            };
            form.BackColor = System.Drawing.Color.White;
            form.TopMost = true;
            return form;
        }

        private System.Windows.Forms.Panel CreatePaginationPanel(System.Windows.Forms.Form form)
        {
            var panel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 40
            };
            form.Controls.Add(panel);
            return panel;
        }

        private System.Windows.Forms.Button CreatePreviousButton(System.Windows.Forms.Panel panel)
        {
            var prevButton = new System.Windows.Forms.Button
            {
                Text = "Previous",
                Width = 100,
                Height = 30,
                Left = 10,
                Top = 5,
                Enabled = false
            };
            panel.Controls.Add(prevButton);
            return prevButton;
        }

        private System.Windows.Forms.Button CreateNextButton(System.Windows.Forms.Panel panel)
        {
            var nextButton = new System.Windows.Forms.Button
            {
                Text = "Next",
                Width = 100,
                Height = 30,
                Left = 120,
                Top = 5
            };
            panel.Controls.Add(nextButton);
            return nextButton;
        }

        private System.Windows.Forms.DataGridView CreateDataGridView(System.Windows.Forms.Form form)
        {
            var grid = new System.Windows.Forms.DataGridView
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                AllowUserToResizeColumns = true,
                AllowUserToResizeRows = true,
                ScrollBars = System.Windows.Forms.ScrollBars.Both,
                AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells,
                ReadOnly = true,
                BackgroundColor = System.Drawing.Color.White
            };
            form.Controls.Add(grid);
            grid.BringToFront();
            return grid;
        }

        private void LoadData(System.Windows.Forms.DataGridView grid, List<string> columns, string tableName, int pageSize, ref int offset, int totalRows, System.Windows.Forms.Button prevButton, System.Windows.Forms.Button nextButton)
        {
            string result = new Sql(_project).Get($"{string.Join(",", columns)}", tableName, where: $"acc0 <= '{_project.Var("rangeEnd")}' ORDER BY acc0");
                

            if (string.IsNullOrEmpty(result))
            {
                _project.SendWarningToLog("No data found in balance table");
                grid.Rows.Clear();
                return;
            }

            var rows = result.Trim().Split('\n');
            _project.SendInfoToLog($"Loaded {rows.Length} rows from {tableName}, offset {offset}", false);

            grid.Columns.Clear();
            grid.Rows.Clear();

            foreach (var col in columns)
            {
                var column = new System.Windows.Forms.DataGridViewColumn
                {
                    Name = col.Trim(),
                    HeaderText = col.Trim(),
                    CellTemplate = new System.Windows.Forms.DataGridViewTextBoxCell(),
                    SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
                };
                grid.Columns.Add(column);
            }

            foreach (var row in rows)
            {
                var values = row.Split('|');
                if (values.Length != columns.Count)
                {
                    _project.SendWarningToLog($"Invalid row format: {row}. Expected {columns.Count} columns, got {values.Length}", false);
                    continue;
                }

                var formattedValues = new string[values.Length];
                formattedValues[0] = values[0]; // acc0 без изменений
                for (int i = 1; i < values.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(values[i]))
                    {
                        formattedValues[i] = "0.0000000";
                        continue;
                    }
                    try
                    {
                        string val = values[i].Replace(",", ".");
                        if (decimal.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
                        {
                            formattedValues[i] = balance.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            formattedValues[i] = "0.0000000";
                            _project.SendWarningToLog($"Invalid format in {columns[i]}, row {grid.Rows.Count + 1}: '{values[i]}'", false);
                        }
                    }
                    catch (Exception ex)
                    {
                        formattedValues[i] = "0.0000000";
                        _project.SendWarningToLog($"Error formatting in {columns[i]}, row {grid.Rows.Count + 1}: {ex.Message}", false);
                    }
                }
                grid.Rows.Add(formattedValues);
            }

            prevButton.Enabled = offset > 0;
            nextButton.Enabled = offset + pageSize < totalRows;

            int totalWidth = grid.Columns.Cast<System.Windows.Forms.DataGridViewColumn>().Sum(col => col.Width);
            if (totalWidth < 1000) totalWidth = 1000;
            grid.Width = totalWidth;
            grid.FindForm().Width = Math.Min(totalWidth - 100, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width);

            var sumRow = CalculateColumnSums(grid, columns);
            grid.Rows.Add(sumRow);

            // Визуальное выделение строки суммы
            var lastRow = grid.Rows[grid.Rows.Count - 1];
            lastRow.DefaultCellStyle.Font = new System.Drawing.Font(grid.Font, System.Drawing.FontStyle.Bold);
            lastRow.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

        }

        private void ConfigurePaginationEvents(System.Windows.Forms.Button prevButton, System.Windows.Forms.Button nextButton, int pageSize, int totalRows, Action loadData)
        {
            prevButton.Click += (s, e) =>
            {
                if (_offset >= pageSize) // Используем поле _offset
                {
                    _offset -= pageSize;
                    loadData();
                }
            };
            nextButton.Click += (s, e) =>
            {
                if (_offset + pageSize < totalRows)
                {
                    _offset += pageSize;
                    loadData();
                }
            };
        }

        private void ConfigureCellFormatting(System.Windows.Forms.DataGridView grid, List<string> columns)
        {
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (grid.Columns[e.ColumnIndex].Name == "acc0")
                {
                    e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9, System.Drawing.FontStyle.Bold);
                    e.CellStyle.BackColor = System.Drawing.Color.Black;
                    e.CellStyle.ForeColor = System.Drawing.Color.White;
                    return;
                }

                string value = grid[e.ColumnIndex, e.RowIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(value)) return;

                try
                {
                    if (decimal.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
                    {
                        e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9);
                        if (balance >= 0.1m)
                            e.CellStyle.BackColor = System.Drawing.Color.SteelBlue;
                        else if (balance >= 0.01m)
                            e.CellStyle.BackColor = System.Drawing.Color.Green;
                        else if (balance >= 0.001m)
                            e.CellStyle.BackColor = System.Drawing.Color.YellowGreen;
                        else if (balance >= 0.0001m)
                            e.CellStyle.BackColor = System.Drawing.Color.Khaki;
                        else if (balance >= 0.00001m)
                            e.CellStyle.BackColor = System.Drawing.Color.LightSalmon;
                        else if (balance > 0)
                            e.CellStyle.BackColor = System.Drawing.Color.IndianRed;
                        else if (balance == 0)
                        {
                            e.CellStyle.BackColor = System.Drawing.Color.White;
                            e.CellStyle.ForeColor = System.Drawing.Color.White;
                        }
                        else
                            e.CellStyle.BackColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        _project.SendWarningToLog($"Invalid balance format in {grid.Columns[e.ColumnIndex].Name}, row {e.RowIndex + 1}: '{value}'", false);
                    }
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error parsing balance in {grid.Columns[e.ColumnIndex].Name}, row {e.RowIndex + 1}: {ex.Message}", false);
                }
            };
        }

        private string[] CalculateColumnSums(System.Windows.Forms.DataGridView grid, List<string> columns)
        {
            var sums = new string[columns.Count];
            sums[0] = "ИТОГО"; // Первый столбец — подпись

            for (int col = 1; col < columns.Count; col++)
            {
                decimal sum = 0;
                foreach (System.Windows.Forms.DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    var val = row.Cells[col].Value?.ToString();
                    if (decimal.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal d))
                        sum += d;
                }
                sums[col] = sum.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture);
            }
            return sums;
        }

        public void ShowBalanceTableFromList(List<string> data)
        {
            var columns = new List<string> { "Счет", "Баланс" };

            // Создаём форму и грид
            var form = CreateForm();
            var grid = CreateDataGridView(form);

            // Очищаем и настраиваем грид
            grid.Columns.Clear();
            grid.Rows.Clear();
            grid.AllowUserToAddRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            // Добавляем столбцы
            grid.Columns.Add("acc", "Счет");
            grid.Columns.Add("balance", "Баланс");

            // Заполняем строки из списка
            decimal sum = 0;
            foreach (var line in data)
            {
                var parts = line.Split(':');
                if (parts.Length != 2)
                    continue;
                string acc = parts[0].Trim();
                string balanceStr = parts[1].Replace(",", ".").Trim();
                if (!decimal.TryParse(balanceStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
                    balance = 0;
                sum += balance;
                grid.Rows.Add(acc, balance.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture));
            }

            // Добавляем итоговую строку
            int sumRowIdx = grid.Rows.Add("TOTAL", sum.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture));
            var sumRow = grid.Rows[sumRowIdx];
            sumRow.DefaultCellStyle.Font = new System.Drawing.Font(grid.Font, System.Drawing.FontStyle.Bold);
            sumRow.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

            // Жирная линия под итоговой строкой
            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex == sumRowIdx && e.RowIndex >= 0)
                {
                    e.Paint(e.CellBounds, System.Windows.Forms.DataGridViewPaintParts.All);
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black, 3))
                    {
                        e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }
                    e.Handled = true;
                }
            };

            // Форматирование балансов (цвета)
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
                string value = grid[e.ColumnIndex, e.RowIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(value)) return;
                if (decimal.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
                {
                    e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9);
                    if (balance >= 0.1m)
                        e.CellStyle.BackColor = System.Drawing.Color.SteelBlue;
                    else if (balance >= 0.01m)
                        e.CellStyle.BackColor = System.Drawing.Color.Green;
                    else if (balance >= 0.001m)
                        e.CellStyle.BackColor = System.Drawing.Color.YellowGreen;
                    else if (balance >= 0.0001m)
                        e.CellStyle.BackColor = System.Drawing.Color.Khaki;
                    else if (balance >= 0.00001m)
                        e.CellStyle.BackColor = System.Drawing.Color.LightSalmon;
                    else if (balance > 0)
                        e.CellStyle.BackColor = System.Drawing.Color.IndianRed;
                    else if (balance == 0)
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.White;
                        e.CellStyle.ForeColor = System.Drawing.Color.White;
                    }
                    else
                        e.CellStyle.BackColor = System.Drawing.Color.White;
                }
            };

            form.ShowDialog();
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


