using System;
using System.Collections.Generic;

using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using ZennoLab.InterfacesLibrary.Enums.Browser;
using System.Globalization;
using System.Runtime.CompilerServices;
using Leaf.xNet;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Security.Policy;
using ZennoLab.InterfacesLibrary;
using z3n;
using NBitcoin;

using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Numerics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;
using Newtonsoft.Json.Linq;
using System.Dynamic;

using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;

using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI;
using Nethereum.Util;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;


namespace w3tools //by @w3bgrep
{

    public  static class TestStatic
    {

        public static string UnixToHuman(this IZennoPosterProjectModel project, string decodedResultExpire = null)
        {
            var _log = new Logger(project, classEmoji: "☻");
            if (string.IsNullOrEmpty(decodedResultExpire)) decodedResultExpire = project.Var("varSessionId");
            if (!string.IsNullOrEmpty(decodedResultExpire))
            {
                int intEpoch = int.Parse(decodedResultExpire);
                string converted = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(intEpoch).ToShortDateString();
                _log.Send(converted);
                return converted;

                
            }
            return string.Empty;
        }
        public static decimal Math(this IZennoPosterProjectModel project, string varA, string operation, string varB, string varRslt = "a_")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            decimal a = decimal.Parse(project.Var(varA));
            decimal b = decimal.Parse(project.Var(varB));
            decimal result;
            switch (operation)
            {
                case "+":

                    result = a + b;
                    break;
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    break;
                default:
                    throw new Exception($"unsuppoted operation {operation}");
            }
            try { project.Var(varRslt, $"{result}"); } catch { }
            return result;
        }
        public static string CookiesToJson(string cookies)
        {
            try
            {
                if (string.IsNullOrEmpty(cookies))
                {
                    return "[]";
                }

                var result = new List<Dictionary<string, string>>();
                var cookiePairs = cookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in cookiePairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                        continue;

                    var keyValue = trimmedPair.Split(new[] { '=' }, 2);
                    if (keyValue.Length != 2)
                    {
                        continue;
                    }

                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        result.Add(new Dictionary<string, string>
                    {
                        { "name", key },
                        { "value", value }
                    });
                    }
                }

                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                return json;
            }
            catch (Exception ex)
            {
                return "[]";
            }
        }



    }
  
    public static class HanaGarden
    {
        private static readonly string GRAPHQL_URL = "https://hanafuda-backend-app-520478841386.us-central1.run.app/graphql";
        private static readonly string API_KEY = "AIzaSyDipzN0VRfTPnMGhQ5PSzO27Cxm3DohJGY";

        private static string ExecuteGraphQLQuery(IZennoPosterProjectModel project, string query, string variables = null)
        {
            // Получаем токен и проверяем его
            string token = project.Variables["TOKEN_CURRENT"].Value.Trim();

            if (string.IsNullOrEmpty(token))
            {
                project.SendErrorToLog("Token is empty or null");
                return null;
            }

            // Форматируем заголовки, убедившись что токен передается корректно
            string[] headers = new string[] {
                "Content-Type: application/json",
                $"Authorization: Bearer {token.Trim()}"
            };

            // Форматируем GraphQL запрос, удаляя лишние пробелы и табуляции
            query = query.Replace("\t", "").Replace("\n", " ").Replace("\r", "").Trim();

            //string jsonBody = JsonConvert.SerializeObject(new { query = query });
            string jsonBody;
            if (variables != null)
            {
                jsonBody = JsonConvert.SerializeObject(new { query = query, variables = JsonConvert.DeserializeObject(variables) });
            }
            else
            {
                jsonBody = JsonConvert.SerializeObject(new { query = query });
            }




            try
            {
                string response = ZennoPoster.HttpPost(
                    GRAPHQL_URL,
                    Encoding.UTF8.GetBytes(jsonBody),
                    "application/json",
                    project.Variables["proxy"].Value,
                    "UTF-8",
                    ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                    30000,
                    "",
                    "HANA/v1",
                    true,
                    5,
                    headers,
                    "",
                    true
                );

                return response;
            }
            catch (Exception ex)
            {
                project.SendErrorToLog($"GraphQL request failed: {ex.Message}");
                return null;
            }
        }
        public static string RefreshToken(IZennoPosterProjectModel project, string currentToken)
        {
            string url = $"https://securetoken.googleapis.com/v1/token?key={API_KEY}";

            string jsonBody = JsonConvert.SerializeObject(new
            {
                grant_type = "refresh_token",
                refresh_token = currentToken
            });


            string[] headers = new string[] {
                "Content-Type: application/json"
            };

            try
            {
                string response = ZennoPoster.HttpPost(
                    url,
                    Encoding.UTF8.GetBytes(jsonBody),
                    "application/json",
                    project.Variables["proxy"].Value,
                    "UTF-8",
                    ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                    30000,
                    "",
                    "Firebase/v1",
                    true,
                    5,
                    headers,
                    "",
                    true
                );


                dynamic tokenData = JObject.Parse(response);
                string newToken = tokenData.access_token;

                // Сохраняем новый токен в переменную проекта
                project.Variables["TOKEN_CURRENT"].Value = newToken;

                return newToken;
            }
            catch (Exception ex)
            {
                project.SendErrorToLog($"Failed to refresh token: {ex.Message}");
                return null;
            }
        }




        private static dynamic GetUserInfo(IZennoPosterProjectModel project)
        {
            string query = @"
			query CurrentUser {
				currentUser {
					id
					sub
					name
					totalPoint
					evmAddress {
						userId
						address
					}
				}
			}";

            string response = ExecuteGraphQLQuery(project, query);
            return JObject.Parse(response);
        }// Получение информации о картах пользователя
        public static string GetUserYakuInfo(IZennoPosterProjectModel project)
        {
            string query = @"
			query GetYakuList {
				getYakuListForCurrentUser {
					cardId
					group
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }
        public static string GetUserYakuInfo2(IZennoPosterProjectModel project)
        {
            string query = @"
			query GetMasterData {
				masterData {
					yaku {
					cardId
					group
					}
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        // Получение информации о саде
        public static string GetGardenInfo(IZennoPosterProjectModel project)
        {
            project.SendInfoToLog("Getting garden info...");
            string query = @"
			query GetGardenForCurrentUser {
				getGardenForCurrentUser {
					id
					inviteCode
					gardenDepositCount
					gardenStatus {
						id
						activeEpoch
						growActionCount
						gardenRewardActionCount
					}
					gardenMembers {
						id
						sub
						name
						iconPath
						depositCount
					}
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        public static void ProcessGarden(IZennoPosterProjectModel project)
        {
            try
            {
                // Получаем и обновляем токен
                string currentToken = project.Variables["TOKEN_CURRENT"].Value;
                project.SendInfoToLog($"Initial token: {currentToken}");

                string refreshedToken = RefreshToken(project, currentToken);
                if (string.IsNullOrEmpty(refreshedToken))
                {
                    project.SendErrorToLog("Failed to refresh token");
                    return;
                }

                project.SendInfoToLog($"Successfully refreshed token: {refreshedToken}");

                // Получаем информацию о саде
                project.SendInfoToLog("Getting garden info...");
                string gardenResponse = ExecuteGraphQLQuery(project, @"
					query GetGardenForCurrentUser {
						getGardenForCurrentUser {
							id
							inviteCode
							gardenDepositCount
							gardenStatus {
								id
								activeEpoch
								growActionCount
								gardenRewardActionCount
							}
							gardenMembers {
								id
								sub
								name
								iconPath
								depositCount
							}
						}
					}");

                project.SendInfoToLog($"Garden response received: {gardenResponse.Substring(0, Math.Min(100, gardenResponse.Length))}...");

                if (string.IsNullOrEmpty(gardenResponse))
                {
                    project.SendErrorToLog("Garden response is empty!");
                    return;
                }

                dynamic gardenData = JObject.Parse(gardenResponse);

                if (gardenData.data == null || gardenData.data.getGardenForCurrentUser == null)
                {
                    project.SendErrorToLog($"Invalid garden data structure: {gardenResponse}");
                    return;
                }

                dynamic gardenStatus = gardenData.data.getGardenForCurrentUser.gardenStatus;
                dynamic gardenMembers = gardenData.data.getGardenForCurrentUser.gardenMembers;

                // Проверяем наличие необходимых данных
                if (gardenStatus == null)
                {
                    project.SendErrorToLog("Garden status is null!");
                    return;
                }

                int totalGrows = (int)gardenStatus.growActionCount;
                int totalRewards = (int)gardenStatus.gardenRewardActionCount;

                project.SendInfoToLog($"Found actions - Grows: {totalGrows}, Rewards: {totalRewards}");

                string accountName = "Unknown";
                string accountId = "Unknown";

                if (gardenMembers != null && gardenMembers.Count > 0)
                {
                    accountName = gardenMembers[0].name;
                    accountId = gardenMembers[0].id;
                }

                project.SendInfoToLog($"Processing account: {accountName} (ID: {accountId})");



                //grow
                string growQuery = @"
				mutation {
					executeGrowAction(withAll: true) {
						baseValue
						leveragedValue
						totalValue
						multiplyRate
						limit
					}
				}";

                project.SendInfoToLog($"Executing grow all action");
                string growResponse = ExecuteGraphQLQuery(project, growQuery);
                project.SendInfoToLog($"Grow response: {growResponse}");

                dynamic growData = JObject.Parse(growResponse);
                if (growData.data != null && growData.data.executeGrowAction != null)
                {
                    var result = growData.data.executeGrowAction;
                    project.SendInfoToLog($"Grow results: Base={result.baseValue}, " +
                                        $"Leveraged={result.leveragedValue}, " +
                                        $"Total={result.totalValue}, " +
                                        $"Rate={result.multiplyRate}, " +
                                        $"Limit={result.limit}");
                }


                // Получаем обновленные очки
                string userInfoResponse = ExecuteGraphQLQuery(project, @"
					query CurrentUser {
						currentUser {
							totalPoint
						}
					}");

                dynamic userInfo = JObject.Parse(userInfoResponse);
                int totalPoints = (int)userInfo.data.currentUser.totalPoint;

                project.SendInfoToLog($"Grow action completed. Current Total Points: {totalPoints}");

                int delay = new Random().Next(1000, 5000);
                project.SendInfoToLog($"Waiting for {delay}ms before next action");
                Thread.Sleep(delay);


                // Получение наград
                if (totalRewards > 0)
                {
                    project.SendInfoToLog($"Starting reward collection. Total rewards: {totalRewards}");

                    string rewardQuery = @"
					mutation executeGardenRewardAction($limit: Int!) {
						executeGardenRewardAction(limit: $limit) {
							data { cardId, group }
							isNew
						}
					}";

                    int steps = (int)Math.Ceiling(totalRewards / 10.0);
                    project.SendInfoToLog($"Will process rewards in {steps} steps");

                    for (int i = 0; i < steps; i++)
                    {
                        try
                        {
                            project.SendInfoToLog($"Processing rewards step {i + 1} of {steps}");
                            string variables = @"{""limit"": 10}";
                            string rewardResponse = ExecuteGraphQLQuery(project, rewardQuery, variables);
                            project.SendInfoToLog($"Reward response: {rewardResponse}");

                            dynamic rewardData = JObject.Parse(rewardResponse);

                            foreach (var reward in rewardData.data.executeGardenRewardAction)
                            {
                                if ((bool)reward.isNew)
                                {
                                    project.SendInfoToLog($"New card received: ID {reward.data.cardId}, Group: {reward.data.group}");
                                }
                            }

                            delay = new Random().Next(1000, 5000);
                            project.SendInfoToLog($"Waiting for {delay}ms before next reward collection");
                            Thread.Sleep(delay);
                        }
                        catch (Exception ex)
                        {
                            project.SendErrorToLog($"Error during reward collection: {ex.Message}\nStack trace: {ex.StackTrace}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                project.SendErrorToLog($"Major error in garden processing: {ex.Message}\nStack trace: {ex.StackTrace}");
            }
        }


        // Выполнение всех доступных действий роста
        public static string ExecuteGrowAll(IZennoPosterProjectModel project)
        {
            string query = @"
			mutation {
				executeGrowAction(withAll: true) {
					baseValue
					leveragedValue
					totalValue
					multiplyRate
					limit
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        // Получение текущих очков пользователя
        public static string GetUserPoints(IZennoPosterProjectModel project)
        {
            string query = @"
			query CurrentUser {
				currentUser {
					totalPoint
				}
			}";

            return ExecuteGraphQLQuery(project, query);
        }

        // Получение наград с указанным лимитом
        public static string CollectRewards(IZennoPosterProjectModel project, int limit)
        {
            string query = @"
			mutation executeGardenRewardAction($limit: Int!) {
				executeGardenRewardAction(limit: $limit) {
					data { 
						cardId
						group 
					}
					isNew
				}
			}";

            string variables = $"{{\"limit\": {limit}}}";
            return ExecuteGraphQLQuery(project, query, variables);
        }








    }
    public static class HanaAPI
    {
        private static readonly string GRAPHQL_URL = "https://hanafuda-backend-app-520478841386.us-central1.run.app/graphql";

        public static string GetSchemaInfo(IZennoPosterProjectModel project)
        {
            string introspectionQuery = @"
			query {
				__schema {
					types {
						name
						fields {
							name
							type {
								name
								kind
							}
						}
					}
					mutationType {
						fields {
							name
							type {
								name
							}
							args {
								name
								type {
									name
								}
							}
						}
					}
				}
			}";

            string[] headers = new string[] {
                "Content-Type: application/json",
                $"Authorization: Bearer {project.Variables["TOKEN_CURRENT"].Value}"
            };

            string jsonBody = JsonConvert.SerializeObject(new { query = introspectionQuery });

            return ZennoPoster.HttpPost(
                GRAPHQL_URL,
                Encoding.UTF8.GetBytes(jsonBody),
                "application/json",
                "",
                "UTF-8",
                ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                30000,
                "",
                "HANA/v1",
                true,
                5,
                headers,
                "",
                true
            );
        }
    }



    public class MerkleTree
    {
        public static string GetMerkleRoot(List<string> tokenIds)
        {
            var leaves = tokenIds
                .Select(tokenId => Sha3Keccack.Current.CalculateHashFromHex(tokenId))
                .OrderBy(hash => hash)
                .ToList();

            while (leaves.Count > 1)
            {
                var newLeaves = new List<string>();
                for (int i = 0; i < leaves.Count; i += 2)
                {
                    var left = leaves[i];
                    var right = i + 1 < leaves.Count ? leaves[i + 1] : left;
                    var combined = left.CompareTo(right) < 0 ? left + right : right + left;
                    newLeaves.Add(Sha3Keccack.Current.CalculateHash(combined));
                }
                leaves = newLeaves;
            }

            return leaves.First();
        }
    }
    public class EIP712OrderSigner
    {
        public static string SignOrder(
            string privateKey,
            string trader,
            byte side,
            string collection,
            BigInteger tokenId,
            string paymentToken,
            BigInteger price,
            BigInteger expirationTime,
            string merkleRoot,
            BigInteger salt,
            string domainName,
            string domainVersion,
            BigInteger chainId,
            string verifyingContract)
        {
            // ABI-кодирование домена
            var domainTypeHash = Sha3Keccack.Current.CalculateHash(
                System.Text.Encoding.UTF8.GetBytes("EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)"));
            var domainData = new ABIEncode().GetABIEncoded(
                new ABIValue("bytes32", domainTypeHash),
                new ABIValue("string", domainName),
                new ABIValue("string", domainVersion),
                new ABIValue("uint256", chainId),
                new ABIValue("address", verifyingContract));
            var domainHash = Sha3Keccack.Current.CalculateHash(domainData);

            // ABI-кодирование сообщения
            var orderTypeHash = Sha3Keccack.Current.CalculateHash(
                System.Text.Encoding.UTF8.GetBytes("Order(address trader,uint8 side,address collection,uint256 tokenId,address paymentToken,uint256 price,uint256 expirationTime,bytes32 merkleRoot,uint256 salt)"));
            var messageData = new ABIEncode().GetABIEncoded(
                new ABIValue("bytes32", orderTypeHash),
                new ABIValue("address", trader),
                new ABIValue("uint8", side),
                new ABIValue("address", collection),
                new ABIValue("uint256", tokenId),
                new ABIValue("address", paymentToken),
                new ABIValue("uint256", price),
                new ABIValue("uint256", expirationTime),
                new ABIValue("bytes32", merkleRoot),
                new ABIValue("uint256", salt));
            var messageHash = Sha3Keccack.Current.CalculateHash(messageData);

            // Финальный хэш
            var finalData = new byte[] { 0x19, 0x01 }
                .Concat(domainHash)
                .Concat(messageHash)
                .ToArray();
            var finalHash = Sha3Keccack.Current.CalculateHash(finalData);

            // Подпись
            var signer = new EthereumMessageSigner();
            var ethECKey = new EthECKey(privateKey);
            var signature = signer.Sign(finalHash, ethECKey);

            return signature;
        }
    }
    public class MarketplaceBidding : W3b
    {
        private readonly string _walletAddress;
        private readonly string _privateKey;
        private readonly string _nftContractAddress;
        private readonly string _paymentTokenAddress;
        private readonly string _marketplaceContractAddress;
        private readonly string _privyIdToken;
        private readonly string _chainRpc;

        public MarketplaceBidding(
            IZennoPosterProjectModel project,
            string walletAddress,
            string privateKey,
            string nftContractAddress,
            string paymentTokenAddress,
            string marketplaceContractAddress,
            string privyIdToken,
            string chain,
            bool log = false)
            : base(project, log)
        {
            _walletAddress = walletAddress;
            _privateKey = ApplyKey(privateKey);
            _nftContractAddress = nftContractAddress;
            _paymentTokenAddress = paymentTokenAddress;
            _marketplaceContractAddress = marketplaceContractAddress;
            _privyIdToken = privyIdToken;
            _chainRpc = Rpc(chain);
        }

        public class BidResponse
        {
            public string Id { get; set; }
        }

        public class CardData
        {
            public List<int> TokenIds { get; set; }
            public string MerkleRoot { get; set; }
        }

        public Tuple<string, string> CreateBid(int heroId, int rarity, decimal bidAmountEth, CardData cardData)
        {
            try
            {
                BigInteger bidAmountWei = (BigInteger)(bidAmountEth * 1000000000000000000m);
                Log("Bid amount in Wei: " + bidAmountWei.ToString());

                if (cardData == null || cardData.TokenIds == null || string.IsNullOrEmpty(cardData.MerkleRoot))
                {
                    throw new Exception("Invalid card data");
                }

                long timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + 43200;
                string salt = new Random().Next(100000, 999999).ToString();

                // Placeholder for signature generation
                string signature = GetSignature(
                    _privateKey,
                    _walletAddress,
                    0,
                    0,
                    bidAmountWei.ToString(),
                    timestamp,
                    cardData.MerkleRoot,
                    int.Parse(salt)
                );

                Log("Signature: " + signature);

                string handle = FetchUserHandle(_walletAddress, _privyIdToken);

                var payload = new Dictionary<string, object>
                {
                    { "trader", _walletAddress },
                    { "side", 0 },
                    { "collection", _nftContractAddress },
                    { "token_id", 0 },
                    { "token_ids", cardData.TokenIds },
                    { "payment_token", _paymentTokenAddress },
                    { "price", bidAmountWei.ToString() },
                    { "expiration_time", timestamp.ToString() },
                    { "salt", salt },
                    { "signature", signature },
                    { "merkle_root", cardData.MerkleRoot },
                    { "hero_id", heroId.ToString() },
                    { "rarity", rarity },
                    { "bidder_handle", handle }
                };

                BidResponse response = SubmitBid(payload);
                if (response != null && !string.IsNullOrEmpty(response.Id))
                {
                    string json = JsonConvert.SerializeObject(response, Formatting.Indented);
                    string filePath = Path.Combine("bids_storage", response.Id + ".json");
                    Directory.CreateDirectory("bids_storage");
                    File.WriteAllText(filePath, json);
                    return new Tuple<string, string>(response.Id, null);
                }
                else
                {
                    return new Tuple<string, string>(null, "Failed to submit bid");
                }
            }
            catch (Exception ex)
            {
                Log("Error in CreateBid: " + ex.Message, log: true);
                return new Tuple<string, string>(null, ex.Message);
            }
        }

        private BidResponse SubmitBid(Dictionary<string, object> payload)
        {
            try
            {
                string url = "https://secret-api.fantasy.top/marketplace/create-bid-order";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + _privyIdToken);

                string jsonPayload = JsonConvert.SerializeObject(payload);
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonPayload);
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Log("Non-200 response: " + response.StatusCode, log: true);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            Log("Response: " + reader.ReadToEnd(), log: true);
                        }
                        return null;
                    }

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseText = reader.ReadToEnd();
                        return JsonConvert.DeserializeObject<BidResponse>(responseText);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Failed to submit bid: " + ex.Message, log: true);
                return null;
            }
        }

        public string AcceptBid(Dictionary<string, object> bidData, int specificTokenId)
        {
            try
            {
                // Placeholder for Merkle proof generation
                Tuple<List<string>, string> merkleData = GenerateMerkleProof(specificTokenId, (List<int>)bidData["token_ids"]);
                List<string> merkleProof = merkleData.Item1;
                string merkleRoot = merkleData.Item2;

                // Prepare order structure (simplified to match Python logic)
                var order = new
                {
                    trader = _walletAddress,
                    side = 0,
                    collection = _nftContractAddress,
                    tokenId = 0,
                    paymentToken = _paymentTokenAddress,
                    price = BigInteger.Parse(bidData["price"].ToString()),
                    expirationTime = long.Parse(bidData["expiration_time"].ToString()),
                    merkleRoot = bidData["merkle_root"].ToString(),
                    salt = int.Parse(bidData["salt"].ToString())
                };

                // Placeholder for encoding (needs actual implementation)
                string encodedData = EncodeOrder(
                    order,
                    bidData["signature"].ToString(),
                    specificTokenId,
                    merkleProof
                );

                // Use Send1559 from W3b.cs (assuming it accepts string RPC)
                string txHash = Send1559(
                    _chainRpc,
                    _marketplaceContractAddress,
                    "0x00cb1eef" + encodedData,
                    0m,
                    _privateKey,
                    1
                );

                Log("Transaction hash: " + txHash);

                // Simplified check (no Web3 simulation due to missing methods)
                if (!string.IsNullOrEmpty(txHash))
                {
                    Log("✅ Success with transaction!");
                    return txHash;
                }
                else
                {
                    throw new Exception("Transaction failed");
                }
            }
            catch (Exception ex)
            {
                Log("❌ Error accepting bid: " + ex.Message, log: true);
                return null;
            }
        }

        public string SetApprovalForAll(int specificTokenId)
        {
            try
            {
                // Placeholder for ABI loading and contract interaction
                string nftAbi = File.ReadAllText("nft_collection_abi.json");

                // Simplified logic (no contract call due to missing Web3)
                string approveToContract = "0xf9fe044bdd557c76c8eb0bd566d8b149186425c3";
                bool isApprovedForAll = false; // Placeholder, needs actual check

                Log("Approved Address: (simulated)");
                Log("Is approved collection-wide: " + isApprovedForAll.ToString());

                if (!isApprovedForAll)
                {
                    Log("[INFO] Collection-wide approval not found. Sending transaction...");

                    // Placeholder encoded data (needs actual function call)
                    string encodedData = "0x"; // Replace with real encoding

                    string txHash = Send1559(
                        _chainRpc,
                        _nftContractAddress,
                        encodedData,
                        0m,
                        _privateKey,
                        1
                    );

                    Log("[SUCCESS] Sent setApprovalForAll tx: " + txHash);
                    return txHash;
                }

                return null;
            }
            catch (Exception ex)
            {
                Log("Error in SetApprovalForAll: " + ex.Message, log: true);
                return null;
            }
        }

        // Placeholders (to be implemented based on your code)
        private string GetSignature(string privateKey, string walletAddress, int side, int tokenId, string price, long expirationTime, string merkleRoot, int salt)
        {
            throw new NotImplementedException("GetSignature not implemented. Please provide the Python get_signature code.");
        }

        private Tuple<List<string>, string> GenerateMerkleProof(int specificTokenId, List<int> tokenIds)
        {
            throw new NotImplementedException("GenerateMerkleProof not implemented. Please provide the Python generate_merkle_proof code.");
        }

        private string FetchUserHandle(string walletAddress, string privyIdToken)
        {
            throw new NotImplementedException("FetchUserHandle not implemented. Please provide the Python fetch_user_handle code.");
        }

        private string EncodeOrder(object order, string signature, int specificTokenId, List<string> merkleProof)
        {
            throw new NotImplementedException("EncodeOrder not implemented. Please provide the Python encode function code.");
        }
    }



    public class Google2
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;
        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _login;
        protected string _pass;
        protected string _2fa;
        protected string _recoveryMail;
        protected string _recoveryCodes;
        protected string _cookies;

        public Google2(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logger = new Logger(project, log: log, classEmoji: "G");
            DbCreds();

        }

        private void DbCreds()
        {
            string[] creds = _sql.Get("status, login, password, otpsecret, recoveryemail, otpbackup, cookies", "private_google").Split('|');
            try { _status = creds[0]; _project.Variables["googleSTATUS"].Value = _status; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _login = creds[1]; _project.Variables["googleLOGIN"].Value = _login; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _pass = creds[2]; _project.Variables["googlePASSWORD"].Value = _pass; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _2fa = creds[3]; _project.Variables["google2FACODE"].Value = _2fa; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _recoveryMail = creds[4]; _project.Variables["googleSECURITY_MAIL"].Value = _recoveryMail; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _recoveryCodes = creds[5]; _project.Variables["googleBACKUP_CODES"].Value = _recoveryCodes; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _cookies = creds[6]; _project.Variables["googleCOOKIES"].Value = _cookies; } catch (Exception ex) { _logger.Send(ex.Message); }
            _logger.Send(_status);
        }
        public string Load(bool log = false, bool cookieBackup = true)
        {
            if (!_instance.ActiveTab.URL.Contains("google")) _instance.Go("https://myaccount.google.com/");
        check:
            Thread.Sleep(1000);
            string state = GetState();
            switch (state)
            {
                case "ok":
                    if (cookieBackup) SaveCookies();
                    return state;

                case "wrong":
                    _instance.CloseAllTabs();
                    _instance.ClearCookie("google.com");
                    _instance.ClearCookie("google.com");
                    goto check;

                case "inputLogin":
                    _instance.HeSet(("identifierId", "id"), _login);
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;

                case "inputPassword":
                    _instance.HeSet(("Passwd", "name"), _pass, deadline: 5);
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;

                case "inputOtp":
                    _instance.HeSet(("totpPin", "id"), OTP.Offline(_2fa));
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;

                case "addRecoveryPhone":
                    _instance.HeClick(("button", "innertext", "Cancel", "regexp", 0));
                    goto check;

                case "CAPCHA":
                    try { _project.CapGuru(); } catch { }
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    //_sql.Upd("status = 'status = '!WCapcha'", "google");
                    //_sql.Upd("status = 'status = '!W fail.Google Capcha or Locked'");
                    goto check;
                    throw new Exception("CAPCHA");

                case "badBrowser":
                    _sql.Upd("status = 'status = '!W BadBrowser'", "google");
                    _sql.Upd("status = 'status = '!W fail.Google BadBrowser'");
                    throw new Exception("BadBrowser");

                default:
                    return state;

            }

        }
        public string GetState(bool log = false)
        {
            

        check:
            var status = "";

            if (!_instance.ActiveTab.FindElementByAttribute("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0).IsVoid)
                status = "signedIn";

            else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Verify\\ it’s\\ you", "regexp", 0).IsVoid)
                status = "CAPCHA";

            else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Try\\ using\\ a\\ different\\ browser.", "regexp", 0).IsVoid)
                status = "BadBrowser";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:email", "fulltagname", "input:email", "text", 0).IsVoid) && 
                    (_instance.ActiveTab.FindElementByAttribute("input:email", "fulltagname", "input:email", "text", 0).GetAttribute("value") == ""))
                status = "inputLogin";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).IsVoid) &&
                    _instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).GetAttribute("value") == "")
                status = "inputPassword";

            else if ((!_instance.ActiveTab.FindElementById("totpPin").IsVoid) &&
                    _instance.ActiveTab.FindElementById("totpPin").GetAttribute("value") == "")
                status = "inputOtp";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).IsVoid) &&
                    _instance.ActiveTab.FindElementById("totpPin").GetAttribute("value") == "")
                status = "addRecoveryPhone";



            else status = "undefined";



            _logger.Send(status);

            switch (status)
            {
                case "signedIn":
                    var currentAcc = _instance.HeGet(("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0), atr: "aria-label").Split('\n')[1];
                    if (currentAcc.ToLower().Contains(_login.ToLower())) 
                    {
                        _logger.Send($"{currentAcc} is Correct. Login done");
                        status = "ok";
                    }
                        
                    else
                    {
                        _logger.Send($"!W {currentAcc} is InCorrect. MustBe {_login}");
                        status = "wrong";
                    }
                    break;

                case "undefined":
                    _instance.HeClick(("a", "class", "h-c-header__cta-li-link\\ h-c-header__cta-li-link--primary\\ button-standard-mobile", "regexp", 1), deadline: 1, thr0w: false);
                    goto check;

                default:
                    break;

            }
            return status;
        }
        public string GAuth(bool log = false)
        {
            try
            {
                var userContainer = _instance.HeGet(("div", "data-authuser", "0", "regexp", 0));
                _logger.Send($"container:{userContainer} catched");
                if (userContainer.IndexOf(_login, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _logger.Send($"correct user found: {_login}");
                    _instance.HeClick(("div", "data-authuser", "0", "regexp", 0), delay: 3);
                    Thread.Sleep(5000);
                    if (!_instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0).IsVoid)
                    {
                        while (true) _instance.HeClick(("div", "data-authuser", "0", "regexp", 0), "clickOut", deadline: 5, delay: 3);
                    }
                    try
                    {
                        _instance.HeClick(("button", "innertext", "Continue", "regexp", 0), deadline: 2, delay: 1);
                        return "SUCCESS with continue";
                    }
                    catch
                    {
                        return "SUCCESS. without confirmation";
                    }
                }
                else
                {
                    _logger.Send($"!Wrong account [{userContainer}]. Expected: {_login}. Cleaning");
                    _instance.CloseAllTabs();
                    _instance.ClearCookie("google.com");
                    _instance.ClearCookie("google.com");
                    return "FAIL. Wrong account";
                }
            }
            catch
            {
                return "FAIL. No loggined Users Found";
            }
        }
        public void SaveCookies()
        {
            _instance.Go("youtube.com");
            Thread.Sleep(5000);
            _instance.Go("https://myaccount.google.com/");
            //new Cookies(_project,_instance).Save();
        }

    }




}


