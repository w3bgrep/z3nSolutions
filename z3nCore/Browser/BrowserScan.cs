﻿
using System.Text.RegularExpressions;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
{
    public class BrowserScan
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        public BrowserScan(IZennoPosterProjectModel project, Instance instance, bool log = false)

        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "🌎");
        }

        private void AddTable()
        {
            var sql = new Sql(_project);
            string[] columns = { "score", "webgl", "webglreport", "unmaskedrenderer", "audio", "clientRects", "WebGPUReport", "Fonts", "TimeZoneBasedonIP", "TimeFromIP" };

            var tableStructure = sql.TblMapForProject(columns);
            var tblName = "public_browser";

            sql.TblAdd(tblName, tableStructure);
            sql.ClmnAdd(tblName, tableStructure);
            sql.ClmnPrune(tblName, tableStructure);
            sql.AddRange(tblName);
        }

        private void LoadStats()
        {
            _instance.Go("https://www.browserscan.net/", true);
        loading:
            _logger.Send("loading...");
            try
            {
                _instance.HeGet(("div", "outerhtml", "use xlink:href=\"#etc2\"", "regexp", 0), deadline: 3);
                goto loading;
            }
            catch
            {
                _logger.Send("loaded");
            }
        }

        public void ParseStats()
        {
            AddTable();
            var _sql = new Sql(_project);
            var toParse = "WebGL,WebGLReport, Audio, ClientRects, WebGPUReport,Fonts,TimeZoneBasedonIP,TimeFromIP";
            var tableName = "public_browser";
            string timezoneOffset = "";
            string timezoneName = "";

            LoadStats();

            var hardware = _instance.ActiveTab.FindElementById("webGL_anchor").ParentElement.GetChildren(false);

            foreach (ZennoLab.CommandCenter.HtmlElement child in hardware)
            {
                var text = child.GetAttribute("innertext");
                var varName = Regex.Replace(text.Split('\n')[0], " ", ""); var varValue = "";
                if (varName == "") continue;
                if (toParse.Contains(varName))
                {
                    try { varValue = text.Split('\n')[2]; } catch { Thread.Sleep(2000); continue; }
                    var upd = $"{varName} = '{varValue}'";
                    //upd = QuoteColumnNames(upd);
                    _sql.Upd(upd, tableName);
                }
            }

            var software = _instance.ActiveTab.FindElementById("lang_anchor").ParentElement.GetChildren(false);
            foreach (ZennoLab.CommandCenter.HtmlElement child in software)
            {
                var text = child.GetAttribute("innertext");
                var varName = Regex.Replace(text.Split('\n')[0], " ", ""); var varValue = "";
                if (varName == "") continue;
                if (toParse.Contains(varName))
                {
                    if (varName == "TimeZone") continue;
                    try { varValue = text.Split('\n')[1]; } catch { continue; }
                    if (varName == "TimeFromIP") timezoneOffset = varValue;
                    if (varName == "TimeZoneBasedonIP") timezoneName = varValue;
                    var upd = $"{varName} = '{varValue}'";
                    //upd = QuoteColumnNames(upd);
                    _sql.Upd(upd, tableName);
                }
            }


        }

        public string GetScore()
        {
            LoadStats();
            string heToWait = _instance.HeGet(("anchor_progress", "id"));
            var score = heToWait.Split(' ')[3].Split('\n')[0]; var problems = "";
            if (!score.Contains("100%"))
            {
                var problemsHe = _instance.ActiveTab.FindElementByAttribute("ul", "fulltagname", "ul", "regexp", 5).GetChildren(false);
                foreach (ZennoLab.CommandCenter.HtmlElement child in problemsHe)
                {
                    var text = child.GetAttribute("innertext");
                    var varValue = "";
                    var varName = text.Split('\n')[0];
                    try { varValue = text.Split('\n')[1]; } catch { continue; }
                    ;
                    problems += $"{varName}: {varValue}; ";
                }
                problems = problems.Trim();

            }
            score = $"[{score}] {problems}";
            return score;
        }

        public void FixTime()
        {
            LoadStats();
            string timezoneOffset = "";
            string timezoneName = "";
            var toParse = "TimeZoneBasedonIP,TimeFromIP";

            var software = _instance.ActiveTab.FindElementById("lang_anchor").ParentElement.GetChildren(false);
            foreach (ZennoLab.CommandCenter.HtmlElement child in software)
            {
                var text = child.GetAttribute("innertext");
                var varName = Regex.Replace(text.Split('\n')[0], " ", "");
                var varValue = "";
                if (varName == "") continue;
                if (toParse.Contains(varName))
                {
                    if (varName == "TimeZone") continue;
                    try { varValue = text.Split('\n')[1]; } catch { continue; }
                    if (varName == "TimeFromIP") timezoneOffset = varValue;
                    if (varName == "TimeZoneBasedonIP") timezoneName = varValue;
                }
            }

            var match = Regex.Match(timezoneOffset, @"GMT([+-]\d{2})");
            if (match.Success)
            {
                int Offset = int.Parse(match.Groups[1].Value);
                _logger.Send($"Setting timezone offset to: {Offset}");
                _instance.TimezoneWorkMode = ZennoLab.InterfacesLibrary.Enums.Browser.TimezoneMode.Emulate;
                _instance.SetTimezone(Offset, 0);
            }
            _instance.SetIanaTimezone(timezoneName);

        }

    }
}
