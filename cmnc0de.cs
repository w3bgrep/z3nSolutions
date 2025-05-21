using System;
using System.Collections.Generic;

using System.Linq;

using System.Net.Http;
using System.Net;

using System.Text;
using System.Text.RegularExpressions;

using System.Threading;

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



#endregion

namespace w3tools //by @w3bgrep
{

    public static class TestStatic
{

public static string[] GetErr(this IZennoPosterProjectModel project, Instance instance){
var error = project.GetLastError();
var ActionId = error.ActionId.ToString();
var ActionComment = error.ActionComment;
var exception = error.Exception;

string path = $"{project.Path}.failed\\{project.Variables["projectName"].Value}\\{project.Name} • {project.Variables["acc0"].Value} • {project.LastExecutedActionId} • {ActionId}.jpg";
ZennoPoster.ImageProcessingResizeFromScreenshot(instance.Port, path, 50, 50, "percent", true, false);

if (exception != null)
{
    var typeEx = exception.GetType();
	string type = typeEx.Name;
	string msg = exception.Message;
	string StackTrace = exception.StackTrace;
	StackTrace = StackTrace.Split('в')[1].Trim();
	string innerMsg = string.Empty;
	
	var innerException = exception.InnerException;
	if (innerException != null) innerMsg = innerException.Message;
	
	
	string toLog = $"{ActionId}\n{type}\n{msg}\n{StackTrace}\n{innerMsg}";
	
	project.SendErrorToLog(toLog);

    string failReport = $"⛔️\\#fail  \\#{project.Name.EscapeMarkdown()} \\#{project.Variables["acc0"].Value} \n" +
                        $"Error: `{ActionId.EscapeMarkdown()}` \n" +
                        $"Type: `{type.EscapeMarkdown()}` \n" +
                        $"Msg: `{msg.EscapeMarkdown()}` \n" +
                        $"Trace: `{StackTrace.EscapeMarkdown()}` \n";

    if (!string.IsNullOrEmpty(innerMsg)) failReport += $"Inner: `{innerMsg.EscapeMarkdown()}` \n";
    if (instance.BrowserType.ToString() == "Chromium")  failReport += $"Page: `{instance.ActiveTab.URL.EscapeMarkdown()}`";
    project.Variables["failReport"].Value = failReport;
    return new string[] {ActionId,type,msg,StackTrace,innerMsg};

}

return null;


}


}


public class Telegram
{
    protected readonly IZennoPosterProjectModel _project;
    protected readonly bool _logShow;
    protected readonly Sql _sql;
    protected readonly NetHttp _http;

    protected string _token;
    protected string _group;
    protected string _topic;

    public Telegram(IZennoPosterProjectModel project, bool log = false)
    {

        _project = project;
        _sql = new Sql(_project);
        _http = new NetHttp(_project);
        _logShow = log;
        LoadCreds();
    }

    protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
    {
        if (!_logShow && !log) return;
        var stackFrame = new System.Diagnostics.StackFrame(1);
        var callingMethod = stackFrame.GetMethod();
        if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
        _project.L0g($"[ 🚀  {callerName}] [{tolog}] ");
    }

    private void LoadCreds()
    {
        _token = _project.Variables["tg_logger_token"].Value;
        _group = _project.Variables["tg_logger_group"].Value;
        _topic = _project.Variables["tg_logger_topic"].Value;

    }

    public void report() 
    {


        string time = _project.ExecuteMacro(DateTime.Now.ToString("MM-dd HH:mm"));
        string report = "";

        if (!string.IsNullOrEmpty(_project.Variables["failReport"].Value))
        {
            string encodedFailReport = Uri.EscapeDataString(_project.Variables["failReport"].Value);
            string failUrl = $"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_group}&text={encodedFailReport}&reply_to_message_id={_topic}&parse_mode=MarkdownV2";
            _http.GET(failUrl);
        }
        else
        {
            report = $"✅️ [{time}]{_project.Name}";

            string successReport = $"✅️  \\#{_project.Name.EscapeMarkdown()} \\#{_project.Variables["acc0"].Value} \n";

            string encodedReport = Uri.EscapeDataString(successReport);
            //_project.Variables["REPORT"].Value = encodedReport;
            string url = $"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_group}&text={encodedReport}&reply_to_message_id={_topic}&parse_mode=MarkdownV2";
            _http.GET(url);
        }
        string toLog = $"✔️ All jobs done. Elapsed: {_project.TimeElapsed()}";
        if (toLog.Contains("fail")) _project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Orange);
        else _project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Green);


    }




}


}
