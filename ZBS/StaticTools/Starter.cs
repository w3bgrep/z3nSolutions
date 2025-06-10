using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;


namespace ZBSolutions
{
    public class Starter
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;
        protected readonly bool _skipCheck;

        public Starter(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project);
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _instance = instance;
            _skipCheck = project.Variables["skipBrowserScan"].Value == "True";
        }
        public Starter(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _sql = new Sql(_project);
            _skipCheck = project.Variables["skipBrowserScan"].Value == "True";
        }

        private void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;

            var stackFrame = new StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ ▶  {callerName}] [{tolog}] ");
        }
        public void StartBrowser(bool strictProxy = true)
        {

            _project.Variables["instancePort"].Value = _instance.Port.ToString();
            _logger.Send($"init browser in port: {_instance.Port}");
            string webGlData = _sql.Get("webgl", "private_profile");
            _instance.SetDisplay(webGlData, _project);

            string proxy = _sql.Get("proxy", "private_profile");
            bool goodProxy = new NetHttp(_project, true).CheckProxy(proxy);
            if (goodProxy)
                _instance.SetProxy(proxy, true, true, true, true);
            else
            {
                if (strictProxy) throw new Exception($"!E bad proxy {proxy}");
            }

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
            if (!_skipCheck)
            {
                BrowserScanCheck();
            }

        }
        private void BrowserScanCheck()
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







            bool set = false;
            string timezoneOffset = "";
            string timezoneName = "";

            while (true)
            {
                _instance.ActiveTab.Navigate("https://www.browserscan.net/", "");
                var toParse = "WebGL,WebGLReport, Audio, ClientRects, WebGPUReport,Fonts,TimeZoneBasedonIP,TimeFromIP";
                Thread.Sleep(5000);
                var hardware = _instance.ActiveTab.FindElementById("webGL_anchor").ParentElement.GetChildren(false);
                foreach (ZennoLab.CommandCenter.HtmlElement child in hardware)
                {
                    var text = child.GetAttribute("innertext");
                    var varName = Regex.Replace(text.Split('\n')[0], " ", ""); var varValue = "";
                    if (varName == "") continue;
                    if (toParse.Contains(varName))
                    {
                        try { varValue = text.Split('\n')[2]; } catch { Thread.Sleep(2000); continue; }
                        //_sql.Upd($"{varName} = '{varValue}'", tableName);
                    }
                }

                var software = _instance.ActiveTab.FindElementById("lang_anchor").ParentElement.GetChildren(false);
                foreach (ZennoLab.CommandCenter.HtmlElement child in software)
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


                string heToWait = _instance.HeGet(("anchor_progress", "id"));

                var score = heToWait.Split(' ')[3].Split('\n')[0]; var problems = "";

                if (!score.Contains("100%"))
                {
                    var problemsHe = _instance.ActiveTab.FindElementByAttribute("ul", "fulltagname", "ul", "regexp", 5).GetChildren(false);
                    foreach (ZennoLab.CommandCenter.HtmlElement child in problemsHe)
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
 
        
        
        //public void StartRandomBrowser(bool strictProxy = true)
        //{
        //    Log("initProfile");
        //    _project.Variables["instancePort"].Value = _instance.Port.ToString();

        //    string webGlData = _sql.Get("webgl", "private_profile");
        //    _instance.SetDisplay(webGlData, _project);

        //    string proxy = _sql.Get("proxy", "private_profile");
        //    bool goodProxy = new NetHttp(_project, true).CheckProxy(proxy);
        //    if (goodProxy)
        //        _instance.SetProxy(proxy, true, true, true, true);
        //    else
        //    {
        //        if (strictProxy) throw new Exception($"!E bad proxy {proxy}");
        //    }

        //    string cookiePath = $"{_project.Variables["profiles_folder"].Value}accounts\\cookies\\{_project.Variables["acc0"].Value}.json";
        //    _project.Variables["pathCookies"].Value = cookiePath;

        //    try
        //    {
        //        string cookies = File.ReadAllText(cookiePath);
        //        _instance.SetCookie(cookies);
        //    }
        //    catch
        //    {
        //        Log($"!W Fail to set cookies from file {cookiePath}");
        //        try
        //        {
        //            string cookies = _sql.Get("cookies", "private_profile");
        //            _instance.SetCookie(cookies);
        //        }
        //        catch (Exception Ex)
        //        {
        //            Log($"!E Fail to set cookies from db Err. {Ex.Message}");
        //        }

        //    }
        //    if (!_skipCheck)
        //    {
        //        BrowserScanCheck();
        //    }

        //}
        
        //private void DisableLogs()
        //{
        //    try
        //    {
        //        StringBuilder logBuilder = new StringBuilder();
        //        string basePath = @"C:\Program Files\ZennoLab";

        //        foreach (string langDir in Directory.GetDirectories(basePath))
        //        {
        //            foreach (string programDir in Directory.GetDirectories(langDir))
        //            {
        //                foreach (string versionDir in Directory.GetDirectories(programDir))
        //                {
        //                    string logsPath = Path.Combine(versionDir, "Progs", "Logs");
        //                    if (Directory.Exists(logsPath))
        //                    {
        //                        Directory.Delete(logsPath, true);
        //                        Process process = new Process();
        //                        process.StartInfo.FileName = "cmd.exe";
        //                        process.StartInfo.Arguments = $"/c mklink /d \"{logsPath}\" \"NUL\"";
        //                        process.StartInfo.UseShellExecute = false;
        //                        process.StartInfo.CreateNoWindow = true;
        //                        process.StartInfo.RedirectStandardOutput = true;
        //                        process.StartInfo.RedirectStandardError = true;

        //                        logBuilder.AppendLine($"Attempting to create symlink: {process.StartInfo.Arguments}");

        //                        process.Start();
        //                        string output = process.StandardOutput.ReadToEnd();
        //                        string error = process.StandardError.ReadToEnd();
        //                        process.WaitForExit();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex) { }
        //}

    }
}
