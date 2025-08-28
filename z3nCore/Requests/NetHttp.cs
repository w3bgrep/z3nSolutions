﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
{
    public class NetHttp
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;
        private readonly bool _logShow;

        public NetHttp(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _logShow = log;
            _logger = new Logger(project, log: log, classEmoji: "↑↓");
        }

        protected void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;
            _logger.Send($"({callerName}) [{message}]");
        }
        protected void ParseJson(string json)
        {
            try
            {
                _project.Json.FromString(json);
            }
            catch (Exception ex)
            {
                _logger.Send($"[!W {ex.Message}] [{json}]");
            }
        }
        public WebProxy ParseProxy(string proxyString, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(proxyString))
            {
                return null;
            }
            if (proxyString == "+")
                proxyString = _project.SqlGet("proxy", "_instance");
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

                }
                else // Прокси без авторизации (proxy:port)
                {
                    proxy.Address = new Uri("http://" + proxyString);
                }

                return proxy;
            }
            catch (Exception e)
            {
                _logger.Send(e.Message + $"[{proxyString}]");
                return null;
            }
        }
        private Dictionary<string, string> BuildHeaders(Dictionary<string, string> inputHeaders = null)
        {
            var defaultHeaders = new Dictionary<string, string>
            {
                { "User-Agent", _project.Profile.UserAgent }, // Already present

            };

            if (inputHeaders == null || inputHeaders.Count == 0)
            {
                return defaultHeaders;
            }

            var mergedHeaders = new Dictionary<string, string>(defaultHeaders);
            foreach (var header in inputHeaders)
            {
                mergedHeaders[header.Key] = header.Value; // Input headers override defaults
            }

            return mergedHeaders;
        }
        private Dictionary<string, string> BuildHeaders(object inputHeaders = null)
        {
            var headers = new Dictionary<string, string>
            {
                { "User-Agent", _project.Profile.UserAgent },

            };

            if (inputHeaders is null)
            {
                return headers;
            }
            else if (inputHeaders is Dictionary<string, string> dictHeaders) 
            {
                foreach (var header in dictHeaders)
                {
                    try { headers.Add(header.Key, header.Value); }
                    catch { headers[header.Key] = header.Value; }

                }
            }
            else if (inputHeaders is IEnumerable<string> stringHeaders) 
            {
                foreach (string header in stringHeaders)
                {
                    string[] parts = header.Split(new[] { ':' }, 2); 
                    if (parts.Length == 2)
                    {
                        string headerKey = parts[0].Trim(); 
                        string headerValue = parts[1].Trim(); 

                        try { headers.Add(headerKey, headerValue); }
                        catch { headers[headerKey] = headerValue; }

                    }
                    // Можно добавить else-блок для обработки некорректных строк заголовков
                }
            }
            // Можно добавить else-блок для обработки неподдерживаемых типов inputHeaders

            return headers;
        }

        public string GET(
            string url,
            string proxyString = "",
            Dictionary<string, string> headers = null,
            bool parse = false,
            int deadline = 15,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
        {
            string debugHeaders = "";
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
                    client.Timeout = TimeSpan.FromSeconds(deadline);

                    var requestHeaders = BuildHeaders(headers);

                    foreach (var header in requestHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        debugHeaders += $"{header.Key}: {header.Value}; ";
                    }

                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    string responseHeaders = string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = string.Join("; ", cookieValues);
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
                _logger.Send($"[GET] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), headers: [{debugHeaders.Trim()}]");
                if (throwOnFail) throw;
                return e.Message.Replace("Response status code does not indicate success:", "").Trim('.').Trim();
            }
            catch (Exception e)
            {
                _logger.Send($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) headers: [{debugHeaders.Trim()}]");
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
            int deadline = 15,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
            {
                string debugHeaders = "";
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
                        client.Timeout = TimeSpan.FromSeconds(deadline);
                        var content = new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");

                        var requestHeaders = BuildHeaders(headers);

                        foreach (var header in requestHeaders)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            debugHeaders += $"{header.Key}: {header.Value}; ";
                        }
                        debugHeaders += "Content-Type: application/json; charset=UTF-8; ";

                        _logger.Send(body);

                        HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                        response.EnsureSuccessStatusCode();

                        string responseHeaders = string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));

                        string cookies = "";
                        if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                        {
                            cookies = string.Join("; ", cookieValues);
                            _logger.Send($"Set-Cookie found: {cookies}");
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
                    _logger.Send($"[POST] SERVER Err: [{e.Message}] url:[{url}] (proxy: {(proxyString)}), headers: [{debugHeaders.Trim()}]");
                    if (throwOnFail) throw;
                    return e.Message.Replace("Response status code does not indicate success:", "").Trim('.').Trim();
                }
                catch (Exception e)
                {
                    _logger.Send($"!W [POST] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString)}) headers: [{debugHeaders.Trim()}]");
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
                return e.Message;
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
                return e.Message;
            }
            catch (Exception e)
            {
                _logger.Send($"!W [DELETE] UnknownErr: [{e.Message}] url:[{url}] (proxy: {proxyString})");
                return $"Ошибка: {e.Message}";
            }
        }


        public bool CheckProxy(string proxyString = null)
        {

            if (string.IsNullOrEmpty(proxyString))
                proxyString = _project.SqlGet("proxy", "_instance");

            //WebProxy proxy = ParseProxy(proxyString);

            string ipLocal = GET("http://api.ipify.org/", null);
            string ipProxified = GET("http://api.ipify.org/", proxyString);

            //_logger.Send($"ipLocal: {ipLocal}, ipProxified: {ipProxified}");

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
                proxyString = _project.SqlGet("proxy", "_instance");


            string ipLocal = GET("http://api.ipify.org/", null);
            string ipProxified = GET("http://api.ipify.org/", proxyString);

            if (string.IsNullOrEmpty(ipProxified) || !System.Net.IPAddress.TryParse(ipProxified, out _))
            {
                _logger.Send($"!W proxy error: Invalid or empty IP [{ipProxified}]");
                return false;
            }

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
