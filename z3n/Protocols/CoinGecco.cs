using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace z3n

{
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

        public static string Price(string CGid = "ethereum")
        {
            try
            {
                string result = new CoinGecco().CoinInfo(CGid).GetAwaiter().GetResult();

                var json = JObject.Parse(result);
                JToken usdPriceToken = json["market_data"]?["current_price"]?["usd"];

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
