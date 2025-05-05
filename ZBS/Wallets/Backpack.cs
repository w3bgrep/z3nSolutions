using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class BackpackWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public BackpackWallet(IZennoPosterProjectModel project, Instance instance, bool log = false,string key = null)
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

        public void BackpackLnch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            Log($"Launching Backpack wallet with file {fileName}", log: log);
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

        public void BackpackUnlock(bool log = false)
        {
            Log("Unlocking Backpack wallet", log: log);
            var password = _pass;


        unlock:
            if (_instance.ActiveTab.URL != $"chrome-extension://{_extId}/popout.html")
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/popout.html", "");

            //_instance.CloseExtraTabs();


            if (!_instance.ActiveTab.FindElementByAttribute("path", "d", "M12 5v14", "text", 0).IsVoid)
            {
                Log("Wallet already unlocked", log: log);
                return;
            }

            try
            {

                try
                {
                    _instance.HeGet(("input:password", "fulltagname", "input:password", "regexp", 0));
                }
                catch
                {
                    _instance.CloseAllTabs();
                    goto unlock;
                }

                _instance.HeSet(("input:password", "fulltagname", "input:password", "regexp", 0), password);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));


                Log("Wallet unlocked successfully", log: log);
            }
            catch (Exception ex)
            {
                Log($"!E Failed to unlock Keplr wallet: {ex.Message}", log: log);
                //throw;
                _instance.CloseAllTabs();
                goto unlock;

            }

        }

        public void BackpackCheck(bool log = false)
        {
            Log("Checking Backpack wallet address", log: log);

            _instance.CloseExtraTabs();

            try
            {
                while (_instance.ActiveTab.FindElementByAttribute("button", "class", "is_Button\\ ", "regexp", 0).IsVoid)
                    _instance.HeClick(("path", "d", "M12 5v14", "text", 0), deadline: 2);

                var publicSOL = _instance.HeGet(("p", "class", "MuiTypography-root\\ MuiTypography-body1", "regexp", 0), "last");
                _instance.HeClick(("button", "aria-label", "TabsNavigator,\\ back", "regexp", 0));
                _project.Variables["addressSol"].Value = publicSOL;
                _sql.Upd($"sol = '{publicSOL}'", "blockchain_public");
                Log($"SOL address: {publicSOL}", log: log);
            }
            catch (Exception ex)
            {
                Log($"Failed to check address: {ex.Message}", log: log);
                throw;
            }
        }

        public void BackpackApprove(bool log = false)
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
    }

}
