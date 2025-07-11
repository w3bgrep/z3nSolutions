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
        public static void ClearShit(this Instance instance, string domain)
        {
            instance.CloseAllTabs();
            instance.ClearCache(domain);
            instance.ClearCookie(domain);
            Thread.Sleep(500);
            instance.ActiveTab.Navigate("about:blank", "");
        }
        public static string ToPrivateKey(this string input)
        {
            // Проверка на null или пустую строку
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            // Преобразуем строку в байты с использованием UTF-8
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Вычисляем SHA-256 хеш (32 байта)
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Преобразуем байты в hex-строку (64 символа)
                StringBuilder hex = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    hex.AppendFormat("{0:x2}", b);
                }

                return hex.ToString();
            }
        }
        public static Dictionary<string, string> ParseCreds2(this string data, string format, char devider = ':')
        {
            var parsedData = new Dictionary<string, string>();
            data = data.Replace("https://2fa.fb.rip/", "").Replace("https://2fa.fb.rip/api/otp/", "");
            if (string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(data))
                return parsedData;

            string[] formatParts = format.Split(devider);
            string[] dataParts = data.Split(devider);

            for (int i = 0; i < formatParts.Length && i < dataParts.Length; i++)
            {
                string key = formatParts[i].Trim('{', '}').Trim();
                if (!string.IsNullOrEmpty(key))
                    parsedData[key] = dataParts[i].Trim();
            }
            return parsedData;
        }
    }

    public class Guild2
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

        public Guild2(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project, log);
            _logger = new Logger(project, log: log, classEmoji: "GUILD");

        }

        public void ParseRoles(string tablename)
        {

            var done = new List<string>();
            var undone = new List<string>();

            var roles = _instance.ActiveTab.FindElementsByAttribute("div", "id", "role-", "regexp").ToList();
            _sql.ClmnAdd(tablename, "guild_done");
            _sql.ClmnAdd(tablename, "guild_undone");

            done = _sql.Get("guild_done", tablename).Split('\r').Select(s => s.Trim()).ToList();
            undone = _sql.Get("guild_undone", tablename).Split('\r').Select(s => s.Trim()).ToList();

            foreach (HtmlElement role in roles)
            {
                string name = role.FindChildByAttribute("div", "class", "flex\\ items-center\\ gap-3\\ p-5", "regexp", 0).InnerText.Trim();
                string state = role.FindChildByAttribute("div", "class", "mb-4\\ flex\\ items-center\\ justify-between\\ p-5\\ pb-0\\ transition-transform\\ md:mb-6", "regexp", 0).InnerText.Trim();
                string total = name.Split('\n')[1].Trim();
                name = name.Split('\n')[0].Trim();
                state = state.Split('\n')[1].Trim();

                if (state.Contains("No access") || state.Contains("Join Guild"))
                {
                    string undoneT = "";
                    var tasksHe = role.FindChildByAttribute("div", "class", "flex\\ flex-col\\ p-5\\ pt-0", "regexp", 0);
                    var tasks = tasksHe.FindChildrenByAttribute("div", "class", "w-full\\ transition-all\\ duration-200\\ translate-y-0\\ opacity-100", "regexp").ToList();
                    foreach (HtmlElement task in tasks)
                    {
                        string taskText = task.InnerText.Split('\n')[0].Trim();
                        if (task.InnerHtml.Contains("M208.49,191.51a12,12,0,0,1-17,17L128,145,64.49,208.49a12,12,0,0,1-17-17L111,128,47.51,64.49a12,12,0,0,1,17-17L128,111l63.51-63.52a12,12,0,0,1,17,17L145,128Z"))
                        {
                            _logger.Send($"[!Undone]: {taskText}");
                            undoneT += taskText.Trim() + ", ";
                        }
                    }
                    undoneT = undoneT.Trim().Trim(',');
                    undone.Add($"[{name}]: {undoneT}".Trim());
                }
                else if (state.Contains("You have access"))
                {
                    done.Add($"[{name}] ({total})".Trim());
                }
                else if (state.Contains("Reconnect"))
                {
                    undone.Add($"[{name}]: reconnect".Trim());
                }
                done = done.Distinct().Select(s => s.Trim()).ToList();
                undone = undone.Distinct().Select(s => s.Trim()).ToList();
                string wDone = string.Join("\r", done).Trim().Replace("'","");
                string wUndone = string.Join("\r", undone).Trim().Replace("'", "");
                _project.Var("guildUndone", wUndone);
                _sql.Upd($"guild_done = '{wDone}', guild_undone = '{wUndone}'", tablename);
            }

        }

        public string Svg(string d)
        {
            string discord = "M108,136a16,16,0,1,1-16-16A16,16,0,0,1,108,136Zm56-16a16,16,0,1,0,16,16A16,16,0,0,0,164,120Zm76.07,76.56-67,29.71A20.15,20.15,0,0,1,146,214.9l-8.54-23.13c-3.13.14-6.27.24-9.45.24s-6.32-.1-9.45-.24L110,214.9a20.19,20.19,0,0,1-27.08,11.37l-67-29.71A19.93,19.93,0,0,1,4.62,173.41L34.15,57A20,20,0,0,1,50.37,42.19l36.06-5.93A20.26,20.26,0,0,1,109.22,51.1l4.41,17.41c4.74-.33,9.52-.51,14.37-.51s9.63.18,14.37.51l4.41-17.41a20.25,20.25,0,0,1,22.79-14.84l36.06,5.93A20,20,0,0,1,221.85,57l29.53,116.38A19.93,19.93,0,0,1,240.07,196.56ZM227.28,176,199.23,65.46l-30.07-4.94-2.84,11.17c2.9.58,5.78,1.2,8.61,1.92a12,12,0,1,1-5.86,23.27A168.43,168.43,0,0,0,128,92a168.43,168.43,0,0,0-41.07,4.88,12,12,0,0,1-5.86-23.27c2.83-.72,5.71-1.34,8.61-1.92L86.85,60.52,56.77,65.46,28.72,176l60.22,26.7,5-13.57c-4.37-.76-8.67-1.65-12.88-2.71a12,12,0,0,1,5.86-23.28A168.43,168.43,0,0,0,128,168a168.43,168.43,0,0,0,41.07-4.88,12,12,0,0,1,5.86,23.28c-4.21,1.06-8.51,1.95-12.88,2.71l5,13.57Z";



            string twitter = "M697.286 531.413 1042.75 130h-81.86L660.919 478.542 421.334 130H145l362.3 527.057L145 1078h81.87l316.776-368.072L796.666 1078H1073L697.266 531.413h.02ZM585.154 661.7l-36.708-52.483-292.077-417.612h125.747l235.709 337.026 36.709 52.483L960.928 1019.2H835.181L585.154 661.72v-.02Z";


            string github = "M212.62,75.17A63.7,63.7,0,0,0,206.39,26,12,12,0,0,0,196,20a63.71,63.71,0,0,0-50,24H126A63.71,63.71,0,0,0,76,20a12,12,0,0,0-10.39,6,63.7,63.7,0,0,0-6.23,49.17A61.5,61.5,0,0,0,52,104v8a60.1,60.1,0,0,0,45.76,58.28A43.66,43.66,0,0,0,92,192v4H76a20,20,0,0,1-20-20,44.05,44.05,0,0,0-44-44,12,12,0,0,0,0,24,20,20,0,0,1,20,20,44.05,44.05,0,0,0,44,44H92v12a12,12,0,0,0,24,0V192a20,20,0,0,1,40,0v40a12,12,0,0,0,24,0V192a43.66,43.66,0,0,0-5.76-21.72A60.1,60.1,0,0,0,220,112v-8A61.5,61.5,0,0,0,212.62,75.17ZM196,112a36,36,0,0,1-36,36H112a36,36,0,0,1-36-36v-8a37.87,37.87,0,0,1,6.13-20.12,11.65,11.65,0,0,0,1.58-11.49,39.9,39.9,0,0,1-.4-27.72,39.87,39.87,0,0,1,26.41,17.8A12,12,0,0,0,119.82,68h32.35a12,12,0,0,0,10.11-5.53,39.84,39.84,0,0,1,26.41-17.8,39.9,39.9,0,0,1-.4,27.72,12,12,0,0,0,1.61,11.53A37.85,37.85,0,0,1,196,104Z";


            string disconnect = "M195.8,60.2a28,28,0,0,0-39.51-.09L144.68,72.28a12,12,0,1,1-17.36-16.56L139,43.43l.2-.2a52,52,0,0,1,73.54,73.54l-.2.2-12.29,11.71a12,12,0,0,1-16.56-17.36l12.17-11.61A28,28,0,0,0,195.8,60.2ZM111.32,183.72,99.71,195.89a28,28,0,0,1-39.6-39.6l12.17-11.61a12,12,0,0,0-16.56-17.36L43.43,139l-.2.2a52,52,0,0,0,73.54,73.54l.2-.2,11.71-12.29a12,12,0,1,0-17.36-16.56ZM216,148H192a12,12,0,0,0,0,24h24a12,12,0,0,0,0-24ZM40,108H64a12,12,0,0,0,0-24H40a12,12,0,0,0,0,24Zm120,72a12,12,0,0,0-12,12v24a12,12,0,0,0,24,0V192A12,12,0,0,0,160,180ZM96,76a12,12,0,0,0,12-12V40a12,12,0,0,0-24,0V64A12,12,0,0,0,96,76Z";


            string google = "M228,128a100,100,0,1,1-22.86-63.64,12,12,0,0,1-18.51,15.28A76,76,0,1,0,203.05,140H128a12,12,0,0,1,0-24h88A12,12,0,0,1,228,128Z";


            string email = "M224,44H32A12,12,0,0,0,20,56V192a20,20,0,0,0,20,20H216a20,20,0,0,0,20-20V56A12,12,0,0,0,224,44ZM193.15,68,128,127.72,62.85,68ZM44,188V83.28l75.89,69.57a12,12,0,0,0,16.22,0L212,83.28V188Z";


            string telegram = "M231.49,23.16a13,13,0,0,0-13.23-2.26L15.6,100.21a18.22,18.22,0,0,0,3.12,34.86L68,144.74V200a20,20,0,0,0,34.4,13.88l22.67-23.51L162.35,223a20,20,0,0,0,32.7-10.54L235.67,35.91A13,13,0,0,0,231.49,23.16ZM139.41,77.52,77.22,122.09l-34.43-6.75ZM92,190.06V161.35l15,13.15Zm81.16,10.52L99.28,135.81,205.59,59.63Z";


            string farcaster = "M257.778 155.556h484.444v688.889h-71.111V528.889h-.697c-7.86-87.212-81.156-155.556-170.414-155.556-89.258 0-162.554 68.344-170.414 155.556h-.697v315.556h-71.111V155.556z";


            string world = "M491.846 156.358c-12.908-30.504-31.357-57.843-54.859-81.345-23.502-23.503-50.902-41.951-81.345-54.86C324.041 6.759 290.553 0 255.97 0c-34.523 0-68.072 6.758-99.673 20.154-30.504 12.908-57.843 31.357-81.345 54.859-23.502 23.502-41.951 50.902-54.86 81.345C6.759 187.898 0 221.447 0 255.97c0 34.523 6.758 68.071 20.154 99.672 12.908 30.504 31.357 57.843 54.859 81.345 23.502 23.502 50.902 41.951 81.345 54.859C187.959 505.181 221.447 512 256.03 512c34.523 0 68.072-6.758 99.673-20.154 30.504-12.908 57.842-31.357 81.345-54.859 23.502-23.502 41.951-50.902 54.859-81.345 13.335-31.601 20.154-65.089 20.154-99.672-.061-34.523-6.88-68.072-20.215-99.612Zm-320.875 75.561c10.655-40.916 47.918-71.177 92.183-71.177h177.73c11.447 22.102 18.753 46.153 21.615 71.177H170.971Zm291.528 48.101a206.5 206.5 0 0 1-21.615 71.177h-177.73c-44.204 0-81.467-30.261-92.183-71.177h291.528ZM108.988 108.988C148.26 69.716 200.44 48.101 255.97 48.101c55.529 0 107.709 21.615 146.981 60.887a196.255 196.255 0 0 1 3.532 3.653H263.154c-38.298 0-74.282 14.918-101.377 42.012-21.31 21.311-35.071 48.162-40.003 77.327H49.501c5.297-46.457 25.938-89.443 59.487-122.992ZM255.97 463.899c-55.53 0-107.71-21.615-146.982-60.887-33.549-33.549-54.19-76.535-59.487-122.931h72.273c4.871 29.165 18.693 56.016 40.003 77.327 27.095 27.094 63.079 42.012 101.377 42.012h143.389c-1.156 1.217-2.374 2.435-3.531 3.653-39.272 39.15-91.513 60.826-147.042 60.826Z";

            if (d.Contains(discord)) return "discord";
            if (d.Contains(twitter)) return "twitter";
            if (d.Contains(github)) return "github";
            if (d.Contains(email)) return "email";
            if (d.Contains(telegram)) return "telegram";
            if (d.Contains(farcaster)) return "farcaster";
            if (d.Contains(world)) return "world";
            if (d.Contains(google)) return "google";

            return string.Empty;
        }

        public string Svg(HtmlElement he)
        {
            string d = he.InnerHtml;

            return Svg(d);

        }

        public Dictionary<string, string> ParseConnections ()
            {
            var dataHe = _instance.GetHe(("div", "class", "flex\\ flex-col\\ gap-3\\ rounded-xl\\ border\\ bg-card-secondary\\ px-4\\ py-3.5\\ mb-6", "regexp", 0)).GetChildren(false);
            var dataDic = new Dictionary<string, string>();
            foreach (HtmlElement he in dataHe)
            {
	
	            string type = Svg(he);
	            if (type != "") {
		            if (he.InnerText.Contains("Connect"))
			            dataDic.Add(type,"none");
			
		            else 
			            dataDic.Add(type, he.InnerText);
	            }

            }
            return dataDic;
        }

        public HtmlElement MainButton()

        {
            return _instance.GetHe(("button", "class", "font-semibold\\ inline-flex\\ items-center\\ justify-center\\ whitespace-nowrap\\ transition-colors\\ focus-visible:outline-none\\ focus-visible:ring-4\\ focus:ring-ring\\ disabled:pointer-events-none\\ disabled:opacity-50\\ text-base\\ text-ellipsis\\ overflow-hidden\\ gap-1.5\\ cursor-pointer\\ \\[&_svg]:shrink-0\\ bg-\\[hsl\\(var\\(--button-bg-subtle\\)/0\\)]\\ hover:bg-\\[hsl\\(var\\(--button-bg-subtle\\)/0.12\\)]\\ active:bg-\\[hsl\\(var\\(--button-bg-subtle\\)/0.24\\)]\\ text-\\[hsl\\(var\\(--button-foreground-subtle\\)\\)]\\ \\[--button-bg:var\\(--secondary\\)]\\ \\[--button-bg-hover:var\\(--secondary-hover\\)]\\ \\[--button-bg-active:var\\(--secondary-active\\)]\\ \\[--button-foreground:var\\(--secondary-foreground\\)]\\ \\[--button-bg-subtle:var\\(--secondary-subtle\\)]\\ \\[--button-foreground-subtle:var\\(--secondary-subtle-foreground\\)]\\ h-11\\ px-4\\ py-2\\ rounded-2xl", "regexp", 0));

        }

    }

    public class X2
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;

        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _token;
        protected string _login;
        protected string _pass;
        protected string _2fa;
        protected string _email;
        protected string _email_pass;

        public X2(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "X");

            LoadCreds();

        }
        public void LoadCreds()
        {
            string[] creds = _sql.Get(" status, token, login, password, otpsecret, email, emailpass", "private_twitter").Split('|');
            try { _status = creds[0].Trim(); _project.Variables["twitterSTATUS"].Value = _status; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _token = creds[1].Trim(); _project.Variables["twitterTOKEN"].Value = _token; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _login = creds[2].Trim(); _project.Variables["twitterLOGIN"].Value = _login; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _pass = creds[3].Trim(); _project.Variables["twitterPASSWORD"].Value = _pass; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _2fa = creds[4].Trim(); _project.Variables["twitterCODE2FA"].Value = _2fa; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _email = creds[5].Trim(); _project.Variables["twitterEMAIL"].Value = _email; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _email_pass = creds[6].Trim(); _project.Variables["twitterEMAIL_PASSWORD"].Value = _email_pass; } catch (Exception ex) { _logger.Send(ex.Message); }

            if (string.IsNullOrEmpty(_login) || string.IsNullOrEmpty(_pass))
                throw new Exception($"invalid credentials login:[{_login}] pass:[{_pass}]");
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
        public void XsetToken()
        {
            var token = _project.Variables["twitterTOKEN"].Value;
            string jsCode = _project.ExecuteMacro($"document.cookie = \"auth_token={token}; domain=.x.com; path=/; expires=${DateTimeOffset.UtcNow.AddYears(1).ToString("R")}; Secure\";\r\nwindow.location.replace(\"https://x.com\")");
            _instance.ActiveTab.MainDocument.EvaluateScript(jsCode);
        }
        private string XgetToken()
        {
            //var cookJson = _instance.GetCookies(_project,".");
            var cookJson = new Cookies(_project, _instance).Get(".");//_instance.GetCookies(_project, ".");

            JArray toParse = JArray.Parse(cookJson);
            int i = 0; var token = "";
            while (token == "")
            {
                if (toParse[i]["name"].ToString() == "auth_token") token = toParse[i]["value"].ToString();
                i++;
            }
            _project.Variables["twitterTOKEN"].Value = token;
            _sql.Upd($"token = '{token}'", "private_twitter");
            return token;
        }
        public string Xlogin()
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
            try { _project.Var("twitterSTATUS", status); } catch (Exception ex) { }
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
            else if (status == "mixed")
            {
                _instance.CloseAllTabs();
                _instance.ClearCookie("x.com");
                _instance.ClearCache("x.com");
                _instance.ClearCookie("twitter.com");
                _instance.ClearCache("twitter.com");
                goto check;

            }
            if (status == "restricted" || status == "suspended" || status == "emailCapcha")
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
                    _logger.Send($"unknown state [{state}]");
                    break;

            }

            if (!_instance.ActiveTab.URL.Contains("x.com") && !_instance.ActiveTab.URL.Contains("twitter.com"))
                _project.L0g("auth done");
            else goto check;
        }



        public string Load(bool log = false)
        {
            bool tokenUsed = false;
            DateTime deadline = DateTime.Now.AddSeconds(60);
        check:

            if (DateTime.Now > deadline) throw new Exception("timeout");

            var status = XcheckState(log: true);
            try { _project.Var("twitterSTATUS", status); } catch (Exception ex) { }
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
            else if (status == "mixed")
            {
                _instance.CloseAllTabs();
                _instance.ClearCookie("x.com");
                _instance.ClearCache("x.com");
                _instance.ClearCookie("twitter.com");
                _instance.ClearCache("twitter.com");
                goto check;

            }
            if (status == "restricted" || status == "suspended" || status == "emailCapcha")
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
        public void Auth()
        {
            _project.Deadline();
            _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 0, thr0w: false);
            _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);



        check:
            _project.Deadline(60);
            string state = State();
             
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
                case "AuthV1SignIn":
                    _instance.HeClick(("allow", "id"));
                    goto check;
                case "InvalidRequestToken":
                    _instance.CloseExtraTabs();
                    throw new Exception(state);
                case "AuthV1Confirm":
                    _instance.HeClick(("allow", "id"));
                    goto check;
                case "!WrongAccount":
                    _instance.ClearShit("x.com");
                    _instance.ClearShit("twitter.com");
                    throw new Exception(state);
                default:
                    _logger.Send($"unknown state [{state}]");
                    break;

            }

            if (!_instance.ActiveTab.URL.Contains("x.com") && !_instance.ActiveTab.URL.Contains("twitter.com"))
                _project.L0g("auth done");
            else goto check;
        }
        public string State()
        {
            string state = "undefined";

            if (_instance.ActiveTab.URL.Contains("oauth/authorize")) 
            {
                if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'The request token for this page is invalid')]", 0).IsVoid) 
                    return "InvalidRequestToken";
                if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'This account is suspended')]", 0).IsVoid)
                    return "Suspended";
                if (!_instance.ActiveTab.FindElementById("session").IsVoid)
                {
                    var currentAcc = _instance.HeGet(("session", "id"));
                    if (currentAcc.ToLower() == _login.ToLower())
                        state = "AuthV1Confirm";
                    else
                    {
                        state = "!WrongAccount";
                        _logger.Send($"{state}: detected:[{currentAcc}] expected:[{_login}]");
                    }
                         
                }

                else if (!_instance.ActiveTab.FindElementById("allow").IsVoid)
                {
                    if (_instance.HeGet(("allow", "id"), atr: "value") == "Sign In")
                        state = "AuthV1SignIn";
                }
                return state;
            }

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
            return state;
        }

        public void UpdXCreds(Dictionary<string, string> data)
        {
            if (data.ContainsKey("CODE2FA"))
            {
                _project.L0g($"CODE2FA raw value: {data["CODE2FA"]}"); // Логируем исходное значение
            }

            var fields = new Dictionary<string, string>
            {
                { "LOGIN", data.ContainsKey("LOGIN") ? data["LOGIN"].Replace("'", "''") : "" },
                { "PASSWORD", data.ContainsKey("PASSWORD") ? data["PASSWORD"].Replace("'", "''") : "" },
                { "EMAIL", data.ContainsKey("EMAIL") ? data["EMAIL"].Replace("'", "''") : "" },
                { "EMAIL_PASSWORD", data.ContainsKey("EMAIL_PASSWORD") ? data["EMAIL_PASSWORD"].Replace("'", "''") : "" },
                { "TOKEN", data.ContainsKey("TOKEN") ? (data["TOKEN"].Contains('=') ? data["TOKEN"].Split('=').Last().Replace("'", "''") : data["TOKEN"].Replace("'", "''")) : "" },
                { "CODE2FA", data.ContainsKey("CODE2FA") ? (data["CODE2FA"].Contains('/') ? data["CODE2FA"].Split('/').Last().Replace("'", "''") : data["CODE2FA"].Replace("'", "''")) : "" },
                { "RECOVERY_SEED", data.ContainsKey("RECOVERY_SEED") ? data["RECOVERY_SEED"].Replace("'", "''") : "" }
            };

            var _sql = new Sql(_project, _logShow);
            try
            {
                _sql.Upd($@"token = '{fields["TOKEN"]}', 
                login = '{fields["LOGIN"]}', 
                password = '{fields["PASSWORD"]}', 
                otpsecret = '{fields["CODE2FA"]}', 
                email = '{fields["EMAIL"]}', 
                emailpass = '{fields["EMAIL_PASSWORD"]}', 
                otpbackup = '{fields["RECOVERY_SEED"]}'", "private_twitter");
            }
            catch (Exception ex)
            {
                _project.L0g("!W{ex.Message}");
            }
            LoadCreds();
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

            _sql.Upd($@"datecreated = '{dateCreated}',
                        id = '{id}',
                        username = '{username}',
                        description = '{description}',
                        givenname = '{givenName}',
                        homelocation = '{homeLocation}',
                        ava = '{ava}',
                        banner = '{banner}',
                        followers = '{Followers}',
                        following = '{Following}',
                        tweets = '{Tweets}',
                        ");


            try
            {
                var toFill = _project.Lists["editProfile"];
                toFill.Clear();

                if (description == "") toFill.Add("description");
                if (homeLocation == "") toFill.Add("homeLocation");
                if (ava == "https://abs.twimg.com/sticky/default_profile_images/default_profile_400x400.png") toFill.Add("ava");
                if (banner == "https://abs.twimg.com/sticky/default_profile_images/default_profile_normal.png") toFill.Add("banner");

            }
            catch { }

        }
        public void ParseSecurity()
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


            try
            {
                email = email.ToLower();
                var emails = _sql.Get("gmail, icloud, firstmail", "public_mail").ToLower();
                var address = _sql.Address("evm_pk").ToLower();
                var toFill = _project.Lists["editSecurity"];
                toFill.Clear();

                if (!emails.Contains(email) || !email.Contains(address)) toFill.Add("email");

            }
            catch { }
        }
    }


    

    public class G2
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
        protected bool _cookRestored = false;

        public G2(IZennoPosterProjectModel project, Instance instance, bool log = false)
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
        }
        public string Load(bool log = false, bool cookieBackup = true)
        {
            if (!_instance.ActiveTab.URL.Contains("google")) _instance.Go("https://myaccount.google.com/");
            check:
            Thread.Sleep(1000);
            string state = State();

            _project.Var("googleSTATUS", state);

            _logger.Send(state);
            switch (state)
            {
                case "ok":
                    if (cookieBackup) SaveCookies();
                    return state;

                case "!WrongAcc":

                    _instance.CloseAllTabs();
                    _instance.ClearCookie("google.com");
                    _instance.ClearCookie("google.com");
                    throw new Exception(state);

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
                    _instance.HeClick(("button", "innertext", "Skip", "regexp", 0),dadline:5, thr0w:false);
                    goto check;

                case "setHome":
                    _instance.HeClick(("button", "innertext", "Skip", "regexp", 0));
                    goto check;

                case "signInAgain":
                    _instance.ClearShit("google.com");
                        Thread.Sleep(3000);
                    _instance.Go("https://myaccount.google.com/");
                    goto check;

                case "CAPTCHA":

                    throw new Exception("gCAPTCHA");
                    try { _project.CapGuru(); } catch { }
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;
                case "phoneVerify":
                case "badBrowser":
                    _sql.Upd($"status = '{state}'", "projects_google");
                    _sql.Upd($"status = '{state}'", "private_google");

                    throw new Exception(state);

                default:
                    return state;

            }

        }
        public string State(bool log = false)
        {
        check:
            var state = "";

            if (!_instance.ActiveTab.FindElementByAttribute("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0).IsVoid)
                state = "signedIn";

            else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Confirm you’re not a robot')]", 0).IsVoid)
                state = "CAPTCHA";

            else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Enter\\ a\\ phone\\ number\\ to\\ get\\ a\\ text\\ message\\ with\\ a\\ verification\\ code.", "regexp", 0).IsVoid)
                state = "PhoneVerify";

            else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Try\\ using\\ a\\ different\\ browser.", "regexp", 0).IsVoid)
                state = "BadBrowser";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:email", "fulltagname", "input:email", "text", 0).IsVoid) &&
                    (_instance.ActiveTab.FindElementByAttribute("input:email", "fulltagname", "input:email", "text", 0).GetAttribute("value") == ""))
                state = "inputLogin";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).IsVoid) &&
                    _instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).GetAttribute("value") == "")
                state = "inputPassword";

            else if ((!_instance.ActiveTab.FindElementById("totpPin").IsVoid) &&
                    _instance.ActiveTab.FindElementById("totpPin").GetAttribute("value") == "")
                state = "inputOtp";

            else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Add a recovery phone')]", 0).IsVoid)
                state = "addRecoveryPhone";

            else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Set a home address')]", 0).IsVoid)
                state = "setHome";

            else if(!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Your account has been disabled')]", 0).IsVoid)
                state = "Disabled";

            else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Google needs to verify it’s you. Please sign in again to continue')]", 0).IsVoid)
                state = "signInAgain";

            else state = "undefined";

           //_logger.Send(state);
            
            switch (state)
            {
                case "signedIn":
                    var currentAcc = _instance.HeGet(("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0), atr: "aria-label").Split('\n')[1];
                    if (currentAcc.ToLower().Contains(_login.ToLower()))
                    {
                        _logger.Send($"{currentAcc} is Correct. Login done");
                        state = "ok";
                    }

                    else
                    {
                        _logger.Send($"!W {currentAcc} is InCorrect. MustBe {_login}");
                        state = "!WrongAcc";
                    }
                    break;

                case "undefined":
                    _instance.HeClick(("a", "class", "h-c-header__cta-li-link\\ h-c-header__cta-li-link--primary\\ button-standard-mobile", "regexp", 1), deadline: 1, thr0w: false);
                    goto check;

                default:
                    break;

            }
            _project.Var("googleSTATUS", state);
            return state;
        }
        public string Auth(bool log = false)
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
                Continue:
                    try
                    {
                        _instance.HeClick(("button", "innertext", "Continue", "regexp", 0), deadline: 5, delay: 1);
                        return "SUCCESS with continue";
                    }
                    catch
                    {
                        try
                        {
                            _instance.HeSet(("totpPin", "id"), OTP.Offline(_2fa), deadline: 1);
                            _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                            goto Continue;
                        }
                        catch (Exception ex) { }
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
            if (_instance.ActiveTab.IsBusy) _instance.ActiveTab.WaitDownloading();
            //Thread.Sleep(5000);
            _instance.Go("https://myaccount.google.com/");
            string gCookies = new Cookies(_project, _instance).Get(".");
            _sql.Upd($"status = 'ok', cookies = '{gCookies}'", "private_google");
            _sql.Upd($"status = 'ok', cookies = '{gCookies}'", "projects_google");
        }
        public void ParseSecurity()
        {
            if (!_instance.ActiveTab.URL.Contains("https://myaccount.google.com/security"))
                _instance.HeClick(("a", "href", "https://myaccount.google.com/security", "regexp", 1));

            var status2fa = _instance.HeGet(("a", "aria-label", "2-Step Verification", "text", 0), "last").Split('\n');
            var statusPassword = _instance.HeGet(("a", "aria-label", "Password", "text", 0), "last").Split('\n');
            var statusSkipPassword = _instance.HeGet(("a", "aria-label", "Skip password when possible", "text", 0), "last").Split('\n');
            var statusAuthenticator = _instance.HeGet(("a", "aria-label", "Authenticator", "text", 0), "last").Split('\n');
            var statusRecoveryPhone = _instance.HeGet(("a", "aria-label", "Recovery phone", "text", 0), "last").Split('\n');
            var statusRecoveryEmail = _instance.HeGet(("a", "aria-label", "Recovery email", "text", 0), "last").Split('\n');
            var statusBackupCodes = _instance.HeGet(("a", "aria-label", "Backup codes", "text", 0), "last").Split('\n');

            string todb = $@"{status2fa[0]} [{status2fa[1]}]
	            {statusPassword[0]} [{statusPassword[1]}]
	            {statusSkipPassword[0]} [{statusSkipPassword[1]}]   
	            {statusAuthenticator[0]} [{statusAuthenticator[1]}]
	            {statusRecoveryPhone[0]} [{statusRecoveryPhone[1]}]
	            {statusRecoveryEmail[0]} [{statusRecoveryEmail[1]}]
	            {statusBackupCodes[0]} [{statusBackupCodes[1]}]";


            new Sql(_project, true).Upd($"security = '{todb}'", "_projects_google");

        }

    }

    public class AI2
    {
        protected readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;
        private protected string _apiKey;
        private protected string _url;
        private protected string _model;

        public AI2(IZennoPosterProjectModel project, string provider, string model, bool log = false)
        {
            _project = project;
            _logger = new Logger(project, log: log, classEmoji: "AI");
            _apiKey = new Sql(_project).Get("apikey", "private_api", where: $"key = '{provider}'");
            SetProvider(provider);
            _model = model;
        }

        private void SetProvider(string provider)
        {
            _apiKey = new Sql(_project).Get("apikey", "private_api", where: $"key = '{provider}'");

            switch (provider)
            {
                case "perplexity":
                    _url = "https://api.perplexity.ai/chat/completions";
                    break;
                case "aiio":
                    _url = "https://api.intelligence.io.solutions/api/v1/chat/completions";
                    break;
                default:
                    throw new Exception($"unknown provider {provider}");
            }
        }

        public string Query(string systemContent, string userContent, bool log = false)
        {
            var requestBody = new
            {
                model = _model, // {Qwen/Qwen2.5-Coder-32B-Instruct|deepseek-ai/DeepSeek-R1|deepseek-ai/DeepSeek-R1-0528|databricks/dbrx-instruct|mistralai/Mistral-Large-Instruct-2411|meta-llama/Llama-3.3-70B-Instruct|Qwen/Qwen3-235B-A22B-FP8|Qwen/QwQ-32B|deepseek-ai/DeepSeek-R1-Distill-Qwen-32B|google/gemma-3-27b-it}
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = systemContent
                    },
                    new
                    {
                        role = "user",
                        content = userContent
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
                $"Authorization: Bearer {_apiKey}"
            };

            string response = _project.POST(_url, jsonBody, "", headers, log);
            _logger.Send($"Full response: {response}");

            try
            {
                var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(response);
                string Text = jsonResponse["choices"][0]["message"]["content"].ToString();
                _logger.Send(Text);
                return Text;
            }
            catch (Exception ex)
            {
                _logger.Send($"!W Error parsing response: {ex.Message}");
                throw;
            }
        }

        public string GenerateTweet(string content, string bio = "", bool log = false)
        {
            string systemContent = string.IsNullOrEmpty(bio)
                            ? "You are a social media account. Generate tweets that reflect a generic social media persona."
                            : $"You are a social media account with the bio: '{bio}'. Generate tweets that reflect this persona, incorporating themes relevant to bio.";

        gen:
            string tweetText = Query(systemContent, content);
            if (tweetText.Length > 220)
            {
                _logger.Send($"tweet is over 220sym `y");
                goto gen;
            }
            return tweetText;

        }

        public string OptimizeCode(string content, bool log = false)
        {
            string systemContent = "You are a web3 developer. Optimize the following code. Return only the optimized code. Do not add explanations, comments, or formatting. Output code only, in plain text.";
            return Query(systemContent, content);

        }

        public string GoogleAppeal(bool log = false)
        {
            string content = "Generate short brief appeal messge (200 symbols) explaining reasons only for google support explainig situation, return only text of generated message";
            string systemContent = "You are a bit stupid man - user, and sometimes you making mistakes in grammar. Also You are a man \"not realy in IT\". Your account was banned by google. You don't understand why it was happend. 100% you did not wanted to violate any rules even if it happened, but you suppose it was google antifraud mistake";
            return Query(systemContent, content);

        }
    }

}
