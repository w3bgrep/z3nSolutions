using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public class GitHub
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;

        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _login;
        protected string _pass;
        protected string _2fa;

        public GitHub(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logger = new Logger(project, log: log, classEmoji: "GitHub");
            LoadCreds();

        }
        public void LoadCreds()
        {
            string[] creds = _sql.Get("status, login, password, otpsecret, cookies", "private_github").Split('|');
            try { _status = creds[0]; _project.Variables["googleSTATUS"].Value = _status; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _login = creds[1]; _project.Variables["github_login"].Value = _login; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _pass = creds[2]; _project.Variables["github_pass"].Value = _pass; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _2fa = creds[3]; _project.Variables["github_code"].Value = _2fa; } catch (Exception ex) { _logger.Send(ex.Message); }
            if (string.IsNullOrEmpty(_login) || string.IsNullOrEmpty(_pass))
                throw new Exception($"invalid credentials login:[{_login}] pass:[{_pass}]");
        }

        public void InputCreds()
        {
            _instance.HeSet(("login_field", "id"), _login);
            _instance.HeSet(("password", "id"), _pass);
            _instance.HeClick(("input:submit", "name", "commit", "regexp", 0), emu: 1);
            _instance.HeSet(("app_totp", "id"), OTP.Offline(_2fa), thr0w: false);
        }

        public void Go()
        {
            Tab tab = _instance.NewTab("github");
            if (tab.IsBusy) tab.WaitDownloading();
            _instance.Go("https://github.com/login");
            _instance.HeClick(("button", "innertext", "Accept", "regexp", 0), deadline: 3, thr0w: false);

        }
        public void Verify2fa()
        {
            _instance.HeClick(("button", "innertext", "Verify\\ 2FA\\ now", "regexp", 0));
            Thread.Sleep(20000);
            _instance.HeSet(("app_totp", "id"), OTP.Offline(_2fa));
            _instance.HeClick(("button", "class", "btn-primary\\ btn\\ btn-block", "regexp", 0), emu: 1);
            _instance.HeClick(("a", "innertext", "Done", "regexp", 0), emu: 1);

        }
        public void Load()
        {
            Go();
            InputCreds();
            try { Verify2fa(); } catch { }

        }


    }
}
