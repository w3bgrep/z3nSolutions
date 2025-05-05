using System;
using System.Linq;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NBitcoin;
using System.Collections.Generic;

namespace ZBSolutions
{
    public enum W
    {
        MetaMask,
        Rabby,
        Backpack,
        Razor,
        Zerion,
        Keplr
    }
    public class Wlt
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly L0g _log;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;
        protected readonly SqLoad _sqLoad;


        public Wlt(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _log = new L0g(_project);
            _logShow = log;
            _sql = new Sql(_project);
            _pass = SAFU.HWPass(_project);
        }

        public void WalLog(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _log.Send($"[ 👛  {callerName}] [{tolog}] ");
        }
        public bool Install(string extId, string fileName, bool log = false)
        {
            string path = $"{_project.Path}.crx\\{fileName}";

            if (!File.Exists(path))
            {
                WalLog($"File not found: {path}", log: log);
                throw new FileNotFoundException($"CRX file not found: {path}");
            }

            var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
            if (!extListString.Contains(extId))
            {
                WalLog($"installing {path}", log:log);
                _instance.InstallCrxExtension(path);
                return true;
            }
            return false;
        }
        
    }


}


