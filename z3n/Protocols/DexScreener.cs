using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace z3n.Protocols
{
    public class DexScreener
    {
        private readonly string _apiKey = "CG-TJ3DRjP93bTSCto6LiPbMgaV";

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
                //{ "x-cg-demo-api-key", _apiKey },
            },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }

        public static string DSPrice(string contract = "So11111111111111111111111111111111111111112", string chain = "solana")
        {
            try
            {
                string result = new DexScreener().CoinInfo(contract,chain).GetAwaiter().GetResult();

                var json = JObject.Parse(result);
                JToken usdPriceToken = json["priceNative"]?["current_price"]?["usd"];

                if (usdPriceToken == null)
                {
                    return "0";
                }

                decimal usdPrice = usdPriceToken.Value<decimal>();
                string priceString = usdPrice.ToString(CultureInfo.InvariantCulture);
                return priceString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



    }
}
