using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace z3n
{

    public class W3B
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


        public static decimal EvmNative(string rpc, string address)
        {
            return new W3B().GetEvmBalance(rpc, address).GetAwaiter().GetResult();
        }
        public static decimal ERC20(string tokenContract, string rpc, string address, string tokenDecimal = "18")
        {
            return new W3B().GetErc20Balance(tokenContract, rpc, address, tokenDecimal).GetAwaiter().GetResult();
        }



        public static decimal SolNative(string address, string rpc = "https://api.mainnet-beta.solana.com")
        {
            return new W3B().GetSolanaBalance(rpc, address).GetAwaiter().GetResult();
        }
        public static decimal SolTxFee(string transactionHash, string rpc = null, string tokenDecimal = "9")
        {
            return new W3B().SolFeeByTx(transactionHash, rpc, tokenDecimal).GetAwaiter().GetResult();
        }


        public static decimal Price(string CGid = "ethereum")
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
                throw new Exception(ex.Message);
            }
        }


    }


}
