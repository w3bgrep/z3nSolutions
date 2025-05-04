using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
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
                    _instance.HeSet(("input", "fulltagname", "input:", "regexp", index), word, delay: 0);
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
                    _instance.HeSet(("password", "name"), password, deadline: 3);
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

                try
                {
                    _instance.HeGet(("input:password", "tagname", "input", "regexp", 0));
                }
                catch
                {
                    _instance.CloseAllTabs();
                    goto unlock;
                }

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
