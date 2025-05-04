using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.Enums.Log;


namespace ZBSolutions
{
    public class L0g
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly DateTime _start = DateTime.Now;
        public L0g(IZennoPosterProjectModel project)
        {
            _project = project;
            _start = DateTime.Now;
        }
        public void Send(string toLog, [CallerMemberName] string callerName = "", bool show = true, bool thr0w = false)
        {
            string acc0 = "notSet";
            string port = "notSet";
            string totalAge = "notSet";
            try { acc0 = _project.Variables["acc0"].Value; } catch { }
            try { port = _project.Variables["instancePort"].Value; } catch { }
            var age = DateTime.Now - _start;
            try { totalAge = TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(_project.Variables["varSessionId"].Value)).ToString(); } catch { }

            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();

            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = _project.Variables["projectName"].Value;
            if (toLog == null) toLog = "null";
            string formated = $"⛑  [{acc0}] ⚙  [{port}] ⏱  [{totalAge}] ⛏  [{callerName}]. elapsed:[{age}]\n          {toLog.Trim()}";
            LogType type = LogType.Info; LogColor color = LogColor.Default;

            if (formated.Contains("!W")) type = LogType.Warning;
            if (formated.Contains("!E")) type = LogType.Error;

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

            foreach (var pair in colorMap)
            {
                if (formated.Contains(pair.Key))
                {
                    color = pair.Value;
                    break;
                }

            }

            _project.SendToLog(formated, type, show, color);
            if (thr0w) throw new Exception($"{formated}");

        }
    }
}

