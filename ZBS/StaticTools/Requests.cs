using System;
using System.Collections.Generic;

using System.Linq;

using System.Net.Http;
using System.Net;

using System.Text;
using System.Text.RegularExpressions;

using System.Threading;


using System.Globalization;
using System.Runtime.CompilerServices;

using Leaf.xNet;

using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Security.Policy;

namespace ZBSolutions
{
    public static class Requests
    {
        private static readonly object LockObject = new object();

        public static string GET(this IZennoPosterProjectModel project, string url, string proxy = "", bool log = false, bool parseJson = false)
        {
            string response;
            lock (LockObject)
            {
                response = ZennoPoster.HttpGet(url, proxy, "UTF-8", ResponceType.BodyOnly, 5000, "", "Mozilla/5.0", true, 5, null, "", false);
            }
            if (parseJson) project.Json.FromString(response);
            if (log) project.L0g($"json received: [{response}]");
            return response;
        }
        private static string POST(this IZennoPosterProjectModel project, string url, string body, string proxy = "", bool log = false, bool parseJson = false)
        {
            string response;
            lock (LockObject)
            {
                response = ZennoPoster.HTTP.Request(ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST, url, body, "application/json", proxy, "UTF-8", ResponceType.BodyOnly, 30000, "", project.Profile.UserAgent, true, 5, null, "", false, false, project.Profile.CookieContainer);
                project.Json.FromString(response);
            }
            if (parseJson) project.Json.FromString(response);
            if (log) project.L0g($"json received: [{response}]");
            return response;
        }
        public static string POSTLeaf(this IZennoPosterProjectModel project, string url, string jsonBody, string proxy = "", bool log = false)
        {
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                lock (LockObject)
                {
                    if (proxy == "+") proxy = project.Variables["proxyLeaf"].Value;
                    if (!string.IsNullOrEmpty(proxy))
                    {
                        try
                        {
                            string[] proxyArray = proxy.Split(':');
                            string username = proxyArray[1];
                            string password = proxyArray[2];
                            string host = proxyArray[3];
                            int port = int.Parse(proxyArray[4]);
                            request.Proxy = new HttpProxyClient(host, port, username, password);
                        }
                        catch (Exception ex)
                        {
                            project.L0g($"!W proxy parse err '{proxy}': {ex.Message}");
                            throw;
                        }
                    }
                    try
                    {
                        HttpResponse httpResponse = request.Post(url, jsonBody, "application/json");
                        return httpResponse.ToString();
                    }
                    catch (HttpException ex)
                    {
                        project.SendErrorToLog($"Rquest err: {ex.Message}");
                        throw;
                    }
                }

            }

        }
        public static string GETLeaf(this IZennoPosterProjectModel project, string url, string proxy = "", bool log = false)
        {
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                lock (LockObject)
                {
                    if (proxy == "+") proxy = project.Variables["proxyLeaf"].Value;
                    if (!string.IsNullOrEmpty(proxy))
                    {
                        try
                        {
                            string[] proxyArray = proxy.Split(':');
                            string username = proxyArray[1];
                            string password = proxyArray[2];
                            string host = proxyArray[3];
                            int port = int.Parse(proxyArray[4]);
                            request.Proxy = new HttpProxyClient(host, port, username, password);
                        }
                        catch (Exception ex)
                        {
                            project.L0g($"!W proxy parse err '{proxy}': {ex.Message}");
                            throw;
                        }
                    }
                    try
                    {
                        HttpResponse httpResponse = request.Get(url);
                        return httpResponse.ToString();
                    }
                    catch (HttpException ex)
                    {
                        project.SendErrorToLog($"Request err: {ex.Message}");
                        throw;
                    }
                }
            }
        }
        public static void TgReport(this IZennoPosterProjectModel project)
        {
            string time = project.ExecuteMacro(DateTime.Now.ToString("MM-dd HH:mm"));
            string varLast = project.Variables["a0debug"].Value;
            string report = "";

            if (!string.IsNullOrEmpty(project.Variables["failReport"].Value))
            {
                string encodedFailReport = Uri.EscapeDataString(project.Variables["failReport"].Value);
                string failUrl = $"https://api.telegram.org/bot{project.Variables["settingsTgLogToken"].Value}/sendMessage?chat_id={project.Variables["settingsTgLogGroup"].Value}&text={encodedFailReport}&reply_to_message_id={project.Variables["settingsTgLogTopic"].Value}&parse_mode=MarkdownV2";
                project.GET(failUrl);
            }
            else
            {
                report = $"✅️ [{time}]{project.Name} | {varLast}";
                string successReport = $"✅️  \\#{project.Name.EscapeMarkdown()} \\#{project.Variables["acc0"].Value} \n" +
                                      $"varLast: [{varLast.EscapeMarkdown()}] \n";

                string encodedReport = Uri.EscapeDataString(successReport);
                project.Variables["REPORT"].Value = encodedReport;
                string url = $"https://api.telegram.org/bot{project.Variables["settingsTgLogToken"].Value}/sendMessage?chat_id={project.Variables["settingsTgLogGroup"].Value}&text={encodedReport}&reply_to_message_id={project.Variables["settingsTgLogTopic"].Value}&parse_mode=MarkdownV2";
                project.GET(url);
            }
            string toLog = $"✔️ All jobs done. lastMark is: {Regex.Replace(varLast, @"\s+", " ").Trim()}. Elapsed: {project.Age<string>()}";
            if (toLog.Contains("fail")) project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Orange);
            else project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Green);
        }

    }



    public class NetHttp
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;
        private readonly bool _logShow;
        private readonly string _proxy;

        public NetHttp(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _proxy = new Sql(_project).Get("proxy", "private_profile");
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "♻");
        }

        protected void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;
            _logger.Send($"({callerName}) [{message}]");
        }
        protected void ParseJson(string json)
        {
            try {
                _project.Json.FromString(json);
            }
            catch (Exception ex) {
                Log($"[!W {ex.Message}] [{json}]");
            }
        }
        public WebProxy ParseProxy(string proxyString, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(proxyString))
            {
                return null;
            }
            if (proxyString == "+") 
                proxyString = _proxy;
            try
            {
                WebProxy proxy = new WebProxy();

                if (proxyString.Contains("//")) proxyString = proxyString.Split('/')[2];

                if (proxyString.Contains("@")) // Прокси с авторизацией (login:pass@proxy:port)
                {
                    string[] parts = proxyString.Split('@');
                    string credentials = parts[0];
                    string proxyHost = parts[1];

                    proxy.Address = new Uri("http://" + proxyHost);
                    string[] creds = credentials.Split(':');
                    proxy.Credentials = new NetworkCredential(creds[0], creds[1]);

                    Log($"proxy set:{proxyHost}", callerName);
                }
                else // Прокси без авторизации (proxy:port)
                {
                    proxy.Address = new Uri("http://" + proxyString);
                    Log($"proxy set: ip:{proxyString}", callerName);
                }

                return proxy;
            }
            catch (Exception e)
            {
                Log($"Ошибка настройки прокси: {e.Message}", callerName, true);
                return null;
            }
        }


        public string GET(
            string url,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
        {
            string debugHeaders = string.Empty;
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(15);

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent; // Same as in POST
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                    }

                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        _logger.Send($"Set-Cookie found: {cookies}");
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (parse) ParseJson(result);
                    _logger.Send(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"[GET] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), Headers\n{debugHeaders.Trim()}");
                if (throwOnFail) throw;

                return string.Empty;
            }
            catch (Exception e)
            {
                _logger.Send($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) Headers\n{debugHeaders.Trim()}");
                if (throwOnFail) throw;

                return string.Empty;
            }
        }
        public string POST(
            string url,
            string body,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)        {
            string debugHeaders = string.Empty;
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var content = new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;//"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                    }

                    headersString.AppendLine($"Content-Type: application/json; charset=UTF-8");

                    _logger.Send(body);

                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        _logger.Send("Set-Cookie found: " + cookies);
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    _logger.Send(result);
                    if (parse) ParseJson(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"[POST] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), Headers\n{debugHeaders.Trim()}");
                if (throwOnFail) throw;
                return string.Empty;
            }
            catch (Exception e)
            {
                _logger.Send($"!W [POST] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) Headers\n{debugHeaders.Trim()}");
                if (throwOnFail) throw;
                return string.Empty;
            }


        }
        public string PUT(
            string url, 
            string body = "",
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            [CallerMemberName] string callerName = "")
        {
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var content = string.IsNullOrEmpty(body) ? null : new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                        }
                    }

                    if (content != null)
                    {
                        headersString.AppendLine($"Content-Type: application/json; charset=UTF-8");
                        _logger.Send(body);
                    }

                    HttpResponseMessage response = client.PutAsync(url, content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        _logger.Send("Set-Cookie found: {cookies}");
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.Send(result);
                    if (parse) ParseJson(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"!W RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})");
                return $"Ошибка: {e.Message}";
            }
            catch (Exception e)
            {
                _logger.Send($"!W UnknownErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})");
                return $"Ошибка: {e.Message}";
            }
        }
        public string DELETE(
            string url,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            [CallerMemberName] string callerName = "")
        {

            string debugHeaders = null;
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                    }

                    HttpResponseMessage response = client.DeleteAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        _logger.Send($"Set-Cookie found: {cookies}");
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.Send(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.Send($"!W [DELETE] RequestErr: [{e.Message}] url:[{url}] (proxy: {proxyString}), Headers\n{debugHeaders.Trim()}");
                return $"Ошибка: {e.Message}";
            }
            catch (Exception e)
            {
                _logger.Send($"!W [DELETE] UnknownErr: [{e.Message}] url:[{url}] (proxy: {proxyString})");
                return $"Ошибка: {e.Message}";
            }
        }


        public bool CheckProxy( string proxyString = null)
        {
            
            if (string.IsNullOrEmpty(proxyString)) 
                proxyString = new Sql(_project).Get("proxy","private_profile");
            
            //WebProxy proxy = ParseProxy(proxyString);

            string ipLocal = GET("http://api.ipify.org/", null);
            string ipProxified = GET("http://api.ipify.org/", proxyString);

            _logger.Send($"ipLocal: {ipLocal}, ipProxified: {ipProxified}");

            if (ipProxified != ipLocal)
            {
                _logger.Send($"proxy `validated: {ipProxified}");
                _project.Var("proxy", proxyString);
                return true;
            }
            else if (ipProxified.StartsWith("Ошибка") || ipProxified == "Прокси не настроен")
            {
                _logger.Send($"!W proxy error: {ipProxified}");
                
            }
            else if (ipLocal == ipProxified)
            {
                _logger.Send($"!W ip still same. ipLocal: [{ipLocal}], ipProxified: [{ipProxified}]. Proxy was not applyed");
            }
            return false;
        }

        public bool ProxySet(Instance instance, string proxyString = null)
        {

            if (string.IsNullOrEmpty(proxyString))
                proxyString = _proxy;


            string ipLocal = GET("http://api.ipify.org/", null);
            string ipProxified = GET("http://api.ipify.org/", proxyString);

            //_logger.Send($"ipLocal: {ipLocal}, ipProxified: {ipProxified}");

            if (ipProxified != ipLocal)
            {
                _logger.Send($"proxy `validated: {ipProxified}");
                _project.Var("proxy", proxyString);
                instance.SetProxy(proxyString, true, true, true, true);
                return true;
            }
            else if (ipProxified.StartsWith("Ошибка") || ipProxified == "Proxy not Set")
            {
                _logger.Send($"!W proxy error: {ipProxified}");
            }
            else if (ipLocal == ipProxified)
            {
                _logger.Send($"!W ip still same. ipLocal: [{ipLocal}], ipProxified: [{ipProxified}]. Proxy was not applyed");
            }
            return false;
        }

    }



}



