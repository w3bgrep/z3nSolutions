using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
{
    public class KeplrWallet0 
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        protected readonly string _extId = "dmkamcknogkgcdfhhbddcghachkejeap";
        protected readonly string _fileName;

        public KeplrWallet0(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string seed = null)
            
        {
            _project = project;
            _instance  = instance;
            _fileName = "Keplr0.12.223.crx";
            _logger = new Logger(project, log: log, classEmoji: "K3PLR");
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

            _logger.Send($"Starting Keplr wallet setup with source {source}");

            while (true)
            {
                var state = KeplrCheck(log);
                switch (state)
                {
                    case "install":
                        new ChromeExt(_project, _instance).Install(_extId, fileName, log);
                        //Install(_extId, fileName, log);
                        continue;
                    case "import":
                        KeplrImportSeed(log);
                        continue;
                    case "inputPass":
                        KeplrUnlock(log);
                        continue;
                    case "setSourse":
                        KeplrSetSource(source, log);
                        _logger.Send($"Keplr wallet set to source {source}");
                        return $"Keplr set from {source}";
                    default:
                        _logger.Send($"Unknown Keplr state: {state}");
                        throw new Exception("Unknown Keplr wallet state");
                }
            }
        }
        public void KeplrLaunch(string source = "seed", string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            _logger.Send($"Launching Rabby wallet with file {fileName}");
            if (new ChromeExt(_project, _instance).Install(_extId, fileName, log))
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
            _logger.Send("Approving Keplr transaction");
            var deadline = DateTime.Now.AddSeconds(20);

            try
            {
                while (!(_instance.ActiveTab.URL.Contains(_extId)) && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                }
                if (DateTime.Now >= deadline)
                {
                    _logger.Send("Timeout waiting for Keplr tab");
                    throw new Exception("No Keplr tab detected");
                }

                _instance.UseFullMouseEmulation = false;
            approve:
                _instance.HeClick(("button", "innertext", "Approve", "regexp", 0));
                _logger.Send("Approve button clicked");

                while (_instance.ActiveTab.URL.Contains(_extId) && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                    goto approve;
                }
                if (DateTime.Now >= deadline)
                {
                    _logger.Send("Keplr tab stuck");
                    throw new Exception("Keplr tab stuck");
                }

                _logger.Send("Keplr transaction approved, tab closed");
                return "done";
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to approve Keplr transaction: {ex.Message}");
                throw;
            }
            finally
            {
                _instance.UseFullMouseEmulation = true;
            }
        }
        private string KeplrCheck(bool log = false)
        {
            _logger.Send("Checking Keplr wallet state");
            _instance.CloseExtraTabs();
            _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popup.html#/", "");

            var deadline = DateTime.Now.AddSeconds(15);

            while (DateTime.Now < deadline)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("div", "class", "error-code", "regexp", 0).IsVoid)
                {
                    _logger.Send("Keplr extension not installed");
                    return "install";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0).IsVoid)
                {
                    _logger.Send("Keplr wallet requires import");
                    return "import";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("input:password", "tagname", "input", "regexp", 0).IsVoid)
                {
                    _logger.Send("Keplr wallet requires unlocking");
                    return "inputPass";
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Copy\\ Address", "regexp", 0).IsVoid)
                {
                    _logger.Send("Keplr wallet is set, ready to select source");
                    return "setSourse";
                }
                Thread.Sleep(1000);
            }

            _logger.Send("Timeout checking Keplr state");
            throw new Exception("Cannot check Keplr wallet state");
        }
        public void KeplrImportSeed(bool log = false)
        {
            _logger.Send("Importing Keplr wallet with seed phrase");
            var password = SAFU.HWPass(_project);
            var seedPhrase = _project.DbKey("seed");


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
                _logger.Send("Successfully imported Keplr wallet with seed");
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to import Keplr wallet with seed: {ex.Message}");
                throw;
            }
        }
        public void KeplrImportPkey(bool temp = false, bool log = false)
        {
            _logger.Send($"Importing Keplr wallet with private key (temp: {temp})");
            var password = SAFU.HWPass(_project);

            var key = temp ? new Key().ToHex() : _project.DbKey("evm");
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
                _logger.Send($"Successfully imported Keplr wallet with private key (name: {walletName})");
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to import Keplr wallet with private key: {ex.Message}");
                throw;
            }
        }
        public void KeplrSetSource(string source, bool log = false)
        {
            _logger.Send($"Setting Keplr wallet source to {source}");

            while (true)
            {
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popup.html#/wallet/select", "");
                _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));

                var imported = KeplrPrune(log);
                if (imported.Contains("seed") && imported.Contains("pkey"))
                {
                    KeplrClick(_instance.GetHe(("div", "innertext", source, "regexp", 0), "last"));
                    _logger.Send($"Source set to {source}");
                    return;
                }

                _logger.Send("Not all wallets imported, adding new wallet");
                KeplrClick(_instance.GetHe(("button", "innertext", "Add\\ Wallet", "regexp", 0)));
                KeplrImportPkey(log: log);
            }
        }
        public void KeplrUnlock(bool log = false)
        {
            _logger.Send("Unlocking Keplr wallet"   );
            var password = SAFU.HWPass(_project);

        unlock:
            if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Copy\\ Address", "regexp", 0).IsVoid)
            {
                _logger.Send("Keplr wallet is set, ready to select source");
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
                    _logger.Send("Invalid password provided");
                    throw new Exception("Wrong password for Keplr");
                }

                _logger.Send("Wallet unlocked successfully");
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to unlock Keplr wallet: {ex.Message}");
                //throw;
                _instance.CloseAllTabs();
                goto unlock;

            }
        }
        public string KeplrPrune(bool keepTemp = false, bool log = false)
        {
            _logger.Send("Pruning Keplr wallets");
            //_instance.UseFullMouseEmulation = true;
            var imported = "";
            int i = 0;
            _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));
            Thread.Sleep(1000);
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
                    _instance.HeSet(("password", "name"), SAFU.HWPass(_project));
                    KeplrClick(_instance.GetHe(("button", "type", "submit", "regexp", 0)));
                    i++;
                }

                _logger.Send($"Pruned wallets, remaining: {imported}");
                return imported;
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to prune Keplr wallets: {ex.Message}");
                throw;
            }
        }

        public void Import(string importType, bool temp = false, bool log = false)
        {
            // importType: "seed" или "pkey"
            _logger.Send($"Importing Keplr wallet type: {importType}, temp: {temp}");

            var password = SAFU.HWPass(_project);
            string keyOrSeed;
            string walletName;

            if (importType == "seed")
            {
                keyOrSeed = _project.DbKey("seed");
                walletName = "seed";
            }
            else if (importType == "keyEvm")
            {
                keyOrSeed = temp ? new Key().ToHex() : _project.DbKey("evm");
                walletName = temp ? "temp" : "keyEvm";
            }
            else
            {
                try
                {
                    string wType = importType.KeyType();
                    if (wType == "seed" || wType == "keyEvm")
                    {
                        keyOrSeed = importType;
                        importType = wType;
                    }
                    else
                        throw new ArgumentException("Unknown importType: " + importType);
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog(ex.Message);
                    throw new ArgumentException("Unknown importType: " + importType);
                }

            }

            //string keyType = key.KeyType();



            try { _instance.HeGet(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0), deadline: 3); }
            catch { _instance.ActiveTab.Navigate("chrome-extension://" + _extId + "/register.html#/", ""); }

            try
            {
                _instance.HeClick(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Use\\ recovery\\ phrase\\ or\\ private\\ key", "regexp", 0));

                if (importType == "keyEvm")
                {
                    _instance.HeClick(("button", "innertext", "Private\\ key", "regexp", 1));
                    _instance.HeSet(("input:password", "tagname", "input", "regexp", 0), keyOrSeed);
                }
                else // seed
                {
                    var words = keyOrSeed.Split(' ');
                    for (int i = 0; i < words.Length; i++)
                        _instance.HeSet(("input", "fulltagname", "input:", "regexp", i), words[i], delay: 0);
                }

                _instance.HeClick(("button", "innertext", "Import", "regexp", 1));
                _instance.HeSet(("name", "name"), importType);

                try
                {
                    _instance.HeSet(("password", "name"), password, deadline: 3);
                    _instance.HeSet(("confirmPassword", "name"), password);
                }
                catch { }

                _instance.HeClick(("button", "innertext", "Next", "regexp", 0));

                if (importType == "seed")
                    _instance.HeClick(("input:checkbox", "fulltagname", "input:checkbox", "regexp", 0));

                _instance.HeClick(("button", "innertext", "Save", "regexp", 0));

                while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).IsVoid)
                {
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    Thread.Sleep(2000);
                }

                _instance.CloseExtraTabs();
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to import Keplr wallet ({importType}): {ex.Message}");
                throw;
            }
        }
        public void Launch(string source = "seed", string fileName = null, bool log = false)
        {
            _project.Deadline();

            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;
            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

        lnch:
            //_logger.Send("Switching Keplr");
            new ChromeExt(_project, _instance, log: log).Switch("dmkamcknogkgcdfhhbddcghachkejeap");
            _logger.Send("Launching" + fileName);

            _project.Deadline(60);
            try
            {
                if (new ChromeExt(_project, _instance).Install(_extId, fileName, log))
                {
                    Import(source, log: log);
                }
                else
                {
                    Unlock(log);
                }
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message);
                goto lnch;
            }
            SetSource(source, log);
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }
        public void SetSource(string source, bool log = false)
        {
            _logger.Send($"Setting Keplr wallet source to {source}");

            while (true)
            {
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popup.html#/wallet/select", "");
                _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));

                var imported = Prune(log);
                if (imported.Contains("seed") && imported.Contains("keyEvm"))
                {
                    KeplrClick(_instance.GetHe(("div", "innertext", source, "regexp", 0), "last"));
                    _logger.Send($"Source set to {source}");
                    return;
                }

                _logger.Send("Not all wallets imported, adding new wallet");
                KeplrClick(_instance.GetHe(("button", "innertext", "Add\\ Wallet", "regexp", 0)));
                Import("keyEvm", log: log);
            }
        }
        public void Unlock(bool log = false)
        {
            _logger.Send("Unlocking Keplr wallet");
            var password = SAFU.HWPass(_project);

        unlock:
            if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Copy\\ Address", "regexp", 0).IsVoid)
                return;

            try
            {
                _instance.Go($"chrome-extension://{_extId}/popup.html#/");
                try
                {
                    var bal = _instance.HeGet(("div", "innertext", "Total\\ Available\\n\\$", "regexp", 0), "last", deadline: 3).Replace("Total Available\n", "");
                    _logger.Send(bal);
                    return;
                }
                catch (Exception ex) { _project.SendWarningToLog(ex.Message); }


                try { _instance.HeGet(("input:password", "tagname", "input", "regexp", 0)); }
                catch { _instance.CloseAllTabs(); goto unlock; }

                _instance.HeSet(("input:password", "tagname", "input", "regexp", 0), password);
                KeplrClick(_instance.GetHe(("button", "innertext", "Unlock", "regexp", 0)));

                if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Invalid\\ password", "regexp", 0).IsVoid)
                {
                    _instance.CloseAllTabs();
                    _instance.UninstallExtension(_extId);
                    throw new Exception("Wrong password for Keplr");
                }
            }
            catch
            {
                _instance.CloseAllTabs();
                goto unlock;
            }
        }
        public string Prune(bool keepTemp = false, bool log = false)
        {
            _logger.Send("Pruning Keplr wallets");
            var imported = "";
            int i = 0;
            _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));
            Thread.Sleep(1000);

            try
            {
                while (true)
                {
                    var dotBtn = _instance.ActiveTab.FindElementByAttribute(
                        "path",
                        "d",
                        "M10.5 6C10.5 5.17157 11.1716 4.5 12 4.5C12.8284 4.5 13.5 5.17157 13.5 6C13.5 6.82843 12.8284 7.5 12 7.5C11.1716 7.5 10.5 6.82843 10.5 6ZM10.5 12C10.5 11.1716 11.1716 10.5 12 10.5C12.8284 10.5 13.5 11.1716 13.5 12C13.5 12.8284 12.8284 13.5 12 13.5C11.1716 13.5 10.5 12.8284 10.5 12ZM10.5 18C10.5 17.1716 11.1716 16.5 12 16.5C12.8284 16.5 13.5 17.1716 13.5 18C13.5 18.8284 12.8284 19.5 12 19.5C11.1716 19.5 10.5 18.8284 10.5 18Z",
                        "text",
                        i);

                    if (dotBtn.IsVoid) break;

                    var tileText = dotBtn.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement.InnerText;

                    if (tileText.Contains("keyEvm")) { imported += "keyEvm"; i++; continue; }
                    if (tileText.Contains("seed")) { imported += "seed"; i++; continue; }
                    if (keepTemp && tileText.Contains("temp")) { imported += "temp"; i++; continue; }

                    KeplrClick(dotBtn);
                    KeplrClick(_instance.GetHe(("div", "innertext", "Delete\\ Wallet", "regexp", 0), "last"));
                    _instance.HeSet(("password", "name"), SAFU.HWPass(_project));
                    KeplrClick(_instance.GetHe(("button", "type", "submit", "regexp", 0)));
                    i++;
                }
                return imported;
            }
            catch (Exception ex)
            {
                _logger.Send("Failed to prune Keplr wallets: " + ex.Message);
                throw;
            }
        }

    }
    public class KeplrWallet
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        protected readonly string _extId = "dmkamcknogkgcdfhhbddcghachkejeap";
        protected readonly string _fileName = "Keplr0.12.223.crx";

        public KeplrWallet(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string seed = null)
        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "K3PLR");
        }

        private void KeplrClick(HtmlElement he)
        {
            int x = int.Parse(he.GetAttribute("leftInTab")); int y = int.Parse(he.GetAttribute("topInTab"));
            x = x - 450; _instance.Click(x, x, y, y, "Left", "Normal"); Thread.Sleep(1000);
            return;
        }
        private string Prune(bool keepTemp = false, bool log = false)
        {
            _logger.Send("Pruning Keplr wallets");
            var imported = "";
            int i = 0;
            _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));
            Thread.Sleep(1000);

            try
            {
                while (true)
                {
                    var dotBtn = _instance.ActiveTab.FindElementByAttribute(
                        "path",
                        "d",
                        "M10.5 6C10.5 5.17157 11.1716 4.5 12 4.5C12.8284 4.5 13.5 5.17157 13.5 6C13.5 6.82843 12.8284 7.5 12 7.5C11.1716 7.5 10.5 6.82843 10.5 6ZM10.5 12C10.5 11.1716 11.1716 10.5 12 10.5C12.8284 10.5 13.5 11.1716 13.5 12C13.5 12.8284 12.8284 13.5 12 13.5C11.1716 13.5 10.5 12.8284 10.5 12ZM10.5 18C10.5 17.1716 11.1716 16.5 12 16.5C12.8284 16.5 13.5 17.1716 13.5 18C13.5 18.8284 12.8284 19.5 12 19.5C11.1716 19.5 10.5 18.8284 10.5 18Z",
                        "text",
                        i);

                    if (dotBtn.IsVoid) break;

                    var tileText = dotBtn.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement.InnerText;

                    if (tileText.Contains("keyEvm")) { imported += "keyEvm"; i++; continue; }
                    if (tileText.Contains("seed")) { imported += "seed"; i++; continue; }
                    if (keepTemp && tileText.Contains("temp")) { imported += "temp"; i++; continue; }

                    KeplrClick(dotBtn);
                    KeplrClick(_instance.GetHe(("div", "innertext", "Delete\\ Wallet", "regexp", 0), "last"));
                    _instance.HeSet(("password", "name"), SAFU.HWPass(_project));
                    KeplrClick(_instance.GetHe(("button", "type", "submit", "regexp", 0)));
                    i++;
                }
                return imported;
            }
            catch (Exception ex)
            {
                _logger.Send("Failed to prune Keplr wallets: " + ex.Message);
                throw;
            }
        }

        private void Import(string importType, bool temp = false, bool log = false)
        {
            // importType: "seed" или "pkey"
            _logger.Send($"Importing Keplr wallet type: {importType}, temp: {temp}");

            var password = SAFU.HWPass(_project);
            string keyOrSeed;
            string walletName;

            if (importType == "seed")
            {
                keyOrSeed = _project.DbKey("seed");
                walletName = "seed";
            }
            else if (importType == "keyEvm")
            {
                keyOrSeed = temp ? new Key().ToHex() : _project.DbKey("evm");
                walletName = temp ? "temp" : "keyEvm";
            }
            else
            {
                try
                {
                    string wType = importType.KeyType();
                    if (wType == "seed" || wType == "keyEvm")
                    {
                        keyOrSeed = importType;
                        importType = wType;
                    }
                    else
                        throw new ArgumentException("Unknown importType: " + importType);
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog(ex.Message);
                    throw new ArgumentException("Unknown importType: " + importType);
                }

            }

            //string keyType = key.KeyType();



            try { _instance.HeGet(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0), deadline: 3); }
            catch { _instance.ActiveTab.Navigate("chrome-extension://" + _extId + "/register.html#/", ""); }

            try
            {
                _instance.HeClick(("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0));
                _instance.HeClick(("button", "innertext", "Use\\ recovery\\ phrase\\ or\\ private\\ key", "regexp", 0));

                if (importType == "keyEvm")
                {
                    _instance.HeClick(("button", "innertext", "Private\\ key", "regexp", 1));
                    _instance.HeSet(("input:password", "tagname", "input", "regexp", 0), keyOrSeed);
                }
                else // seed
                {
                    var words = keyOrSeed.Split(' ');
                    for (int i = 0; i < words.Length; i++)
                        _instance.HeSet(("input", "fulltagname", "input:", "regexp", i), words[i], delay: 0);
                }

                _instance.HeClick(("button", "innertext", "Import", "regexp", 1));
                _instance.HeSet(("name", "name"), importType);

                try
                {
                    _instance.HeSet(("password", "name"), password, deadline: 3);
                    _instance.HeSet(("confirmPassword", "name"), password);
                }
                catch { }

                _instance.HeClick(("button", "innertext", "Next", "regexp", 0));

                if (importType == "seed")
                    _instance.HeClick(("input:checkbox", "fulltagname", "input:checkbox", "regexp", 0));

                _instance.HeClick(("button", "innertext", "Save", "regexp", 0));

                while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).IsVoid)
                {
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    Thread.Sleep(2000);
                }

                _instance.CloseExtraTabs();
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to import Keplr wallet ({importType}): {ex.Message}");
                throw;
            }
        }
    
        
        public void Launch(string source = "seed", string fileName = null, bool log = false)
        {
            _project.Deadline();

            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;
            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

        lnch:
            new ChromeExt(_project, _instance, log: log).Switch(_extId);
            _logger.Send("Launching" + fileName);

            _project.Deadline(60);
            try
            {
                if (new ChromeExt(_project, _instance).Install(_extId, fileName, log))
                {
                    Import(source, log: log);
                }
                else
                {
                    Unlock(log);
                }
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message);
                goto lnch;
            }
            SetSource(source, log);
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }
        public void SetSource(string source, bool log = false)
        {
            _logger.Send($"Setting Keplr wallet source to {source}");

            while (true)
            {
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popup.html#/wallet/select", "");
                _instance.HeGet(("button", "innertext", "Add\\ Wallet", "regexp", 0));

                var imported = Prune(log);
                if (imported.Contains("seed") && imported.Contains("keyEvm"))
                {
                    KeplrClick(_instance.GetHe(("div", "innertext", source, "regexp", 0), "last"));
                    _logger.Send($"Source set to {source}");
                    return;
                }

                _logger.Send("Not all wallets imported, adding new wallet");
                KeplrClick(_instance.GetHe(("button", "innertext", "Add\\ Wallet", "regexp", 0)));
                Import("keyEvm", log: log);
            }
        }
        public void Unlock(bool log = false)
        {
            _logger.Send("Unlocking Keplr wallet");
            var password = SAFU.HWPass(_project);

        unlock:
            if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Copy\\ Address", "regexp", 0).IsVoid)
                return;

            try
            {
                _instance.Go($"chrome-extension://{_extId}/popup.html#/");
                try
                {
                    var bal = _instance.HeGet(("div", "innertext", "Total\\ Available\\n\\$", "regexp", 0), "last", deadline: 3).Replace("Total Available\n", "");
                    _logger.Send(bal);
                    return;
                }
                catch (Exception ex) { _project.SendWarningToLog(ex.Message); }


                try { _instance.HeGet(("input:password", "tagname", "input", "regexp", 0)); }
                catch { _instance.CloseAllTabs(); goto unlock; }

                _instance.HeSet(("input:password", "tagname", "input", "regexp", 0), password);
                KeplrClick(_instance.GetHe(("button", "innertext", "Unlock", "regexp", 0)));

                if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Invalid\\ password", "regexp", 0).IsVoid)
                {
                    _instance.CloseAllTabs();
                    _instance.UninstallExtension(_extId);
                    throw new Exception("Wrong password for Keplr");
                }
            }
            catch
            {
                _instance.CloseAllTabs();
                goto unlock;
            }
        }
        public void Sign(bool log = false)
        {
            _logger.Send("Approving Keplr transaction");
            var deadline = DateTime.Now.AddSeconds(20);

            try
            {
                while (!(_instance.ActiveTab.URL.Contains(_extId)) && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                }
                if (DateTime.Now >= deadline)
                {
                    _logger.Send("Timeout waiting for Keplr tab");
                    throw new Exception("No Keplr tab detected");
                }

                _instance.UseFullMouseEmulation = false;
            approve:
                _instance.HeClick(("button", "innertext", "Approve", "regexp", 0));
                _logger.Send("Approve button clicked");

                while (_instance.ActiveTab.URL.Contains(_extId) && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                    goto approve;
                }
                if (DateTime.Now >= deadline)
                {
                    _logger.Send("Keplr tab stuck");
                    throw new Exception("Keplr tab stuck");
                }

                _logger.Send("Keplr transaction approved, tab closed");
                return ;
            }
            catch (Exception ex)
            {
                _logger.Send($"Failed to approve Keplr transaction: {ex.Message}");
                throw;
            }
            finally
            {
                _instance.UseFullMouseEmulation = true;
            }
        }

        public string KeplrApprove(bool log = false)
        {
            Sign(log);
            return "done";
        }

    }
}
