using System;
using System.Collections.Generic;

using System.Linq;

using System.Net.Http;
using System.Net;

using System.Text;
using System.Text.RegularExpressions;

using System.Threading;
using Newtonsoft.Json;
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

using Newtonsoft.Json.Linq;

#endregion

namespace w3tools //by @w3bgrep
{

    public static class TestStatic
    {

        private static readonly object FileLock = new object();

        public static string GetNewCreds(this IZennoPosterProjectModel project, string dataType)
        {
            string pathFresh = $"{project.Path}.data\\fresh\\{dataType}.txt";
            string pathUsed = $"{project.Path}.data\\used\\{dataType}.txt";

            lock (FileLock)
            {
                try
                {
                    if (!File.Exists(pathFresh))
                    {
                        project.SendWarningToLog($"File not found: {pathFresh}");
                        return null;
                    }

                    var freshAccs = File.ReadAllLines(pathFresh).ToList();
                    project.SendInfoToLog($"Loaded {freshAccs.Count} accounts from {pathFresh}");

                    if (freshAccs.Count == 0)
                    {
                        project.SendInfoToLog($"No accounts available in {pathFresh}");
                        return string.Empty;
                    }

                    string creds = freshAccs[0];
                    freshAccs.RemoveAt(0);

                    File.WriteAllLines(pathFresh, freshAccs);
                    File.AppendAllText(pathUsed, creds + Environment.NewLine);

                    return creds;
                }
                catch (Exception ex)
                {
                    project.SendWarningToLog($"Error processing files for {dataType}: {ex.Message}");
                    return null;
                }
            }

        }
        public static string CookiesToJson(string cookies)
        {
            try
            {
                if (string.IsNullOrEmpty(cookies))
                {
                    //project.SendWarningToLog("Cookie string is empty or null");
                    return "[]"; // Возвращаем пустой массив
                }

                var result = new List<Dictionary<string, string>>();
                var cookiePairs = cookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in cookiePairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                        continue;

                    var keyValue = trimmedPair.Split(new[] { '=' }, 2);
                    if (keyValue.Length != 2)
                    {
                        //project.SendWarningToLog($"Invalid cookie format: {trimmedPair}");
                        continue;
                    }

                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        result.Add(new Dictionary<string, string>
                    {
                        { "name", key },
                        { "value", value }
                    });
                    }
                }

                // Преобразуем список в JSON-массив
                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                return json;
            }
            catch (Exception ex)
            {
                //project.SendWarningToLog($"Error parsing cookies to JSON: {ex.Message}");
                return "[]"; // Возвращаем пустой массив в случае ошибки
            }
        }
    
    public static string NewNickName()
    {
        // Списки слов для комбинации
       string[] adjectives = {
        "Sunny", "Mystic", "Wild", "Cosmic", "Shadow", "Lunar", "Blaze", "Dream", "Star", "Vivid",
        "Frost", "Neon", "Gloomy", "Swift", "Silent", "Fierce", "Radiant", "Dusk", "Nova", "Spark",
        "Crimson", "Azure", "Golden", "Midnight", "Velvet", "Stormy", "Echo", "Vortex", "Phantom", "Bright",
        "Chill", "Rogue", "Daring", "Lush", "Savage", "Twilight", "Crystal", "Zesty", "Bold", "Hazy",
        "Vibrant", "Gleam", "Frosty", "Wicked", "Serene", "Bliss", "Rusty", "Hollow", "Sleek", "Pale"
        };

        // Список существительных (50 элементов)
        string[] nouns = {
            "Wolf", "Viper", "Falcon", "Spark", "Catcher", "Rider", "Echo", "Flame", "Voyage", "Knight",
            "Raven", "Hawk", "Storm", "Tide", "Drift", "Shade", "Quest", "Blaze", "Wraith", "Comet",
            "Lion", "Phantom", "Star", "Cobra", "Dawn", "Arrow", "Ghost", "Sky", "Vortex", "Wave",
            "Tiger", "Ninja", "Dreamer", "Seeker", "Glider", "Rebel", "Spirit", "Hunter", "Flash", "Beacon",
            "Jaguar", "Drake", "Scout", "Path", "Glow", "Riser", "Shadow", "Bolt", "Zephyr", "Forge"
        };

        // Список суффиксов (10 элементов, как расширение)
        string[] suffixes = { "", "", "", "", "", "X", "Z", "Vibe", "Glow", "Rush", "Peak", "Core", "Wave", "Zap" };

        // Потокобезопасный генератор случайных чисел
        Random random = new Random(Guid.NewGuid().GetHashCode());

        // Выбираем случайные слова
        string adjective = adjectives[random.Next(adjectives.Length)];
        string noun = nouns[random.Next(nouns.Length)];
        string suffix = suffixes[random.Next(suffixes.Length)];

        // Комбинируем никнейм
        string nickname = $"{adjective}{noun}{suffix}";

        // Убедимся, что никнейм не слишком длинный (например, до 15 символов, как на TikTok)
        if (nickname.Length > 15)
        {
            nickname = nickname.Substring(0, 15);
        }

        return nickname;
    }


}


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
