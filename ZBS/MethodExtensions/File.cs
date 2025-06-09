using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class Sys
    {
        protected readonly IZennoPosterProjectModel _project;
        protected bool _logShow = false;
        private readonly Logger _logger;

        public Sys(IZennoPosterProjectModel project, bool log = false, string classEmoji = null)
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

    }
}
