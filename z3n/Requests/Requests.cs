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

    }




}



