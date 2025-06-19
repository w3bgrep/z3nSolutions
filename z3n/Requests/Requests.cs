using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace z3n
{
    //public static class Requests
    //{
    //    private static readonly object LockObject = new object();

    //    public static string GET(this IZennoPosterProjectModel project, string url, string proxy = "", bool log = false, bool parseJson = false)
    //    {
    //        string response;
    //        lock (LockObject)
    //        {
    //            response = ZennoPoster.HttpGet(url, proxy, "UTF-8", ResponceType.BodyOnly, 5000, "", "Mozilla/5.0", true, 5, null, "", false);
    //        }
    //        if (parseJson) project.Json.FromString(response);
    //        if (log) project.L0g($"json received: [{response}]");
    //        return response;
    //    }
    //    private static string POST(this IZennoPosterProjectModel project, string url, string body, string proxy = "", bool log = false, bool parseJson = false)
    //    {
    //        string response;
    //        lock (LockObject)
    //        {
    //            response = ZennoPoster.HTTP.Request(ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST, url, body, "application/json", proxy, "UTF-8", ResponceType.BodyOnly, 30000, "", project.Profile.UserAgent, true, 5, null, "", false, false, project.Profile.CookieContainer);
    //            project.Json.FromString(response);
    //        }
    //        if (parseJson) project.Json.FromString(response);
    //        if (log) project.L0g($"json received: [{response}]");
    //        return response;
    //    }
        
    //    public static string POSTLeaf(this IZennoPosterProjectModel project, string url, string jsonBody, string proxy = "", bool log = false)
    //    {
    //        using (var request = new HttpRequest())
    //        {
    //            request.UserAgent = "Mozilla/5.0";
    //            request.IgnoreProtocolErrors = true;
    //            request.ConnectTimeout = 5000;

    //            lock (LockObject)
    //            {
    //                if (proxy == "+") proxy = project.Variables["proxyLeaf"].Value;
    //                if (!string.IsNullOrEmpty(proxy))
    //                {
    //                    try
    //                    {
    //                        string[] proxyArray = proxy.Split(':');
    //                        string username = proxyArray[1];
    //                        string password = proxyArray[2];
    //                        string host = proxyArray[3];
    //                        int port = int.Parse(proxyArray[4]);
    //                        request.Proxy = new HttpProxyClient(host, port, username, password);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        project.L0g($"!W proxy parse err '{proxy}': {ex.Message}");
    //                        throw;
    //                    }
    //                }
    //                try
    //                {
    //                    HttpResponse httpResponse = request.Post(url, jsonBody, "application/json");
    //                    return httpResponse.ToString();
    //                }
    //                catch (HttpException ex)
    //                {
    //                    project.SendErrorToLog($"Rquest err: {ex.Message}");
    //                    throw;
    //                }
    //            }

    //        }

    //    }
    //    public static string GETLeaf(this IZennoPosterProjectModel project, string url, string proxy = "", bool log = false)
    //    {
    //        using (var request = new HttpRequest())
    //        {
    //            request.UserAgent = "Mozilla/5.0";
    //            request.IgnoreProtocolErrors = true;
    //            request.ConnectTimeout = 5000;

    //            lock (LockObject)
    //            {
    //                if (proxy == "+") proxy = project.Variables["proxyLeaf"].Value;
    //                if (!string.IsNullOrEmpty(proxy))
    //                {
    //                    try
    //                    {
    //                        string[] proxyArray = proxy.Split(':');
    //                        string username = proxyArray[1];
    //                        string password = proxyArray[2];
    //                        string host = proxyArray[3];
    //                        int port = int.Parse(proxyArray[4]);
    //                        request.Proxy = new HttpProxyClient(host, port, username, password);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        project.L0g($"!W proxy parse err '{proxy}': {ex.Message}");
    //                        throw;
    //                    }
    //                }
    //                try
    //                {
    //                    HttpResponse httpResponse = request.Get(url);
    //                    return httpResponse.ToString();
    //                }
    //                catch (HttpException ex)
    //                {
    //                    project.SendErrorToLog($"Request err: {ex.Message}");
    //                    throw;
    //                }
    //            }
    //        }
    //    }

    //}

    public static class Requests
    {
        private static readonly object LockObject = new object();

        public static string GET(
            this IZennoPosterProjectModel project,
            string url,
            string proxy = "",
            bool log = false,
            bool parseJson = false,
            int deadline = 30,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            var logger = new Logger(project, log, classEmoji: "↑↓");
            string debugProxy = proxy;

            try
            {
                string response;

                lock (LockObject)
                {
                    string proxyString = ParseProxy(project, proxy, logger, callerName);
                    response = ZennoPoster.HTTP.Request(
                        ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.GET,
                        url,
                        "application/json",
                        "",
                        proxyString,
                        "UTF-8",
                        ResponceType.BodyOnly,
                        deadline * 1000,
                        "",
                        project.Profile.UserAgent,
                        true,
                        5,
                        null,
                        "",
                        false,
                        false,
                        project.Profile.CookieContainer);
                    if (parseJson) ParseJson(project, response, logger);
                    if (log) logger.Send($"({callerName}) [GET] json received: [{response}]");
                }


                return response.Trim();
            }
            catch (Exception e)
            {
                logger.Send($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: [{debugProxy})]");
                if (throwOnFail) throw;
                return string.Empty;
            }
        }

        public static string POST(
            this IZennoPosterProjectModel project,
            string url,
            string body,
            string proxy = "",
            bool log = false,
            bool parseJson = false,
            int deadline = 30,
            [CallerMemberName] string callerName = "",
            bool throwOnFail = false)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            var logger = new Logger(project, log, classEmoji: "↑↓");
            string debugProxy = proxy;

            try
            {
                string response;
                lock (LockObject)
                {
                    string proxyString = ParseProxy(project, proxy, logger, callerName);
                    response = ZennoPoster.HTTP.Request(
                        ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST,
                        url,
                        body,
                        "application/json",
                        proxyString,
                        "UTF-8",
                        ResponceType.BodyOnly,
                        deadline * 1000,
                        "",
                        project.Profile.UserAgent,
                        true,
                        5,
                        null,
                        "",
                        false,
                        false,
                        project.Profile.CookieContainer);
                }

                if (log) logger.Send($"({callerName}) [POST] body sent: [{body}]");
                if (parseJson) ParseJson(project, response, logger);
                if (log) logger.Send($"({callerName}) [POST] json received: [{response}]");
                return response.Trim();
            }
            catch (Exception e)
            {
                logger.Send($"!W [POST] RequestErr: [{e.Message}] url:[{url}] (proxy: [{debugProxy})]");
                if (throwOnFail) throw;
                return string.Empty;
            }
        }

        private static string ParseProxy(IZennoPosterProjectModel project, string proxyString, Logger logger, string callerName)
        {
            if (string.IsNullOrEmpty(proxyString))
            {
                return "";
            }

            if (proxyString == "+")
            {
                proxyString = new Sql(project).Get("proxy", "private_profile");
                logger.Send($"({callerName}) Proxy retrieved from SQL: [{proxyString}]");
            }

            try
            {
                if (proxyString.Contains("//"))
                {
                    proxyString = proxyString.Split('/')[2];
                }

                if (proxyString.Contains("@")) // Proxy with authorization (login:pass@proxy:port)
                {
                    string[] parts = proxyString.Split('@');
                    string credentials = parts[0];
                    string proxyHost = parts[1];
                    string[] creds = credentials.Split(':');
                    logger.Send($"({callerName}) Proxy set with credentials: [{proxyHost}]");
                    return $"http://{creds[0]}:{creds[1]}@{proxyHost}";
                }
                else // Proxy without authorization (proxy:port)
                {
                    logger.Send($"({callerName}) Proxy set: [{proxyString}]");
                    return $"http://{proxyString}";
                }
            }
            catch (Exception e)
            {
                logger.Send($"({callerName}) Proxy parsing error: [{e.Message}] [{proxyString}]");
                return "";
            }
        }

        private static void ParseJson(IZennoPosterProjectModel project, string json, Logger logger)
        {
            try
            {
                project.Json.FromString(json);
            }
            catch (Exception ex)
            {
                logger.Send($"[!W JSON parsing error: {ex.Message}] [{json}]");
            }
        }
    }



}



