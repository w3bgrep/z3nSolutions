using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Numerics;

using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Nethereum.Model;
using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;

namespace ZBSolutions
{

    public enum GZto
    {
        Sepolia,
        Soneum,
        BNB,
        Gravity,
        Zero,
    }

    public class W3bWrite : W3b
    {
        private readonly string _key;
        private readonly string _adrEvm;
        private readonly W3bRead _read;
        public W3bWrite(IZennoPosterProjectModel project,string key = null, bool log = false)
        : base(project, log)
        {
            _key = Key(key);
            _adrEvm = Address("evm");
            _read = new W3bRead(project);
        }

        private string Key(string key = null) 
        {
            if (string.IsNullOrEmpty(key))
            {
                string encryptedkey = _sql.Get("secp256k1", "accounts.blockchain_private");
                key =  SAFU.Decode(_project, encryptedkey);
            }

            if (string.IsNullOrEmpty(key)) 
            {
                Log("!W key is null or empty");
                throw new Exception("emptykey");
            };
            return key;

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


        public string GzTarget(GZto destination, bool log = false)
        {
            // 0x010066 Sepolia | 0x01019e Soneum | 0x01000e BNB | 0x0100f0 Gravity | 0x010169 Zero

            switch (destination)
            {
                case GZto.Sepolia:
                    return "0x010066";
                case GZto.Soneum:
                    return "0x01019e";
                case GZto.BNB:
                    return "0x01000e";
                case GZto.Gravity:
                    return "0x0100f0";
                case GZto.Zero:
                    return "0x010169";

                default:
                    return "null";
            }

        }

        public string GZ(string chainTo, decimal value, string rpc = null, bool log = false) 

        {

             // 0x010066 Sepolia | 0x01019e Soneum | 0x01000e BNB | 0x0100f0 Gravity | 0x010169 Zero
            string txHash = null;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Random rnd = new Random();
            var accountAddress = _adrEvm;
            string key = _key;

            if (string.IsNullOrEmpty(rpc))
            {
                string chainList = @"https://mainnet.era.zksync.io,
				https://linea-rpc.publicnode.com,
				https://arb1.arbitrum.io/rpc,
				https://optimism-rpc.publicnode.com,
				https://scroll.blockpi.network/v1/rpc/public,
				https://rpc.taiko.xyz,
				https://base.blockpi.network/v1/rpc/public,
				https://rpc.zora.energy";


                bool found = false;
                foreach (string RPC in chainList.Split(','))
                {
                    rpc = RPC.Trim();
                    var native = _read.NativeEVM<decimal>(rpc);
                    var required = value + 0.00015m;
                    if (native > required)
                    {
                        _project.L0g($"CHOSEN: rpc:[{rpc}] native:[{native}]");
                        found = true; break;
                    }
                    if (log) Log($"rpc:[{rpc}] native:[{native}] lower than [{required}]");
                    Thread.Sleep(1000);
                }


                if (!found)
                {
                    return $"fail: no balance over {value}ETH found by all Chains";
                }
            }

            else
            {
                var native = _read.NativeEVM<decimal>(rpc);
                if (log) Log($"rpc:[{rpc}] native:[{native}]");
                if (native < value + 0.0002m)
                {
                    return $"fail: no balance over {value}ETH found on {rpc}";
                }
            }
            string[] types = { };
            object[] values = { };


            try
            {
                string dataEncoded = chainTo;//0x010066 for Sepolia | 0x01019e Soneum | 0x01000e BNB
                txHash = Send1559(
                    rpc,
                    "0x391E7C679d29bD940d63be94AD22A25d25b5A604",//gazZipContract
                    dataEncoded,
                    value,  // value в ETH
                    key,
                    3   // speedup %
                );
                Thread.Sleep(1000);
                _project.Variables["blockchainHash"].Value = txHash;
            }
            catch (Exception ex) { _project.SendWarningToLog($"{ex.Message}", true); throw; }

            if (log) Log(txHash);
            _read.WaitTransaction(rpc, txHash);
            return txHash;
        }
        public string Approve(string contract, string spender, string amount, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;
            string key = _sql.KeyEVM();

            string abi = @"[{""inputs"":[{""name"":""spender"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""}]";

            string txHash = null;

            string[] types = { "address", "uint256" };
            BigInteger amountValue;


            if (amount.ToLower() == "max")
            {
                amountValue = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935"); // max uint256
            }
            else if (amount.ToLower() == "cancel")
            {
                amountValue = BigInteger.Zero;
            }
            else
            {
                try
                {
                    amountValue = BigInteger.Parse(amount);
                    if (amountValue < 0)
                        throw new ArgumentException("Amount cannot be negative");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to parse amount '{amount}': {ex.Message}");
                }
            }

            object[] values = { spender, amountValue };

            try
            {
                txHash = SendLegacy(
                    rpc,
                    contract,
                    Encoder.EncodeTransactionData(abi, "approve", types, values),
                    0,
                    key,
                    3
                );
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    Log($"!W:{ex.Message}");
                }

            }
            catch (Exception ex)
            {
                Log($"!W:{ex.Message}");
                throw;
            }

            Log($"[APPROVE] {contract} for spender {spender} with amount {amount}...");
            return txHash;
        }
        public string WrapNative(string contract, decimal value, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;
            string key = _sql.KeyEVM();

            string abi = @"[{""inputs"":[],""name"":""deposit"",""outputs"":[],""stateMutability"":""payable"",""type"":""function""}]";

            string txHash = null;

            string[] types = { };
            object[] values = { };

            try
            {
                txHash = SendLegacy(
                    rpc,
                    contract,
                    Encoder.EncodeTransactionData(abi, "deposit", types, values),
                    value,
                    key,
                    3
                );
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    Log($"!W:{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Log($"!W:{ex.Message}");
                throw;
            }

            Log($"[WRAP] {value} native to {contract}...");
            return txHash;
        }
        public string SendNative(string to, decimal amount, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;
            string key = _sql.KeyEVM();

            string txHash = null;

            try
            {
                txHash = SendLegacy(
                    rpc,
                    to,
                    "",
                    amount,
                    key,
                    3
                );
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    Log($"!W:{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Log($"!W:{ex.Message}");
                throw;
            }

            Log($"[SEND_NATIVE] {amount} to {to}...");
            return txHash;
        }



    }
}
