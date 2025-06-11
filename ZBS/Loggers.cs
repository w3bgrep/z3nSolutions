using OtpNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class Logger
    {
        protected readonly IZennoPosterProjectModel _project;
        protected bool _logShow = false;
        private string _emoji = null;

        public Logger(IZennoPosterProjectModel project, bool log = false, string classEmoji = null)
        {
            _project = project;
            _logShow = log || _project.Var("debug") == "True";
            //if (!log) _logShow = _project.Var("debug") == "True";
            _emoji = classEmoji;
        }

        public void Send(string toLog, [CallerMemberName] string callerName = "", bool show = false, bool thr0w = false, bool toZp = true, int cut = 0, bool wrap = true)
        {
            if (!show && !_logShow) return;
            string header = string.Empty;
            string body = toLog;

            if (wrap)
            {
                var stackFrame = new System.Diagnostics.StackFrame(1);
                header = LogHeader(stackFrame, callerName);
                body = LogBody(toLog, cut);
            }
            string toSend = header + body;
            (LogType type, LogColor color) = LogColour(header, toLog);
            Execute(toSend, type, color, toZp, thr0w);

        }

        private string LogHeader(System.Diagnostics.StackFrame stackFrame, string callerName)
        {
            string formated = null;
            try
            {
                string acc0 = _project.Var("acc0");
                if (!string.IsNullOrEmpty(acc0)) formated += $"⛑  [{acc0}]";
            }
            catch { }

            try
            {
                string port = _project.Var("instancePort");
                if (!string.IsNullOrEmpty(port)) formated += $" 🌐  [{port}]";
            }
            catch { }

            try
            {
                string totalAge = _project.Age<string>();
                if (!string.IsNullOrEmpty(totalAge)) formated += $" ⌛  [{totalAge}]";
            }
            catch { }

            try
            {
                var callingMethod = stackFrame.GetMethod();
                if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno"))
                    formated += $" 🔳  [{_project.Name.Replace(".zp", "")}]";//🔳
                else
                    formated += $" 🔲  [{callingMethod.DeclaringType.Name}.{callerName}]";
            }
            catch { }

            return formated ?? string.Empty;
        }
        private string LogBody(string toLog, int cut)
        {
            if (!string.IsNullOrEmpty(toLog))
            {
                if (cut != 0)
                {
                    int lineCount = toLog.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Length;
                    if (lineCount > cut) toLog = toLog.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
                }
                if (!string.IsNullOrEmpty(_emoji))
                {
                    toLog = $"[ {_emoji} ] {toLog}";
                }
                return $"\n          {toLog.Trim()}";
            }
            return string.Empty;

        }
        private (LogType, LogColor) LogColour(string header, string toLog)
        {
            LogType type = LogType.Info;
            LogColor color = LogColor.Default;

            var colorMap = new Dictionary<string, LogColor>
    {
        { "`.", LogColor.Default },
        { "`w", LogColor.Gray },
        { "`y", LogColor.Yellow },
        { "`o", LogColor.Orange },
        { "`r", LogColor.Red },
        { "`p", LogColor.Pink },
        { "`v", LogColor.Violet },
        { "`b", LogColor.Blue },
        { "`lb", LogColor.LightBlue },
        { "`t", LogColor.Turquoise },
        { "`g", LogColor.Green },
        { "!W", LogColor.Orange },
        { "!E", LogColor.Orange },
        { "relax", LogColor.LightBlue },
    };

            string combined = (header ?? "") + (toLog ?? "");
            foreach (var pair in colorMap)
            {
                if (combined.Contains(pair.Key))
                {
                    color = pair.Value;
                    break;
                }
            }

            if (combined.Contains("!W")) type = LogType.Warning;
            if (combined.Contains("!E")) type = LogType.Error;

            return (type, color);
        }
        private void Execute(string toSend, LogType type, LogColor color, bool toZp, bool thr0w)
        {
            _project.SendToLog(toSend, type, toZp, color);
            if (thr0w) throw new Exception($"{toSend}");
        }
        public void SendToTelegram(string message = null)
        {
            var _http = new NetHttp(_project);
            string _token = _project.Variables["tg_logger_token"].Value;
            string _group = _project.Variables["tg_logger_group"].Value;
            string _topic = _project.Variables["tg_logger_topic"].Value;

            //string time = _project.ExecuteMacro(DateTime.Now.ToString("MM-dd HH:mm"));
            

            if (!string.IsNullOrEmpty(_project.Variables["failReport"].Value))
            {
                string encodedFailReport = Uri.EscapeDataString(_project.Variables["failReport"].Value);
                string failUrl = $"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_group}&text={encodedFailReport}&reply_to_message_id={_topic}&parse_mode=MarkdownV2";
                _http.GET(failUrl);
            }
            else
            {
                string lastQuery = string.Empty; try { lastQuery = _project.Var("lastQuery"); } catch { }

                string successReport = $"✅️\\#succsess  \\#{_project.Name.EscapeMarkdown()} \\#{_project.Var("acc0")} \n";
                if (lastQuery != string.Empty) successReport += $"LastUpd: `{lastQuery}` \n";               
                if (!string.IsNullOrEmpty(message)) successReport += $"Message:`{message}` \n";               
                successReport += $"TookTime: {_project.TimeElapsed()}s \n";

                string encodedReport = Uri.EscapeDataString(successReport);
                string url = $"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_group}&text={encodedReport}&reply_to_message_id={_topic}&parse_mode=MarkdownV2";
                _http.GET(url);
            }

            string toLog = $"✔️ All jobs done. Elapsed: {_project.TimeElapsed()}s \n███ ██ ██  ██ █  █  █  ▓▓▓ ▓▓ ▓▓  ▓  ▓  ▓  ▒▒▒ ▒▒ ▒▒ ▒  ▒  ░░░ ░░  ░░ ░ ░ ░ ░ ░ ░  ░  ░  ░   ░   ░   ░    ░    ░    ░     ░        ░          ░";
            if (toLog.Contains("fail")) _project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Orange);
            else _project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Green);
        }
    }
}
