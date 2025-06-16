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

    public class DBuilder2 : Sql
    {
        private readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly F0rms _f0rm;
        private readonly int _range;

        public DBuilder2(IZennoPosterProjectModel project, Instance instance, bool log = false)
            : base(project, log: log)
        {
            _instance = instance;
            _project = project;
            _f0rm = new F0rms(_project);
            _range = _project.Range();
        }

        public DBuilder2(IZennoPosterProjectModel project, bool log = false)
          : base(project, log: log)
        {
            _project = project;
            _f0rm = new F0rms(_project);
            _range = _project.Range();
        }


        public string[] DefaultColumns(schema tableSchem)
        {

            switch (tableSchem)
            {

                case schema.public_native:
                case schema.public_deposits:
                    return new string[] { };
                case schema.private_google:
                    return new string[] { "status", "last", "cookies", "login", "password", "otpsecret", "otpbackup", "recoveryemail", "recovery_phone" };
                case schema.private_twitter:
                    return new string[] { "status", "last", "cookies", "token", "login", "password", "otpsecret", "otpbackup", "email", "emailpass" };
                case schema.private_discord:
                    return new string[] { "status", "last", "token", "login", "password", "otpsecret", "otpbackup", "email", "emailpass", "recovery_phone" };
                case schema.private_github:
                    return new string[] { "status", "last", "cookies", "token", "login", "password", "otpsecret", "otpbackup", "email", "emailpass" };
                case schema.public_blockchain:
                    return new string[] { "evm_pk", "sol_pk", "apt_pk", "evm_seed" };
                case schema.private_blockchain:
                    return new string[] { "secp256k1", "base58", "bip39" };

                case schema.private_settings:
                    return new string[] { "value" };
                case schema.private_api:
                    return new string[] { "apikey", "apisecret", "passphrase", "proxy" };
                case schema.private_profile:
                    return new string[] { "proxy", "cookies", "webgl", "zb_id" };


                case schema.public_profile:
                    return new string[] { "nickname", "bio", "brsr_score" };

                case schema.public_rpc:
                    return new string[] { "rpc", "explorer", "explorer_api" };

                case schema.public_mail:
                    return new string[] { "google", "icloud", "firstmail" };
                case schema.public_google:
                    return new string[] { "status", "last" };
                case schema.public_twitter:
                    return new string[] { "status", "last", "id", "following", "followers", "creation", "givenname", "description", "lang", "birth", "country", "gender", "homelocation", };
                case schema.public_discord:
                    return new string[] { "status", "last", "username", "servers", "roles" };


                default:
                    throw new Exception("no schema");
            }

        }

        public Dictionary<string, string> LoadSchema(schema tableSchem)
        {
            var tableStructure = new Dictionary<string, string>();

            string primary = "acc0";
            string primaryType = "INTEGER PRIMARY KEY";
            var toFill = new List<string>();
            string defaultColumn = "TEXT DEFAULT ''";



            switch (tableSchem)
            {

                case schema.private_twitter:
                case schema.private_google:
                case schema.private_discord:
                case schema.private_github:

                case schema.public_google:
                case schema.public_twitter:
                case schema.public_discord:
                case schema.public_github:

                case schema.private_blockchain:
                case schema.public_blockchain:
                case schema.public_native:
                case schema.public_deposits:

                case schema.private_profile:
                case schema.public_profile:
                case schema.public_mail:
                    foreach (string column in DefaultColumns(tableSchem)) toFill.Add(column);
                    break;

                case schema.private_settings:
                case schema.private_api:
                case schema.public_rpc:
                    primary = "key";
                    foreach (string column in DefaultColumns(tableSchem)) toFill.Add(column);
                    break;

                default:
                    throw new Exception("no schema");

            }


            if (primary != "acc0") primaryType = "TEXT PRIMARY KEY";
            tableStructure.Add(primary, primaryType);

            foreach (string name in toFill)
            {
                if (!tableStructure.ContainsKey(name)) tableStructure.Add(name, defaultColumn);
            }
            return tableStructure;
        }

        //private string ImportData(string tableName, string[] availableFields, Dictionary<string, string> columnMapping, string formTitle = "title", string message = "Select format (one field per box):", int startFrom = 1)
        //{
        //    TblName(tableName);
        //    if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
        //    int lineCount = 0;

        //    //int rangeEnd = _rangeEnd;
        //    //var acc0 = _project.Variables["acc0"];
        //    //acc0.Value = startFrom.ToString();


        //    var formFont = new System.Drawing.Font("Iosevka", 10); //System.Drawing.FontStyle.Bold
        //    System.Windows.Forms.Form form = new System.Windows.Forms.Form();
        //    form.Text = formTitle;
        //    form.Width = 800;
        //    form.Height = 700;
        //    form.BackColor = System.Drawing.Color.DarkGray;
        //    form.TopMost = true;
        //    form.Location = new System.Drawing.Point(108, 108);

        //    List<string> selectedFormat = new List<string>();
        //    System.Windows.Forms.TextBox formatDisplay = new System.Windows.Forms.TextBox();
        //    System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();

        //    System.Windows.Forms.Label formatLabel = new System.Windows.Forms.Label();
        //    formatLabel.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold);
        //    formatLabel.Text = message;
        //    formatLabel.AutoSize = true;
        //    formatLabel.Left = 10;
        //    formatLabel.Top = 10;

        //    form.Controls.Add(formatLabel);

        //    System.Windows.Forms.ComboBox[] formatComboBoxes = new System.Windows.Forms.ComboBox[availableFields.Length - 1];
        //    int spacing = 5;
        //    int totalSpacing = spacing * (formatComboBoxes.Length - 1);
        //    int comboWidth = (form.ClientSize.Width - 20 - totalSpacing) / formatComboBoxes.Length;
        //    for (int i = 0; i < formatComboBoxes.Length; i++)
        //    {
        //        formatComboBoxes[i] = new System.Windows.Forms.ComboBox();
        //        formatComboBoxes[i].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        //        formatComboBoxes[i].Font = formFont;
        //        formatComboBoxes[i].Items.AddRange(availableFields);
        //        formatComboBoxes[i].SelectedIndex = 0;
        //        formatComboBoxes[i].Left = 10 + i * (comboWidth + spacing);
        //        formatComboBoxes[i].Top = 30;
        //        formatComboBoxes[i].Width = comboWidth;
        //        formatComboBoxes[i].SelectedIndexChanged += (s, e) =>
        //        {
        //            selectedFormat.Clear();
        //            foreach (var combo in formatComboBoxes)
        //            {
        //                if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
        //                    selectedFormat.Add(combo.SelectedItem.ToString());
        //            }
        //            formatDisplay.Text = string.Join(":", selectedFormat);
        //        };
        //        form.Controls.Add(formatComboBoxes[i]);
        //    }

        //    formatDisplay.Left = 10;
        //    formatDisplay.Top = 60;
        //    formatDisplay.Font = new System.Drawing.Font("Iosevka", 11, System.Drawing.FontStyle.Bold);
        //    formatDisplay.BackColor = System.Drawing.Color.Black;
        //    formatDisplay.ForeColor = System.Drawing.Color.White;
        //    formatDisplay.Width = form.ClientSize.Width - 20;
        //    formatDisplay.ReadOnly = true;

        //    form.Controls.Add(formatDisplay);

        //    System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
        //    dataLabel.Text = "Input data (one per line, matching format):";
        //    dataLabel.Font = formFont;
        //    dataLabel.AutoSize = true;
        //    dataLabel.Left = 10;
        //    dataLabel.Top = 90;
        //    form.Controls.Add(dataLabel);

        //    dataInput.Left = 10;
        //    dataInput.Top = 110;
        //    dataInput.Width = form.ClientSize.Width - 20;
        //    dataInput.Multiline = true;
        //    dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        //    dataInput.BackColor = System.Drawing.Color.Black;
        //    dataInput.ForeColor = System.Drawing.Color.White;
        //    form.Controls.Add(dataInput);

        //    System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
        //    okButton.Text = "OK";
        //    okButton.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold);
        //    okButton.BackColor = System.Drawing.Color.Black;
        //    okButton.ForeColor = System.Drawing.Color.LightGreen;
        //    okButton.Width = form.ClientSize.Width - 10;
        //    okButton.Height = 25;
        //    okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
        //    okButton.Top = form.ClientSize.Height - okButton.Height - 5;
        //    okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
        //    form.Controls.Add(okButton);
        //    dataInput.Height = okButton.Top - dataInput.Top - 5;

        //    form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };
        //    form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

        //    form.ShowDialog();

        //    if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
        //    {
        //        _project.SendInfoToLog($"Import to {tableName} cancelled by user", true);
        //        return "0";
        //    }

        //    selectedFormat.Clear();
        //    foreach (var combo in formatComboBoxes)
        //    {
        //        if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
        //            selectedFormat.Add(combo.SelectedItem.ToString());
        //    }

        //    if (string.IsNullOrEmpty(dataInput.Text) || selectedFormat.Count == 0)
        //    {
        //        _project.SendWarningToLog("Data or format cannot be empty");
        //        return "0";
        //    }

        //    string[] lines = dataInput.Text.Trim().Split('\n');
        //    _project.SendInfoToLog($"Parsing [{lines.Length}] {tableName} data lines", true);

        //    for (int acc0unt = 1; acc0unt <= lines.Length; acc0unt++)
        //    {
        //        string line = lines[acc0unt - 1].Trim();
        //        if (string.IsNullOrWhiteSpace(line))
        //        {
        //            _project.SendWarningToLog($"Line {acc0unt} is empty", false);
        //            continue;
        //        }
        //        else
        //        {
        //            string[] data_parts = line.Split(':');
        //            Dictionary<string, string> parsed_data = new Dictionary<string, string>();

        //            for (int i = 0; i < selectedFormat.Count && i < data_parts.Length; i++)
        //            {
        //                parsed_data[selectedFormat[i]] = data_parts[i].Trim();
        //            }

        //            var queryParts = new List<string>();
        //            foreach (var field in columnMapping.Keys)
        //            {
        //                string value = parsed_data.ContainsKey(field) ? parsed_data[field].Replace("'", "''") : "";
        //                if (field == "CODE2FA" && value.Contains('/'))
        //                    value = value.Split('/').Last();
        //                queryParts.Add($"{columnMapping[field]} = '{value}'");
        //            }

        //            try
        //            {

        //                Upd(string.Join(", ", queryParts), _tableName, last: false, acc: acc0unt);
        //                lineCount++;
        //            }
        //            catch (Exception ex)
        //            {
        //                _project.SendWarningToLog($"Error processing line {acc0unt}: {ex.Message}", false);
        //            }
        //        }
        //    }

        //    _project.SendInfoToLog($"[{lineCount}] records added to [{_tableName}]", true);
        //    return lineCount.ToString();
        //}
        private string ImportData(string tableName, string[] availableFields, Dictionary<string, string> columnMapping, string formTitle = "title", string message = "Select format (one field per box):", int startFrom = 1)
        {
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int lineCount = 0;

            var formFont = new System.Drawing.Font("Iosevka", 10);
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = formTitle;
            form.Width = 800;
            form.Height = 700;
            form.BackColor = System.Drawing.Color.DarkGray;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            List<string> selectedFormat = new List<string>();
            System.Windows.Forms.TextBox formatDisplay = new System.Windows.Forms.TextBox();
            System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();
            System.Windows.Forms.TextBox dividerInput = new System.Windows.Forms.TextBox();

            System.Windows.Forms.Label formatLabel = new System.Windows.Forms.Label();
            formatLabel.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold);
            formatLabel.Text = message;
            formatLabel.AutoSize = true;
            formatLabel.Left = 10;
            formatLabel.Top = 10;

            form.Controls.Add(formatLabel);

            System.Windows.Forms.ComboBox[] formatComboBoxes = new System.Windows.Forms.ComboBox[availableFields.Length - 1];
            int spacing = 5;
            int totalSpacing = spacing * (formatComboBoxes.Length - 1);
            int comboWidth = (form.ClientSize.Width - 20 - totalSpacing) / formatComboBoxes.Length;
            for (int i = 0; i < formatComboBoxes.Length; i++)
            {
                formatComboBoxes[i] = new System.Windows.Forms.ComboBox();
                formatComboBoxes[i].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                formatComboBoxes[i].Font = formFont;
                formatComboBoxes[i].Items.AddRange(availableFields);
                formatComboBoxes[i].SelectedIndex = 0;
                formatComboBoxes[i].Left = 10 + i * (comboWidth + spacing);
                formatComboBoxes[i].Top = 30;
                formatComboBoxes[i].Width = comboWidth;
                formatComboBoxes[i].SelectedIndexChanged += (s, e) =>
                {
                    selectedFormat.Clear();
                    foreach (var combo in formatComboBoxes)
                    {
                        if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
                            selectedFormat.Add(combo.SelectedItem.ToString());
                    }
                    string divider = dividerInput.Text == "\\t" ? "\t" : dividerInput.Text;
                    formatDisplay.Text = string.Join(divider, selectedFormat);
                };
                form.Controls.Add(formatComboBoxes[i]);
            }

            System.Windows.Forms.Label dividerLabel = new System.Windows.Forms.Label();
            dividerLabel.Text = "Divider:";
            dividerLabel.Font = formFont;
            dividerLabel.AutoSize = true;
            dividerLabel.Left = 10;
            dividerLabel.Top = 60;
            form.Controls.Add(dividerLabel);

            dividerInput.Left = 80;
            dividerInput.Top = 60;
            dividerInput.Width = 50;
            dividerInput.Text = ":";
            dividerInput.Font = formFont;
            dividerInput.BackColor = System.Drawing.Color.Black;
            dividerInput.ForeColor = System.Drawing.Color.White;
            dividerInput.TextChanged += (s, e) =>
            {
                string divider1 = dividerInput.Text == "\\t" ? "\t" : dividerInput.Text;
                formatDisplay.Text = string.Join(divider1, selectedFormat);
            };
            form.Controls.Add(dividerInput);

            formatDisplay.Left = 10;
            formatDisplay.Top = 90;
            formatDisplay.Font = new System.Drawing.Font("Iosevka", 11, System.Drawing.FontStyle.Bold);
            formatDisplay.BackColor = System.Drawing.Color.Black;
            formatDisplay.ForeColor = System.Drawing.Color.White;
            formatDisplay.Width = form.ClientSize.Width - 20;
            formatDisplay.ReadOnly = true;

            form.Controls.Add(formatDisplay);

            System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
            dataLabel.Text = "Input data (one per line, matching format):";
            dataLabel.Font = formFont;
            dataLabel.AutoSize = true;
            dataLabel.Left = 10;
            dataLabel.Top = 120;
            form.Controls.Add(dataLabel);

            dataInput.Left = 10;
            dataInput.Top = 140;
            dataInput.Width = form.ClientSize.Width - 20;
            dataInput.Multiline = true;
            dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dataInput.BackColor = System.Drawing.Color.Black;
            dataInput.ForeColor = System.Drawing.Color.White;
            form.Controls.Add(dataInput);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold);
            okButton.BackColor = System.Drawing.Color.Black;
            okButton.ForeColor = System.Drawing.Color.LightGreen;
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            dataInput.Height = okButton.Top - dataInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };
            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog($"Import to {tableName} cancelled by user", true);
                return "0";
            }

            selectedFormat.Clear();
            foreach (var combo in formatComboBoxes)
            {
                if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
                    selectedFormat.Add(combo.SelectedItem.ToString());
            }

            if (string.IsNullOrEmpty(dataInput.Text) || selectedFormat.Count == 0 || string.IsNullOrEmpty(dividerInput.Text))
            {
                _project.SendWarningToLog("Data, format, or divider cannot be empty");
                return "0";
            }

            string divider0 = dividerInput.Text == "\\t" ? "\t" : dividerInput.Text;

            string[] lines = dataInput.Text.Trim().Split('\n');
            _project.SendInfoToLog($"Parsing [{lines.Length}] {tableName} data lines", true);

            for (int acc0unt = 1; acc0unt <= lines.Length; acc0unt++)
            {
                string line = lines[acc0unt - 1].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    _project.SendWarningToLog($"Line {acc0unt} is empty", false);
                    continue;
                }
                else
                {
                    string[] data_parts = line.Split(new[] { divider0 }, StringSplitOptions.None);
                    Dictionary<string, string> parsed_data = new Dictionary<string, string>();

                    for (int i = 0; i < selectedFormat.Count && i < data_parts.Length; i++)
                    {
                        parsed_data[selectedFormat[i]] = data_parts[i].Trim();
                    }

                    var queryParts = new List<string>();
                    foreach (var field in columnMapping.Keys)
                    {
                        string value = parsed_data.ContainsKey(field) ? parsed_data[field].Replace("'", "''") : "";
                        if (field == "CODE2FA" && value.Contains('/'))
                            value = value.Split('/').Last();
                        queryParts.Add($"{columnMapping[field]} = '{value}'");
                    }

                    try
                    {
                        Upd(string.Join(", ", queryParts), _tableName, last: false, acc: acc0unt);
                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        _project.SendWarningToLog($"Error processing line {acc0unt}: {ex.Message}", false);
                    }
                }
            }

            _project.SendInfoToLog($"[{lineCount}] records added to [{_tableName}]", true);
            return lineCount.ToString();
        }
        public string ImportKeys(string keyType, int startFrom = 1)
        {

            TblName("private_blockchain");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            //int rangeEnd = _rangeEnd + 1;
            var acc0 = _project.Variables["acc0"];
            acc0.Value = startFrom.ToString();

            var blockchain = new Blockchain();

            var formFont = new System.Drawing.Font("Iosevka", 10); //System.Drawing.FontStyle.Bold

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = $"Import {keyType} keys";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true; // Форма поверх всех окон
            form.Location = new System.Drawing.Point(108, 108);

            System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
            dataLabel.Text = $"Input {keyType} keys (one per line):";
            dataLabel.AutoSize = true;
            dataLabel.Left = 10;
            dataLabel.Top = 10;
            form.Controls.Add(dataLabel);

            System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();
            dataInput.Left = 10;
            dataInput.Top = 30;
            dataInput.Width = form.ClientSize.Width - 20;
            dataInput.Multiline = true;
            dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dataInput.MaxLength = 1000000;
            form.Controls.Add(dataInput);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            dataInput.Height = okButton.Top - dataInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            if (string.IsNullOrEmpty(dataInput.Text))
            {
                _project.SendWarningToLog("Input could not be empty");
                return "0";
            }

            string[] lines = dataInput.Text.Trim().Split('\n');
            _project.SendInfoToLog($"Parsing [{lines.Length}] strings", false);

            for (int i = 0; i < lines.Length; i++)
            {
                string key = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;
                _project.acc0w(i + 1);
                try
                {
                    switch (keyType)
                    {
                        case "seed":
                            string encodedSeed = SAFU.Encode(_project, key);
                            Upd($"bip39 = '{encodedSeed}'", _tableName);
                            break;

                        case "evm":
                            string privateKey;
                            string address;

                            if (key.Split(' ').Length > 1)
                            {
                                var mnemonicObj = new Mnemonic(key);
                                var hdRoot = mnemonicObj.DeriveExtKey();
                                var derivationPath = new NBitcoin.KeyPath("m/44'/60'/0'/0/0");
                                privateKey = hdRoot.Derive(derivationPath).PrivateKey.ToHex();
                            }
                            else
                            {
                                privateKey = key;
                            }

                            string encodedEvmKey = SAFU.Encode(_project, privateKey);
                            address = blockchain.GetAddressFromPrivateKey(privateKey);
                            DbQ($@"UPDATE {_tableName} SET secp256k1 = '{encodedEvmKey}' WHERE acc0 = {acc0.Value};", true);
                            break;

                        case "sol":
                            string encodedSolKey = SAFU.Encode(_project, key);
                            DbQ($@"UPDATE {_tableName} SET base58 = '{encodedSolKey}' WHERE acc0 = {acc0.Value};", true);
                            break;

                        default:
                            _project.SendWarningToLog($"Unknown key type: {keyType}");
                            return lines.Length.ToString();
                    }

                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing record {acc0.Value}: {ex.Message}", false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
            }

            return lines.Length.ToString();
        }
        public string ImportAddresses(int startFrom = 1)
        {
            string tablename = "public_blockchain";
            TblName(tablename);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            //int rangeEnd = _rangeEnd;
            var acc0 = _project.Variables["acc0"];
            acc0.Value = startFrom.ToString();


            acc0.Value = "1";

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Import Addresses";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            // Поле для ввода имени столбца
            System.Windows.Forms.Label columnLabel = new System.Windows.Forms.Label();
            columnLabel.Text = "Column name (e.g., evm, sol):";
            columnLabel.AutoSize = true;
            columnLabel.Left = 10;
            columnLabel.Top = 10;
            form.Controls.Add(columnLabel);

            System.Windows.Forms.TextBox columnInput = new System.Windows.Forms.TextBox();
            columnInput.Left = 10;
            columnInput.Top = 30;
            columnInput.Width = form.ClientSize.Width - 20;
            columnInput.Text = "input address label here ex: evm | apt |sol ";//_project.Variables["addressType"].Value; // Предполагаем, что переменная существует
            form.Controls.Add(columnInput);

            // Поле для ввода адресов
            System.Windows.Forms.Label addressLabel = new System.Windows.Forms.Label();
            addressLabel.Text = "Addresses (one per line):";
            addressLabel.AutoSize = true;
            addressLabel.Left = 10;
            addressLabel.Top = 60;
            form.Controls.Add(addressLabel);

            System.Windows.Forms.TextBox addressInput = new System.Windows.Forms.TextBox();
            addressInput.Left = 10;
            addressInput.Top = 80;
            addressInput.Width = form.ClientSize.Width - 20;
            addressInput.Multiline = true;
            addressInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            addressInput.MaxLength = 1000000;
            form.Controls.Add(addressInput);

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 20;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            addressInput.Height = okButton.Top - addressInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            if (string.IsNullOrEmpty(columnInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Column name or addresses cannot be empty");
                return "0";
            }

            string columnName = columnInput.Text.ToLower();

            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {columnName, "TEXT DEFAULT ''"}
            };
            TblAdd(tablename, tableStructure);

            string[] lines = addressInput.Text.Trim().Split('\n');
            int lineCount = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string address = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(address))
                {
                    _project.SendWarningToLog($"Line {acc0.Value} is empty");
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                    continue;
                }

                try
                {
                    _project.SendInfoToLog($"Processing acc0 = {acc0.Value}, address = '{address}'", false);
                    Upd($"{columnName} = '{address}'", _tableName, last: false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                    lineCount++;
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing record {acc0.Value} for {columnName}: {ex.Message}", false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
            }

            _project.SendInfoToLog($"[{lineCount}] strings added to [{_tableName}]", true);
            return lineCount.ToString();
        }
        public string ImportDepositAddresses(int startFrom = 1)
        {
            string tablename = "public_deposits";
            TblName(tablename);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            _project.L0g($"{_tableName} `b");
            //int rangeEnd = _rangeEnd;
            var acc0 = _project.Variables["acc0"];
            acc0.Value = startFrom.ToString();


            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Import Deposit Addresses";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true; // Форма поверх всех окон
            //form.Location = new System.Drawing.Point(108, 108);

            // Поле для ввода CHAIN
            System.Windows.Forms.Label chainLabel = new System.Windows.Forms.Label();
            chainLabel.Text = "Chain (e.g., ETH, BSC):";
            chainLabel.AutoSize = true;
            chainLabel.Left = 10;
            chainLabel.Top = 10;
            form.Controls.Add(chainLabel);

            System.Windows.Forms.TextBox chainInput = new System.Windows.Forms.TextBox();
            chainInput.Left = 10;
            chainInput.Top = 30;
            chainInput.Width = form.ClientSize.Width - 20;
            chainInput.Text = "ETH";//_project.Variables["depositChain"].Value; // Текущее значение из переменной
            form.Controls.Add(chainInput);

            // Поле для ввода CEX
            System.Windows.Forms.Label cexLabel = new System.Windows.Forms.Label();
            cexLabel.Text = "CEX (e.g., binance, kucoin):";
            cexLabel.AutoSize = true;
            cexLabel.Left = 10;
            cexLabel.Top = 60;
            form.Controls.Add(cexLabel);

            System.Windows.Forms.TextBox cexInput = new System.Windows.Forms.TextBox();
            cexInput.Left = 10;
            cexInput.Top = 80;
            cexInput.Width = form.ClientSize.Width - 20;
            cexInput.Text = "OKX";//_project.Variables["depositCEX"].Value; // Текущее значение из переменной
            form.Controls.Add(cexInput);

            // Поле для ввода адресов
            System.Windows.Forms.Label addressLabel = new System.Windows.Forms.Label();
            addressLabel.Text = "Deposit addresses (one per line):";
            addressLabel.AutoSize = true;
            addressLabel.Left = 10;
            addressLabel.Top = 110;
            form.Controls.Add(addressLabel);

            System.Windows.Forms.TextBox addressInput = new System.Windows.Forms.TextBox();
            addressInput.Left = 10;
            addressInput.Top = 130;
            addressInput.Width = form.ClientSize.Width - 20;
            addressInput.Multiline = true;
            addressInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            form.Controls.Add(addressInput);

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            addressInput.Height = okButton.Top - addressInput.Top - 5;
            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            if (string.IsNullOrEmpty(chainInput.Text) || string.IsNullOrEmpty(cexInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Chain, CEX, or addresses cannot be empty");
                return "0";
            }

            string CHAIN = chainInput.Text.ToLower();
            string CEX = cexInput.Text.ToLower();
            string columnName = $"{CEX}_{CHAIN}";

            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {columnName, "TEXT DEFAULT ''"}
            };
            ClmnAdd(_tableName, tableStructure);


            string[] lines = addressInput.Text.Trim().Split('\n');
            int lineCount = 0;

            AddRange(_tableName, lines.Length);

            for (int acc0index = startFrom; acc0index <= lines.Length; acc0index++)
            {
                string line = lines[acc0index - 1].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    _project.SendWarningToLog($"Line {acc0index} is empty");
                    continue;
                }

                try
                {
                    acc0.Value = acc0index.ToString();
                    Upd($"{columnName} = '{line}'", _tableName, last: false);

                    lineCount++;
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}");
                    continue;
                }
            }

            _project.SendInfoToLog($"[{lineCount}] strings added to [{_tableName}]", true);
            return lineCount.ToString();
        }

        public void MapAndImport(schema tableSchem, int startFrom = 1)
        {
            _project.Variables["acc0"].Value = startFrom.ToString();

            var mapping = new Dictionary<string, string>();

            Log($"mapping {tableSchem}");
            switch (tableSchem)
            {

                case schema.private_twitter:
                    string[] twitterFields = new string[] { "", "LOGIN", "PASSWORD", "EMAIL", "EMAIL_PASSWORD", "TOKEN", "2FA_SECRET", "2FA_BACKUP" };
                    var twitterMapping = new Dictionary<string, string>
                    {
                        { "LOGIN", "login" },
                        { "PASSWORD", "password" },
                        { "EMAIL", "email" },
                        { "EMAIL_PASSWORD", "emailpass" },
                        { "TOKEN", "token" },
                        { "2FA_SECRET", "otpsecret" },
                        { "2FA_BACKUP", "otpbackup" }
                    };
                    ImportData(tableSchem.ToString(), twitterFields, twitterMapping, "Import Twitter Data");
                    return;


                case schema.private_discord:
                    string[] discordFields = new string[] { "", "LOGIN", "PASSWORD", "TOKEN", "2FA_SECRET" };
                    var discordMapping = new Dictionary<string, string>
                    {
                        { "LOGIN", "login" },
                        { "PASSWORD", "password" },
                        { "TOKEN", "token" },
                        { "2FA_SECRET", "otpsecret" }
                    };
                    ImportData(tableSchem.ToString(), discordFields, discordMapping, "Import Discord Data");
                    return;

                case schema.private_google:
                    string[] googleFields = new string[] { "", "LOGIN", "PASSWORD", "RECOVERY_EMAIL", "2FA_SECRET", "2FA_BACKUP" };
                    var googleMapping = new Dictionary<string, string>
                    {
                        { "LOGIN", "login" },
                        { "PASSWORD", "password" },
                        { "RECOVERY_EMAIL", "recoveryemail" },
                        { "2FA_SECRET", "otpsecret" },
                        { "2FA_BACKUP", "otpbackup" }
                    };
                    ImportData(tableSchem.ToString(), googleFields, googleMapping, "Import Google Data");
                    return;

                case schema.private_github:
                    string[] githubFields = new string[] { "", "LOGIN", "PASSWORD", "TOKEN", "EMAIL", "EMAIL_PASSWORD", "2FA_SECRET", "2FA_BACKUP" };
                    var githubMapping = new Dictionary<string, string>
                    {
                        { "LOGIN", "login" },
                        { "PASSWORD", "password" },

                        { "EMAIL", "email" },
                        { "EMAIL_PASSWORD", "emailpass" },

                        { "TOKEN", "token" },
                        { "2FA_SECRET", "otpsecret" },
                        { "2FA_BACKUP", "otpbackup" }
                    };
                    ImportData(tableSchem.ToString(), githubFields, githubMapping, "Import GitHub Data");
                    return;


                case schema.public_mail:
                    var icloud = _f0rm.GetLinesByKey("icloud", "input data, don't change key!");
                    Upd(icloud, tableSchem.ToString(), last: false);
                    return;

                case schema.public_profile:
                    var nicknames = _f0rm.GetLinesByKey("nickname", "input data, don't change key!");
                    Upd(nicknames, tableSchem.ToString());
                    var bio = _f0rm.GetLinesByKey("bio", "input data, don't change key!");
                    Upd(bio, tableSchem.ToString(), last: false);
                    return;

                case schema.private_profile:
                    var proxy = _f0rm.GetLinesByKey("proxy", "input proxy, don't change key!");
                    if (proxy.Count != 0)
                    {
                        Upd(proxy, tableSchem.ToString(), last: false);
                    }
                    else Log("!W empty input");

                    var vendors = new List<string> {
                     "NVIDIA",
                     "AMD",
                     "Intel",
                     };

                    var vendor = _f0rm.GetSelectedItem(vendors);

                    if (!string.IsNullOrEmpty(vendor))
                    {
                        var webGl = _instance.ParseWebGl(vendor, _range, _project);
                        Upd(webGl, "webgl", tableSchem.ToString(), last: false);
                    }
                    else Log("!W empty input");

                    return;

                case schema.public_blockchain:

                    var acc0 = 1;
                    _project.acc0w(acc0);
                    while (true)
                    {
                        _project.acc0w(acc0);
                        var pk = Key("evm");
                        if (string.IsNullOrEmpty(pk)) break;
                        var addrs = pk.ToPubEvm();
                        Upd($"evm_pk = '{addrs}'", "public_blockchain");
                        acc0++;
                    }

                    acc0 = 1;
                    while (true)
                    {
                        _project.acc0w(acc0);
                        var pk = Key("seed");
                        if (string.IsNullOrEmpty(pk)) break;
                        var addrs = pk.ToPubEvm();
                        Upd($"evm_seed = '{addrs}'", "public_blockchain");
                        acc0++;
                    }

                    return;

                case schema.private_blockchain:
                    ImportKeys("seed");
                    ImportKeys("evm");
                    ImportKeys("sol");
                    return;

                case schema.public_deposits:
                    ImportDepositAddresses();
                    return;

                case schema.private_settings:
                    var phK = new List<string> {
                        "discord_invite",
                        "discord_hub",
                        "tg_logger_token",
                        "tg_logger_group",
                        "tg_logger_topic",
                        "profiles_folder"
                        };


                    var phV = new List<string> {
                        "Инвайт на свой сервер",
                        "ссылка на канал с инвайтами на вашем сервере",
                        "Токен Telegram логгера",
                        "Id группы для логов в Telegram. Формат {-1002000000009}",
                        "Id топика в группе для логов. 0 - если нет топиков",
                        "Путь к папке с профилями и причастным данным. Формат: {F:\\farm\\}"
                        };


                    var settings = _f0rm.GetKeyValuePairs(phK.Count(), phK, phV, "input settings", prepareUpd: false);
                    Write(settings, tableSchem.ToString());
                    return;

                case schema.private_api:
                    phK = new List<string> { "apikey", "apisecret", "proxy", };
                    var toWrite = _f0rm.GetKeyValueString(phK.Count(), phK, null, "input binance api");
                    UpdTxt(toWrite, tableSchem.ToString(), "binance");

                    phK = new List<string> { "apikey", "apisecret", "passphrase", };
                    toWrite = _f0rm.GetKeyValueString(phK.Count(), phK, null, "input okx api");
                    UpdTxt(toWrite, tableSchem.ToString(), "okx");

                    phK = new List<string> { "apikey", "apisecret", "passphrase", };
                    toWrite = _f0rm.GetKeyValueString(phK.Count(), phK, null, "input firstmail login as apisecret)");
                    UpdTxt(toWrite, tableSchem.ToString(), "firstmail");

                    phK = new List<string> { "apikey", };
                    toWrite = _f0rm.GetKeyValueString(phK.Count(), phK, null, "input perplexity api");
                    UpdTxt(toWrite, tableSchem.ToString(), "perplexity");

                    return;

                case schema.public_rpc:
                case schema.public_native:
                case schema.public_google:
                case schema.public_twitter:
                case schema.public_discord:
                    Log($"{tableSchem.ToString()} is for manual fill or not compoulsary");

                    return;


                default:
                    throw new Exception($"no schema [{tableSchem}]");

            }

        }

        public void CopyTable(string sourceTable, string targetTable)
        {
            // Обрабатываем имена таблиц
            string sourceTableName = TblName(sourceTable); // Чистое имя таблицы источника
            string sourceSchema = _schemaName; // Схема источника
            string targetTableName = TblName(targetTable); // Чистое имя целевой таблицы
            string targetSchema = _schemaName; // Схема цели

            // Формируем полные имена таблиц для PostgreSQL
            string sourceFullName = _pstgr ? $"{sourceSchema}.{sourceTableName}" : sourceTableName;
            string targetFullName = _pstgr ? $"{targetSchema}.{targetTableName}" : targetTableName;

            if (_pstgr)
            {
                // PostgreSQL: Проверяем существование исходной таблицы
                string existQuery = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{sourceSchema}' AND table_name = '{sourceTableName}';";
                string existResult = DbQ(existQuery);
                if (existResult == "0" || string.IsNullOrEmpty(existResult))
                {
                    _project.SendWarningToLog($"Source table {sourceFullName} not found");
                    return;
                }

                // Удаляем целевую таблицу, если существует
                DbQ($"DROP TABLE IF EXISTS {targetFullName};");

                // Создаём новую таблицу с той же структурой
                DbQ($"CREATE TABLE {targetFullName} (LIKE {sourceFullName} INCLUDING ALL);");

                // Копируем данные
                DbQ($"INSERT INTO {targetFullName} SELECT * FROM {sourceFullName};");

                _project.SendInfoToLog($"Table {sourceFullName} successfully copied to {targetFullName}");
            }
            else
            {
                string existQuery = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{sourceTableName}';";
                string existResult = DbQ(existQuery);
                if (existResult == "0" || string.IsNullOrEmpty(existResult))
                {
                    _project.SendWarningToLog($"Source table {sourceTableName} not found");
                    return;
                }

                string createTableSQL = DbQ($"SELECT sql FROM sqlite_master WHERE type='table' AND name='{sourceTableName}';");
                if (string.IsNullOrEmpty(createTableSQL))
                {
                    _project.SendWarningToLog($"Failed to retrieve schema for table {sourceTableName}");
                    return;
                }

                string newTableSQL = createTableSQL.Replace(sourceTableName, targetTableName);

                DbQ($"DROP TABLE IF EXISTS {targetTableName};");

                DbQ(newTableSQL);

                DbQ($"INSERT INTO {targetTableName} SELECT * FROM {sourceTableName};");

                _project.SendInfoToLog($"Table {sourceTableName} successfully copied to {targetTableName}");
            }
        }

        public void RenameColumns(string tblName, Dictionary<string, string> renameMap)
        {
            Log($"{_tableName} {tblName} {_pstgr}`b");
            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            Log($"{_tableName} {tblName}  {_pstgr} `lb");
            var currentColumns = TblColumns(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            Log($"{_tableName} `g");
            foreach (var pair in renameMap)
            {
                string oldName = pair.Key;
                string newName = pair.Value;


                if (!currentColumns.Contains(oldName))
                {
                    _project.SendWarningToLog($"Column [{oldName}] not found in table {_tableName}");
                    continue;
                }

                // Проверяем, отличается ли новое имя от старого
                if (oldName == newName)
                {
                    _project.SendInfoToLog($"Column [{oldName}] already has the target name, skipping");
                    continue;
                }

                // Формируем запрос для переименования
                string query = _pstgr
                    ? $"ALTER TABLE {_tableName} RENAME COLUMN {oldName} TO {newName};"
                    : $"ALTER TABLE {_tableName} RENAME COLUMN {oldName} TO {newName};";

                DbQ(query, true);
                _project.SendInfoToLog($"Renamed column [{oldName}] to [{newName}] in table {_tableName}");
            }
        }

        public void ImportDB(schema? schemaValue = null)
        {
            try
            {
                new Sql(_project).DbQ("CREATE SCHEMA IF NOT EXISTS private;");
                new Sql(_project).DbQ("CREATE SCHEMA IF NOT EXISTS public;");
                new Sql(_project).DbQ("CREATE SCHEMA IF NOT EXISTS projects;");
            }
            catch (Exception ex) { }

            var phK = new List<string> {
                "private_settings",
                "private_profile",
                "private_blockchain",
                "private_api",
                "private_google",
                "private_twitter",
                "private_discord",
                "private_github",
                "public_blockchain",
                "public_native",
                "public_deposits",
                "public_profile",
                "public_rpc",
                "public_mail",
                "public_google",
                "public_twitter",
                "public_discord",
            };

            var phV = new List<string> {
                "Folders, loggers & etc. general settings",
                "Proxy, cookies, webgl",
                "Blockchain keys, seeds, etc. ",
                "Api keys ",
                "Google Credentials",
                "Twitter Credentials",
                "Discord Credentials",
                "GitHub Credentials",
                "Blockchain addresses",
                "Balancees",
                "CEX deposit addresses",
                "Username, bio, pfp, etc.",
                "Custom RPC & Explorers list",
                "All emails",
                "Google Stats",
                "Twitter Stats",
                "Discord Stats",
            };

            // If a specific schema is provided, process only that schema
            if (schemaValue.HasValue)
            {
                string tablename = schemaValue.ToString();
                if (!Enum.IsDefined(typeof(schema), schemaValue.Value))
                    throw new Exception($"Error: '{tablename}' is not a valid schema value. Valid values: {string.Join(", ", Enum.GetNames(typeof(schema)))}");

                var dic = LoadSchema(schemaValue.Value);
                TblAdd(tablename, dic);
                try
                {
                    AddRange(tablename);
                }
                catch { }
                MapAndImport(schemaValue.Value);
            }
            else
            {
                // Original behavior: process all selected schemas from the form
                var import = _f0rm.GetKeyBoolPairs(phK.Count(), phK, phV, "check boxes to import", prepareUpd: false);

                foreach (KeyValuePair<string, bool> pair in import)
                {
                    string tablename = pair.Key;
                    bool execute = pair.Value;

                    var table = Enum.TryParse<schema>(pair.Key, true, out var parsed) ? parsed : default;
                    if (!Enum.IsDefined(typeof(schema), table))
                        throw new Exception($"Error: '{tablename}' is not a valid schema value. Valid values: {string.Join(", ", Enum.GetNames(typeof(schema)))}");

                    if (execute)
                    {
                        var dic = LoadSchema(table);
                        TblAdd(tablename, dic);
                        try
                        {
                            AddRange(tablename);
                        }
                        catch { }
                        MapAndImport(table);
                    }
                }
            }
        }

    }


}
