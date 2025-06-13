using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Numerics;

namespace ZBSolutions
{
    public class CommonTx : W3b
    {

        private readonly W3bRead _read;
        public CommonTx(IZennoPosterProjectModel project, string key = null, bool log = false)
        : base(project, log)
        {
            _key = ApplyKey(key);
            _adrEvm = _key.ToPubEvm();//_sql.Address("evm");
            _read = new W3bRead(project);
        }


        
        public string Approve(string contract, string spender, string amount, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;
            string key = _sql.Key("EVM");

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
        public string Wrap(string contract, decimal value, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;
            string key = _sql.Key("EVM");

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
        public string Send(string to, decimal amount, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;

            string txHash = null;

            try
            {
                txHash = SendLegacy(
                    rpc,
                    to,
                    "",
                    amount,
                    _key,
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
