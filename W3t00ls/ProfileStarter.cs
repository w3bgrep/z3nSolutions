using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using W3t00ls;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using static ZennoLab.CommandCenter.ZennoPoster;

using System.IO;


using static W3t00ls.Requests;
using System.Text.RegularExpressions;
using static Global.Env.EnvironmentVariables;
using System.Diagnostics;
using System.Collections;

namespace W3t00ls
{
    public class Starter
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;
        protected readonly bool _skipCheck;
        public Starter(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project);
            _logShow = log;
            _instance = instance;
            _skipCheck = project.Variables["skipBrowserScan"].Value == "True";
        }
        public Starter(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logShow = log;
            _sql = new Sql(_project);
            _skipCheck = project.Variables["skipBrowserScan"].Value == "True";
        }

        public void StarterLog(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;

            var stackFrame = new StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ ▶  {callerName}] [{tolog}] ");
        }


        public void StartBrowser()
        {
            StarterLog("initProfile");
            _project.Variables["instancePort"].Value = _instance.Port.ToString();
            
            var webGlData = _sql.Get("webGL", "profile");
            _instance.SetDisplay(webGlData, _project);

            var proxy = _sql.Proxy();
            _instance.SetProxy(proxy, _project);

            var cookiePath = $"{_project.Variables["settingsZenFolder"].Value}accounts\\cookies\\{_project.Variables["acc0"].Value}.json";
            _project.Variables["pathCookies"].Value = cookiePath;

            try
            {
                var cookies = File.ReadAllText(cookiePath);
                _instance.SetCookie(cookies);
            }
            catch 
            {
                StarterLog($"!W Fail to set cookies from file {cookiePath}");
                try
                {
                    var cookies = _sql.Get("cookies", "profile");
                    _instance.SetCookie(cookies);
                }
                catch (Exception Ex)
                    {
                    StarterLog($"!E Fail to set cookies from db Err. {Ex.Message}");

                }

            }
            if (!_skipCheck)
            {
                var tableName = "browser";

                var tableStructure = new Dictionary<string, string>
                {
                    {"acc0", "INTEGER PRIMARY KEY"},
                    {"score", "TEXT DEFAULT '_'"},
                    {"WebGL", "TEXT DEFAULT '_'"},
                    {"WebGLReport", "INTEGER DEFAULT 0"},
                    {"UnmaskedRenderer", "TEXT DEFAULT '_'"},
                    {"Audio", "TEXT DEFAULT '_'"},
                    {"ClientRects", "TEXT DEFAULT '_'"},
                    {"WebGPUReport", "TEXT DEFAULT '_'"},
                    {"Fonts", "TEXT DEFAULT '_'"},
                    {"TimeZoneBasedonIP", "TEXT DEFAULT '_'"},
                    {"TimeFromIP", "TEXT DEFAULT '_'"},

                };

                if (_project.Variables["DBmode"].Value == "PostgreSQL")
                    tableName = $"accounts.{tableName}";
                if (_project.Variables["makeTable"].Value == "True")
                    _sql.MkTable(tableStructure, tableName, log: _logShow);

                BrowserScanCheck();

            } 

        }
        private void BrowserScanCheck()
        {
            bool set = false;
            string timezoneOffset = "";
            string timezoneName = "";
            var tableName = "browser";

            while (true)
            {
                _instance.ActiveTab.Navigate("https://www.browserscan.net/", "");
                var toParse = "WebGL,WebGLReport, Audio, ClientRects, WebGPUReport,Fonts,TimeZoneBasedonIP,TimeFromIP";
                Thread.Sleep(5000);
                var hardware = _instance.ActiveTab.FindElementById("webGL_anchor").ParentElement.GetChildren(false);
                foreach (HtmlElement child in hardware)
                {
                    var text = child.GetAttribute("innertext");
                    var varName = Regex.Replace(text.Split('\n')[0], " ", ""); var varValue = "";
                    if (varName == "") continue;
                    if (toParse.Contains(varName))
                    {
                        try { varValue = text.Split('\n')[2]; } catch { Thread.Sleep(2000); continue; }
                        _sql.Upd($"{varName} = '{varValue}'", tableName);
                    }
                }

                var software = _instance.ActiveTab.FindElementById("lang_anchor").ParentElement.GetChildren(false);
                foreach (HtmlElement child in software)
                {
                    var text = child.GetAttribute("innertext");
                    var varName = Regex.Replace(text.Split('\n')[0], " ", ""); var varValue = "";
                    if (varName == "") continue;
                    if (toParse.Contains(varName))
                    {
                        if (varName == "TimeZone") continue;
                        try { varValue = text.Split('\n')[1]; } catch { continue; }
                        if (varName == "TimeFromIP") timezoneOffset = varValue;
                        if (varName == "TimeZoneBasedonIP") timezoneName = varValue;
                        _sql.Upd($"{varName} = '{varValue}'", tableName);
                    }
                }


                string heToWait = _instance.HeGet(("anchor_progress"), "id");

                var score = heToWait.Split(' ')[3].Split('\n')[0]; var problems = "";

                if (!score.Contains("100%"))
                {
                    var problemsHe = _instance.ActiveTab.FindElementByAttribute("ul", "fulltagname", "ul", "regexp", 5).GetChildren(false);
                    foreach (HtmlElement child in problemsHe)
                    {
                        var text = child.GetAttribute("innertext");
                        var varValue = "";
                        var varName = text.Split('\n')[0];
                        try { varValue = text.Split('\n')[1]; } catch { continue; };
                        problems += $"{varName}: {varValue}; ";
                    }
                    problems = problems.Trim();

                }

                score = $"[{score}] {problems}";
                _sql.Upd($"score = '{score}'", tableName);


                if (!score.Contains("100%") && !set)
                {
                    var match = Regex.Match(timezoneOffset, @"GMT([+-]\d{2})");
                    if (match.Success)
                    {
                        int Offset = int.Parse(match.Groups[1].Value);
                        _project.L0g($"Setting timezone offset to: {Offset}");

                        _instance.TimezoneWorkMode = ZennoLab.InterfacesLibrary.Enums.Browser.TimezoneMode.Emulate;
                        _instance.SetTimezone(Offset, 0);
                    }
                    _instance.SetIanaTimezone(timezoneName);
                    set = true;
                    _instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");
                    continue;
                }
                break;
            }


        }

        public void InitVariables(string author = "")
        {
            DisableLogs();
            _project.Variables["varSessionId"].Value = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();

            string projectName = _project.ExecuteMacro(_project.Name).Split('.')[0];
            _project.Variables["projectName"].Value = projectName;

            string[] vars = { "cfgPin", "DBsqltPath" };
            CheckVars(vars);

            string tablename;
            string schema = "projects.";
            if (_project.Variables["DBmode"].Value == "PostgreSQL") tablename = schema + projectName.ToLower();
            else tablename = "_" + projectName.ToLower();
            _project.Variables["projectTable"].Value = tablename;

            _project.SetRange(log:true);
            SAFU.Initialize(_project);
            Logo(author);

        }


        private void DisableLogs()
        {
            try
            {
                StringBuilder logBuilder = new StringBuilder();
                string basePath = @"C:\Program Files\ZennoLab";

                foreach (string langDir in Directory.GetDirectories(basePath))
                {
                    foreach (string programDir in Directory.GetDirectories(langDir))
                    {
                        foreach (string versionDir in Directory.GetDirectories(programDir))
                        {
                            string logsPath = Path.Combine(versionDir, "Progs", "Logs");
                            if (Directory.Exists(logsPath))
                            {
                                Directory.Delete(logsPath, true);
                                Process process = new Process();
                                process.StartInfo.FileName = "cmd.exe";
                                process.StartInfo.Arguments = $"/c mklink /d \"{logsPath}\" \"NUL\"";
                                process.StartInfo.UseShellExecute = false;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.RedirectStandardOutput = true;
                                process.StartInfo.RedirectStandardError = true;

                                logBuilder.AppendLine($"Attempting to create symlink: {process.StartInfo.Arguments}");

                                process.Start();
                                string output = process.StandardOutput.ReadToEnd();
                                string error = process.StandardError.ReadToEnd();
                                process.WaitForExit();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
        private void Logo(string author)
        {
            string name = _project.ExecuteMacro(_project.Name).Split('.')[0];
            if (author != "") author = $" script author: @{author}";
            string logo = $@"using w3tools;
            ┌by─┐					
            │    w3bgrep			
            └─→┘
                        ► init {name} ░▒▓█ {author}";
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

        public void ConfigFromDb()
        {
            string settings = _sql.Settings();
            foreach (string varData in settings.Split('\n'))
            {
                string varName = varData.Split('|')[0];
                string varValue = varData.Split('|')[1].Trim();
                try { _project.Variables[$"{varName}"].Value = varValue; }
                catch (Exception ex) { _project.L0g($"⚙  {ex.Message}"); }
            }
        }

        public void FilterAccList(List<string> dbQueries, bool log = false)
        {
            if (!string.IsNullOrEmpty(_project.Variables["acc0Forced"].Value))
            {
                _project.Lists["accs"].Clear();
                _project.Lists["accs"].Add(_project.Variables["acc0Forced"].Value);
                StarterLog($@"manual mode on with {_project.Variables["acc0Forced"].Value}");
                return;
            }

            var allAccounts = new HashSet<string>();
            foreach (var query in dbQueries)
            {
                try
                {
                    var accsByQuery = _sql.DbQ(query).Trim();
                    if (!string.IsNullOrWhiteSpace(accsByQuery))
                    {
                        var accounts = accsByQuery.Split('\n').Select(x => x.Trim().TrimStart(','));
                        allAccounts.UnionWith(accounts);
                    }
                }
                catch
                {

                    StarterLog($"{query}");
                }
            }

            if (allAccounts.Count == 0)
            {
                _project.Variables["noAccsToDo"].Value = "True";
                StarterLog($"♻ noAccountsAvailable by queries [{string.Join(" | ", dbQueries)}]");
                return;
            }
            StarterLog($"Initial availableAccounts: [{string.Join(", ", allAccounts)}]");

            if (!string.IsNullOrEmpty(_project.Variables["requiredSocial"].Value))
            {
                string[] demanded = _project.Variables["requiredSocial"].Value.Split(',');
                StarterLog($"Filtering by socials: [{string.Join(", ", demanded)}]");

                foreach (string social in demanded)
                {
                    string tableName = social.Trim().ToLower();
                    if (_project.Variables["DBmode"].Value == "PostgreSql") tableName = $"accounts.{tableName}";
                    var notOK = _sql.DbQ($"SELECT acc0 FROM {tableName} WHERE status NOT LIKE '%ok%'", log)
                        .Split('\n')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x));
                    allAccounts.ExceptWith(notOK);
                    StarterLog($"After {social} filter: [{string.Join("|", allAccounts)}]");
                }
            }
            _project.Lists["accs"].Clear();
            _project.Lists["accs"].AddRange(allAccounts);
            StarterLog($"final list [{string.Join("|", _project.Lists["accs"])}]");
        }

        public List<string> MkToDoQueries(string toDo = null, string defaultRange = null, string defaultDoFail = null)
        {
            var nowIso = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            if (string.IsNullOrEmpty(toDo)) toDo = _project.Variables["cfgToDo"].Value;

            string[] toDoItems = (toDo ?? "").Split(',');

            var allQueries = new List<string>();

            foreach (string taskId in toDoItems)
            {
                string trimmedTaskId = taskId.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedTaskId))
                {
                    string tableName = _project.Variables["projectTable"].Value;
                    string range = defaultRange ?? _project.Variables["range"].Value;
                    string doFail = defaultDoFail ?? _project.Variables["doFail"].Value;
                    string failCondition = (doFail != "True" ? "AND status NOT LIKE '%fail%'" : "");
                    string query = $@"SELECT acc0 FROM {tableName} WHERE acc0 in ({range}) {failCondition} AND status NOT LIKE '%skip%' 
                AND ({trimmedTaskId} < '{nowIso}' OR {trimmedTaskId} = '_')";
                    allQueries.Add(query);
                }
            }

            return allQueries;
        }
    }
}
