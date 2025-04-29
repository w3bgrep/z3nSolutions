using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json.Linq;

namespace W3t00ls
{
    public class Cookies
    {
        private readonly Instance _instance;
        private readonly IZennoPosterProjectModel _project;
        public Cookies(IZennoPosterProjectModel project, Instance instance)
        {
            _project = project;
            _instance = instance;
        }
        public string Get (string domainFilter = "")
        {
            if (domainFilter == ".") domainFilter = _instance.ActiveTab.MainDomain;
            var cookieContainer = _project.Profile.CookieContainer;
            var cookieList = new List<object>();

            foreach (var domain in cookieContainer.Domains)
            {
                if (string.IsNullOrEmpty(domainFilter) || domain.Contains(domainFilter))
                {
                    var cookies = cookieContainer.Get(domain);
                    cookieList.AddRange(cookies.Select(cookie => new
                    {
                        domain = cookie.Host,
                        expirationDate = cookie.Expiry == DateTime.MinValue ? (double?)null : new DateTimeOffset(cookie.Expiry).ToUnixTimeSeconds(),
                        hostOnly = !cookie.IsDomain,
                        httpOnly = cookie.IsHttpOnly,
                        name = cookie.Name,
                        path = cookie.Path,
                        sameSite = cookie.SameSite.ToString(),
                        secure = cookie.IsSecure,
                        session = cookie.IsSession,
                        storeId = (string)null,
                        value = cookie.Value,
                        id = cookie.GetHashCode()
                    }));
                }
            }
            string cookiesJson = Global.ZennoLab.Json.JsonConvert.SerializeObject(cookieList, Global.ZennoLab.Json.Formatting.Indented);

            cookiesJson = cookiesJson.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");
            _project.Json.FromString(cookiesJson);
            return cookiesJson;
        }
        public string GetByJs(bool log = false)
        {
            string jsCode = @"
			var cookies = document.cookie.split('; ').map(function(cookie) {
				var parts = cookie.split('=');
				var name = parts[0];
				var value = parts.slice(1).join('=');
				return {
					'domain': window.location.hostname,
					'name': name,
					'value': value,
					'path': '/', 
					'expirationDate': null, 
					'hostOnly': true,
					'httpOnly': false,
					'secure': window.location.protocol === 'https:',
					'session': false,
					'sameSite': 'Unspecified',
					'storeId': null,
					'id': 1
				};
			});
			return JSON.stringify(cookies);
			";

            string jsonResult = _instance.ActiveTab.MainDocument.EvaluateScript(jsCode).ToString();

            //if (log) Loggers.l0g(_project, jsonResult);
            JArray cookiesArray = JArray.Parse(jsonResult);
            var escapedJson = jsonResult.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(" ", "").Replace("'", "''").Trim();
            _project.Json.FromString(jsonResult);
            return escapedJson;
        }
        public void c00kiesJSet(string cookiesJson, bool log = false)
        {
            try
            {
                JArray cookies = JArray.Parse(cookiesJson);

                var uniqueCookies = cookies
                    .GroupBy(c => new { Domain = c["domain"].ToString(), Name = c["name"].ToString() })
                    .Select(g => g.Last())
                    .ToList();

                string currentDomain = _instance.ActiveTab.Domain;
                //long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                //long weekFromNowUnixTime = currentUnixTime + (7 * 24 * 60 * 60);
                //string weekFromNow = DateTimeOffset.UtcNow.AddYears(1).ToString("R");

                string[] domainParts = currentDomain.Split('.');
                string parentDomain = "." + string.Join(".", domainParts.Skip(domainParts.Length - 2));


                string jsCode = "";
                int cookieCount = 0;
                foreach (JObject cookie in uniqueCookies)
                {
                    string domain = cookie["domain"].ToString();
                    string name = cookie["name"].ToString();
                    string value = cookie["value"].ToString();

                    if (domain == currentDomain || domain == "." + currentDomain)
                    {
                        string path = cookie["path"]?.ToString() ?? "/";
                        string expires;

                        if (cookie["expirationDate"] != null && cookie["expirationDate"].Type != JTokenType.Null)
                        {
                            double expValue = double.Parse(cookie["expirationDate"].ToString());
                            if (expValue < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) expires = DateTimeOffset.UtcNow.AddYears(1).ToString("R");
                            else expires = DateTimeOffset.FromUnixTimeSeconds((long)expValue).ToString("R");
                        }
                        else
                            expires = DateTimeOffset.UtcNow.AddYears(1).ToString("R");

                        jsCode += $"document.cookie = '{name}={value}; domain={parentDomain}; path={path}; expires={expires}'; Secure';\n";
                        cookieCount++;
                    }
                }
                _project.SendInfoToLog($"Found cookies for {currentDomain}: [{cookieCount}] runingJs...\n + {jsCode}");
                if (!string.IsNullOrEmpty(jsCode))
                {
                    _instance.ActiveTab.MainDocument.EvaluateScript(jsCode);
                }
                _project.SendInfoToLog($"!W No cookies Found for {currentDomain}");
            }
            catch (Exception ex)
            {
                _project.SendInfoToLog($"!W cant't parse JSON: [{cookiesJson}]  {ex.Message}");
            }
        }
    }
}
