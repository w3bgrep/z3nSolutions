﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
//using static Global.FileSystem.FileHelper;

namespace z3n
{
    public class Sys
    {
        protected readonly IZennoPosterProjectModel _project;
        protected bool _logShow = false;
        private readonly Logger _logger;
        private readonly object LockObject = new object();
        private readonly object FileLock = new object();

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
                                using (Process process = new Process())
                                {
                                    process.StartInfo.FileName = "cmd.exe";
                                    process.StartInfo.Arguments = $"/c mklink /d \"{logsPath}\" \"NUL\"";
                                    process.StartInfo.UseShellExecute = false;
                                    process.StartInfo.CreateNoWindow = true;
                                    process.StartInfo.RedirectStandardOutput = true;
                                    process.StartInfo.RedirectStandardError = true;

                                    process.Start();
                                    string output = process.StandardOutput.ReadToEnd();
                                    string error = process.StandardError.ReadToEnd();
                                    process.WaitForExit();
                                }
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
