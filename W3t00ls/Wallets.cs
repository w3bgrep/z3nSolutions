using System;
using System.Linq;

using System.Data;

using System.IO;
using System.Text.RegularExpressions;

using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NBitcoin;
using System.Collections.Generic;

namespace W3t00ls
{
    public enum W
    {
        MetaMask,
        Rabby,
        Backpack,
        Razor,
        Zerion,
        Keplr
    }
    public class Wlt
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly L0g _log;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;


        public Wlt(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _log = new L0g(_project);
            _logShow = log;
            _sql = new Sql(_project);
            _pass = SAFU.HWPass(_project);
        }

        public void WalLog(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _log.Send($"[ 👛  {callerName}] [{tolog}] ");
        }
        public bool Install(string extId, string fileName, bool log = false)
        {
            string path = $"{_project.Path}.crx\\{fileName}";
            var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
            if (!extListString.Contains(extId))
            {
                WalLog($"installing {fileName}", log:log);
                _instance.InstallCrxExtension(path);
                return true;
            }
            return false;
        }
        
    }

    public class WltMngr
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly bool _log;

        public WltMngr(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _log = log;
        }

        public void Launch(IEnumerable<W> requiredWallets, bool log = false)
        {
            WalLog($"Switching wallets: {string.Join(", ", requiredWallets)}", log: log || _log);
            foreach (var wallet in requiredWallets)
            {
                switch (wallet)
                {
                    case W.MetaMask:
                        new MetaMaskWallet(_project, _instance, log).MetaMaskLnch(log: log);
                        break;
                    case W.Rabby:
                        new RabbyWallet(_project, _instance, log).RabbyLnch(log: log);
                        break;
                    case W.Backpack:
                        new BackpackWallet(_project, _instance, log).BackpackLnch(log: log);
                        break;
                    case W.Razor:
                        new RazorWallet(_project, _instance, log).RazorLnch(log: log);
                        break;
                    case W.Zerion:
                        new ZerionWallet(_project, _instance, log).ZerionLnch(log: log);
                        break;
                    case W.Keplr:
                        new KeplrWallet(_project, _instance, log).KeplrLaunch(log: log);
                        break;
                    default:
                        WalLog($"Unknown wallet: {wallet}", log: log || _log);
                        break;
                }
            }
        }

        public void Launch(string requiredWallets, bool log = false)
        {
            var walletTypes = ParseWallets(requiredWallets);
            Launch(walletTypes, log);
        }

        public void Switch(string toUse = "", bool log = false)
        {


            WalLog($"switching extentions  {toUse}", log: log);

            try
            {
                if (_instance.BrowserType.ToString() == "Chromium" && _project.Variables["acc0"].Value != "")
                {
                    var wlt = new Wlt(_project, _instance, log);
                    string fileName = $"One-Click-Extensions-Manager.crx";
                    var managerId = "pbgjpgbpljobkekbhnnmlikbbfhbhmem";
                    wlt.Install(managerId, fileName, log);

                    var em = _instance.UseFullMouseEmulation;

                    int i = 0; string extName = ""; string outerHtml = ""; string extId = ""; string extStatus = "enabled";

                    while (_instance.ActiveTab.URL != "chrome-extension://pbgjpgbpljobkekbhnnmlikbbfhbhmem/index.html")
                    {
                        _instance.ActiveTab.Navigate("chrome-extension://pbgjpgbpljobkekbhnnmlikbbfhbhmem/index.html", "");
                        _instance.CloseExtraTabs();
                        WalLog($"URL is correct {_instance.ActiveTab.URL}", log: log);
                    }

                    while (!_instance.ActiveTab.FindElementByAttribute("button", "class", "ext-name", "regexp", i).IsVoid)
                    {
                        extName = Regex.Replace(_instance.ActiveTab.FindElementByAttribute("button", "class", "ext-name", "regexp", i).GetAttribute("innertext"), @" Wallet", "");
                        outerHtml = _instance.ActiveTab.FindElementByAttribute("li", "class", "ext\\ type-normal", "regexp", i).GetAttribute("outerhtml");
                        extId = Regex.Match(outerHtml, @"extension-icon/([a-z0-9]+)").Groups[1].Value;
                        if (outerHtml.Contains("disabled")) extStatus = "disabled";
                        if (toUse.Contains(extName) && extStatus == "disabled" || toUse.Contains(extId) && extStatus == "disabled" || !toUse.Contains(extName) && !toUse.Contains(extId) && extStatus == "enabled")
                            _instance.HeClick(("button", "class", "ext-name", "regexp", i));
                        i++;
                    }

                    _instance.CloseExtraTabs();
                    _instance.UseFullMouseEmulation = em;
                    WalLog($"Enabled  {toUse}", log: log);
                }

            }
            catch
            {
                try
                {
                    string securePrefsPath = _project.Variables["pathProfileFolder"].Value + @"\Default\Secure Preferences";
                    string json = File.ReadAllText(securePrefsPath);
                    JObject jObj = JObject.Parse(json);
                    JObject settings = (JObject)jObj["extensions"]?["settings"];

                    if (settings == null)
                    {
                        throw new Exception("Секция extensions.settings не найдена");
                    }

                    bool changesMade = false;
                    foreach (var extension in settings)
                    {
                        string extId = extension.Key;
                        JObject extData = (JObject)extension.Value;

                        string extName = (string)extData["manifest"]?["name"] ?? "";
                        extName = System.Text.RegularExpressions.Regex.Replace(extName, @" Wallet", "");
                        int state = (int?)extData["state"] ?? -1;
                        string extStatus = state == 1 ? "enabled" : "disabled";

                        if (state == -1) continue;

                        if ((toUse.Contains(extName) && extStatus == "disabled") ||
                            (toUse.Contains(extId) && extStatus == "disabled") ||
                            (!toUse.Contains(extName) && !toUse.Contains(extId) && extStatus == "enabled"))
                        {
                            extData["state"] = extStatus == "disabled" ? 1 : 0;
                            changesMade = true;
                            WalLog($"Changed: [{extName}] : [{extStatus} -> {(extData["state"].ToString() == "1" ? "enabled" : "disabled")}] : [{extId}]", log: log);
                        }
                    }

                    if (changesMade)
                    {
                        File.WriteAllText(securePrefsPath, jObj.ToString());
                    }
                }
                catch (Exception ex)
                {
                    WalLog($"Err: {ex.Message}", log: log);
                    throw;
                }
            }


        }

        public void Approve(W wallet, bool log = false)
        {
            WalLog($"Approving for wallet: {wallet}", log: log || _log);
            switch (wallet)
            {
                case W.MetaMask:
                    new MetaMaskWallet(_project, _instance, log).MetaMaskConfirm(log: log);
                    break;
                case W.Backpack:
                    new BackpackWallet(_project, _instance, log).BackpackApprove(log: log);
                    break;
                case W.Keplr:
                    new KeplrWallet(_project, _instance, log).KeplrApprove(log: log);
                    break;
                default:
                    WalLog($"Approve not supported for wallet: {wallet}", log: log || _log);
                    throw new Exception($"Approve not supported for {wallet}");
            }
        }

        public void Approve(string wallet, bool log = false)
        {
            if (!Enum.TryParse<W>(wallet, true, out var walletType))
            {
                WalLog($"Invalid wallet name: {wallet}", log: log || _log);
                throw new Exception($"Invalid wallet name: {wallet}");
            }
            Approve(walletType, log);
        }

        public void KeplrSetSource(string source, bool log = false)
        {
            new KeplrWallet(_project, _instance, log).KeplrSetSource(source, log);
        }

        private List<W> ParseWallets(string requiredWallets)
        {
            var result = new List<W>();
            if (string.IsNullOrEmpty(requiredWallets))
                return result;

            foreach (var wallet in requiredWallets.Split(','))
            {
                if (Enum.TryParse<W>(wallet.Trim(), true, out var walletType))
                {
                    result.Add(walletType);
                }
                else
                {
                    WalLog($"Invalid wallet name in requiredWallets: {wallet}", log: _log);
                }
            }
            return result;
        }

        private void WalLog(string message, bool log = false)
        {
            if (log)
                _project.SendInfoToLog($"[WalletManager] {message}");
        }
    }

    public class ZerionWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public ZerionWallet(IZennoPosterProjectModel project, Instance instance, bool log = false)
            : base(project, instance, log)
        {
            _extId = "klghhnkeealcohjjanjjdaeeggmfmlpl";
            _fileName = "Zerion1.21.3.crx";
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
                string key = _sql.KeyEVM();
                _instance.ActiveTab.FindElementByName("key").SetValue(key, "Full", false);
            }
            else if (source == "seed")
            {
                _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/mnemonic", "regexp", 0));
                string seedPhrase = _sql.Seed();
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
                _instance.HeSet((("referralCode","name")), refCode);
                _instance.HeClick(("button", "class", "_regular", "regexp", 0));
            }
            return true;
        }

        public void ZerionUnlock(bool log = false)
        {
            _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview", "");

            string active = null;
            try
            {
                active = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0));
            }
            catch
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                active = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0));
            }
            WalLog(active, log: log); 
        }

        public string ZerionCheck(bool log = false)
        {
            if (_instance.ActiveTab.URL != "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview")
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview", "");

            var active = _instance.HeGet(("div", "class", "_uitext_", "regexp", 0));
            var balance = _instance.HeGet(("div", "class", "_uitext_", "regexp", 1));
            var pnl = _instance.HeGet(("div", "class", "_uitext_", "regexp", 2));

            WalLog($"{active} {balance} {pnl}", log: log);
            return active;
        }
    }

    public class RazorWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public RazorWallet(IZennoPosterProjectModel project, Instance instance, bool log = false)
            : base(project, instance, log)
        {
            _extId = "fdcnegogpncmfejlfnffnofpngdiejii";
            _fileName = "Razor2.0.9.crx";
        }

        public void RazorLnch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            WalLog(log: log);
            if (Install(_extId, fileName, log)) RazorImport(log: log);
            else RazorUnlock(log: log);

            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }

        public bool RazorImport(bool log = false)
        {
            WalLog(log: log);
            var key = _sql.KeySOL();
            var password = _pass; 

            _instance.CloseExtraTabs();
            Tab walTab = _instance.NewTab("wal");
            walTab.SetActive();
            walTab.Navigate($"chrome-extension://{_extId}/index.html#/account/initialize/import/private-key", "");

            try
            {
                RazorUnlock(log: false);
                return true;
            }
            catch { }

            _instance.HeSet(("name", "name"), "pkey");
            _instance.HeSet(("privateKey", "name"), key);
            _instance.HeClick(("button", "innertext", "Proceed", "regexp", 0));

            _instance.HeSet(("password", "name"), password);
            _instance.HeSet(("repeatPassword", "name"), password);
            _instance.HeClick(("button", "innertext", "Proceed", "regexp", 0));

            _instance.HeClick(("button", "innertext", "Done", "regexp", 0));

            return true;
        }

        public void RazorUnlock(bool log = false)
        {
            WalLog(log: log);
            var password = _pass;

            try
            {
                _instance.HeSet(("password", "name"), password, deadline: 3);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
            }
            catch
            {
                try
                {
                    Tab walTab = _instance.NewTab("wal");
                    walTab.SetActive();
                    walTab.Navigate($"chrome-extension://{_extId}/index.html", "");
                    _instance.HeSet(("password", "name"), password, deadline: 3);
                    _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                }
                catch
                {
                    throw;
                }
            }
        }

        public void RazorCheck(bool log = false)
        {
            WalLog( log: log);

            if (_instance.ActiveTab.URL != $"chrome-extension://{_extId}/index.html#/overview")
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/index.html#/overview", "");

            // Предполагаемая реализация, аналогичная ZerionCheck
            var active = _instance.HeGet(("div", "class", "_uitext_", "regexp", 0)) ?? "unknown";
            var balance = _instance.HeGet(("div", "class", "_uitext_", "regexp", 1)) ?? "0";
            var pnl = _instance.HeGet(("div", "class", "_uitext_", "regexp", 2)) ?? "0";

            WalLog($"Active: {active}, Balance: {balance}, PnL: {pnl}", log: log);
        }
    }

    public class BackpackWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public BackpackWallet(IZennoPosterProjectModel project, Instance instance, bool log = false)
            : base(project, instance, log)
        {
            _extId = "aflkmfhebedbjioipglgcbcmnbpgliof";
            _fileName = "Backpack0.10.94.crx";
        }

        public void BackpackLnch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            WalLog($"Launching Backpack wallet with file {fileName}", log: log);
            if (Install(_extId, fileName, log))
                BackpackImport(log: log);
            else
                BackpackUnlock(log: log);

            BackpackCheck(log: log);
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }

        public bool BackpackImport(bool log = false)
        {
            WalLog("Importing Backpack wallet with private key", log: log);
            var key = _sql.KeySOL();
            var password = _pass;

            _instance.CloseExtraTabs();
            _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/options.html?onboarding=true", "");

            while (true)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("p", "innertext", "Already\\ setup", "regexp", 0).IsVoid)
                {
                    WalLog("Wallet already set up, skipping import", log: log);
                    return false;
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ Wallet", "regexp", 0).IsVoid)
                {
                    _instance.HeClick(("button", "innertext", "Import\\ Wallet", "regexp", 0));
                    _instance.HeClick(("div", "class", "_dsp-flex\\ _ai-stretch\\ _fd-row\\ _fb-auto\\ _bxs-border-box\\ _pos-relative\\ _mih-0px\\ _miw-0px\\ _fs-0\\ _btc-889733467\\ _brc-889733467\\ _bbc-889733467\\ _blc-889733467\\ _w-10037\\ _pt-1316333121\\ _pr-1316333121\\ _pb-1316333121\\ _pl-1316333121\\ _gap-1316333121", "regexp", 0));
                    _instance.HeClick(("button", "innertext", "Import\\ private\\ key", "regexp", 0));
                    _instance.HeSet(("textarea", "fulltagname", "textarea", "regexp", 0), key);
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    _instance.HeSet(("input:password", "placeholder", "Password", "regexp", 0), password);
                    _instance.HeSet(("input:password", "placeholder", "Confirm\\ Password", "regexp", 0), password);
                    _instance.HeClick(("input:checkbox", "class", "PrivateSwitchBase-input\\ ", "regexp", 0));
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    _instance.HeClick(("button", "innertext", "Open\\ Backpack", "regexp", 0));
                    WalLog("Successfully imported Backpack wallet", log: log);
                    return true;
                }
            }
        }

        public void BackpackUnlock(bool log = false)
        {
            WalLog("Unlocking Backpack wallet", log: log);
            var password = _pass;

            if (_instance.ActiveTab.URL != $"chrome-extension://{_extId}/popout.html")
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popout.html", "");

            _instance.CloseExtraTabs();

            try
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), password);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                WalLog("Wallet unlocked successfully", log: log);
            }
            catch
            {
                if (!_instance.ActiveTab.FindElementByAttribute("path", "d", "M12 5v14", "text", 0).IsVoid)
                {
                    WalLog("Wallet already unlocked", log: log);
                    return;
                }
                throw new Exception("Failed to unlock Backpack wallet");
            }
        }

        public void BackpackCheck(bool log = false)
        {
            WalLog("Checking Backpack wallet address", log: log);

            _instance.CloseExtraTabs();

            try
            {
                while (_instance.ActiveTab.FindElementByAttribute("button", "class", "is_Button\\ ", "regexp", 0).IsVoid)
                    _instance.HeClick(("path", "d", "M12 5v14", "text", 0), deadline: 2);

                var publicSOL = _instance.HeGet(("p", "class", "MuiTypography-root\\ MuiTypography-body1", "regexp", 0), "last");
                _instance.HeClick(("button", "aria-label", "TabsNavigator,\\ back", "regexp", 0));
                _project.Variables["addressSol"].Value = publicSOL;
                _sql.UpdAddressSol();
                WalLog($"SOL address: {publicSOL}", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to check address: {ex.Message}", log: log);
                throw;
            }
        }

        public void BackpackApprove(bool log = false)
        {
            WalLog("Approving Backpack wallet action", log: log);

            try
            {
                _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last");
                _instance.CloseExtraTabs();
                WalLog("Action approved successfully", log: log);
            }
            catch
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), _pass);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last");
                _instance.CloseExtraTabs();
                WalLog("Action approved after unlocking", log: log);
            }
        }
    }

    public class RabbyWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public RabbyWallet(IZennoPosterProjectModel project, Instance instance, bool log = false)
            : base(project, instance, log)
        {
            _extId = "acmacodkjbdgmoleebolmdjonilkdbch";
            _fileName = "Rabby0.93.24.crx";
        }

        public void RabbyLnch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = true;

            WalLog($"Launching Rabby wallet with file {fileName}", log: log);
            if (Install(_extId, fileName, log))
                RabbyImport(log: log);
            else
                RabbyUnlock(log: log);

            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }

        public void RabbyImport(bool log = false)
        {
            WalLog("Importing Rabby wallet with private key", log: log);
            var key = _sql.KeyEVM();
            var password = _pass;

            try
            {
                _instance.HeClick(("button", "innertext", "I\\ already\\ have\\ an\\ address", "regexp", 0));
                _instance.HeClick(("img", "src", $"chrome-extension://{_extId}/generated/svgs/d5409491e847b490e71191a99ddade8b.svg", "regexp", 0));
                _instance.HeSet(("privateKey", "id"), key);
                _instance.HeClick(("button", "innertext", "Confirm", "regexp", 0));
                _instance.HeSet(("password", "id"), password);
                _instance.HeSet(("confirmPassword", "id"), password);
                _instance.HeClick(("button", "innertext", "Confirm", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Get\\ Started", "regexp", 0));
                WalLog("Successfully imported Rabby wallet", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to import Rabby wallet: {ex.Message}", log: log);
                throw;
            }
        }

        public void RabbyUnlock(bool log = false)
        {
            WalLog("Unlocking Rabby wallet", log: log);
            var password = _pass;

            _instance.UseFullMouseEmulation = true;

            while (_instance.ActiveTab.URL == $"chrome-extension://{_extId}/offscreen.html")
            {
                WalLog("Closing offscreen tab and retrying unlock", log: log);
                _instance.ActiveTab.Close();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/index.html#/unlock", "");
            }

            try
            {
                _instance.HeSet(("password", "id"), password);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                WalLog("Wallet unlocked successfully", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to unlock Rabby wallet: {ex.Message}", log: log);
                throw;
            }
        }
    }

    public class MetaMaskWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public MetaMaskWallet(IZennoPosterProjectModel project, Instance instance, bool log = false)
            : base(project, instance, log)
        {
            _extId = "nkbihfbeogaeaoehlefnkodbefgpgknn";
            _fileName = "MetaMask11.16.0.crx";
        }

        public void MetaMaskLnch(string key = null, string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            WalLog($"Launching MetaMask wallet with file {fileName}", log: log);

            var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
            if (!extListString.Contains(_extId))
            {
                WalLog($"Installing MetaMask extension from {fileName}", log: log);
                _instance.InstallCrxExtension($"{_project.Path}.crx\\{fileName}");
                _instance.CloseExtraTabs();
            }

            string state = CheckWalletState(log);
            while (state != "mainPage")
            {
                if (state == "initPage")
                {
                    MetaMaskImport(key, log);
                }
                else if (state == "passwordPage")
                {
                    MetaMaskUnlock(log);
                }
                state = CheckWalletState(log);
            }

            string address = MetaMaskChkAddress(log);
            WalLog($"MetaMask wallet address: {address}", log: log);

            _instance.UseFullMouseEmulation = em;
        }

        private string CheckWalletState(bool log = false)
        {
            _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/home.html", "");
            var deadline = DateTime.Now.AddSeconds(60);

            while (DateTime.Now < deadline)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-options-menu-button", "regexp", 0).IsVoid)
                {
                    WalLog("Wallet is on main page", log: log);
                    return "mainPage";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0).IsVoid)
                {
                    WalLog("Wallet is on initialization page", log: log);
                    return "initPage";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0).IsVoid)
                {
                    WalLog("Wallet is on password page", log: log);
                    return "passwordPage";
                }
                Thread.Sleep(1000);
            }

            WalLog("Timeout waiting for wallet state", log: log);
            throw new Exception("Timeout waiting for MetaMask wallet state");
        }

        public void MetaMaskImport(string key = null, bool log = false)
        {
            WalLog("Importing MetaMask wallet with private key", log: log);
            var password = _pass;
            if (string.IsNullOrEmpty(key)) key = _sql.KeyEVM();

            var deadline = DateTime.Now.AddSeconds(60);
            while (!_instance.ActiveTab.URL.Contains("#onboarding/welcome") && DateTime.Now < deadline)
            {
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/home.html#onboarding/welcome", "");
                Thread.Sleep(1000);
            }
            if (DateTime.Now >= deadline)
            {
                WalLog("Timeout waiting for onboarding page", log: log);
                throw new Exception("Timeout waiting for MetaMask onboarding page");
            }

            try
            {
                _instance.HeClick(("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0));
                _instance.HeClick(("span", "innertext", "I\\ agree\\ to\\ MetaMask\'s\\ Terms\\ of\\ use", "regexp", 1));
                _instance.HeClick(("button", "aria-label", "Close", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "onboarding-create-wallet", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "metametrics-no-thanks", "regexp", 0));
                _instance.HeSet(("input:password", "data-testid", "create-password-new", "regexp", 0), password);
                _instance.HeSet(("input:password", "data-testid", "create-password-confirm", "regexp", 0), password);
                _instance.HeClick(("span", "innertext", "I\\ understand\\ that\\ MetaMask\\ cannot\\ recover\\ this\\ password\\ for\\ me.\\ Learn\\ more", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "create-password-wallet", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "secure-wallet-later", "regexp", 0));
                _instance.HeClick(("label", "class", "skip-srp-backup-popover__label", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "skip-srp-backup", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "onboarding-complete-done", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "pin-extension-next", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "pin-extension-done", "regexp", 0));

                while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid)
                {
                    try { _instance.HeClick(("button", "data-testid", "popover-close", "regexp", 0)); }
                    catch { _instance.HeClick(("button", "innertext", "Got\\ it", "regexp", 0)); }
                }

                _instance.HeClick(("button", "data-testid", "account-menu-icon", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "multichain-account-menu-popover-action-button", "regexp", 0));
                _instance.HeClick(("span", "style", "mask-image:\\ url\\(\"./images/icons/import.svg\"\\);", "regexp", 0));
                _instance.HeSet(("private-key-box", "id"), key);
                _instance.HeClick(("button", "data-testid", "import-account-confirm-button", "regexp", 0));
                WalLog("Successfully imported MetaMask wallet", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to import MetaMask wallet: {ex.Message}", log: log);
                throw;
            }
        }

        public void MetaMaskUnlock(bool log = false)
        {
            WalLog("Unlocking MetaMask wallet", log: log);
            var password = _pass;

            try
            {
                _instance.HeSet(("password", "id"), password);
                _instance.HeClick(("button", "data-testid", "unlock-submit", "regexp", 0));

                if (!_instance.ActiveTab.FindElementByAttribute("p", "innertext", "Incorrect password", "text", 0).IsVoid)
                {
                    _instance.CloseAllTabs();
                    _instance.UninstallExtension(_extId);
                    WalLog("Incorrect password provided", log: log);
                    throw new Exception("Wrong password for MetaMask");
                }
                WalLog("Wallet unlocked successfully", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to unlock MetaMask wallet: {ex.Message}", log: log);
                throw;
            }
        }

        public string MetaMaskChkAddress(bool skipCheck = false, bool log = false)
        {
            WalLog("Checking MetaMask wallet address", log: log);

            try
            {
                while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid)
                {
                    try { _instance.HeClick(("button", "data-testid", "popover-close", "regexp", 0)); }
                    catch { _instance.HeClick(("button", "innertext", "Got\\ it", "regexp", 0)); }
                }

                _instance.HeClick(("button", "data-testid", "account-options-menu-button", "regexp", 0));
                _instance.HeClick(("button", "data-testid", "account-list-menu-details", "regexp", 0));
                string address = _instance.HeGet(("button", "data-testid", "address-copy-button-text", "regexp", 0));

                if (!skipCheck && !string.Equals(address, _project.Variables["addressEvm"].Value, StringComparison.OrdinalIgnoreCase))
                {
                    _instance.CloseAllTabs();
                    _instance.UninstallExtension(_extId);
                    WalLog("Incorrect address detected", log: log);
                    throw new Exception("Wrong address for MetaMask");
                }

                WalLog($"Retrieved address: {address}", log: log);
                return address;
            }
            catch (Exception ex)
            {
                WalLog($"Failed to check MetaMask address: {ex.Message}", log: log);
                throw;
            }
        }

        public string MetaMaskConfirm(bool log = false)
        {
            WalLog("Confirming MetaMask transaction", log: log);
            var me = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;
            var deadline = DateTime.Now.AddSeconds(60);

            try
            {
                while (!_instance.ActiveTab.URL.Contains(_extId) && DateTime.Now < deadline)
                {
                    WalLog($"Waiting for MetaMask URL, current: {_instance.ActiveTab.URL}", log: log);
                    Thread.Sleep(1000);
                }
                if (DateTime.Now >= deadline)
                {
                    WalLog("Timeout waiting for MetaMask URL", log: log);
                    throw new Exception("Timeout waiting for MetaMask URL");
                }

                Thread.Sleep(2000);

                var alert = _instance.ActiveTab.FindElementByAttribute("div", "class", "mm-box\\ mm-banner-base\\ mm-banner-alert\\ mm-banner-alert--severity-danger", "regexp", 0);
                var simulation = _instance.ActiveTab.FindElementByAttribute("div", "data-testid", "simulation-details-layout", "regexp", 0);
                var detail = _instance.ActiveTab.FindElementByAttribute("div", "class", "transaction-detail", "regexp", 0);

                if (!simulation.IsVoid)
                    WalLog($"Simulation details: {Regex.Replace(simulation.GetAttribute("innertext").Trim(), @"\s+", " ")}", log: log);
                if (!detail.IsVoid)
                    WalLog($"Transaction details: {Regex.Replace(detail.GetAttribute("innertext").Trim(), @"\s+", " ")}", log: log);

                if (!alert.IsVoid)
                {
                    var error = Regex.Replace(alert.GetAttribute("innertext").Trim(), @"\s+", " ");
                    WalLog($"Alert detected: {error}", log: log);
                    while (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "page-container-footer-cancel", "regexp", 0).IsVoid)
                    {
                        _instance.ActiveTab.Touch.SwipeBetween(600, 400, 600, 300);
                        _instance.HeClick(("button", "data-testid", "page-container-footer-cancel", "regexp", 0));
                    }
                    throw new Exception($"MetaMask alert: {error}");
                }

                while (_instance.ActiveTab.URL.Contains(_extId) && DateTime.Now < deadline)
                {
                    try
                    {
                        _instance.HeClick(("button", "class", "button btn--rounded btn-primary", "regexp", 0), deadline: 3);
                        WalLog("Confirm button clicked", log: log);
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        WalLog($"Failed to click confirm button: {ex.Message}", log: log);
                    }
                }

                if (DateTime.Now >= deadline)
                {
                    WalLog("Timeout during MetaMask interaction", log: log);
                    throw new Exception("Timeout exceeded while interacting with MetaMask");
                }

                WalLog("MetaMask transaction confirmed", log: log);
                return "done";
            }
            finally
            {
                _instance.UseFullMouseEmulation = me;
            }
        }
    }

    public class KeplrWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public KeplrWallet(IZennoPosterProjectModel project, Instance instance, bool log = false)
            : base(project, instance, log)
        {
            _extId = "dmkamcknogkgcdfhhbddcghachkejeap";
            _fileName = "Keplr0.12.223.crx";
        }

        public void KeplrClick(HtmlElement he)
        {
            int x = int.Parse(he.GetAttribute("leftInTab")); int y = int.Parse(he.GetAttribute("topInTab"));
            x = x - 450; _instance.Click(x, x, y, y, "Left", "Normal"); Thread.Sleep(1000);
            return;
        }


        public string KeplrMain(string source = "pkey", string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            WalLog($"Starting Keplr wallet setup with source {source}", log: log);

            while (true)
            {
                var state = KeplrCheck(log);
                switch (state)
                {
                    case "install":
                        Install(_extId, fileName, log);
                        continue;
                    case "import":
                        KeplrImportSeed(log);
                        continue;
                    case "inputPass":
                        KeplrUnlock(log);
                        continue;
                    case "setSourse":
                        KeplrSetSource(source, log);
                        WalLog($"Keplr wallet set to source {source}", log: log);
                        return $"Keplr set from {source}";
                    default:
                        WalLog($"Unknown Keplr state: {state}", log: log);
                        throw new Exception("Unknown Keplr wallet state");
                }
            }
        }

        public void KeplrLaunch(string source = "seed", string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            WalLog($"Launching Rabby wallet with file {fileName}", log: log);
            if (Install(_extId, fileName, log))
            {
                KeplrImportSeed(log: log);
                KeplrImportPkey(log: log);
            }
            else
                KeplrUnlock(log: log);

            KeplrSetSource(source, log);

            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }



        public string KeplrApprove(bool log = false)
        {
            WalLog("Approving Keplr transaction", log: log);
            var deadline = DateTime.Now.AddSeconds(20);

            try
            {
                while (!(_instance.ActiveTab.URL.Contains(_extId)) && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                }
                if (DateTime.Now >= deadline)
                {
                    WalLog("Timeout waiting for Keplr tab", log: log);
                    throw new Exception("No Keplr tab detected");
                }

                _instance.UseFullMouseEmulation = false;
                approve:
                _instance.HeClick(("button", "innertext", "Approve", "regexp", 0));
                WalLog("Approve button clicked", log: log);

                while (_instance.ActiveTab.URL.Contains(_extId) && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                    goto approve;
                }
                if (DateTime.Now >= deadline)
                {
                    WalLog("Keplr tab stuck", log: log);
                    throw new Exception("Keplr tab stuck");
                }

                WalLog("Keplr transaction approved, tab closed", log: log);
                return "done";
            }
            catch (Exception ex)
            {
                WalLog($"Failed to approve Keplr transaction: {ex.Message}", log: log);
                throw;
            }
            finally
            {
                _instance.UseFullMouseEmulation = true;
            }
        }

        private string KeplrCheck(bool log = false)
        {
            WalLog("Checking Keplr wallet state", log: log);
            _instance.CloseExtraTabs();
            _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popup.html#/", "");

            var deadline = DateTime.Now.AddSeconds(15);

            while (DateTime.Now < deadline)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("div", "class", "error-code", "regexp", 0).IsVoid)
                {
                    WalLog("Keplr extension not installed", log: log);
                    return "install";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0).IsVoid)
                {
                    WalLog("Keplr wallet requires import", log: log);
                    return "import";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("input:password", "tagname", "input", "regexp", 0).IsVoid)
                {
                    WalLog("Keplr wallet requires unlocking", log: log);
                    return "inputPass";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Copy\\ Address", "regexp", 0).IsVoid)
                {
                    WalLog("Keplr wallet is set, ready to select source", log: log);
                    return "setSourse";
                }
                Thread.Sleep(1000);
            }

            WalLog("Timeout checking Keplr state", log: log);
            throw new Exception("Cannot check Keplr wallet state");
        }

        public void KeplrImportSeed(bool log = false)
        {
            WalLog("Importing Keplr wallet with seed phrase", log: log);
            var password = _pass;
            var seedPhrase = _sql.Seed();


            try { _instance.HeGet(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0)); }
            catch
            {
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/register.html#/", "");
            }

                try
            {
                _instance.HeClick(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Use\\ recovery\\ phrase\\ or\\ private\\ key", "regexp", 0));

                int index = 0;
                foreach (string word in seedPhrase.Split(' '))
                {
                    _instance.HeSet(("input", "fulltagname", "input:", "regexp", index), word, delay:0);
                    index++;
                }

                _instance.HeClick(("button", "innertext", "Import", "regexp", 1));
                _instance.HeSet(("name", "name"), "seed");
                _instance.HeSet(("password", "name"), password);
                _instance.HeSet(("confirmPassword", "name"), password);
                _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                _instance.HeClick(("input:checkbox", "fulltagname", "input:checkbox", "regexp", 0));
        
                _instance.HeClick(("button", "innertext", "Save", "regexp", 0));

                while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).IsVoid)
                {
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    Thread.Sleep(2000);
                }

                _instance.CloseExtraTabs();
                WalLog("Successfully imported Keplr wallet with seed", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to import Keplr wallet with seed: {ex.Message}", log: log);
                throw;
            }
        }

        public void KeplrImportPkey(bool temp = false, bool log = false)
        {
            WalLog($"Importing Keplr wallet with private key (temp: {temp})", log: log);
            var password = _pass;
            var key = temp ? new Key().ToHex() : _sql.KeyEVM();
            var walletName = temp ? "temp" : "pkey";

            try { _instance.HeGet(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0), deadline: 3); }
            catch { _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/register.html#/", ""); }

            try
            {
                _instance.HeClick(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Use\\ recovery\\ phrase\\ or\\ private\\ key", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Private\\ key", "regexp", 1));
                _instance.HeSet(("input:password", "tagname", "input", "regexp", 0), key);
                _instance.HeClick(("button", "innertext", "Import", "regexp", 1));
                _instance.HeSet(("name", "name"), walletName);
                try
                {
                    _instance.HeSet(("password", "name"), password, deadline:3);
                    _instance.HeSet(("confirmPassword", "name"), password);
                }
                catch { }
                _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Save", "regexp", 0));

                while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).IsVoid)
                {
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    Thread.Sleep(2000);
                }

                _instance.CloseExtraTabs();
                WalLog($"Successfully imported Keplr wallet with private key (name: {walletName})", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to import Keplr wallet with private key: {ex.Message}", log: log);
                throw;
            }
        }

        public void KeplrSetSource(string source, bool log = false)
        {
            WalLog($"Setting Keplr wallet source to {source}", log: log);

            while (true)
            {
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popup.html#/wallet/select", "");
                _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));

                var imported = KeplrPrune(log);
                if (imported.Contains("seed") && imported.Contains("pkey"))
                {
                    KeplrClick(_instance.GetHe(("div", "innertext", source, "regexp", 0), "last"));
                    WalLog($"Source set to {source}", log: log);
                    return;
                }

                WalLog("Not all wallets imported, adding new wallet", log: log);
                KeplrClick(_instance.GetHe(("button", "innertext", "Add\\ Wallet", "regexp", 0)));
                KeplrImportPkey(log: log);
            }
        }

        public void KeplrUnlock(bool log = false)
        {
            WalLog("Unlocking Keplr wallet", log: log);
            var password = _pass;

            unlock:
            if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Copy\\ Address", "regexp", 0).IsVoid)
            {
                WalLog("Keplr wallet is set, ready to select source", log: log);
                return;
            }
            try
            {
                
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popup.html#/", "");
                _instance.HeSet(("input:password", "tagname", "input", "regexp", 0), password);
                KeplrClick(_instance.GetHe(("button", "innertext", "Unlock", "regexp", 0)));
                //_instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));

                if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Invalid\\ password", "regexp", 0).IsVoid)
                {
                    _instance.CloseAllTabs();
                    _instance.UninstallExtension(_extId);
                    WalLog("Invalid password provided", log: log);
                    throw new Exception("Wrong password for Keplr");
                }

                WalLog("Wallet unlocked successfully", log: log);
            }
            catch (Exception ex)
            {
                WalLog($"Failed to unlock Keplr wallet: {ex.Message}", log: log);
                //throw;
                _instance.CloseAllTabs();
                goto unlock;

            }
        }

        public string KeplrPrune(bool keepTemp = false, bool log = false)
        {
            WalLog("Pruning Keplr wallets", log: log);
            //_instance.UseFullMouseEmulation = true;
            var imported = "";
            int i = 0;
            _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));
            _project.Sleep(1, 1);
            try
            {
                while (true)
                {
                    var dotBtn = _instance.ActiveTab.FindElementByAttribute("path", "d", "M10.5 6C10.5 5.17157 11.1716 4.5 12 4.5C12.8284 4.5 13.5 5.17157 13.5 6C13.5 6.82843 12.8284 7.5 12 7.5C11.1716 7.5 10.5 6.82843 10.5 6ZM10.5 12C10.5 11.1716 11.1716 10.5 12 10.5C12.8284 10.5 13.5 11.1716 13.5 12C13.5 12.8284 12.8284 13.5 12 13.5C11.1716 13.5 10.5 12.8284 10.5 12ZM10.5 18C10.5 17.1716 11.1716 16.5 12 16.5C12.8284 16.5 13.5 17.1716 13.5 18C13.5 18.8284 12.8284 19.5 12 19.5C11.1716 19.5 10.5 18.8284 10.5 18Z", "text", i);

                    if (dotBtn.IsVoid)
                        break;

                    var tile = dotBtn.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement;
                    var tileText = tile.InnerText;

                    if (tileText.Contains("pkey"))
                    {
                        imported += "pkey";
                        i++;
                        continue;
                    }
                    if (tileText.Contains("seed"))
                    {
                        imported += "seed";
                        i++;
                        continue;
                    }
                    if (keepTemp && tileText.Contains("temp"))
                    {
                        imported += "temp";
                        i++;
                        continue;
                    }

                    KeplrClick(dotBtn);
                    KeplrClick(_instance.GetHe(("div", "innertext", "Delete\\ Wallet", "regexp", 0), "last"));
                    _instance.HeSet(("password", "name"), _pass);
                    KeplrClick(_instance.GetHe(("button", "type", "submit", "regexp", 0)));
                    i++;
                }

                WalLog($"Pruned wallets, remaining: {imported}", log: log);
                return imported;
            }
            catch (Exception ex)
            {
                WalLog($"Failed to prune Keplr wallets: {ex.Message}", log: log);
                throw;
            }
        }
    }


}


