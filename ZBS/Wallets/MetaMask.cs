using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class MetaMaskWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public MetaMaskWallet(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string seed = null)
            : base(project, instance, log)
        {
            _extId = "nkbihfbeogaeaoehlefnkodbefgpgknn";
            _fileName = "MetaMask11.16.0.crx";
            _key = KeyCheck(key);
            _seed = SeedCheck(seed);
        }

        private string KeyCheck(string key)
        {
            if (string.IsNullOrEmpty(key))
                key = Decrypt(KeyT.secp256k1);
            if (string.IsNullOrEmpty(key))
                throw new Exception("emptykey");
            return key;
        }
        private string SeedCheck(string seed)
        {
            if (string.IsNullOrEmpty(seed))
                seed = Decrypt(KeyT.bip39);
            if (string.IsNullOrEmpty(seed))
                throw new Exception("emptykey");
            return seed;
        }

        public void MetaMaskLnchold(string key = null, string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;


            Log($"Launching MetaMask wallet with file {fileName}", log: log);

            var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
            if (!extListString.Contains(_extId))
            {
                Log($"Installing MetaMask extension from {fileName}", log: log);
                _instance.InstallCrxExtension($"{_project.Path}.crx\\{fileName}");
                _instance.CloseExtraTabs();
            }

            string state = CheckWalletState(log: log);
            while (state != "mainPage")
            {
                if (state == "initPage")
                {
                    MetaMaskImport(key, log: log);
                }
                else if (state == "passwordPage")
                {
                    MetaMaskUnlock(log: log);
                }
                state = CheckWalletState(log: log);
            }

            string address = MetaMaskChkAddress(log: log);
            Log($"MetaMask wallet address: {address}", log: log);

            _instance.UseFullMouseEmulation = em;
        }


        public void MetaMaskLnch(string fileName = null, string key = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            Log($"Launching MM wallet with file {fileName}", log: log);
            if (Install(_extId, fileName, log))
                MetaMaskImport(key, log: log);
            else
                MetaMaskUnlock(log: log);

            string address = MetaMaskChkAddress(log: log);
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;

        }

        public bool MetaMaskWaitTx(int gap = 2, bool log = false)
        {
            bool result = false;
            Tab tab = _instance.NewTab("mm");
            if (tab.IsBusy) tab.WaitDownloading();

            _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/home.html", "");

        check:
            var txBoxText = _instance.HeGet(("div", "class", "transaction-list__transactions", "regexp", 0));
            var txBoxList = _instance.ActiveTab.FindElementByAttribute("div", "class", "transaction-list__transactions", "regexp", 0).GetChildren(false).ToList();

            if (txBoxList.Count > 1)
            {
                _project.L0g(txBoxText);
                _project.Sleep(gap, gap);
                goto check;
            }

            var completedList = _instance.ActiveTab.FindElementByAttribute("div", "class", " transaction-list__completed-transactions", "regexp", 0).GetChildren(false).ToList();
            var last = completedList[1];
            if (last.InnerText.Contains("Confirmed")) result = true;
            _project.L0g(last.InnerText);
            _instance.CloseExtraTabs();
            return result;
        }

        private string CheckWalletState(bool log = false)
        {
            _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/home.html", "");
            var deadline = DateTime.Now.AddSeconds(60);

            while (DateTime.Now < deadline)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-options-menu-button", "regexp", 0).IsVoid)
                {
                    Log("Wallet is on main page", log: log);
                    return "mainPage";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0).IsVoid)
                {
                    Log("Wallet is on initialization page", log: log);
                    return "initPage";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0).IsVoid)
                {
                    Log("Wallet is on password page", log: log);
                    return "passwordPage";
                }
                Thread.Sleep(1000);
            }

            Log("Timeout waiting for wallet state", log: log);
            throw new Exception("Timeout waiting for MetaMask wallet state");
        }

        public void MetaMaskImport(string key = null, bool log = false)
        {
            Log("Importing MetaMask wallet with private key", log: log);
            var password = _pass;
            if (string.IsNullOrEmpty(key)) key = _key;

            var deadline = DateTime.Now.AddSeconds(60);
            while (!_instance.ActiveTab.URL.Contains("#onboarding/welcome") && DateTime.Now < deadline)
            {
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/home.html#onboarding/welcome", "");
                Thread.Sleep(1000);
            }
            if (DateTime.Now >= deadline)
            {
                Log("Timeout waiting for onboarding page", log: log);
                throw new Exception("Timeout waiting for MetaMask onboarding page");
            }

            try
            {
                _instance.HeClick(("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0), delay: 0);
                _instance.HeClick(("span", "innertext", "I\\ agree\\ to\\ MetaMask\'s\\ Terms\\ of\\ use", "regexp", 1), delay: 0);
                _instance.HeClick(("button", "aria-label", "Close", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "onboarding-create-wallet", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "metametrics-no-thanks", "regexp", 0), delay: 0);
                _instance.HeSet(("input:password", "data-testid", "create-password-new", "regexp", 0), password);
                _instance.HeSet(("input:password", "data-testid", "create-password-confirm", "regexp", 0), password);
                _instance.HeClick(("span", "innertext", "I\\ understand\\ that\\ MetaMask\\ cannot\\ recover\\ this\\ password\\ for\\ me.\\ Learn\\ more", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "create-password-wallet", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "secure-wallet-later", "regexp", 0), delay: 0);
                _instance.HeClick(("label", "class", "skip-srp-backup-popover__label", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "skip-srp-backup", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "onboarding-complete-done", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "pin-extension-next", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "pin-extension-done", "regexp", 0), delay: 0);

                while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid)
                {
                    try { _instance.HeClick(("button", "data-testid", "popover-close", "regexp", 0), delay: 0); }
                    catch { _instance.HeClick(("button", "innertext", "Got\\ it", "regexp", 0), delay: 0); }
                }

                _instance.HeClick(("button", "data-testid", "account-menu-icon", "regexp", 0), delay: 0);
                _instance.HeClick(("button", "data-testid", "multichain-account-menu-popover-action-button", "regexp", 0), delay: 0);
                _instance.HeClick(("span", "style", "mask-image:\\ url\\(\"./images/icons/import.svg\"\\);", "regexp", 0), delay: 0);
                _instance.HeSet(("private-key-box", "id"), key);
                _instance.HeClick(("button", "data-testid", "import-account-confirm-button", "regexp", 0), delay: 0);
                Log("Successfully imported MetaMask wallet", log: log);
            }
            catch (Exception ex)
            {
                Log($"Failed to import MetaMask wallet: {ex.Message}", log: log);
                throw;
            }
        }

        public void MetaMaskUnlock(bool log = false)
        {
            Log("Unlocking MetaMask wallet", log: log);
            var password = _pass;
            if (!_instance.ActiveTab.URL.Contains(_extId)) _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/home.html", "");
            try
            {
                _instance.HeSet(("password", "id"), password);
                _instance.HeClick(("button", "data-testid", "unlock-submit", "regexp", 0));

                if (!_instance.ActiveTab.FindElementByAttribute("p", "innertext", "Incorrect password", "text", 0).IsVoid)
                {
                    _instance.CloseAllTabs();
                    _instance.UninstallExtension(_extId);
                    Log("Incorrect password provided", log: log);
                    throw new Exception("Wrong password for MetaMask");
                }
                Log("Wallet unlocked successfully", log: log);
            }
            catch (Exception ex)
            {
                Log($"Failed to unlock MetaMask wallet: {ex.Message}", log: log);
                throw;
            }
        }

        public string MetaMaskChkAddress(bool skipCheck = false, bool log = false)
        {
            string expectedAddress = new W3b(_project).Address("evm");
            Log("Checking MetaMask wallet address", log: log);

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

                if (!skipCheck && !string.Equals(address, expectedAddress, StringComparison.OrdinalIgnoreCase))
                {
                    _instance.CloseAllTabs();
                    _instance.UninstallExtension(_extId);
                    Log("Incorrect address detected", log: log);
                    throw new Exception("Wrong address for MetaMask");
                }

                Log($"Retrieved address: {address}", log: log);
                return address;
            }
            catch (Exception ex)
            {
                Log($"Failed to check MetaMask address: {ex.Message}", log: log);
                throw;
            }
        }

        public string MetaMaskConfirm(bool log = false)
        {
            Log("Confirming MetaMask transaction", log: log);
            var me = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;
            var deadline = DateTime.Now.AddSeconds(20);

            try
            {
                while (!_instance.ActiveTab.URL.Contains(_extId) && DateTime.Now < deadline)
                {
                    Log($"Waiting for MetaMask URL, current: {_instance.ActiveTab.URL}", log: log);
                    Thread.Sleep(1000);
                }
                if (DateTime.Now >= deadline)
                {
                    Log("Timeout waiting for MetaMask URL", log: log);
                    throw new Exception("Timeout waiting for MetaMask URL");
                }

                Thread.Sleep(2000);

                var alert = _instance.ActiveTab.FindElementByAttribute("div", "class", "mm-box\\ mm-banner-base\\ mm-banner-alert\\ mm-banner-alert--severity-danger", "regexp", 0);
                var simulation = _instance.ActiveTab.FindElementByAttribute("div", "data-testid", "simulation-details-layout", "regexp", 0);
                var detail = _instance.ActiveTab.FindElementByAttribute("div", "class", "transaction-detail", "regexp", 0);

                if (!simulation.IsVoid)
                    Log($"Simulation details: {Regex.Replace(simulation.GetAttribute("innertext").Trim(), @"\s+", " ")}", log: log);
                if (!detail.IsVoid)
                    Log($"Transaction details: {Regex.Replace(detail.GetAttribute("innertext").Trim(), @"\s+", " ")}", log: log);

                if (!alert.IsVoid)
                {
                    var error = Regex.Replace(alert.GetAttribute("innertext").Trim(), @"\s+", " ");
                    Log($"Alert detected: {error}", log: log);
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
                        Log("Confirm button clicked", log: log);
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to click confirm button: {ex.Message}", log: log);
                    }
                }

                if (DateTime.Now >= deadline)
                {
                    Log("Timeout during MetaMask interaction", log: log);
                    throw new Exception("Timeout exceeded while interacting with MetaMask");
                }

                Log("MetaMask transaction confirmed", log: log);
                return "done";
            }
            finally
            {
                _instance.UseFullMouseEmulation = me;
            }
        }
    }

}
