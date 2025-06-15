using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.IO;

namespace ZBSolutions
{
    public class Init
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;

        public Init(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _sql = new Sql(_project);
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "►");
            _instance = instance;
        }
        public Init(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "►");
            _sql = new Sql(_project);
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
            CheckVariables(vars);

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
        private void CheckVariables(string[] vars)
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
        public void SetBrowser(bool strictProxy = true, string cookies = null)
        {
            _project.Variables["instancePort"].Value = _instance.Port.ToString();
            _logger.Send($"init browser in port: {_instance.Port}");

            string webGlData = _sql.Get("webgl", "private_profile");
            _instance.SetDisplay(webGlData, _project);

            bool goodProxy = new NetHttp(_project, true).ProxySet(_instance);
            if (strictProxy && !goodProxy) throw new Exception($"!E bad proxy");

            string cookiePath = $"{_project.Variables["profiles_folder"].Value}accounts\\cookies\\{_project.Variables["acc0"].Value}.json";
            _project.Variables["pathCookies"].Value = cookiePath;

            if (cookies != null) 
                _instance.SetCookie(cookies);
            else
                try
                {
                    cookies = _sql.Get("cookies", "private_profile");
                    _instance.SetCookie(cookies);
                }
                catch (Exception Ex)
                {
                    _logger.Send($"!W Fail to set cookies from file {cookiePath}");
                    try
                    {
                        _logger.Send($"!E Fail to set cookies from db Err. {Ex.Message}");

                        cookies = File.ReadAllText(cookiePath);
                        _instance.SetCookie(cookies);
                    }
                    catch (Exception E)
                    {
                        _logger.Send($"!W Fail to set cookies from file {cookiePath} {E.Message}");
                    }

                }

            if (_project.Var("skipBrowserScan") != "True")
            {
                var bs = new BrowserScan(_project, _instance);
                if (bs.GetScore().Contains("timezone")) bs.FixTime();
            }

        }




    }
}
