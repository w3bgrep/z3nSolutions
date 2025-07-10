using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Leaf.xNet;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public class AI
    {
        public static string OptimizeCode(IZennoPosterProjectModel project, string content, bool log = false)
        {
            var _project = project;
            _project.Variables["api_response"].Value = "";
            var _logger = new Logger(project, log);

            var requestBody = new
            {
                model = "sonar",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                       content = "You are a web3 developer. Optimize the following code. Return only the optimized code. Do not add explanations, comments, or formatting. Output code only, in plain text."


                    },
                    new
                    {
                        role = "user",
                        content = content
                    }
                },
                temperature = 0.8,
                top_p = 0.9,
                top_k = 0,
                stream = false,
                presence_penalty = 0,
                frequency_penalty = 1
            };

            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, Newtonsoft.Json.Formatting.None);

            string[] headers = new string[]
            {
                "Content-Type: application/json",
                $"Authorization: Bearer {_project.Variables["settingsApiPerplexity"].Value}"
            };
        gen:
            string response;

            //response = _project.POST("https://api.perplexity.ai/chat/completions", jsonBody,"", headers, log);

            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 30000;

                foreach (var header in headers)
                {
                    var parts = header.Split(new[] { ": " }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        request.AddHeader(parts[0], parts[1]);
                    }
                }

                try
                {
                    HttpResponse httpResponse = request.Post("https://api.perplexity.ai/chat/completions", jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _logger.Send($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            _project.Variables["api_response"].Value = response;

            _logger.Send($"Full response: {response}");

            try
            {
                var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(response);
                string Text = jsonResponse["choices"][0]["message"]["content"].ToString();
                _logger.Send(Text);
                return Text;
            }
            catch (Exception ex)
            {
                _logger.Send($"!W Error parsing response: {ex.Message}");
                throw;
            }
        }

    }
}
