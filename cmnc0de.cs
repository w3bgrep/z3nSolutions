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

#endregion

namespace w3tools //by @w3bgrep
{

    public static class TestStatic
{
        public static void Deadline(this IZennoPosterProjectModel project, int sec = 0)
        {
            if (sec != 0)
            {
                var start = project.Variables[$"t0"].Value;
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long startTime = long.Parse(start);
                int difference = (int)(currentTime - startTime);
                if (difference > sec) throw new Exception("Timeout");

            }
            else 
            {
                project.Variables["t0"].Value = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();
            }
        }

            public static double randomPercent(this object input, double percent, double maxPercent)
            {
                if (percent < 0 || maxPercent < 0 || percent > 100 || maxPercent > 100)
                    throw new ArgumentException("Percent and MaxPercent must be between 0 and 100");

                if (!double.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                    throw new ArgumentException("Input cannot be converted to double");

                double percentageValue = number * (percent / 100.0);

                Random random = new Random();
                double randomReductionPercent = random.NextDouble() * maxPercent;
                double reduction = percentageValue * (randomReductionPercent / 100.0);

                double result = percentageValue - reduction;

                if (result <= 0)
                {
                    result = Math.Max(percentageValue * 0.01, 0.0001);
                }

                return result;
            }

}


public class NetHttp2
{
    private readonly IZennoPosterProjectModel _project;
    private readonly bool _logShow;

    public NetHttp2(IZennoPosterProjectModel project, bool log = false)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _logShow = log;
    }

    protected void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
    {
        if (!_logShow && !forceLog) return;
        _project.L0g($"[ 🌍 {callerName}] [{message}]");
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
        if (proxyString == "+") proxyString = _project.Variables["proxy"].Value;
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


    public string GET(string url, string proxyString = "", Dictionary<string, string> headers = null, bool parse = false, [CallerMemberName] string callerName = "")
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
                    Log("Set-Cookie found: " + cookies, callerName);
                }

                try
                {
                    _project.Variables["debugCookies"].Value = cookies;
                }
                catch { }

                string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (parse) ParseJson(result);
                Log($"{result}", callerName);
                return result.Trim();
            }
        }
        catch (HttpRequestException e)
        {
            Log($"[GET] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), Headers\n{debugHeaders.Trim()}", callerName);
            return string.Empty;
        }
        catch (Exception e)
        {
            Log($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) Headers\n{debugHeaders.Trim()}", callerName);
            return string.Empty;
        }
    }
    public string POST(string url, string body, string proxyString = "", Dictionary<string, string> headers = null, bool parse = false, [CallerMemberName] string callerName = "", bool throwOnFail = false)
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
                    }
                }

                headersString.AppendLine($"Content-Type: application/json; charset=UTF-8");

                Log(body);

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
                    Log("Set-Cookie found: " + cookies, callerName);
                }

                try
                {
                    _project.Variables["debugCookies"].Value = cookies;
                }
                catch { }

                string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                Log(result);
                if (parse) ParseJson(result);
                return result.Trim();
            }
        }
        catch (HttpRequestException e)
        {
            Log($"!W RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
            if (throwOnFail) throw;
            return "";
        }
        catch (Exception e)
        {
            Log($"!W UnknownErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
            if (throwOnFail) throw;
            return "";
        }
    }
    public string PUT(string url, string body = "", string proxyString = "", Dictionary<string, string> headers = null, bool parse = false, [CallerMemberName] string callerName = "")
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
                    Log(body, callerName);
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
                    Log("Set-Cookie found: " + cookies, callerName);
                }

                try
                {
                    _project.Variables["debugCookies"].Value = cookies;
                }
                catch { }

                string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Log(result, callerName);
                if (parse) ParseJson(result);
                return result.Trim();
            }
        }
        catch (HttpRequestException e)
        {
            Log($"!W RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
            return $"Ошибка: {e.Message}";
        }
        catch (Exception e)
        {
            Log($"!W UnknownErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
            return $"Ошибка: {e.Message}";
        }
    }
    public string DELETE(string url, string proxyString = "", Dictionary<string, string> headers = null, [CallerMemberName] string callerName = "")
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
                    Log("Set-Cookie found: " + cookies, callerName);
                }

                try
                {
                    _project.Variables["debugCookies"].Value = cookies;
                }
                catch { }

                string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Log(result, callerName);
                return result.Trim();
            }
        }
        catch (HttpRequestException e)
        {
            Log($"!W [DELETE] RequestErr: [{e.Message}] url:[{url}] (proxy: {proxyString}), Headers\n{debugHeaders.Trim()}", callerName);
            return $"Ошибка: {e.Message}";
        }
        catch (Exception e)
        {
            Log($"!W [DELETE] UnknownErr: [{e.Message}] url:[{url}] (proxy: {proxyString}) Headers\n{debugHeaders.Trim()}", callerName);
            return $"Ошибка: {e.Message}";
        }
    }


    public void CheckProxy(string url = "http://api.ipify.org/", string proxyString = null)
    {
        if (string.IsNullOrEmpty(proxyString)) proxyString = _project.Variables["proxy"].Value;
        WebProxy proxy = ParseProxy(proxyString);

        string ipWithoutProxy = GET(url, null);

        string ipWithProxy = "notSet";
        if (proxy != null)
        {
            ipWithProxy = GET(url, proxyString);
        }
        else
        {
            ipWithProxy = "noProxy";
        }

        Log($"local: {ipWithoutProxy}, proxified: {ipWithProxy}");

        if (ipWithProxy != ipWithoutProxy && !ipWithProxy.StartsWith("Ошибка") && ipWithProxy != "Прокси не настроен")
        {
            Log($"Succsessfuly proxified: {ipWithProxy}");
        }
        else if (ipWithProxy.StartsWith("Ошибка") || ipWithProxy == "Прокси не настроен")
        {
            Log($"!W proxy error: {ipWithProxy}");
        }
        else
        {
            Log($"!W ip still same. Proxy was not applyed");
        }
    }


}






}
