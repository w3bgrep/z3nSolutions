using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Nethereum.Model;
namespace z3nCore
{
    public class WebWallet
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;

        public WebWallet(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
        }

        public void Launch(IEnumerable<W> requiredWallets, bool log = false)
        {
            WalLog($"Switching wallets: {string.Join(", ", requiredWallets)}", log: log);
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
                        new BackpackWallet(_project, _instance, log:log).Launch(log: log);
                        break;
                    case W.Razor:
                        new RazorWallet(_project, _instance, log).RazorLnch(log: log);
                        break;
                    case W.Zerion:
                        new ZerionWallet(_project, _instance, log).Launch(log: log);
                        break;
                    case W.Keplr:
                        new KeplrWallet(_project, _instance, log).KeplrLaunch(log: log);
                        break;
                    default:
                        WalLog($"Unknown wallet: {wallet}", log: log);
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
            WalLog($"Approving for wallet: {wallet}", log: log);
            switch (wallet)
            {
                case W.MetaMask:
                    new MetaMaskWallet(_project, _instance, log).MetaMaskConfirm(log: log);
                    break;
                case W.Backpack:
                    new BackpackWallet(_project, _instance, log: log).Approve(log: log);
                    break;
                case W.Keplr:
                    new KeplrWallet(_project, _instance, log).KeplrApprove(log: log);
                    break;
                case W.Zerion:
                    new ZerionWallet(_project, _instance, log).Sign(log: log);
                    break;
                default:
                    WalLog($"Approve not supported for wallet: {wallet}", log: log);
                    throw new Exception($"Approve not supported for {wallet}");
            }
        }

        public void Approve(string wallet, bool log = false)
        {
            if (!Enum.TryParse<W>(wallet, true, out var walletType))
            {
                WalLog($"Invalid wallet name: {wallet}", log: log);
                throw new Exception($"Invalid wallet name: {wallet}");
            }
            Approve(walletType, log);
        }

        public void KeplrSetSource(string source, bool log = false)
        {
            new KeplrWallet(_project, _instance, log).KeplrSetSource(source, log);
        }

        private List<W> ParseWallets(string requiredWallets, bool log = false)
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
                    WalLog($"Invalid wallet name in requiredWallets: {wallet}", log: log);
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

}
