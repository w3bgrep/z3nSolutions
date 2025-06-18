using NBitcoin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public class W3b
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly bool _logShow;
        public readonly Sql _sql;
        protected readonly string _acc0;
        protected readonly Dictionary<string, string> _rpcs;
        protected readonly Dictionary<string, string> _adrs;
        public string _adrEvm;
        protected string _key;
        protected readonly Logger _logger;



        public W3b(IZennoPosterProjectModel project, bool log = false, string key = null)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project;
            _acc0 = Acc0();
            _sql = new Sql(_project);
            _logShow = log;
            _rpcs = LoadRPCs();
            _logger = new Logger(project, log: log, classEmoji: "💠");

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
                { "unichain", "https://unichain.drpc.org" },
                { "zero", "https://zero.drpc.org" },
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
                { "monad", "https://testnet-rpc.monad.xyz" },
                // nonEvm
                { "aptos", "https://fullnode.mainnet.aptoslabs.com/v1" },
                { "movement", "https://mainnet.movementnetwork.xyz/v1" },
                //sol
                { "solana", "https://api.mainnet-beta.solana.com" },
                { "solana_devnet", "https://api.devnet.solana.com" },
                { "solana_testnet", "https://api.testnet.solana.com" },
            };

            if (rpcs == null)
            {
                rpcs = rpcs_fallback;
            }
            return rpcs;
        }
        
        
        
        private string Acc0()
        {
            try
            {
                return int.TryParse(_project.Variables["acc0"].Value, out _)
                    ? _project.Variables["acc0"].Value
                    : "";
            }
            catch
            {
                Log("acc0 is empty `y");
                return "";
            }
        }

        protected string ApplyKey(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = _sql.Key("evm");
            }

            if (string.IsNullOrEmpty(key))
            {
                Log("!W key is null or empty");
                throw new Exception("emptykey");
            }
            ;
            return key;

        }



        protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _logger.Send($"({callerName}) [{tolog}] ");
        }
        protected void Log(string address, string balance, string rpc, string contract = null, [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 💠  {callerName}] [{address}] balance {contract} is\n		  [{balance}] by [{rpc}]");
        }
       


        protected string HexToDecimalString(string hexValue, string convert = "")
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
                throw new Exception($"noRpcProvided for {chain}");
            }
        }
        public List<string> Rpc(string[] chains)
        {
            var resultList = new List<string>();
            foreach (var cha in chains)
            {
                string chain = cha.ToLower().Trim();
                if (_rpcs.TryGetValue(cha, out var url))
                {
                    resultList.Add(url);
                }
                else Log($"!W rpc for [{chain}] not found in dictionary");
            }
            return resultList;
        }

        
        
        public string SendLegacy(string chainRpc, string contractAddress, string encodedData, decimal value, string walletKey, int speedup = 1)
        {
            var web3 = new Nethereum.Web3.Web3(chainRpc);

            var chainIdTask = web3.Eth.ChainId.SendRequestAsync();
            chainIdTask.Wait();
            int chainId = (int)chainIdTask.Result.Value;

            string fromAddress = new Nethereum.Signer.EthECKey(walletKey).GetPublicAddress();

            BigInteger _value = (BigInteger)(value * 1000000000000000000m);

            BigInteger gasLimit = 0;
            BigInteger gasPrice = 0;

            try
            {
                var gasPriceTask = web3.Eth.GasPrice.SendRequestAsync();
                gasPriceTask.Wait();
                BigInteger baseGasPrice = gasPriceTask.Result.Value / 100 + gasPriceTask.Result.Value;
                gasPrice = baseGasPrice / 100 * speedup + gasPriceTask.Result.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fail get gasPrice: {ex.Message}");
            }

            try
            {
                var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
                {
                    To = contractAddress,
                    From = fromAddress,
                    Data = encodedData,
                    Value = new Nethereum.Hex.HexTypes.HexBigInteger(_value),
                    GasPrice = new Nethereum.Hex.HexTypes.HexBigInteger(gasPrice)
                };

                var gasEstimateTask = web3.Eth.Transactions.EstimateGas.SendRequestAsync(transactionInput);
                gasEstimateTask.Wait();
                var gasEstimate = gasEstimateTask.Result;
                gasLimit = gasEstimate.Value + (gasEstimate.Value / 2);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is Nethereum.JsonRpc.Client.RpcResponseException rpcEx)
                {
                    var error = $"Err: {rpcEx.RpcError.Code}, Msg: {rpcEx.RpcError.Message}, Errdata: {rpcEx.RpcError.Data}";
                    throw new Exception($"RpcErr : {error}");
                }
                throw;
            }

            try
            {
                var blockchain = new Blockchain(walletKey, chainId, chainRpc);
                string hash = blockchain.SendTransaction(contractAddress, value, encodedData, gasLimit, gasPrice).Result;
                return hash;
            }
            catch (Exception ex)
            {
                throw new Exception($"Send fail: {ex.Message}");
            }
        }
        public string Send1559(string chainRpc, string contractAddress, string encodedData, decimal value, string walletKey, int speedup = 1)
        {
            var web3 = new Nethereum.Web3.Web3(chainRpc);
            var chainIdTask = web3.Eth.ChainId.SendRequestAsync(); chainIdTask.Wait();
            int chainId = (int)chainIdTask.Result.Value;
            string fromAddress = new Nethereum.Signer.EthECKey(walletKey).GetPublicAddress();
            //
            BigInteger _value = (BigInteger)(value * 1000000000000000000m);
            //
            BigInteger gasLimit = 0; BigInteger priorityFee = 0; BigInteger maxFeePerGas = 0; BigInteger baseGasPrice = 0;
            try
            {
                var gasPriceTask = web3.Eth.GasPrice.SendRequestAsync(); gasPriceTask.Wait();
                baseGasPrice = gasPriceTask.Result.Value / 100 + gasPriceTask.Result.Value;
                priorityFee = baseGasPrice / 100 * speedup + gasPriceTask.Result.Value;
                maxFeePerGas = baseGasPrice / 100 * speedup + gasPriceTask.Result.Value;
            }
            catch (Exception ex) { throw new Exception($"failedEstimateGas: {ex.Message}"); }

            try
            {
                var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
                {
                    To = contractAddress,
                    From = fromAddress,
                    Data = encodedData,
                    Value = new Nethereum.Hex.HexTypes.HexBigInteger((BigInteger)_value),
                    MaxPriorityFeePerGas = new Nethereum.Hex.HexTypes.HexBigInteger(priorityFee),
                    MaxFeePerGas = new Nethereum.Hex.HexTypes.HexBigInteger(maxFeePerGas),
                    Type = new Nethereum.Hex.HexTypes.HexBigInteger(2)
                };

                var gasEstimateTask = web3.Eth.Transactions.EstimateGas.SendRequestAsync(transactionInput);
                gasEstimateTask.Wait();
                var gasEstimate = gasEstimateTask.Result;
                gasLimit = gasEstimate.Value + (gasEstimate.Value / 2);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is Nethereum.JsonRpc.Client.RpcResponseException rpcEx)
                {
                    var error = $"Code: {rpcEx.RpcError.Code}, Message: {rpcEx.RpcError.Message}, Data: {rpcEx.RpcError.Data}";
                    throw new Exception($"FailedSimulate RPC Error: {error}");
                }
                throw;
            }
            try
            {
                var blockchain = new Blockchain(_key, chainId, chainRpc);
                string hash = blockchain.SendTransactionEIP1559(contractAddress, value, encodedData, gasLimit, maxFeePerGas, priorityFee).Result;
                return hash;
            }
            catch (Exception ex)
            {
                throw new Exception($"FailedSend: {ex.Message}");
            }
        }


    }
}
