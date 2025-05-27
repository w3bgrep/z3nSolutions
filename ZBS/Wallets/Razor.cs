using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class RazorWallet : Wlt
    {
        protected readonly string _extId;
        protected readonly string _fileName;
        protected readonly string _urlImportKey = "chrome-extension://fdcnegogpncmfejlfnffnofpngdiejii/index.html#/account/initialize/import/private-key";

        public RazorWallet(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string seed = null)
            : base(project, instance, log)
        {
            _extId = "fdcnegogpncmfejlfnffnofpngdiejii";
            _fileName = "Razor2.0.9.crx";
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

        public void RazorLnch(string fileName = null, bool log = false)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

            var em = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;

            Log(log: log);
            if (Install(_extId, fileName, log)) RazorImport(log: log);
            else RazorUnlock(log: log);

            _instance.CloseExtraTabs();
            _instance.UseFullMouseEmulation = em;
        }

        public bool RazorImport(bool log = false)
        {
            Log(log: log);
            var key = _key;
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

            
            _instance.AllTabs[1].SetActive();


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
            Log(log: log);
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
            Log(log: log);

            if (_instance.ActiveTab.URL != $"chrome-extension://{_extId}/index.html#/overview")
                _instance.ActiveTab.Navigate($"chrome-extension://{_extId}/index.html#/overview", "");

            // Предполагаемая реализация, аналогичная ZerionCheck
            var active = _instance.HeGet(("div", "class", "_uitext_", "regexp", 0)) ?? "unknown";
            var balance = _instance.HeGet(("div", "class", "_uitext_", "regexp", 1)) ?? "0";
            var pnl = _instance.HeGet(("div", "class", "_uitext_", "regexp", 2)) ?? "0";

            Log($"Active: {active}, Balance: {balance}, PnL: {pnl}", log: log);
        }
    }
}
