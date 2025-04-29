using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace W3t00ls
{
    public static class OTP
    {
        public static string Offline(string keyString, int waitIfTimeLess = 5)
        {
            var key = OtpNet.Base32Encoding.ToBytes(keyString);
            var otp = new OtpNet.Totp(key);
            string code = otp.ComputeTotp();
            int remainingSeconds = otp.RemainingSeconds();

            if (remainingSeconds <= waitIfTimeLess)
            {
                Thread.Sleep(remainingSeconds * 1000 + 1);
                code = otp.ComputeTotp();
            }

            return code;
        }
        public static string FirstMail(IZennoPosterProjectModel project, string email = "", string proxy = "")
        {
            string encodedLogin = Uri.EscapeDataString(project.Variables["settingsFmailLogin"].Value);
            string encodedPass = Uri.EscapeDataString(project.Variables["settingsFmailPass"].Value);
            if (email == "") email = project.Variables["googleLOGIN"].Value;
            string url = $"https://api.firstmail.ltd/v1/mail/one?username={encodedLogin}&password={encodedPass}";

            string[] headers = new string[]
            {
                $"accept: application/json",
                "accept-encoding: gzip, deflate, br",
                $"accept-language: {project.Profile.AcceptLanguage}",
                "sec-ch-ua-mobile: ?0",
                "sec-ch-ua-platform: \"Windows\"",
                "sec-fetch-dest: document",
                "sec-fetch-mode: navigate",
                "sec-fetch-site: none",
                "sec-fetch-user: ?1",
                "upgrade-insecure-requests: 1",
                $"user-agent: {project.Profile.UserAgent}",
                $"X-API-KEY: {project.Variables["settingsApiFirstMail"].Value}"
            };

            string result = ZennoPoster.HttpGet(
                url,
                proxy,
                "UTF-8",
                ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                5000,
                "",
                project.Profile.UserAgent,
                true,
                5,
                headers,
                "",
                false);

            project.Json.FromString(result);

            string deliveredTo = project.Json.to[0];
            string text = project.Json.text;
            string html = project.Json.html;


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


    }
}
