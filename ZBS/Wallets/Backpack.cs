using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class BackpackWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;
        protected readonly string _popout = $"chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html";


        public BackpackWallet(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null)
            : base(project, instance, log)
        {
            _extId = "aflkmfhebedbjioipglgcbcmnbpgliof";
            _fileName = "Backpack0.10.94.crx";
            _key = KeyCheck(key);
        }

        string KeyCheck(string key)
        {
            if (string.IsNullOrEmpty(key))
                key = Decrypt(KeyT.base58);
            if (string.IsNullOrEmpty(key))
                throw new Exception("emptykey");
            return key;
        }

        public void Launch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            Log($"Launching Backpack wallet with file {fileName}", log: log);
            if (Install(_extId, fileName, log))
                Import(log: log);
            else
                Unlock(log: log);
            Log($"checking", log: log);
            Check(log: log);
            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }

        public bool Import(bool log = false)
        {
            Log("Importing Backpack wallet with private key", log: log);
            var key = _key;
            var password = _pass;

            _instance.CloseExtraTabs();
            _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/options.html?onboarding=true", "");

            while (true)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("p", "innertext", "Already\\ setup", "regexp", 0).IsVoid)
                {
                    Log("Wallet already set up, skipping import", log: log);
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
                    Log("Successfully imported Backpack wallet", log: log);
                    return true;
                }
            }
        }

        public void Unlock(bool log = false)
        {
            Log("Unlocking Backpack wallet", log: log);
            var password = _pass;
            _project.Deadline();



            if (!_instance.ActiveTab.URL.Contains( _popout))
                _instance.ActiveTab.Navigate(_popout, "");

            check:
            string state = null;
            _project.Deadline(30);
            if (!_instance.ActiveTab.FindElementByAttribute("path", "d", "M12 5v14", "text", 0).IsVoid) state = "unlocked";
            else if (!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "regexp", 0).IsVoid) state = "unlock";


            switch (state)
            {
                case null:
                    Log("unknown state");
                    Thread.Sleep(1000);
                    goto check;
                case "unlocked":
                    return;
                case "unlock":
                    _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), password);
                    _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                    Thread.Sleep(2000);
                    goto check;
            }

        }

        public void Check(bool log = false)
        {
            Log("Checking Backpack wallet address", log: log);
            if (_instance.ActiveTab.URL != _popout)
                _instance.ActiveTab.Navigate(_popout, "");
            _instance.CloseExtraTabs();

            try
            {
                while (_instance.ActiveTab.FindElementByAttribute("button", "class", "is_Button\\ ", "regexp", 0).IsVoid)
                    _instance.HeClick(("path", "d", "M12 5v14", "text", 0));

                var publicSOL = _instance.HeGet(("p", "class", "MuiTypography-root\\ MuiTypography-body1", "regexp", 0), "last");
                _instance.HeClick(("button", "aria-label", "TabsNavigator,\\ back", "regexp", 0));
                _project.Variables["addressSol"].Value = publicSOL;
                //_sql.Upd($"sol = '{publicSOL}'", "public_blockchain");
                Log($"SOL address: {publicSOL}", log: log);
            }
            catch (Exception ex)
            {
                Log($"Failed to check address: {ex.Message}", log: log);
                throw;
            }
        }

        public void Approve(bool log = false)
        {
            Log("Approving Backpack wallet action", log: log);

            try
            {
                _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last");
                _instance.CloseExtraTabs();
                Log("Action approved successfully", log: log);
            }
            catch
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), _pass);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last");
                _instance.CloseExtraTabs();
                Log("Action approved after unlocking", log: log);
            }
        }

        public void Connect(bool log = false)
        {
            string action = null;
        getState:

            try
            {
                action = _instance.HeGet(("div", "innertext", "Approve", "regexp", 0), "last");
            }
            catch (Exception ex)
            {
                if (!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "regexp", 0).IsVoid)
                {
                    _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), _pass);
                    _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                    Thread.Sleep(2000);
                    goto getState;
                }
                _project.L0g($"No Wallet tab found. 0");
                return;
            }

            _project.L0g(action);

            switch (action)
            {
                case "Approve":
                    _instance.HeClick(("div", "innertext", "Approve", "regexp", 0), "last", emu: 1);
                    goto getState;

                default:
                    goto getState;

            }


        }

        public void Add(string type = "Ethereum", string source = "key")//"Solana" | "Ethereum" //"key" | "phrase"
        {
            string _urlAdd = "chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/options.html?add-user-account=true";
            string key;
            if (type == "Ethereum") key = new Sql(_project).Key("evm");
            else key = new Sql(_project).Key("sol");
            _instance.Go(_urlAdd, true);

        check:

            string state = null;

            if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ Wallet", "regexp", 0).IsVoid) state = "importButton";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Backpack\\ supports\\ multiple\\ blockchains.\\nWhich\\ do\\ you\\ want\\ to\\ use\\?\\ You\\ can\\ add\\ more\\ later.", "regexp", 0).IsVoid) state = "chooseChain";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Choose\\ a\\ method\\ to\\ import\\ your\\ wallet.", "regexp", 0).IsVoid) state = "chooseSource";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Enter private key", "text", 0).IsVoid) state = "enterKey";
            else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Open\\ Backpack", "regexp", 0).IsVoid) state = "open";

            switch (state)
            {
                case "importButton":
                    _instance.HeClick(("button", "innertext", "Import\\ Wallet", "regexp", 0));
                    goto check;

                case "chooseChain":
                    _instance.HeClick(("button", "innertext", type, "regexp", 0));
                    goto check;

                case "chooseSource":
                    _instance.HeClick(("button", "innertext", source, "text", 0));
                    goto check;

                case "enterKey":
                    _instance.HeSet(("textarea", "fulltagname", "textarea", "regexp", 0), key);
                    _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
                    goto check;
                case "open":
                    _instance.HeClick(("button", "innertext", "Open\\ Backpack", "regexp", 0));
                    _instance.CloseExtraTabs();
                    return;
                default:
                    goto check;

            }





        }



        public void Switch(string type)//"Solana" | "Ethereum" //"key" | "phrase"

        {
        start:
            if (_instance.ActiveTab.URL != _popout) _instance.ActiveTab.Navigate(_popout, "");
            //_instance.CloseExtraTabs();

            int toUse = 0;
            if (type == "Ethereum")
                toUse = 1;
            _instance.HeClick(("button", "class", "MuiButtonBase-root\\ MuiIconButton-root\\ MuiIconButton-sizeMedium\\ css-xxmhpt\\ css-yt63r3", "regexp", 0));
            int i = 0;
            while (!_instance.ActiveTab.FindElementByAttribute("button", "class", "MuiButtonBase-root\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ css-1y4j1ko", "regexp", i).InnerText.Contains("Add")) i++;


            if (i < 2)
            {
                Add();
                goto start;
            }
            _instance.HeClick(("button", "class", "MuiButtonBase-root\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ MuiButton-root\\ MuiButton-text\\ MuiButton-textPrimary\\ MuiButton-sizeMedium\\ MuiButton-textSizeMedium\\ css-1y4j1ko", "regexp", toUse));

        }

        public string Current()//"Solana" | "Ethereum" //"key" | "phrase"

        {
        start:
            if (_instance.ActiveTab.URL != _popout) _instance.ActiveTab.Navigate(_popout, "");
            var chan = _instance.HeGet(("div", "aria-haspopup", "dialog", "regexp", 0), atr: "innerhtml");
            if (chan.Contains("solana")) return "Solana";
            else if (chan.Contains("ethereum")) return "Ethereum";
            else return "Undefined";
        }

    }


}
