using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json.Linq;
using Leaf.xNet;
using Nethereum.Contracts.Standards.ENS.ETHRegistrarController.ContractDefinition;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;


namespace ZBSolutions
{
    public class X
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _token;
        protected string _login;
        protected string _pass;
        protected string _2fa;
        protected string _email;
        protected string _email_pass;

        public X(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
                      
            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logShow = log;
            
            LoadCreds();

        }

        protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 💠  {callerName}] [{tolog}] ");
        }

        private string LoadCreds()
        {
            string[] xCreds = _sql.Get(" status, token, login, password, code2fa, emailLogin, emailPass", "twitter").Split('|');
            _status = xCreds[0];
            _token  = xCreds[1];
            _login = xCreds[2];
            _pass = xCreds[3];
            _2fa =  xCreds[4];
            _email = xCreds[5];
            _email_pass = xCreds[6];
            try {
                _project.Variables["twitterSTATUS"].Value = _status;
                _project.Variables["twitterTOKEN"].Value = _token;
                _project.Variables["twitterLOGIN"].Value = _login;
                _project.Variables["twitterPASSWORD"].Value = _pass;
                _project.Variables["twitterCODE2FA"].Value = _2fa;
                _project.Variables["twitterEMAIL"].Value = _email;
                _project.Variables["twitterEMAIL_PASSWORD"].Value = _email_pass;
            }
            catch (Exception ex)
            {
                _project.SendInfoToLog(ex.Message);
            }

            return _status;

        }

        private string XcheckState(bool log = false)
        {
            log = _project.Variables["debug"].Value == "True";
            DateTime start = DateTime.Now;
            DateTime deadline = DateTime.Now.AddSeconds(60);
            string login = _project.Variables["twitterLOGIN"].Value;
            _instance.ActiveTab.Navigate($"https://x.com/{login}", "");
            var status = "";

            while (string.IsNullOrEmpty(status))
            {
                Thread.Sleep(5000);
                _project.L0g($"{DateTime.Now - start}s check... URLNow:[{_instance.ActiveTab.URL}]");
                if (DateTime.Now > deadline) throw new Exception("timeout");

                else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Caution:\s+This\s+account\s+is\s+temporarily\s+restricted", "regexp", 0).IsVoid)
                    status = "restricted";
                else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Account\s+suspended\s+X\s+suspends\s+accounts\s+which\s+violate\s+the\s+X\s+Rules", "regexp", 0).IsVoid)
                    status = "suspended";
                else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Log\ in", "regexp", 0).IsVoid || !_instance.ActiveTab.FindElementByAttribute("a", "data-testid", "loginButton", "regexp", 0).IsVoid)
                    status = "login";

                else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", "erify\\ your\\ email\\ address", "regexp", 0).IsVoid ||
                    !_instance.ActiveTab.FindElementByAttribute("div", "innertext", "We\\ sent\\ your\\ verification\\ code.", "regexp", 0).IsVoid)
                    status = "emailCapcha";
                else if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "SideNav_AccountSwitcher_Button", "regexp", 0).IsVoid)
                {
                    var check = _instance.ActiveTab.FindElementByAttribute("button", "data-testid", "SideNav_AccountSwitcher_Button", "regexp", 0).FirstChild.FirstChild.GetAttribute("data-testid");
                    if (check == $"UserAvatar-Container-{login}") status = "ok";
                    else
                    {
                        status = "mixed";
                        _project.L0g($"!W {status}. Detected  [{check}] instead [UserAvatar-Container-{login}] {DateTime.Now - start}");
                    }
                }
                else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Something\\ went\\ wrong.\\ Try\\ reloading.", "regexp", 0).IsVoid)
                {
                    _instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");
                    Thread.Sleep(3000);
                    continue;
                }
            }
            _project.L0g($"{status} {DateTime.Now - start}");
            return status;
        }
        private void XsetToken()
        {
            var token = _project.Variables["twitterTOKEN"].Value;
            string jsCode = _project.ExecuteMacro($"document.cookie = \"auth_token={token}; domain=.x.com; path=/; expires=${DateTimeOffset.UtcNow.AddYears(1).ToString("R")}; Secure\";\r\nwindow.location.replace(\"https://x.com\")");
            _instance.ActiveTab.MainDocument.EvaluateScript(jsCode);
        }
        private string XgetToken()
        {
            var cookJson = _instance.GetCookies(_project,".");
            JArray toParse = JArray.Parse(cookJson);
            int i = 0; var token = "";
            while (token == "")
            {
                if (toParse[i]["name"].ToString() == "auth_token") token = toParse[i]["value"].ToString();
                i++;
            }
            _project.Variables["twitterTOKEN"].Value = token;
            _sql.Upd($"token = '{token}'", "twitter");
            return token;
        }
        private string Xlogin()
        {
            DateTime deadline = DateTime.Now.AddSeconds(60);
            var login = _project.Variables["twitterLOGIN"].Value;

            _instance.ActiveTab.Navigate("https://x.com/", ""); Thread.Sleep(2000);
            _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 1, thr0w: false);
            _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);
            _instance.HeClick(("a", "data-testid", "login", "regexp", 0));
            _instance.HeSet(("input:text", "autocomplete", "username", "text", 0), login, deadline: 30);
            _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");

            if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Sorry, we could not find your account')]", 0).IsVoid) return "NotFound";

            _instance.HeSet(("password", "name"), _project.Variables["twitterPASSWORD"].Value);


            _instance.HeClick(("button", "data-testid", "LoginForm_Login_Button", "regexp", 0), "clickOut");

            if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Wrong password!')]", 0).IsVoid) return "WrongPass";

            var codeOTP = OTP.Offline(_project.Variables["twitterCODE2FA"].Value);
            _instance.HeSet(("text", "name"), codeOTP);


            _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");

            if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Your account is suspended')]", 0).IsVoid) return "Suspended";
            if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Oops,\\ something\\ went\\ wrong.\\ Please\\ try\\ again\\ later.", "regexp", 0).IsVoid) return "SomethingWentWrong";
            if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", "Suspicious\\ login\\ prevented", "regexp", 0).IsVoid) return "SuspiciousLogin";

            _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 1, thr0w: false);
            _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);
            XgetToken();
            return "ok";
        }
        public string Xload(bool log = false)
        {


            bool tokenUsed = false;
            DateTime deadline = DateTime.Now.AddSeconds(60);
        check:

            if (DateTime.Now > deadline) throw new Exception("timeout");

            var status = XcheckState(log: true);

            if (status == "login" && !tokenUsed)
            {
                XsetToken();
                tokenUsed = true;
                Thread.Sleep(3000);
            }
            else if (status == "login" && tokenUsed)
            {
                var login = Xlogin();
                _project.L0g($"{login}");
                Thread.Sleep(3000);
            }
            if (status == "restricted" || status == "suspended" || status == "emailCapcha" || status == "mixed" || status == "ok")
            {
                _sql.Upd($"status = '{status}'", "twitter");
                return status;
            }
            else if (status == "ok")
            {
                _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 0, thr0w: false);
                _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);

                XgetToken();
                return status;
            }
            else
                _project.L0g($"unknown {status}");
            goto check;
        }

        public void XAuth()
        {
            DateTime deadline = DateTime.Now.AddSeconds(60);
        check:
            if (DateTime.Now > deadline) throw new Exception("timeout");
            _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 0, thr0w: false);
            _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);

            string state = null;


            if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Sorry, we could not find your account')]", 0).IsVoid) state = "NotFound";
            else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Your account is suspended')]", 0).IsVoid) state = "Suspended";
            else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Wrong password!')]", 0).IsVoid) state = "WrongPass";
            else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Oops,\\ something\\ went\\ wrong.\\ Please\\ try\\ again\\ later.", "regexp", 0).IsVoid) state = "SomethingWentWrong";
            else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", "Suspicious\\ login\\ prevented", "regexp", 0).IsVoid) state = "SuspiciousLogin";



            else if (!_instance.ActiveTab.FindElementByAttribute("input:text", "autocomplete", "username", "text", 0).IsVoid) state = "InputLogin";
            else if (!_instance.ActiveTab.FindElementByAttribute("input:password", "autocomplete", "current-password", "text", 0).IsVoid) state = "InputPass";
            else if (!_instance.ActiveTab.FindElementByAttribute("input:text", "data-testid", "ocfEnterTextTextInput", "text", 0).IsVoid) state = "InputOTP";


            else if (!_instance.ActiveTab.FindElementByAttribute("a", "data-testid", "login", "regexp", 0).IsVoid) state = "ClickLogin";
            else if (!_instance.ActiveTab.FindElementByAttribute("li", "data-testid", "UserCell", "regexp", 0).IsVoid) state = "CheckUser";


            _project.L0g(state);

            switch (state)
            {
                case "NotFound":
                case "Suspended":
                case "SuspiciousLogin":
                case "WrongPass":
                    _sql.Upd($"status = '{state}'", "twitter");
                    throw new Exception($"{state}");
                case "ClickLogin":
                    _instance.HeClick(("a", "data-testid", "login", "regexp", 0));
                    goto check;
                case "InputLogin":
                    _instance.HeSet(("input:text", "autocomplete", "username", "text", 0), _login, deadline: 30);
                    _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");
                    goto check;
                case "InputPass":
                    _instance.HeSet(("input:password", "autocomplete", "current-password", "text", 0), _pass);
                    _instance.HeClick(("button", "data-testid", "LoginForm_Login_Button", "regexp", 0), "clickOut");
                    goto check;
                case "InputOTP":
                    _instance.HeSet(("input:text", "data-testid", "ocfEnterTextTextInput", "text", 0), OTP.Offline(_2fa));
                    _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");
                    goto check;
                case "CheckUser":
                    string userdata = _instance.HeGet(("li", "data-testid", "UserCell", "regexp", 0));
                    if (userdata.Contains(_login))
                    {
                        _instance.HeClick(("button", "data-testid", "OAuth_Consent_Button", "regexp", 0));
                        goto check;
                    }
                    else
                    {
                        throw new Exception("wrong account");
                    }
                default:
                    _project.L0g($"unknown state [{state}]");
                    break;

            }

            if (!_instance.ActiveTab.URL.Contains("x.com") && !_instance.ActiveTab.URL.Contains("twitter.com"))
                _project.L0g("auth done");
            else goto check;
        }

        public void UpdXCreds(Dictionary<string, string> data)
        {
            var fields = new Dictionary<string, string>
            {
                { "LOGIN", data.ContainsKey("LOGIN") ? data["LOGIN"].Replace("'", "''") : "" },
                { "PASSWORD", data.ContainsKey("PASSWORD") ? data["PASSWORD"].Replace("'", "''") : "" },
                { "EMAIL", data.ContainsKey("EMAIL") ? data["EMAIL"].Replace("'", "''") : "" },
                { "EMAIL_PASSWORD", data.ContainsKey("EMAIL_PASSWORD") ? data["EMAIL_PASSWORD"].Replace("'", "''") : "" },
                //{ "TOKEN", data.ContainsKey("TOKEN") ? data["TOKEN"].Replace("'", "''") : "" },

                { "TOKEN", data.ContainsKey("TOKEN") ? (data["TOKEN"].Contains('=') ? data["TOKEN"].Split('=').Last().Replace("'", "''") : data["TOKEN"].Replace("'", "''")) : "" },

                { "CODE2FA", data.ContainsKey("CODE2FA") ? (data["CODE2FA"].Contains('/') ? data["CODE2FA"].Split('/').Last().Replace("'", "''") : data["CODE2FA"].Replace("'", "''")) : "" },
                { "RECOVERY_SEED", data.ContainsKey("RECOVERY_SEED") ? data["RECOVERY_SEED"].Replace("'", "''") : "" }
            };

            var _sql = new Sql(_project);
            try
            {
                _sql.Upd($@"token = '{fields["TOKEN"]}', 
                login = '{fields["LOGIN"]}', 
                password = '{fields["PASSWORD"]}', 
                code2fa = '{fields["CODE2FA"]}', 
                emaillogin = '{fields["EMAIL"]}', 
                emailpass = '{fields["EMAIL_PASSWORD"]}', 
                recovery2fa = '{fields["RECOVERY_SEED"]}'", "twitter");
            }
            catch (Exception ex)
            {
                _project.L0g("!W{ex.Message}");
            }
        }

        public string GenerateTweet(string content, string bio = "", bool log = false)
        {
            _project.Variables["api_response"].Value = "";

            var requestBody = new
            {
                model = "sonar",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = string.IsNullOrEmpty(bio)
                            ? "You are a social media account. Generate tweets that reflect a generic social media persona."
                            : $"You are a social media account with the bio: '{bio}'. Generate tweets that reflect this persona, incorporating themes relevant to bio."
                    },
                    new
                    {
                        role = "user",
                        content = content
                    }
                },
                temperature = 0.8,
                top_p = 0.9,
                top_k = 0,
                stream = false,
                presence_penalty = 0,
                frequency_penalty = 1
            };

            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, Newtonsoft.Json.Formatting.None);

            string[] headers = new string[]
            {
                "Content-Type: application/json",
                $"Authorization: Bearer {_project.Variables["settingsApiPerplexity"].Value}"
            };
            gen:
            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                foreach (var header in headers)
                {
                    var parts = header.Split(new[] { ": " }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        request.AddHeader(parts[0], parts[1]);
                    }
                }

                try
                {
                    HttpResponse httpResponse = request.Post("https://api.perplexity.ai/chat/completions", jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    Log($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }
            
            _project.Variables["api_response"].Value = response;

            Log($"Full response: {response}");

            try
            {
                var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(response);
                string tweetText = jsonResponse["choices"][0]["message"]["content"].ToString();
                if (tweetText.Length > 220)
                {
                    Log($"tweet is over 220sym `y");
                    goto gen;
                }
                
                Log($"tweet: {tweetText}");
                return tweetText; 
            }
            catch (Exception ex)
            {
                Log($"!W Error parsing response: {ex.Message}");
                throw;
            }
        }


        public void ParseProfile()
        {
            _instance.HeClick(("*", "data-testid", "AppTabBar_Profile_Link", "regexp", 0));


            string json = _instance.HeGet(("*", "data-testid", "UserProfileSchema-test", "regexp", 0));

            var jo = JObject.Parse(json);
            var main = jo["mainEntity"];

            string dateCreated = jo["dateCreated"].ToString();
            string id = main["identifier"].ToString();

            string username = main["additionalName"].ToString();
            string description = main["description"].ToString();
            string givenName = main["givenName"].ToString();
            string homeLocation = main["homeLocation"]["name"].ToString();

            string ava = main["image"]["contentUrl"].ToString();
            string banner = main["image"]["thumbnailUrl"].ToString();

            var interactionStatistic = main["interactionStatistic"];

            string Followers = interactionStatistic[0]["userInteractionCount"].ToString();
            string Following = interactionStatistic[1]["userInteractionCount"].ToString();
            string Tweets = interactionStatistic[2]["userInteractionCount"].ToString();

            _sql.Upd($@"dateCreated = '{dateCreated}',
                        id = '{id}',
                        username = '{username}',
                        description = '{description}',
                        givenName = '{givenName}',
                        homeLocation = '{homeLocation}',
                        ava = '{ava}',
                        banner = '{banner}',
                        Followers = '{Followers}',
                        Following = '{Following}',
                        Tweets = '{Tweets}',
                        ");

        }

        public void ParseSecured()
        {

            _instance.ActiveTab.Navigate("https://x.com/settings/your_twitter_data/account", "");

        scan:
            try
            {
                _instance.HeSet(("current_password", "name"), _pass, deadline: 1);
                _instance.HeClick(("button", "innertext", "Confirm", "regexp", 0));
            }
            catch { }
            var tIdList = _instance.ActiveTab.FindElementsByAttribute("*", "data-testid", ".", "regexp").ToList();

            if (tIdList.Count < 50)
            {
                Thread.Sleep(3000);
                goto scan;
            }

            string email = null;
            string phone = null;
            string creation = null;
            string country = null;
            string lang = null;
            string gender = null;
            string birth = null;


            foreach (HtmlElement he in tIdList)
            {
                string pName = null;
                string pValue = null;
                string testid = he.GetAttribute("data-testid");
                string href = he.GetAttribute("href");
                string text = he.InnerText;

                switch (testid)
                {
                    case "account-creation":
                        pName = text.Split('\n')[0];
                        pValue = text.Replace(pName, "").Replace("\n", " ").Trim();
                        creation = pValue;
                        continue;
                    case "pivot":
                        pName = text.Split('\n')[0];
                        pValue = text.Replace(pName, "").Replace("\n", " ").Trim();
                        switch (pName)
                        {
                            case "Phone":
                                phone = pValue;
                                break;
                            case "Email":
                                email = pValue;
                                break;
                            case "Country":
                                country = pValue;
                                break;
                            case "Languages":
                                lang = pValue;
                                break;
                            case "Gender":
                                gender = pValue;
                                break;
                            case "Birth date":
                                birth = pValue;
                                break;
                        }
                        continue;
                    default:
                        continue;
                }
            }
            _sql.Upd($@"creation = '{creation}',
                        email = '{email}',
                        phone = '{phone}',
                        country = '{country}',
                        lang = '{lang}',
                        gender = '{gender}',
                        birth = '{birth}',
                        ");
        }
    }
}
