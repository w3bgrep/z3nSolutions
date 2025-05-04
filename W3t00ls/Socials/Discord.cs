using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace W3t00ls
{
    public class Discord
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly L0g _log;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;
        public Discord(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _log = new L0g(_project);
            _logShow = log;
            _sql = new Sql(_project);
        }
        public void DsLog(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _log.Send($"[ 👾  {callerName}] [{tolog}] ");
        }




        private void DSsetToken()
        {
            var jsCode = "function login(token) {\r\n    setInterval(() => {\r\n        document.body.appendChild(document.createElement `iframe`).contentWindow.localStorage.token = `\"${token}\"`\r\n    }, 50);\r\n    setTimeout(() => {\r\n        location.reload();\r\n    }, 1000);\r\n}\r\n    login(\'discordTOKEN\');\r\n".Replace("discordTOKEN", _project.Variables["discordTOKEN"].Value);
            _instance.ActiveTab.MainDocument.EvaluateScript(jsCode);
        }
        private string DSgetToken()
        {
            var token = _instance.ActiveTab.MainDocument.EvaluateScript("return (webpackChunkdiscord_app.push([[\'\'],{},e=>{m=[];for(let c in e.c)m.push(e.c[c])}]),m).find(m=>m?.exports?.default?.getToken!== void 0).exports.default.getToken();\r\n");
            return token;
        }
        private string DSlogin()
        {
            _project.SendInfoToLog("DLogin");
            DateTime deadline = DateTime.Now.AddSeconds(60);
            _instance.CloseExtraTabs();
            _instance.HeSet(("input:text", "aria-label", "Email or Phone Number", "text", 0), _project.Variables["discordLOGIN"].Value);
            _instance.HeSet(("input:password", "aria-label", "Password", "text", 0), _project.Variables["discordPASSWORD"].Value);
            _instance.HeClick(("button", "type", "submit", "regexp", 0));

            while (_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Are\\ you\\ human\\?", "regexp", 0).IsVoid &&
                _instance.ActiveTab.FindElementByAttribute("input:text", "autocomplete", "one-time-code", "regexp", 0).IsVoid) Thread.Sleep(1000);

            if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Are\\ you\\ human\\?", "regexp", 0).IsVoid)
            {
                if ((_project.Variables["humanNear"].Value) != "True") return "capcha";
                else _instance.WaitForUserAction(100, "dsCap");
            }
            _instance.HeSet(("input:text", "autocomplete", "one-time-code", "regexp", 0), OTP.Offline(_project.Variables["discord2FACODE"].Value));
            _instance.HeClick(("button", "type", "submit", "regexp", 0));
            Thread.Sleep(3000);
            return "ok";
        }
        public string DSload(bool log = false)
        {

            _sql.Discord();

            string state = null;
            var emu = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;
            bool tokenUsed = false;
            _instance.ActiveTab.Navigate("https://discord.com/channels/@me", "");

        start:
            state = null;
            while (string.IsNullOrEmpty(state))
            {
                _instance.HeClick(("button", "innertext", "Continue\\ in\\ Browser", "regexp", 0), thr0w: false);
                if (!_instance.ActiveTab.FindElementByAttribute("input:text", "aria-label", "Email or Phone Number", "text", 0).IsVoid) state = "login";
                if (!_instance.ActiveTab.FindElementByAttribute("section", "aria-label", "User\\ area", "regexp", 0).IsVoid) state = "logged";
            }

            DsLog( state);


            if (state == "login" && !tokenUsed)
            {
                DSsetToken();
                tokenUsed = true;
                //Thread.Sleep(5000);					
                goto start;
            }

            else if (state == "login" && tokenUsed)
            {
                var login = DSlogin();
                if (login == "ok")
                {
                    Thread.Sleep(5000);
                    goto start;
                }
                else if (login == "capcha")
                    DsLog( "!W capcha");
                _instance.UseFullMouseEmulation = emu;
                state = "capcha";
            }

            else if (state == "logged")
            {
                state = _instance.ActiveTab.FindElementByAttribute("div", "class", "avatarWrapper__", "regexp", 0).FirstChild.GetAttribute("aria-label");
                
                DsLog( state);
                var token = DSgetToken();
                _sql.Upd($"token = '{token}', status = 'ok'", "discord");
               // DSupdateDb($"token = '{token}', status = 'ok'");
                _instance.UseFullMouseEmulation = emu;
            }
            return state;

        }
        public string DSservers()
        {
            _instance.UseFullMouseEmulation = true;
            var folders = new List<HtmlElement>();
            var servers = new List<string>();
            var list = _instance.ActiveTab.FindElementByAttribute("div", "aria-label", "Servers", "regexp", 0).GetChildren(false).ToList();
            foreach (HtmlElement item in list)
            {

                if (item.GetAttribute("class").Contains("listItem"))
                {
                    var server = item.FindChildByTag("div", 1).FirstChild.GetAttribute("data-dnd-name");
                    servers.Add(server);
                }

                if (item.GetAttribute("class").Contains("wrapper"))
                {
                    _instance.HeClick(item);
                    var FolderServer = item.FindChildByTag("ul", 0).GetChildren(false).ToList();
                    //_project.SendInfoToLog(FolderServer.Count.ToString());
                    foreach (HtmlElement itemInFolder in FolderServer)
                    {
                        var server = itemInFolder.FindChildByTag("div", 1).FirstChild.GetAttribute("data-dnd-name");
                        servers.Add(server);
                    }
                }

            }

            string result = string.Join(" | ", servers);
            _sql.Upd($"servers = '{result}'", "discord");
            //DSupdateDb($"servers = '{result}'");
            //_project.SendInfoToLog(servers.Count.ToString());
            //_project.SendInfoToLog(string.Join(" | ",servers));
            return result;
        }
    }

}
