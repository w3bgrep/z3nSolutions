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

#region using
using System;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary;
using ZBSolutions;
using NBitcoin;
using Nethereum.Model;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;


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
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Reflection;
using System.Security.Policy;
using ZBSolutions;

#endregion

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

    public class Starter2
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;


        public Starter2(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project, true);
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _instance = instance;
        }
        public Starter2(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _sql = new Sql(_project,true);
        }
        public void StartBrowser(bool strictProxy = true)
        {
            if (string.IsNullOrEmpty(_project.Var("acc0")))
               throw new Exception("!EmptyVar: acc0");
            
            _project.Variables["instancePort"].Value = _instance.Port.ToString();
            _logger.Send($"init browser in port: {_instance.Port}");

            string webGlData = _sql.Get("webgl", "private_profile");
            _instance.SetDisplay(webGlData, _project);

            bool goodProxy = new NetHttp(_project, true).ProxySet(_instance);
            if (strictProxy && !goodProxy) throw new Exception($"!E bad proxy");

            string cookiePath = $"{_project.Variables["profiles_folder"].Value}accounts\\cookies\\{_project.Variables["acc0"].Value}.json";
            _project.Variables["pathCookies"].Value = cookiePath;

            try
            {
                string cookies = File.ReadAllText(cookiePath);
                _instance.SetCookie(cookies);
            }
            catch
            {
                _logger.Send($"!W Fail to set cookies from file {cookiePath}");
                try
                {
                    string cookies = _sql.Get("cookies", "private_profile");
                    _instance.SetCookie(cookies);
                }
                catch (Exception Ex)
                {
                    _logger.Send($"!E Fail to set cookies from db Err. {Ex.Message}");
                }

            }
            if (_project.Var("skipBrowserScan") != "True")
            {
                var bs = new BrowserScan(_project, _instance);
                if (bs.GetScore().Contains("timezone")) bs.FixTime();
            }

        }
        public void InitVariables(string author = "")
        {
            new Sys(_project).DisableLogs();

            string sessionId = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();
            string projectName = _project.ExecuteMacro(_project.Name).Split('.')[0];
            string version = Assembly.GetExecutingAssembly()
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
               ?.InformationalVersion ?? "Unknown";
            string dllTitle = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyTitleAttribute>()
                ?.Title ?? "Unknown";


            _project.Variables["projectName"].Value = projectName;
            _project.Variables["varSessionId"].Value = sessionId;
            try { _project.Variables["nameSpace"].Value = dllTitle; } catch { }

            string[] vars = { "cfgPin", "DBsqltPath" };
            CheckVars(vars);

            _project.Variables["projectTable"].Value = "projects_" + projectName;

            _project.Range();
            SAFU.Initialize(_project);
            Logo(author, dllTitle);

        }
        private void Logo(string author, string dllTitle)
        {
            string version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";

            string name = _project.ExecuteMacro(_project.Name).Split('.')[0];
            if (author != "") author = $" script author: @{author}";
            string logo = $@"using {dllTitle} v{version};
            ┌by─┐					
            │    w3bgrep			
            └─→┘
                        ► init {name} ░▒▓█  {author}";
            _project.SendInfoToLog(logo, true);
        }
        private void CheckVars(string[] vars)
        {
            foreach (string var in vars)
            {
                try
                {
                    if (string.IsNullOrEmpty(_project.Variables[var].Value))
                    {
                        throw new Exception($"!E {var} is null or empty");
                    }
                }
                catch (Exception ex)
                {
                    _project.L0g(ex.Message);
                    throw;
                }
            }
        }
        public bool ChooseSingleAcc()
        {
            var listAccounts = _project.Lists["accs"];

        check:
            if (listAccounts.Count == 0)
            {
                _project.Variables["noAccsToDo"].Value = "True";
                _project.SendToLog($"♻ noAccoutsAvaliable", LogType.Info, true, LogColor.Turquoise);
                _project.Variables["acc0"].Value = "";
                return false;
                throw new Exception($"TimeToChill");
            }

            int randomAccount = new Random().Next(0, listAccounts.Count);
            _project.Variables["acc0"].Value = listAccounts[randomAccount];
            listAccounts.RemoveAt(randomAccount);
            if (!_project.GlobalSet())
                goto check;
            _project.Var("pathProfileFolder", $"{_project.Var("profiles_folder")}accounts\\profilesFolder\\{_project.Var("acc0")}");
            _project.L0g($"`working with: [acc{_project.Var("acc0")}] accs left: [{listAccounts.Count}]");
            return true;

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

    public class NetHttp2
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;
        private readonly bool _logShow;

        public NetHttp2(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "↑↓");
        }

        protected void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;
            _logger.Send($"({callerName}) [{message}]");
        }
        protected void ParseJson(string json)
        {
            try
            {
                _project.Json.FromString(json);
            }
            catch (Exception ex)
            {
                _logger.Send($"[!W {ex.Message}] [{json}]");
            }
        }
        public WebProxy ParseProxy(string proxyString, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(proxyString))
            {
                return null;
            }
            if (proxyString == "+")
                proxyString = new Sql(_project).Get("proxy", "private_profile");
            try
            {
                WebProxy proxy = new WebProxy();

                if (proxyString.Contains("//")) proxyString = proxyString.Split('/')[2];

                if (proxyString.Contains("@")) // Прокси с авторизацией (login:pass@proxy:port)
                {
                    string[] parts = proxyString.Split('@');
                    string credentials = parts[0];
                    string proxyHost = parts[1];

                    proxy.Address = new Uri("http://" + proxyHost);
                    string[] creds = credentials.Split(':');
                    proxy.Credentials = new NetworkCredential(creds[0], creds[1]);

                }
                else // Прокси без авторизации (proxy:port)
                {
                    proxy.Address = new Uri("http://" + proxyString);
                    //_logger.Send($"proxy set: ip:{proxyString}", callerName);
                }

                return proxy;
            }
            catch (Exception e)
            {
                _logger.Send(e.Message + $"[{proxyString}]");
                return null;
            }
        }
        private Dictionary<string, string> BuildHeaders(Dictionary<string, string> inputHeaders = null)
        {
            var defaultHeaders = new Dictionary<string, string>
            {
                { "User-Agent", _project.Profile.UserAgent }, // Already present
                //{ "Accept", "application/json" },
                //{ "Accept-Encoding", "" },
                //{ "Accept-Language", _project.Profile.AcceptLanguage },
                //{ "Priority", "u=1, i" },
                //{ "Content-Type", "application/json; charset=UTF-8" }, // For GET; POST overrides via StringContent
                //{ "Sec-Ch-Ua", "\"Chromium\";v=\"136\", \"Google Chrome\";v=\"136\", \"Not.A/Brand\";v=\"99\"" },
                //{ "Sec-Ch-Ua-Mobile", "?0" },
                //{ "Sec-Ch-Ua-Platform", "\"Windows\"" },
                //{ "Sec-Fetch-Dest", "empty" },
                //{ "Sec-Fetch-Mode", "cors" },
                //{ "Sec-Fetch-Site", "cross-site" },
                //{ "Sec-Fetch-Storage-Access", "active" }
            };

            if (inputHeaders == null || inputHeaders.Count == 0)
            {
                return defaultHeaders;
            }

            var mergedHeaders = new Dictionary<string, string>(defaultHeaders);
            foreach (var header in inputHeaders)
            {
                mergedHeaders[header.Key] = header.Value; // Input headers override defaults
            }

            return mergedHeaders;
        }




        public string GET(
            string url,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
        {
            string debugHeaders = "";
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // Build headers
                    var requestHeaders = BuildHeaders(headers);

                    // Add headers to client and build debug string
                    foreach (var header in requestHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        debugHeaders += $"{header.Key}: {header.Value}; ";
                    }

                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    // Build response headers debug string
                    string responseHeaders = string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = string.Join("; ", cookieValues);
                        _logger.Send($"Set-Cookie found: {cookies}");
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (parse) ParseJson(result);
                    _logger.Send(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"[GET] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), headers: [{debugHeaders.Trim()}]");
                if (throwOnFail) throw;
                return e.Message.Replace("Response status code does not indicate success:", "").Trim('.').Trim();
            }
            catch (Exception e)
            {
                _logger.Send($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) headers: [{debugHeaders.Trim()}]");
                if (throwOnFail) throw;
                return string.Empty;
            }
        }

        public string POST(
            string url,
            string body,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
        {
            string debugHeaders = "";
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var content = new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");

                    // Build headers
                    var requestHeaders = BuildHeaders(headers);

                    // Add headers to client and build debug string
                    foreach (var header in requestHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        debugHeaders += $"{header.Key}: {header.Value}; ";
                    }
                    debugHeaders += "Content-Type: application/json; charset=UTF-8; ";

                    _logger.Send(body);

                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    // Build response headers debug string
                    string responseHeaders = string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = string.Join("; ", cookieValues);
                        _logger.Send($"Set-Cookie found: {cookies}");
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.Send(result);
                    if (parse) ParseJson(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"[POST] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), headers: [{debugHeaders.Trim()}]");
                if (throwOnFail) throw;
                return e.Message.Replace("Response status code does not indicate success:", "").Trim('.').Trim();
            }
            catch (Exception e)
            {
                _logger.Send($"!W [POST] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) headers: [{debugHeaders.Trim()}]");
                if (throwOnFail) throw;
                return string.Empty;
            }
        }


        public string PUT(
            string url,
            string body = "",
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            [CallerMemberName] string callerName = "")
        {
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var content = string.IsNullOrEmpty(body) ? null : new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                        }
                    }

                    if (content != null)
                    {
                        headersString.AppendLine($"Content-Type: application/json; charset=UTF-8");
                        _logger.Send(body);
                    }

                    HttpResponseMessage response = client.PutAsync(url, content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        _logger.Send("Set-Cookie found: {cookies}");
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.Send(result);
                    if (parse) ParseJson(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"!W RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})");
                return e.Message;
            }
            catch (Exception e)
            {
                _logger.Send($"!W UnknownErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})");
                return $"Ошибка: {e.Message}";
            }
        }

        public string DELETE(
            string url,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            [CallerMemberName] string callerName = "")
        {

            string debugHeaders = null;
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                    }

                    HttpResponseMessage response = client.DeleteAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        _logger.Send($"Set-Cookie found: {cookies}");
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.Send(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"!W [DELETE] RequestErr: [{e.Message}] url:[{url}] (proxy: {proxyString}), Headers\n{debugHeaders.Trim()}");
                return e.Message;
            }
            catch (Exception e)
            {
                _logger.Send($"!W [DELETE] UnknownErr: [{e.Message}] url:[{url}] (proxy: {proxyString})");
                return $"Ошибка: {e.Message}";
            }
        }


        public bool CheckProxy(string proxyString = null)
        {

            if (string.IsNullOrEmpty(proxyString))
                proxyString = new Sql(_project).Get("proxy", "private_profile");

            //WebProxy proxy = ParseProxy(proxyString);

            string ipLocal = GET("http://api.ipify.org/", null);
            string ipProxified = GET("http://api.ipify.org/", proxyString);

            //_logger.Send($"ipLocal: {ipLocal}, ipProxified: {ipProxified}");

            if (ipProxified != ipLocal)
            {
                _logger.Send($"proxy `validated: {ipProxified}");
                _project.Var("proxy", proxyString);
                return true;
            }
            else if (ipProxified.StartsWith("Ошибка") || ipProxified == "Прокси не настроен")
            {
                _logger.Send($"!W proxy error: {ipProxified}");

            }
            else if (ipLocal == ipProxified)
            {
                _logger.Send($"!W ip still same. ipLocal: [{ipLocal}], ipProxified: [{ipProxified}]. Proxy was not applyed");
            }
            return false;
        }

        public bool ProxySet(Instance instance, string proxyString = null)
        {

            if (string.IsNullOrEmpty(proxyString))
                proxyString = new Sql(_project).Get("proxy", "private_profile");


            string ipLocal = GET("http://api.ipify.org/", null);
            string ipProxified = GET("http://api.ipify.org/", proxyString);

            //_logger.Send($"ipLocal: {ipLocal}, ipProxified: {ipProxified}");

            if (ipProxified != ipLocal)
            {
                _logger.Send($"proxy `validated: {ipProxified}");
                _project.Var("proxy", proxyString);
                instance.SetProxy(proxyString, true, true, true, true);
                return true;
            }
            else if (ipProxified.StartsWith("Ошибка") || ipProxified == "Proxy not Set")
            {
                _logger.Send($"!W proxy error: {ipProxified}");
            }
            else if (ipLocal == ipProxified)
            {
                _logger.Send($"!W ip still same. ipLocal: [{ipLocal}], ipProxified: [{ipProxified}]. Proxy was not applyed");
            }
            return false;
        }

    }


}
