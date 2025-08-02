using NBitcoin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
{
    public class CommonTx
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Logger _logger;
        public CommonTx(IZennoPosterProjectModel project, string key = null, bool log = false)   
        {
            _project = project;
            _logger = new Logger(project, log: log, classEmoji: "💠");
        }
 
        public string Approve(string contract, string spender, string amount, string rpc)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string key = new Sql(_project).Key("evm");

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
            string encoded = Encoder.EncodeTransactionData(abi, "approve", types, values);


            try
            {
                txHash = new W3b(_project).SendTx(rpc, contract, encoded, 0, key, 0, 3);        
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    _logger.Send($"!W:{ex.Message}");
                }

            }
            catch (Exception ex)
            {
                _logger.Send($"!W:{ex.Message}");
                throw;
            }

            _logger.Send($"[APPROVE] {contract} for spender {spender} with amount {amount}...");
            return txHash;
        }
        public string Wrap(string contract, decimal value, string rpc )
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string key = new Sql(_project).Key("evm");

            string abi = @"[{""inputs"":[],""name"":""deposit"",""outputs"":[],""stateMutability"":""payable"",""type"":""function""}]";

            string txHash = null;

            string[] types = { };
            object[] values = { };
            string encoded = Encoder.EncodeTransactionData(abi, "deposit", types, values);


            try
            {
                txHash = new W3b(_project).SendTx(rpc, contract, encoded, value, key, 0, 3);

                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    _logger.Send($"!W:{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.Send($"!W:{ex.Message}");
                throw;
            }

            _logger.Send($"[WRAP] {value} native to {contract}...");
            return txHash;
        }
        public string SendNative(string to, decimal amount, string rpc)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string key = new Sql(_project).Key("evm");
            string txHash = null;
            string encoded = "";
            try
            {
                txHash = new W3b(_project).SendTx(rpc, to, encoded, amount, key, 0, 3);
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    _logger.Send($"!W:{ex.Message}",show:true);
                }
            }
            catch (Exception ex)
            {
                _logger.Send($"!W:{ex.Message}", show: true);
                throw;
            }
            _logger.Send($"sent [{amount}] to [{to}] by [{rpc}] [{txHash}]");

            return txHash;
        }
        public string SendERC20(string contract, string to, decimal amount, string rpc)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string key = new Sql(_project).Key("evm");
            string txHash = null;

            try
            {

                string abi = @"[{""inputs"":[{""name"":""to"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""}]";
                string[] types = { "address", "uint256" };
                decimal scaledAmount = amount * 1000000000000000000m;
                BigInteger amountValue = (BigInteger)Math.Floor(scaledAmount); 
                object[] values = { to, amountValue };
                string encoded = z3nCore.Encoder.EncodeTransactionData(abi, "transfer", types, values);
                txHash = new W3b(_project).SendTx(rpc, contract, encoded, 0, key, 0, 3);      
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    _logger.Send($"!W:{ex.Message}", show:true);
                }
            }
            catch (Exception ex)
            {
                _logger.Send($"!W:{ex.Message}", show:true);
                throw;
            }

            _logger.Send($"sent [{amount}] of [{contract}]  to [{to}] by [{rpc}] [{txHash}]");
            return txHash;
        }
        public string SendERC721(string contract, string to, BigInteger tokenId, string rpc)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            string txHash = null;
            string key = new Sql(_project).Key("evm");
            try
            {
                string abi = @"[{""inputs"":[{""name"":""from"",""type"":""address""},{""name"":""to"",""type"":""address""},{""name"":""tokenId"",""type"":""uint256""}],""name"":""safeTransferFrom"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""}]";
                string[] types = { "address", "address", "uint256" };
                object[] values = { key.ToPubEvm(), to, tokenId };
                string encoded = z3nCore.Encoder.EncodeTransactionData(abi, "safeTransferFrom", types, values);
                

                txHash = new W3b(_project).SendTx(rpc, contract, encoded, 0, key, 0, 3);
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    _logger.Send($"!W:{ex.Message}", show: true);
                }
            }
            catch (Exception ex)
            {
                _logger.Send($"!W:{ex.Message}", show: true);
                throw;
            }

            _logger.Send($"sent [{contract}/{tokenId}] to [{to}] by [{rpc}] [{txHash}]");
            return txHash;
        }
    }
}
