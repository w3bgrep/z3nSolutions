using Leaf.xNet;
using NBitcoin;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.Enums.Browser;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using z3n;
using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;


namespace w3tools //by @w3bgrep
{

    public  static class TestStatic
    {
        public static string Body(this IZennoPosterProjectModel project, Instance instance, string url, string parametr = "ResponseBody", bool reload = false)
        {
            return new Traffic(project, instance).Get(url, parametr);



        }



        public static string HmacSha256(this string message, string _secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            using (var hmacSha256 = new HMACSHA256(keyBytes))
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hashBytes = hmacSha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static string SPLcontract(this string tiker)
        {
            switch (tiker)
            {
                case "Sol":
                    tiker = "So11111111111111111111111111111111111111112";
                    break;
                case "pSol":
                    tiker = "pSo1f9nQXWgXibFtKf7NWYxb5enAM4qfP6UJSiXRQfL";
                    break;
                case "jitoSol":
                    tiker = "J1toso1uCk3RLmjorhTtrVwY9HJ7X8V9yYac6Y7kGCPn";
                    break;
                case "mSol":
                    tiker = "mSoLzYCxHdYgdzU16g5QSh3i5K3z3KZK7ytfqcJm7So";
                    break;
                case "bbSol":
                    tiker = "Bybit2vBJGhPF52GBdNaQfUJ6ZpThSgHBobjWZpLPb4B";
                    break;
                case "soLayer":
                    tiker = "sSo14endRuUbvQaJS3dq36Q829a3A6BEfoeeRGJywEh";
                    break;
                case "ezSol":
                    tiker = "ezSoL6fY1PVdJcJsUpe5CM3xkfmy3zoVCABybm5WtiC";
                    break;
                case "lotusSol":
                    tiker = "gangqfNY8fA7eQY3tHyjrevxHCLnhKRrLGRwUMBR4y6";
                    break;
                case "stepSol":
                    tiker = "StPsoHokZryePePFV8N7iXvfEmgUoJ87rivABX7gaW6";
                    break;
                case "binanceSol":
                    tiker = "BNso1VUJnh4zcfpZa6986Ea66P6TCp59hvtNJ8b1X85";
                    break;
                default:
                    throw new Exception($"unexpected {tiker}");


                
            }
            return tiker;
        }

        public static decimal EthPrice(this IZennoPosterProjectModel project)
        {
            return new OKXApi(project).OKXPrice<decimal>("ETH-USDT");

        }
        public static decimal SolPrice(this IZennoPosterProjectModel project)
        {
            return new OKXApi(project).OKXPrice<decimal>("SOL-USDT");

        }
        public static decimal Price(this IZennoPosterProjectModel project,string tiker)
        {
            tiker = tiker.ToUpper();
            return new OKXApi(project).OKXPrice<decimal>($"{tiker}-USDT");

        }
    }

    public class Binance
    {


        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;

        private readonly bool _logShow;
        private readonly Sql _sql;


        private string _apiKey;
        private string _secretKey;
        private string _proxy;

        public Binance(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logger = new Logger(project, log: log, classEmoji: "BINANCE");
            LoadKeys();
        }


        public string Withdraw(string coin, string network, string address, string amount)
        {

            network = MapNetwork(network);
            string timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            string message = $"coin={coin}&network={network}&address={address}&amount={amount}&timestamp={timestamp}";
            string signature = CalculateHmacSha256Signature(message);
            string payload = $"coin={coin}&network={network}&address={address}&amount={amount}&timestamp={timestamp}&signature={signature}";
            string url = "https://api.binance.com/sapi/v1/capital/withdraw/apply";

            var result = Post(url, payload);
            _logger.Send($" => {address} [{amount} {coin} by {network}]: {result}");
            return result;

        }

        public Dictionary<string, string> GetUserAsset()
        {
            string url = "https://api.binance.com/sapi/v3/asset/getUserAsset";
            string message = $"timestamp={DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}";
            string signature = CalculateHmacSha256Signature(message);
            string payload = $@"{message}&signature={signature}";

            var result = Post(url, payload);

            _project.Json.FromString(result);

            var balances = new Dictionary<string, string>();
            foreach (var item in _project.Json)
            {
                string asset = item.asset;
                string free = item.free;
                balances.Add(asset, free);
            }
            return balances;
        }
        public string GetUserAsset(string coin)
        {
            return GetUserAsset()[coin];
        }

        public List<string> GetWithdrawHistory()
        {

            string url = "https://api.binance.com/sapi/v1/capital/withdraw/history";
            string message = $"timestamp={DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}";
            string signature = CalculateHmacSha256Signature(message);
            string payload = $"{message}&signature={signature}";
            url = url + payload;


            string response = Get(url);


            _project.Json.FromString(response);

            var historyList = new List<string>();
            foreach (var item in _project.Json)
            {
                string id = item.id;
                string amount = item.amount;
                string coin = item.coin;
                string status = item.status.ToString();
                historyList.Add($"{id}:{amount}:{coin}:{status}");
            }
            return historyList;
        }
        public string GetWithdrawHistory(string searchId = "")
        {
            var historyList = GetWithdrawHistory();

            foreach (string withdrawal in historyList)
            {
                if (withdrawal.Contains(searchId))
                    return withdrawal;
            }
            return $"NoIdFound: {searchId}";
        }

        private string MapNetwork(string chain)
        {
            chain = chain.ToLower();
            switch (chain)
            {
                case "arbitrum": return "ARBITRUM";
                case "ethereum": return "ETH";
                case "base": return "BASE";
                case "bsc": return "BSC";
                case "avalanche": return "AVAXC";
                case "polygon": return "MATIC";
                case "optimism": return "OPTIMISM";
                case "trc20": return "TRC20";
                case "zksync": return "ZkSync";
                case "aptos": return "APT";
                case "solana": return "SOLANA";
                default:
                    throw new ArgumentException("Unsupported network: " + chain);
            }
        }
        private string CalculateHmacSha256Signature(string message)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            using (var hmacSha256 = new HMACSHA256(keyBytes))
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hashBytes = hmacSha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        private string Post(string url, string payload)
        {
            var result = ZennoPoster.HTTP.Request(
                ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST,
                url, // url
                payload,
                "application/x-www-form-urlencoded; charset=utf-8",
                _proxy,
                "UTF-8",
                ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                10000,
                "",
                _project.Profile.UserAgent,
                true,
                5,
                new string[] {
                    "X-MBX-APIKEY: "+ _apiKey,
                    "Content-Type: application/x-www-form-urlencoded; charset=utf-8"
                },
                "",
                false,
                false,
                _project.Profile.CookieContainer
                );
            return result;
        }
        private string Get(string url)
        {

            string result = ZennoPoster.HttpGet(
                            url,
                            _proxy,
                            "UTF-8",
                            ResponceType.BodyOnly,
                            30000,
                            "",
                            "Mozilla/4.0",
                            true,
                            5,
                            new string[] {
                            "X-MBX-APIKEY: "+_apiKey,
                            "Content-Type: application/x-www-form-urlencoded; charset=utf-8"
                            },
                            "",
                            true
                        );
            _logger.Send($"json received: [{result}]");
            _project.Json.FromString(result);

            return result;
        }
        private void LoadKeys()
        {
            var creds = new Sql(_project).Get("apikey, apisecret, proxy", "private_api", where: "key = 'binance'").Split('|');


            _apiKey = creds[0];
            _secretKey = creds[1];
            _proxy = creds[2];


        }

    }


}
