using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
{
    public class RabbyWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;

        public RabbyWallet(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string seed = null)
            : base(project, instance, log)
        {
            _extId = "acmacodkjbdgmoleebolmdjonilkdbch";
            _fileName = "Rabby0.93.24.crx";
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

        public void RabbyLnch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = true;

            Log($"Launching Rabby wallet with file {fileName}", log: log);
            if (Install(_extId, fileName, log))
                RabbyImport(log: log);
            else
                RabbyUnlock(log: log);

            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }

        public void RabbyImport(bool log = false)
        {
            Log("Importing Rabby wallet with private key", log: log);
            var key = _key;
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
                Log("Successfully imported Rabby wallet", log: log);
            }
            catch (Exception ex)
            {
                Log($"Failed to import Rabby wallet: {ex.Message}", log: log);
                throw;
            }
        }

        public void RabbyUnlock(bool log = false)
        {
            Log("Unlocking Rabby wallet", log: log);
            var password = _pass;

            _instance.UseFullMouseEmulation = true;

            while (_instance.ActiveTab.URL == $"chrome-extension://{_extId}/offscreen.html")
            {
                Log("Closing offscreen tab and retrying unlock", log: log);
                _instance.ActiveTab.Close();
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/index.html#/unlock", "");
            }

            try
            {
                _instance.HeSet(("password", "id"), password);
                _instance.HeClick(("button", "innertext", "Unlock", "regexp", 0));
                Log("Wallet unlocked successfully", log: log);
            }
            catch (Exception ex)
            {
                Log($"Failed to unlock Rabby wallet: {ex.Message}", log: log);
                throw;
            }
        }
    }

}
