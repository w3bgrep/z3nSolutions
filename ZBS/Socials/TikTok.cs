using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class Tiktok
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _token;
        protected string _login;
        protected string _pass;
        protected string _2fa;
        protected string _email;
        protected string _email_pass;

        public Tiktok(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logShow = log;

            //LoadCreds();

        }

        protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 💠  {callerName}] [{tolog}] ");
        }

        private string LoadCreds()
        {
            string[] xCreds = _sql.Get(" status, token, login, password, otpsecret, email, emailpass", "private_tiktok").Split('|');
            _status = xCreds[0];
            _token = xCreds[1];
            _login = xCreds[2];
            _pass = xCreds[3];
            _2fa = xCreds[4];
            _email = xCreds[5];
            _email_pass = xCreds[6];
            try
            {
                _project.Variables["ttStatus"].Value = _status;
                _project.Variables["ttToken"].Value = _token;
                _project.Variables["ttLogin"].Value = _login;
                _project.Variables["ttPass"].Value = _pass;
                _project.Variables["tt2fa"].Value = _2fa;
                _project.Variables["ttEmail"].Value = _email;
                _project.Variables["ttEmailPass"].Value = _email_pass;
            }
            catch (Exception ex)
            {
                _project.SendInfoToLog(ex.Message);
            }

            return _status;

        }

        public string GetCurrent()
        {

            string acc = _instance.HeGet(("a", "href", "https://www.tiktok.com/@", "regexp", 0), atr: "href").Split('@')[1].Trim();
            Log(acc);
            return acc;

        }


    }

}
