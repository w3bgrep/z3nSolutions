using System;
using System.Collections.Generic;

using System.Linq;

using System.Net.Http;
using System.Net;

using System.Text;
using System.Text.RegularExpressions;

using System.Threading;
using Newtonsoft.Json;
using ZennoLab.InterfacesLibrary.Enums.Browser;

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
using Nethereum.Model;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;


using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Numerics;

using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Nethereum.Model;

using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Reflection;
using System.Security.Policy;
using ZBSolutions;

#endregion

namespace w3tools //by @w3bgrep
{

    public  static class TestStatic
    {
        public static string QuoteColumnNames(string updateString)
        {
            var parts = updateString.Split(',').Select(p => p.Trim()).ToList();
            var result = new List<string>();

            foreach (var part in parts)
            {
                int equalsIndex = part.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string columnName = part.Substring(0, equalsIndex).Trim();
                    string valuePart = part.Substring(equalsIndex).Trim();
                    result.Add($"\"{columnName}\" {valuePart}");
                }
                else
                {
                    result.Add(part);
                }
            }
            return string.Join(", ", result);
        }

        public static string UnixToHuman(this IZennoPosterProjectModel project, string decodedResultExpire = null)
        {
            var _log = new Logger(project, classEmoji: "☻");
            if (string.IsNullOrEmpty(decodedResultExpire)) decodedResultExpire = project.Var("varSessionId");
            if (!string.IsNullOrEmpty(decodedResultExpire))
            {
                int intEpoch = int.Parse(decodedResultExpire);
                string converted = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(intEpoch).ToShortDateString();
                _log.Send(converted);
                return converted;

                
            }
            return string.Empty;
        }
        public static decimal Math(this IZennoPosterProjectModel project, string varA, string operation, string varB, string varRslt = "a_")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            decimal a = decimal.Parse(project.Var(varA));
            decimal b = decimal.Parse(project.Var(varB));
            decimal result;
            switch (operation)
            {
                case "+":

                    result = a + b;
                    break;
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    break;
                default:
                    throw new Exception($"unsuppoted operation {operation}");
            }
            try { project.Var(varRslt, $"{result}"); } catch { }
            return result;
        }
        public static string CookiesToJson(string cookies)
        {
            try
            {
                if (string.IsNullOrEmpty(cookies))
                {
                    return "[]";
                }

                var result = new List<Dictionary<string, string>>();
                var cookiePairs = cookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in cookiePairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                        continue;

                    var keyValue = trimmedPair.Split(new[] { '=' }, 2);
                    if (keyValue.Length != 2)
                    {
                        continue;
                    }

                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        result.Add(new Dictionary<string, string>
                    {
                        { "name", key },
                        { "value", value }
                    });
                    }
                }

                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                return json;
            }
            catch (Exception ex)
            {
                return "[]";
            }
        }

    }

    public class Sys2
    {
        protected readonly IZennoPosterProjectModel _project;
        protected bool _logShow = false;
        private readonly Logger _logger;

        public Sys2(IZennoPosterProjectModel project, bool log = false, string classEmoji = null)
        {
            _project = project;
            if (!log) _logShow = _project.Var("debug") == "True";
            _logger = new Logger(project, log: log, classEmoji: "⚙️");
        }

        public void RmRf(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    dir.Attributes = FileAttributes.Normal;

                    foreach (FileInfo file in dir.GetFiles())
                    {
                        file.IsReadOnly = false;
                        file.Delete();
                    }

                    foreach (DirectoryInfo subDir in dir.GetDirectories())
                    {
                        RmRf(subDir.FullName);
                    }
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Send(ex.Message);
            }
        }
        public void DisableLogs()
        {
            try
            {
                StringBuilder logBuilder = new StringBuilder();
                string basePath = @"C:\Program Files\ZennoLab";

                foreach (string langDir in Directory.GetDirectories(basePath))
                {
                    foreach (string programDir in Directory.GetDirectories(langDir))
                    {
                        foreach (string versionDir in Directory.GetDirectories(programDir))
                        {
                            string logsPath = Path.Combine(versionDir, "Progs", "Logs");
                            if (Directory.Exists(logsPath))
                            {
                                Directory.Delete(logsPath, true);
                                Process process = new Process();
                                process.StartInfo.FileName = "cmd.exe";
                                process.StartInfo.Arguments = $"/c mklink /d \"{logsPath}\" \"NUL\"";
                                process.StartInfo.UseShellExecute = false;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.RedirectStandardOutput = true;
                                process.StartInfo.RedirectStandardError = true;

                                logBuilder.AppendLine($"Attempting to create symlink: {process.StartInfo.Arguments}");

                                process.Start();
                                string output = process.StandardOutput.ReadToEnd();
                                string error = process.StandardError.ReadToEnd();
                                process.WaitForExit();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Send(ex.Message);
            }
        }

        public void CopyDir(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir)) throw new DirectoryNotFoundException("Source directory does not exist: " + sourceDir);
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            DirectoryInfo source = new DirectoryInfo(sourceDir);
            DirectoryInfo target = new DirectoryInfo(destDir);


            foreach (FileInfo file in source.GetFiles())
            {
                string targetFilePath = Path.Combine(target.FullName, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo subDir in source.GetDirectories())
            {
                string targetSubDirPath = Path.Combine(target.FullName, subDir.Name);
                CopyDir(subDir.FullName, targetSubDirPath);
            }
        }

    }



    



}
