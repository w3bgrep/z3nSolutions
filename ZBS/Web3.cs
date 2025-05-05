using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class W3b
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly bool _logShow;
        protected readonly Sql _sql;


        protected readonly Dictionary<string, string> _rpcs;
        protected readonly Dictionary<string, string> _adrs;

        public W3b(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project;
            _sql = new Sql(_project);
            _logShow = log;
            _rpcs = LoadRPCs();
            _adrs = LoadAddresses();
        }
        protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 💠  {callerName}] [{tolog}] ");
        }
        private Dictionary<string, string> LoadRPCs(Dictionary<string, string> rpcs = null)
        {

            var rpcs_fallback = new Dictionary<string, string>
            {
                { "ethereum", "https://ethereum-rpc.publicnode.com" },
                { "arbitrum", "https://arbitrum-one.publicnode.com" },
                { "base", "https://base-rpc.publicnode.com" },
                { "blast", "https://rpc.blast.io" },
                { "fantom", "https://rpc.fantom.network" },
                { "linea", "https://rpc.linea.build" },
                { "manta", "https://pacific-rpc.manta.network/http" },
                { "optimism", "https://optimism-rpc.publicnode.com" },
                { "scroll", "https://rpc.scroll.io" },
                { "soneium", "https://rpc.soneium.org" },
                { "taiko", "https://rpc.mainnet.taiko.xyz" },
                { "zksync", "https://mainnet.era.zksync.io" },
                { "zora", "https://rpc.zora.energy" },
                // nonEthEvm
                { "avalanche", "https://avalanche-c-chain.publicnode.com" },
                { "bsc", "https://bsc-rpc.publicnode.com" },
                { "gravity", "https://rpc.gravity.xyz" },
                { "fantom", "https://rpc.fantom.network" },
                { "opbnb", "https://opbnb-mainnet-rpc.bnbchain.org" },
                { "polygon", "https://polygon-rpc.com" },
                // Testnets
                { "sepolia", "https://ethereum-sepolia-rpc.publicnode.com" },
                { "reddio", "https://reddio-dev.reddio.com" },
                { "xrp", "https://rpc.testnet.xrplevm.org/" },
                // nonEvm
                { "aptos", "https://fullnode.mainnet.aptoslabs.com/v1" },
                { "movement", "https://mainnet.movementnetwork.xyz/v1" }
            };

            if (rpcs == null)
            {
                Log("default Rpc Loaded");
                rpcs = rpcs_fallback;
            }          
            return rpcs;   
        }
        private Dictionary<string, string> LoadAddresses(Dictionary<string, string> addresses = null)
        {
            if (addresses == null)
            {
                Log("default Rpc Loaded");
                addresses = _sql.GetAddresses();
            }
            return addresses;

        }

        public string Rpc(string chain)
        {
            chain = chain.ToLower().Trim();
            if (_rpcs.TryGetValue(chain, out var url))
            {
                return url;
            }
            else
            {
                Log($"!W rpc for [{chain}] not found in dictionary");
                throw new Exception("noRpcProvided");
            }
        }
        public string Address(string chainType)
        {
            chainType = chainType.ToLower().Trim();
            if (_adrs.TryGetValue(chainType, out var rpc))
            {
                return rpc;
            }
            else
            {
                Log($"!W rpc for [{chainType}] not found in dictionary");
                throw new Exception("noRpcProvided");
            }
        }


    }
}
