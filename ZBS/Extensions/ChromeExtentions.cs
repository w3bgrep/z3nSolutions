using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;


namespace ZBSolutions
{
    public class ChromeExt
    {

        protected readonly IZennoPosterProjectModel _project;
        protected bool _logShow = false;
        protected readonly Instance _instance;
        private readonly Logger _logger;

        public ChromeExt(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            if (!log) _logShow = _project.Var("debug") == "True";
            _logger = new Logger(project);

        }
        public ChromeExt(IZennoPosterProjectModel project, Instance instance,  bool log = false)
        {
            _project = project;
            _instance = instance;
            if (!log) _logShow = _project.Var("debug") == "True";
            _logger = new Logger(project);

        }

        public string GetVer(string extId)
        {
            string securePrefsPath = _project.Variables["pathProfileFolder"].Value + @"\Default\Secure Preferences";
            string json = File.ReadAllText(securePrefsPath);
            JObject jObj = JObject.Parse(json);
            JObject settings = (JObject)jObj["extensions"]?["settings"];

            if (settings == null)
            {
                throw new Exception("Секция extensions.settings не найдена");
            }

            JObject extData = (JObject)settings[extId];
            if (extData == null)
            {
                throw new Exception($"Расширение с ID {extId} не найдено");
            }

            string version = (string)extData["manifest"]?["version"];
            if (string.IsNullOrEmpty(version))
            {
                throw new Exception($"Версия для расширения {extId} не найдена");
            }
            return version;
        }

        public bool Install(string extId, string fileName, bool log = false)
        {
            string path = $"{_project.Path}.crx\\{fileName}";

            if (!File.Exists(path))
            {
                _logger.Send($"File not found: {path}");
                throw new FileNotFoundException($"CRX file not found: {path}");
            }

            var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
            if (!extListString.Contains(extId))
            {
                _logger.Send($"installing {path}");
                _instance.InstallCrxExtension(path);
                return true;
            }
            return false;
        }



    }
}
