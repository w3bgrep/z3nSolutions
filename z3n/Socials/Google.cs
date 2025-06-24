using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public class Google
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
        protected string _recoveryMail;
        protected string _recoveryCodes;
        protected string _cookies;

        public Google(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logger = new Logger(project, log: log, classEmoji: "G");
            DbCreds();

        }
        private void DbCreds()
        {
            string[] creds = _sql.Get("status, login, password, otpsecret, recoveryemail, otpbackup, cookies", "private_google").Split('|');
            try { _status = creds[0]; _project.Variables["googleSTATUS"].Value = _status; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _login = creds[1]; _project.Variables["googleLOGIN"].Value = _login; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _pass = creds[2]; _project.Variables["googlePASSWORD"].Value = _pass; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _2fa = creds[3]; _project.Variables["google2FACODE"].Value = _2fa; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _recoveryMail = creds[4]; _project.Variables["googleSECURITY_MAIL"].Value = _recoveryMail; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _recoveryCodes = creds[5]; _project.Variables["googleBACKUP_CODES"].Value = _recoveryCodes; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _cookies = creds[6]; _project.Variables["googleCOOKIES"].Value = _cookies; } catch (Exception ex) { _logger.Send(ex.Message); }
            _logger.Send(_status);
        }
        public string Load(bool log = false, bool cookieBackup = true)
        {
            if (!_instance.ActiveTab.URL.Contains("google")) _instance.Go("https://myaccount.google.com/");
            check:
            Thread.Sleep(1000);
            string state = GetState();
            switch (state)
            {
                case "ok":
                    if (cookieBackup) SaveCookies();
                    return state;

                case "wrong":
                    _instance.CloseAllTabs();
                    _instance.ClearCookie("google.com");
                    _instance.ClearCookie("google.com");
                    goto check;

                case "inputLogin":
                    _instance.HeSet(("identifierId", "id"), _login);
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;

                case "inputPassword":
                    _instance.HeSet(("Passwd", "name"), _pass, deadline: 5);
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;

                case "inputOtp":
                    _instance.HeSet(("totpPin", "id"), OTP.Offline(_2fa));
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    goto check;

                case "addRecoveryPhone":
                    _instance.HeClick(("button", "innertext", "Cancel", "regexp", 0));
                    goto check;

                case "CAPCHA":
                    try { _project.CapGuru(); } catch { }
                    _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    //_sql.Upd("status = 'status = '!WCapcha'", "google");
                    //_sql.Upd("status = 'status = '!W fail.Google Capcha or Locked'");
                    goto check;
                    throw new Exception("CAPCHA");

                case "badBrowser":
                    _sql.Upd("status = 'status = '!W BadBrowser'", "google");
                    _sql.Upd("status = 'status = '!W fail.Google BadBrowser'");
                    throw new Exception("BadBrowser");

                default:
                    return state;

            }

        }
        public string GetState(bool log = false)
        {


        check:
            var status = "";

            if (!_instance.ActiveTab.FindElementByAttribute("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0).IsVoid)
                status = "signedIn";

            else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Verify\\ it’s\\ you", "regexp", 0).IsVoid)
                status = "CAPCHA";

            else if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Try\\ using\\ a\\ different\\ browser.", "regexp", 0).IsVoid)
                status = "BadBrowser";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:email", "fulltagname", "input:email", "text", 0).IsVoid) &&
                    (_instance.ActiveTab.FindElementByAttribute("input:email", "fulltagname", "input:email", "text", 0).GetAttribute("value") == ""))
                status = "inputLogin";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).IsVoid) &&
                    _instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).GetAttribute("value") == "")
                status = "inputPassword";

            else if ((!_instance.ActiveTab.FindElementById("totpPin").IsVoid) &&
                    _instance.ActiveTab.FindElementById("totpPin").GetAttribute("value") == "")
                status = "inputOtp";

            else if ((!_instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "text", 0).IsVoid) &&
                    _instance.ActiveTab.FindElementById("totpPin").GetAttribute("value") == "")
                status = "addRecoveryPhone";



            else status = "undefined";



            _logger.Send(status);

            switch (status)
            {
                case "signedIn":
                    var currentAcc = _instance.HeGet(("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0), atr: "aria-label").Split('\n')[1];
                    if (currentAcc.ToLower().Contains(_login.ToLower()))
                    {
                        _logger.Send($"{currentAcc} is Correct. Login done");
                        status = "ok";
                    }

                    else
                    {
                        _logger.Send($"!W {currentAcc} is InCorrect. MustBe {_login}");
                        status = "wrong";
                    }
                    break;

                case "undefined":
                    _instance.HeClick(("a", "class", "h-c-header__cta-li-link\\ h-c-header__cta-li-link--primary\\ button-standard-mobile", "regexp", 1), deadline: 1, thr0w: false);
                    goto check;

                default:
                    break;

            }
            return status;
        }
        public string GAuth(bool log = false)
        {
            try
            {
                var userContainer = _instance.HeGet(("div", "data-authuser", "0", "regexp", 0));
                _logger.Send($"container:{userContainer} catched");
                if (userContainer.IndexOf(_login, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _logger.Send($"correct user found: {_login}");
                    _instance.HeClick(("div", "data-authuser", "0", "regexp", 0), delay: 3);
                    Thread.Sleep(5000);
                    if (!_instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0).IsVoid)
                    {
                        while (true) _instance.HeClick(("div", "data-authuser", "0", "regexp", 0), "clickOut", deadline: 5, delay: 3);
                    }
                    try
                    {
                        _instance.HeClick(("button", "innertext", "Continue", "regexp", 0), deadline: 2, delay: 1);
                        return "SUCCESS with continue";
                    }
                    catch
                    {
                        return "SUCCESS. without confirmation";
                    }
                }
                else
                {
                    _logger.Send($"!Wrong account [{userContainer}]. Expected: {_login}. Cleaning");
                    _instance.CloseAllTabs();
                    _instance.ClearCookie("google.com");
                    _instance.ClearCookie("google.com");
                    return "FAIL. Wrong account";
                }
            }
            catch
            {
                return "FAIL. No loggined Users Found";
            }
        }
        public void SaveCookies()
        {
            _instance.Go("youtube.com");
            Thread.Sleep(5000);
            _instance.Go("https://myaccount.google.com/");
            new Cookies(_project, _instance).Save();
        }











        private void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;

            var stackFrame = new StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ ▶  {callerName}] [{tolog}] ");
        }

        public string GoogleCheckLogin(bool log = false)
        {
            _instance.ActiveTab.Navigate("https://myaccount.google.com/", "");
            var status = "";
            try
            {
                var heToWait = _instance.HeGet(("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0), atr: "aria-label");

                var currentAcc = heToWait.Split('\n')[1];
                if (currentAcc.IndexOf(_login, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    status = "ok";
                    return status;
                }
                else
                {
                    _project.L0g($"!W {currentAcc} is InCorrect. MustBe {_login}");
                    status = "wrong";
                    return status;
                }
            }
            catch
            {
                status = "undefined";
            }
            try
            {
                var heToWait = _instance.HeGet(("a", "class", "h-c-header__cta-li-link\\ h-c-header__cta-li-link--primary\\ button-standard-mobile", "regexp", 1), atr: "aria-label");


                if (heToWait == "Go to your Google Account")
                {
                    status = "unlogged";
                    _instance.HeClick(("a", "class", "h-c-header__cta-li-link\\ h-c-header__cta-li-link--primary\\ button-standard-mobile", "regexp", 1));
                }
            }
            catch
            {
                status = "unknown";
                _project.SendInfoToLog("no ontop buttons found");
            }
            return status;
        }
        public string GoogLoad(bool log = false)
        {
            var status = "";
            while (true)
            {
                status = GoogleCheckLogin();
                if (status == "ok") return status;
                if (status == "wrong")
                {
                    _instance.CloseAllTabs();
                    _instance.ClearCookie("google.com");
                    _instance.ClearCookie("google.com");
                    continue;
                }
                break;
            }



            while (true)
                try
                {
                    var userContainer = _instance.HeGet(("div", "data-authuser", "0", "regexp", 0));
                    Log($"userContainer found: {userContainer}");

                    if (userContainer.IndexOf(_login, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _instance.HeClick(("div", "data-authuser", "0", "regexp", 0));
                        try
                        {
                            _instance.HeClick(("button", "innertext", "Continue", "regexp", 0), deadline: 2);
                        }
                        catch { }
                        status = "ok";
                        return status;
                    }
                    else
                    {
                        _instance.CloseAllTabs();
                        _instance.ClearCookie("google.com");
                        _instance.ClearCookie("google.com");
                        Log($"!W {userContainer} is Wrong. MustBe {_login}");
                        status = "wrong";
                        continue;
                    }
                }
                catch
                {
                    Log($"no loggined Accounts detected. logining with {_login}");
                    try
                    {

                        _instance.HeSet(("identifierId", "id"), _login);
                        _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                        status = "unlogged";
                    }
                    catch
                    {

                    }

                    try
                    {
                        string Capcha = _instance.HeGet(("div", "innertext", "Verify\\ it’s\\ you", "regexp", 0), deadline: 5);
                        status = "capcha";
                        _sql.Upd("status = 'status = '!WCapcha'", "google");
                        _sql.Upd("status = 'status = '!W fail.Google Capcha or Locked'");
                        throw new Exception("CAPCHA");
                    }
                    catch
                    {
                        if (status == "capcha") throw;
                    }

                    try
                    {
                        string BadBrowser = _instance.HeGet((("div", "innertext", "Try\\ using\\ a\\ different\\ browser.", "regexp", 0), "regexp", 0), deadline: 1);
                        status = "BadBrowser";
                        _sql.Upd("status = 'status = '!W BadBrowser'", "google");
                        _sql.Upd("status = 'status = '!W fail.Google BadBrowser'");
                        throw new Exception("BadBrowser");
                    }
                    catch
                    {
                        if (status == "BadBrowser") throw;
                    }

                    if (!_instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "-1", "regexp", 0).IsVoid)
                    {
                        var userContainer = _instance.HeGet(("div", "data-authuser", "-1", "regexp", 0));
                        if (userContainer.IndexOf(_login, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Log($"Signed Out acc detected [{userContainer}]");
                            _instance.HeClick(("div", "data-authuser", "-1", "regexp", 0));
                        }
                        else
                        {
                            _instance.CloseAllTabs();
                            _instance.ClearCookie("google.com");
                            _instance.ClearCookie("google.com");
                            Log($"!W {userContainer} is Wrong. MustBe {_login}");
                            status = "wrong";
                            continue;
                        }
                    }
                    try
                    {
                        _instance.HeSet(("Passwd", "name"), _pass, deadline: 5);
                        _instance.HeClick(("button", "innertext", "Next", "regexp", 0));

                    }
                    catch
                    {
                        Log($"no Passwd demanded");
                    }

                    try
                    {
                        _instance.HeSet(("totpPin", "id"), OTP.Offline(_2fa));
                        _instance.HeClick(("button", "innertext", "Next", "regexp", 0));
                    }
                    catch
                    {
                        Log($"no OTP demanded");
                    }

                    try
                    {
                        string BadBrowser = _instance.HeGet((("div", "innertext", "To\\ continue,\\ you’ll\\ need\\ to\\ verify\\ that\\ it’s\\ you", "regexp", 0), "regexp", 0), deadline: 2);
                        status = "verify";
                        _sql.Upd("status = 'status = '!W Verify", "google");
                        _sql.Upd("status = 'status = '!W Google verify. Fail");
                        throw new Exception("BadBrowser");
                    }
                    catch
                    {
                        if (status == "verify") throw;
                    }

                    try
                    {
                        _instance.HeGet(("*", "innertext", "error\\nAdd\\ a\\ recovery\\ phone", "regexp", 0));
                        _instance.HeClick(("button", "innertext", "Cancel", "regexp", 0));

                    }
                    catch { }

                    try
                    {
                        _instance.HeClick(("span", "innertext", "Not\\ now", "regexp", 0), deadline: 1);
                    }
                    catch { }
                    try
                    {
                        _instance.HeClick(("span", "innertext", "skip", "regexp", 0), deadline: 1);
                    }
                    catch { }

                    status = "mustBeOk";
                    return status;
                }

        }
        public string GoogleAuth(bool log = false)
        {
            try
            {
                var userContainer = _instance.HeGet(("div", "data-authuser", "0", "regexp", 0));
                Log($"container:{userContainer} catched");
                if (userContainer.IndexOf(_login, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log($"correct user found: {_login}");
                    _instance.HeClick(("div", "data-authuser", "0", "regexp", 0), delay: 3);
                    Thread.Sleep(5000);
                    if (!_instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0).IsVoid)
                    {
                        while (true) _instance.HeClick(("div", "data-authuser", "0", "regexp", 0), "clickOut", deadline: 5, delay: 3);
                    }
                    try
                    {
                        _instance.HeClick(("button", "innertext", "Continue", "regexp", 0), deadline: 2, delay: 1);
                        return "SUCCESS with continue";
                    }
                    catch
                    {
                        return "SUCCESS. without confirmation";
                    }
                }
                else
                {
                    Log($"!Wrong account [{userContainer}]. Expected: {_login}. Cleaning");
                    _instance.CloseAllTabs();
                    _instance.ClearCookie("google.com");
                    _instance.ClearCookie("google.com");
                    return "FAIL. Wrong account";
                }
            }
            catch
            {
                return "FAIL. No loggined Users Found";
            }
        }
    }
}
