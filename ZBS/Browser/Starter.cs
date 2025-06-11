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

        public Starter(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project);
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _instance = instance;
        }
        public Starter(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "🚀");
            _sql = new Sql(_project);
        }
        public void StartBrowser(bool strictProxy = true)
        {

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
}
