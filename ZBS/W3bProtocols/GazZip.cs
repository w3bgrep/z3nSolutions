
using System;

using System.Globalization;

using System.Numerics;

using System.Threading;

using ZennoLab.InterfacesLibrary.ProjectModel;


namespace ZBSolutions.OnChain
{
    public enum GZto
    {
        Sepolia,
        Soneum,
        BNB,
        Gravity,
        Zero,
    }

    public class GazZip : W3b
    {

        private readonly W3bRead _read;
        public GazZip(IZennoPosterProjectModel project, string key = null, bool log = false)
        : base(project, log)
        {
            _key = ApplyKey(key);
            _adrEvm = _key.ToPubEvm();//_sql.Address("evm");
            _read = new W3bRead(project);
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
 


    }
}
