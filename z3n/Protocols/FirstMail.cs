using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public class FirstMail
    {

        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;

        private string _key;
        private string _login;
        private string _pass;

        public FirstMail(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logger = new Logger(project, log: log, classEmoji: "FirstMail");
            LoadKeys();
        }

        private void LoadKeys()
        {
            var creds = new Sql(_project).Get("apikey, apisecret, passphrase", "private_api", where: "key = 'firstmail'").Split('|');

            _key = creds[0];
            _login = creds[1];
            _pass = creds[2];

        }



        public string GetMail(string email)
        {
            string encodedLogin = Uri.EscapeDataString(_login);
            string encodedPass = Uri.EscapeDataString(_pass);

            string url = $"https://api.firstmail.ltd/v1/mail/one?username={encodedLogin}&password={encodedPass}";

            string[] headers = new string[]
            {
                $"accept: application/json",
                "accept-encoding: gzip, deflate, br",
                $"accept-language: {_project.Profile.AcceptLanguage}",
                "sec-ch-ua-mobile: ?0",
                "sec-ch-ua-platform: \"Windows\"",
                "sec-fetch-dest: document",
                "sec-fetch-mode: navigate",
                "sec-fetch-site: none",
                "sec-fetch-user: ?1",
                "upgrade-insecure-requests: 1",
                $"X-API-KEY: {_key}"
            };

            string result = _project.GET(url,"", headers);
            _project.Json.FromString(result);
            return result;

        }

        public string GetOTP(string email)
        {

            string json = GetMail(email);
            _project.Json.FromString(json);
            string deliveredTo = _project.Json.to[0];
            string text = _project.Json.text;
            string html = _project.Json.html;


            if (!deliveredTo.Contains(email)) throw new Exception($"Fmail: Email {email} not found in last message");
            else
            {
                Match match = Regex.Match(text, @"\b\d{6}\b");
                if (match.Success) return match.Value;
                match = Regex.Match(html, @"\b\d{6}\b");
                if (match.Success) return match.Value;
                else throw new Exception("Fmail: OTP not found in message with correct email");
            }

        }

        //public string GetLink(string email)
        //{
        //    string json = GetMail(email);
        //    _project.Json.FromString(json);
        //    string deliveredTo = _project.Json.to[0];
        //    string text = _project.Json.text;

        //    if (!deliveredTo.Contains(email)) 
        //        throw new Exception($"Fmail: Email {email} not found in last message");
        //    else
        //    {

        //        int httpIndex = text.IndexOf("http://");
        //        int httpsIndex = text.IndexOf("https://");
        //        int startIndex = httpIndex != -1 && (httpsIndex == -1 || httpIndex < httpsIndex) ? httpIndex : httpsIndex;

        //        if (startIndex == -1) return "";

        //        string potentialLink = text.Substring(startIndex);
        //        int endIndex = potentialLink.IndexOfAny(new[] { ' ', '\n', '\r', '\t', '"' });
        //        if (endIndex != -1)
        //        {
        //            potentialLink = potentialLink.Substring(0, endIndex);
        //        }

        //        if (Uri.TryCreate(potentialLink, UriKind.Absolute, out _))
        //        {
        //            return potentialLink;
        //        }
        //        throw new Exception($"No Link found in message {text}");
        //    }

        //}
        public string GetLink(string email)
        {
            string json = GetMail(email);
            _project.Json.FromString(json);
            string deliveredTo = _project.Json.to[0];
            string text = _project.Json.text;

            if (!deliveredTo.Contains(email))
                throw new Exception($"Fmail: Email {email} not found in last message");

            int startIndex = text.IndexOf("https://");
            if (startIndex == -1) startIndex = text.IndexOf("http://");
            if (startIndex == -1) throw new Exception($"No Link found in message {text}");

            string potentialLink = text.Substring(startIndex);
            int endIndex = potentialLink.IndexOfAny(new[] { ' ', '\n', '\r', '\t', '"' });
            if (endIndex != -1)
                potentialLink = potentialLink.Substring(0, endIndex);

            return Uri.TryCreate(potentialLink, UriKind.Absolute, out _)
                ? potentialLink
                : throw new Exception($"No Link found in message {text}");
        }
    }
}
