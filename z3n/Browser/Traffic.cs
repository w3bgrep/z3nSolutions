using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{


    public class Traffic
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;


        public Traffic(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "🌎");
        }


        public string Get(string url, string parametr, bool reload = false)
        {
            string param;
            Dictionary<string, string> data = Get(url, reload);
            if (string.IsNullOrEmpty(parametr))
                return string.Join("\n", data.Select(kvp => $"{kvp.Key}-{kvp.Value}"));
            else data.TryGetValue(parametr, out param);
            return param;
        }
        public Dictionary<string, string> Get(string url, bool reload = false,int deadline = 10)
        {
            _project.Deadline();
            _instance.UseTrafficMonitoring = true;
            if (reload) _instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");

            get:
            _project.Deadline(deadline);
            Thread.Sleep(1000);
            var traffic = _instance.ActiveTab.GetTraffic();
            var data = new Dictionary<string, string>();
            string param;
            foreach (var t in traffic)
            {
                if (t.Url.Contains(url))
                {
                    var Method = t.Method;
                    var ResultCode = t.ResultCode.ToString();
                    var Url = t.Url;
                    var ResponseContentType = t.ResponseContentType;
                    var RequestHeaders = t.RequestHeaders;
                    var RequestCookies = t.RequestCookies;
                    var RequestBody = t.RequestBody;
                    var ResponseHeaders = t.ResponseHeaders;
                    var ResponseCookies = t.ResponseCookies;
                    var ResponseBody = t.ResponseBody == null ? "" : Encoding.UTF8.GetString(t.ResponseBody, 0, t.ResponseBody.Length);

                    if (Method == "OPTIONS") continue;
                    data.Add("Method", Method);
                    data.Add("ResultCode", ResultCode);
                    data.Add("Url", Url);
                    data.Add("ResponseContentType", ResponseContentType);
                    data.Add("RequestHeaders", RequestHeaders);
                    data.Add("RequestCookies", RequestCookies);
                    data.Add("RequestBody", RequestBody);
                    data.Add("ResponseHeaders", ResponseHeaders);
                    data.Add("ResponseCookies", ResponseCookies);
                    data.Add("ResponseBody", ResponseBody);
                    break;
                }
            }
            if (data.Count == 0) goto get;
            return data;
        }
        //public string GetHeader(string url, string headerToGet = "Authorization", bool trim = true, bool reload = false)
        //{
        //    Dictionary<string, string> data = Get(url, reload);
        //    data.TryGetValue("RequestHeaders", out string headersString);

        //    var headers = headersString.Split('\n');

        //    foreach (string header in headers)
        //    {
        //        string headerName = header.Split(':')[0];
        //        string headerValue = header.Split(':')[1];
        //        data.Add(headerName, headerValue);
        //    }

        //    data.TryGetValue(headerToGet, out string Value);

        //    if (trim) Value = Value.Replace("Bearer", "").Trim();
        //    return Value;

        //}
        public string GetHeader(string url, string headerToGet = "Authorization", bool reload = false)
        {
            Dictionary<string, string> data = Get(url, reload);
            data.TryGetValue("RequestHeaders", out string headersString);

            if (string.IsNullOrEmpty(headersString))
                return string.Empty;

            var headers = headersString.Split('\n');

            foreach (string header in headers)
            {
                var parts = header.Split(':');
                if (parts.Length >= 2) // Проверка на корректный формат заголовка
                {
                    string headerName = parts[0].Trim();
                    string headerValue = parts[1].Trim();
                    data[headerName] = headerValue; // Обновление или добавление значения
                }
            }

            data.TryGetValue(headerToGet, out string value);
            return value?.Trim() ?? string.Empty; // Возвращаем пустую строку, если значение не найдено
        }
    }
}