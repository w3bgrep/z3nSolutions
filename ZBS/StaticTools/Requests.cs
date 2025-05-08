
using Leaf.xNet;
using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    internal static class Requests
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







}



