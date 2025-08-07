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
    public class Zerion
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        private readonly string _pass;
        private readonly string _extId = "klghhnkeealcohjjanjjdaeeggmfmlpl";

        public Zerion(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "🇿");
        }
        private string KeyLoad(string key)
        {
            if (string.IsNullOrEmpty(key)) key = "key";
            switch (key)
            {
                case "key":
                    _logger.Send("using key from db");
                    key = _project.DbKey("evm");
                    break;
                case "seed":
                    _logger.Send("using seed from db");
                    key = _project.DbKey("seed");
                    break;
                default:
                    _logger.Send("using provided key");
                    return key;
            }
            if (string.IsNullOrEmpty(key)) 
                throw new Exception("keyIsEmpy");
            return key;
        }
        public string Install(string filename = "Zerion1.26.1.crx") 
        {
            string path = $"{_project.Path}.crx\\{filename}";
            string id = _extId;

        install:
            _instance.InstallCrxExtension(path);
            Thread.Sleep(2000);
            string securePrefsPath = _project.Variables["pathProfileFolder"].Value + @"\Default\Secure Preferences";
            try
            {
                string v = Utils.GetExtVer(securePrefsPath, id);
                if (v != "1.26.1") _instance.UninstallExtension(id);
                return v;
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message, true);
                goto install;
            }


        }
        public string Import(string key = null, string refCode = null)
        {
            key = KeyLoad(key);
            var keyType = key.KeyType();
            _logger.Send($"importing {keyType}");
            try
            {
                if (_instance.ActiveTab.IsBusy) _instance.ActiveTab.WaitDownloading();

                _instance.HeClick(("div", "innertext", "Import\\ Existing\\ Wallet", "regexp", 0), "last");

                if (keyType == "seed")
                {
                    _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/mnemonic", "regexp", 0));
                    int index = 0;
                    foreach (string word in key.Split(' '))
                    {
                        _instance.ActiveTab.FindElementById($"word-{index}").SetValue(word, "Full", false);
                        index++;
                    }
                }
                else
                {
                    _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/private-key", "regexp", 0));
                    _instance.HeSet(("key", "name"), _project.Variables["key"].Value);
                }
                
                _instance.HeClick(("button", "innertext", "Import\\ Wallet", "regexp", 0));

                var pass = SAFU.HWPass(_project);
                _instance.HeSet(("password", "name"), pass);
                _instance.HeClick(("button", "innertext", "Confirm\\ Password", "regexp", 0));

                _instance.HeSet(("confirmPassword", "name"), pass);
                _instance.HeClick(("button", "innertext", "Set\\ Password", "regexp", 0));
                _instance.HeGet(("button", "innertext", "Enter\\ Referral\\ Code", "regexp", 0));

                if (!string.IsNullOrEmpty(refCode))
                {
                    _instance.HeClick(("button", "innertext", "Enter\\ Referral\\ Code", "regexp", 0));
                    _instance.HeSet((("referralCode", "name")), refCode);
                    _instance.HeClick(("button", "class", "_regular", "regexp", 0));
                }
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html", "");

                var address = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive\\?address=", "regexp", 0), atr: "href")
                    .Replace("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive?address=", "");

                return address;
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message, true);

                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html", "");
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html", "");

                var address = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive\\?address=", "regexp", 0), atr: "href")
                    .Replace("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive?address=", "");

                return address;
            }
        }
        private void Unlock()
        {
            try
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass, deadline: 3);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            }
            catch (Exception ex)
            {
                _logger.Send(ex.Message);
            }
        }
        public string Load(string key = null, string refCode = null, string filename = "Zerion1.26.1.crx")
        {
            Install(filename);
        import:
            try
            {
                var address = Import(key, refCode);
                return address;
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message);
                Unlock();
                goto import;
            }
        }
        public void Connect(bool log = false)
        {

            string action = null;
        getState:

            try
            {
                action = _instance.HeGet(("button", "class", "_primary", "regexp", 0), "last");
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
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0), "last");
                    goto getState;
                case "Close":
                    _project.L0g($"added {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0), "last");
                    goto getState;
                case "Connect":
                    _project.L0g($"connecting {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0), "last");
                    goto getState;
                case "Sign":
                    _project.L0g($"sign {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0), "last");
                    goto getState;
                case "Sign In":
                    _project.L0g($"sign {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                    _instance.HeClick(("button", "class", "_primary", "regexp", 0), "last");
                    goto getState;

                default:
                    goto getState;

            }


        }

    }
}