using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace z3n
{
    public static class Rpc
    {
        private static readonly Dictionary<string, string> _rpcs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"Ethereum", "https://ethereum-rpc.publicnode.com"},
            {"Arbitrum", "https://arbitrum-one.publicnode.com"},
            {"Base", "https://base-rpc.publicnode.com"},
            {"Blast", "https://rpc.blast.io"},
            {"Fantom", "https://rpc.fantom.network"},
            {"Linea", "https://rpc.linea.build"},
            {"Manta", "https://pacific-rpc.manta.network/http"},
            {"Optimism", "https://optimism-rpc.publicnode.com"},
            {"Scroll", "https://rpc.scroll.io"},
            {"Soneium", "https://rpc.soneium.org"},
            {"Taiko", "https://rpc.mainnet.taiko.xyz"},
            {"Unichain", "https://unichain.drpc.org"},
            {"Zero", "https://zero.drpc.org"},
            {"Zksync", "https://mainnet.era.zksync.io"},
            {"Zora", "https://rpc.zora.energy"},

            {"Avalanche", "https://avalanche-c-chain.publicnode.com"},
            {"Bsc", "https://bsc-rpc.publicnode.com"},
            {"Gravity", "https://rpc.gravity.xyz"},
            {"Opbnb", "https://opbnb-mainnet-rpc.bnbchain.org"},
            {"Polygon", "https://polygon-rpc.com"},

            {"Sepolia", "https://ethereum-sepolia-rpc.publicnode.com"},
            {"Monad", "https://testnet-rpc.monad.xyz"},
            {"Aptos", "https://fullnode.mainnet.aptoslabs.com/v1"},
            {"Movement", "https://mainnet.movementnetwork.xyz/v1"},

            {"Solana", "https://api.mainnet-beta.solana.com"},
            {"Solana_Devnet", "https://api.devnet.solana.com"},
            {"Solana_Testnet", "https://api.testnet.solana.com"}
        };

        public static string Get(string name)
        {
            if (_rpcs.TryGetValue(name, out var url))
                return url;
            foreach (var key in _rpcs.Keys)
            {
                if (string.Equals(key.Replace("_", ""), name.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                    return _rpcs[key];
            }
            throw new ArgumentException($"No RpcUrl provided for '{name}'");
        }
        public static string Ethereum => _rpcs["Ethereum"];
        public static string Arbitrum => _rpcs["Arbitrum"];
        public static string Base => _rpcs["Base"];
        public static string Blast => _rpcs["Blast"];
        public static string Fantom => _rpcs["Fantom"];
        public static string Linea => _rpcs["Linea"];
        public static string Manta => _rpcs["Manta"];
        public static string Optimism => _rpcs["Optimism"];
        public static string Scroll => _rpcs["Scroll"];
        public static string Soneium => _rpcs["Soneium"];
        public static string Taiko => _rpcs["Taiko"];
        public static string Unichain => _rpcs["Unichain"];
        public static string Zero => _rpcs["Zero"];
        public static string Zksync => _rpcs["Zksync"];
        public static string Zora => _rpcs["Zora"];
        public static string Avalanche => _rpcs["Avalanche"];
        public static string Bsc => _rpcs["Bsc"];
        public static string Gravity => _rpcs["Gravity"];
        public static string Opbnb => _rpcs["Opbnb"];
        public static string Polygon => _rpcs["Polygon"];
        public static string Sepolia => _rpcs["Sepolia"];
        public static string Reddio => _rpcs["Reddio"];
        public static string Xrp => _rpcs["Xrp"];
        public static string Monad => _rpcs["Monad"];
        public static string Aptos => _rpcs["Aptos"];
        public static string Movement => _rpcs["Movement"];
        public static string Solana => _rpcs["Solana"];
        public static string Solana_Devnet => _rpcs["Solana_Devnet"];
        public static string Solana_Testnet => _rpcs["Solana_Testnet"];
    }





    public class EvmTools
    {
        public async Task<decimal> GetEvmBalance(string rpc, string address)
        {
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_getBalance"", ""params"": [""{address}"", ""latest""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string hexBalance = json["result"]?.ToString().Replace("0x", "") ?? "0";
                    BigInteger balanceWei = BigInteger.Parse(hexBalance, NumberStyles.AllowHexSpecifier);
                    decimal balance = (decimal)balanceWei / 1000000000000000000m;
                    return balance;
                }
            }
        }

        public async Task<decimal> GetErc20Balance(string tokenContract, string rpc, string address, string tokenDecimal = "18")
        {
            string data = "0x70a08231000000000000000000000000" + address.Replace("0x", "");
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string hexBalance = json["result"]?.ToString().Replace("0x", "") ?? "0";
                    BigInteger balanceRaw = BigInteger.Parse(hexBalance, NumberStyles.AllowHexSpecifier);
                    decimal decimals = (decimal)Math.Pow(10, double.Parse(tokenDecimal, CultureInfo.InvariantCulture));
                    decimal balance = (decimal)balanceRaw / decimals;
                    return balance;
                }
            }
        }

        public async Task<decimal> GetErc721Balance(string tokenContract, string rpc, string address)
        {
            string data = "0x70a08231000000000000000000000000" + address.Replace("0x", "").ToLower();
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string hexBalance = json["result"]?.ToString().Replace("0x", "") ?? "0";
                    BigInteger balanceRaw = BigInteger.Parse(hexBalance, NumberStyles.AllowHexSpecifier);
                    return (decimal)balanceRaw;
                }
            }
        }

        public async Task<decimal> GetErc1155Balance(string tokenContract, string tokenId, string rpc, string address)
        {
            string data = "0x00fdd58e" + address.Replace("0x", "").ToLower().PadLeft(64, '0') + BigInteger.Parse(tokenId).ToString("x").PadLeft(64, '0');
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string hexBalance = json["result"]?.ToString().Replace("0x", "") ?? "0";
                    BigInteger balanceRaw = BigInteger.Parse(hexBalance, NumberStyles.AllowHexSpecifier);
                    return (decimal)balanceRaw;
                }
            }
        }

    }


    public class SolTools
    {
        public async Task<decimal> GetSolanaBalance(string rpc, string address)
        {
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getBalance"", ""params"": [""{address}""], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    BigInteger balanceLamports = json["result"]?["value"]?.ToObject<BigInteger>() ?? 0;
                    decimal balance = (decimal)balanceLamports / 1_000_000_000m; // Convert lamports to SOL
                    return balance;
                }
            }
        }
        
        public async Task<decimal> GetSplTokenBalance(string rpc, string walletAddress, string tokenMint)
        {
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getTokenAccountsByOwner"", ""params"": [""{walletAddress}"", {{""mint"": ""{tokenMint}""}}, {{""encoding"": ""jsonParsed""}}], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(body);

                    var accounts = json["result"]?["value"] as JArray;
                    if (accounts == null || accounts.Count == 0)
                        return 0m;

                    var tokenData = accounts[0]?["account"]?["data"]?["parsed"]?["info"];
                    if (tokenData == null)
                        return 0m;

                    string amount = tokenData["tokenAmount"]?["uiAmountString"]?.ToString();
                    if (string.IsNullOrEmpty(amount))
                        return 0m;

                    return decimal.Parse(amount, CultureInfo.InvariantCulture);
                }
            }
        }
        
        public async Task<decimal> SolFeeByTx(string transactionHash, string rpc = null, string tokenDecimal = "9")
        {
            if (string.IsNullOrEmpty(rpc)) rpc = "https://api.mainnet-beta.solana.com";

            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getTransaction"", ""params"": [""{transactionHash}"", {{""encoding"": ""jsonParsed"", ""maxSupportedTransactionVersion"": 0}}], ""id"": 1 }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Post,
                    RequestUri = new Uri(rpc),
                    Content = new System.Net.Http.StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(body);
                    string feeLamports = json["result"]?["meta"]?["fee"]?.ToString() ?? "0";
                    BigInteger balanceRaw = BigInteger.Parse(feeLamports);
                    decimal decimals = (decimal)Math.Pow(10, double.Parse(tokenDecimal, CultureInfo.InvariantCulture));
                    decimal balance = (decimal)balanceRaw / decimals;
                    return balance;
                }
            }
        }

    }

    public class CoinGecco
    {

        private readonly string _apiKey = "CG-TJ3DRjP93bTSCto6LiPbMgaV";
        public async Task<string> CoinInfo(string CGid = "ethereum")
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = System.Net.Http.HttpMethod.Get,
                RequestUri = new Uri($"https://api.coingecko.com/api/v3/coins/{CGid}"),
                Headers =
                {
                    { "accept", "application/json" },
                    { "x-cg-demo-api-key", _apiKey },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }

    }

    public class DexScreener
    {

        public async Task<string> CoinInfo(string contract, string chain)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = System.Net.Http.HttpMethod.Get,
                RequestUri = new Uri($"https://api.dexscreener.com/tokens/v1/{chain}/{contract}"),
                Headers =
                {
                    { "accept", "application/json" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }

    }


    public static class W3bTools 
    
    {

        public static decimal EvmNative(string rpc, string address)
        {
            return new EvmTools().GetEvmBalance(rpc, address).GetAwaiter().GetResult();
        }
        public static decimal ERC20(string tokenContract, string rpc, string address, string tokenDecimal = "18")
        {
            return new EvmTools().GetErc20Balance(tokenContract, rpc, address, tokenDecimal).GetAwaiter().GetResult();
        }
        public static decimal ERC721(string tokenContract, string rpc, string address)
        {
            return new EvmTools().GetErc721Balance(tokenContract, rpc, address).GetAwaiter().GetResult();
        }
        public static decimal ERC1155(string tokenContract, string tokenId, string rpc, string address)
        {
            return new EvmTools().GetErc1155Balance(tokenContract, tokenId, rpc, address).GetAwaiter().GetResult();
        }

        public static decimal SolNative(string address, string rpc = "https://api.mainnet-beta.solana.com")
        {
            return new SolTools().GetSolanaBalance(rpc, address).GetAwaiter().GetResult();
        }
        public static decimal SPL(string tokenMint, string walletAddress, string rpc = "https://api.mainnet-beta.solana.com")
        {
            return new SolTools().GetSplTokenBalance(rpc, walletAddress, tokenMint).GetAwaiter().GetResult();
        }
        public static decimal SolTxFee(string transactionHash, string rpc = null, string tokenDecimal = "9")
        {
            return new SolTools().SolFeeByTx(transactionHash, rpc, tokenDecimal).GetAwaiter().GetResult();
        }


        public static decimal CGPrice(string CGid = "ethereum",[CallerMemberName] string callerName = "")
        {
            try
            {
                string result = new CoinGecco().CoinInfo(CGid).GetAwaiter().GetResult();

                var json = JObject.Parse(result);
                JToken usdPriceToken = json["market_data"]?["current_price"]?["usd"];

                if (usdPriceToken == null)
                {
                    return 0m;
                }

                decimal usdPrice = usdPriceToken.Value<decimal>();
                return usdPrice;
            }
            catch (Exception ex)
            {
                var stackFrame = new System.Diagnostics.StackFrame(1);
                var callingMethod = stackFrame.GetMethod();
                string method = string.Empty;
                if (callingMethod != null)
                    method = $"{callingMethod.DeclaringType.Name}.{callerName}";
                throw new Exception(ex.Message + $"\n{method}");
            }
        }
        public static decimal DSPrice(string contract = "So11111111111111111111111111111111111111112", string chain = "solana",[CallerMemberName] string callerName = "")
        {
            try
            {
                string result = new DexScreener().CoinInfo(contract, chain).GetAwaiter().GetResult();

                var json = JArray.Parse(result);
                JToken priceToken = json.FirstOrDefault()?["priceNative"];

                if (priceToken == null)
                {
                    return 0m;
                }

                return priceToken.Value<decimal>();
            }
            catch (Exception ex)
            {
                var stackFrame = new System.Diagnostics.StackFrame(1);
                var callingMethod = stackFrame.GetMethod();
                string method = string.Empty;
                if (callingMethod != null)
                    method = $"{callingMethod.DeclaringType.Name}.{callerName}";
                throw new Exception(ex.Message + $"\n{method}");
            }
        }


    }

}
