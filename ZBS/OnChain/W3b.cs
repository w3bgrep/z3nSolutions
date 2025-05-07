using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
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

        protected void Log(string address, string balance, string rpc, string contract = null, [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 💠  {callerName}] [{address}] balance {contract} is\n		  [{balance}] by [{rpc}]");
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
        protected string HexToString(string hexValue, string convert = "")
        {
            try
            {
                hexValue = hexValue?.Replace("0x", "").Trim();
                if (string.IsNullOrEmpty(hexValue)) return "0";
                BigInteger number = BigInteger.Parse("0" + hexValue, NumberStyles.AllowHexSpecifier);
                switch (convert.ToLower())
                {
                    case "gwei":
                        decimal gweiValue = (decimal)number / 1000000000m;
                        return gweiValue.ToString("0.#########", CultureInfo.InvariantCulture);
                    case "eth":
                        decimal ethValue = (decimal)number / 1000000000000000000m;
                        return ethValue.ToString("0.##################", CultureInfo.InvariantCulture);
                    default:
                        return number.ToString();
                }
            }
            catch
            {
                return "0";
            }
        }
        protected T FloorDecimal<T>(decimal value, int? decimalPlaces = null)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            int effectiveDecimalPlaces = decimalPlaces ?? 18;

            if (effectiveDecimalPlaces < 0)
                throw new ArgumentException("Decimal places must be non-negative", nameof(decimalPlaces));

            try
            {
                string valueStr = value.ToString(CultureInfo.InvariantCulture);
                int actualDecimalPlaces = 0;
                if (valueStr.Contains("."))
                {
                    actualDecimalPlaces = valueStr.Split('.')[1].Length;
                }

                effectiveDecimalPlaces = Math.Min(effectiveDecimalPlaces, actualDecimalPlaces);

                if (effectiveDecimalPlaces > 28) // decimal type supports up to 28-29 digits
                {
                    _project.SendWarningToLog($"Requested decimal places ({effectiveDecimalPlaces}) exceeds decimal type limit. Adjusting to 28.");
                    effectiveDecimalPlaces = 28;
                }

                decimal multiplier = (decimal)Math.Pow(10, effectiveDecimalPlaces);
                decimal flooredValue = Math.Floor(value * multiplier) / multiplier;

                if (typeof(T) == typeof(string))
                {
                    string format = "0." + new string('#', effectiveDecimalPlaces);
                    return (T)Convert.ChangeType(flooredValue.ToString(format, CultureInfo.InvariantCulture), typeof(T));
                }
                if (typeof(T) == typeof(int))
                    return (T)Convert.ChangeType((int)flooredValue, typeof(T));
                if (typeof(T) == typeof(double))
                    return (T)Convert.ChangeType((double)flooredValue, typeof(T));
                return (T)Convert.ChangeType(flooredValue, typeof(T));
            }
            catch (OverflowException ex)
            {
                _project.SendWarningToLog($"Overflow error while flooring {value} to {effectiveDecimalPlaces} decimal places: {ex.Message}");
                return (T)Convert.ChangeType(value, typeof(T)); // Return original value as fallback
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog($"Error while flooring {value} to {effectiveDecimalPlaces} decimal places: {ex.Message}");
                return (T)Convert.ChangeType(value, typeof(T)); // Return original value as fallback
            }
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
