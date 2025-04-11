try
{
    string githubApiBase = "https://api.github.com/repos/w3bgrep/w3tools/contents/";
    string githubRawBase = "https://raw.githubusercontent.com/w3bgrep/w3tools/master/";
    string externalAssembliesRepoPath = "ExternalAssemblies";
    string w3toolsFile = "w3t00ls.cs";
	string driverFile = "sqliteodbc_w64.exe";
	string driverUrl = "https://raw.githubusercontent.com/w3bgrep/w3tools/master/drivers/sqliteodbc_w64.exe";
	

    string targetExternalAssemblies = null;
    string latestVersionPath = null;
    Version latestVersion = null;

    foreach (var drive in DriveInfo.GetDrives())
    {
        if (drive.DriveType != DriveType.Fixed || !drive.IsReady) continue;
        string driveRoot = drive.RootDirectory.FullName; 
        string[] localizations = { "RU", "EN" };
        foreach (string loc in localizations)
        {
            string basePath = Path.Combine(driveRoot, "Program Files", "ZennoLab", loc, "ZennoPoster Pro V7");
            if (!Directory.Exists(basePath)) continue;
            foreach (string versionDir in Directory.GetDirectories(basePath))
            {
                string versionName = Path.GetFileName(versionDir);
                try
                {
                    Version currentVersion = new Version(versionName);
                    string potentialPath = Path.Combine(versionDir, "Progs", "ExternalAssemblies");
                    if (Directory.Exists(potentialPath))
                    {
                        if (latestVersion == null || currentVersion > latestVersion)
                        {
                            latestVersion = currentVersion;
                            latestVersionPath = potentialPath;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
    }

    targetExternalAssemblies = latestVersionPath;
    if (targetExternalAssemblies == null)  throw new Exception("ExternalAssemblies not found");
    project.SendInfoToLog($"folder is: {targetExternalAssemblies}", true);


    string projectW3ToolsPath = project.Path + @".w3tools\";
    if (!Directory.Exists(projectW3ToolsPath))
    {
        Directory.CreateDirectory(projectW3ToolsPath);
        project.SendInfoToLog($"mkdir: {projectW3ToolsPath}", true);
    }

    using (var client = new System.Net.WebClient())
    {
        client.Headers.Add("User-Agent", "Mozilla/5.0");
        string apiUrl = githubApiBase + externalAssembliesRepoPath;
        string response = client.DownloadString(apiUrl);
        project.SendInfoToLog($"apiResp {response}", true);
        var fileNames = new System.Collections.Generic.List<string>();
        string pattern = @"""name"":""([^""]+\.\w+)""";
        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(response, pattern))
        {
            fileNames.Add(match.Groups[1].Value);
        }
        project.SendInfoToLog($"Files in repo: {fileNames.Count}", true);
        foreach (string fileName in fileNames)
        {
            string sourceUrl = githubRawBase + externalAssembliesRepoPath + "/" + fileName;
            string targetPath = Path.Combine(targetExternalAssemblies, fileName);

			if (!File.Exists(targetPath))
			{
			    try
			    {
			        project.SendInfoToLog($"geting {targetPath} from {sourceUrl}", true);
			        client.DownloadFile(sourceUrl, targetPath);
			        project.SendInfoToLog($"Successfully downloaded {fileName}", true);
			    }
			    catch (WebException wex)
			    {
			        string errorDetails = wex.Message;
			        if (wex.Response is HttpWebResponse resp1)
			        {
			            errorDetails += $"; HTTP Status: {(int)resp1.StatusCode} {resp1.StatusDescription}";
			        }
			        project.SendWarningToLog($"ERR geting {targetPath} from {sourceUrl}: {errorDetails}", true);
			    }
			    catch (Exception ex)
			    {
			        project.SendWarningToLog($"ERR geting {targetPath} from {sourceUrl}: {ex.Message}", true);
			    }
			}
			else
			{
			    project.SendInfoToLog($"File {fileName} already exists, skipping", true);
			}
        }

        // Скачиваем w3t00ls.cs
        string w3toolsUrl = githubRawBase + w3toolsFile;
        string w3toolsTargetPath = Path.Combine(projectW3ToolsPath, w3toolsFile);

		if (!File.Exists(w3toolsTargetPath))
		{
		    try
		    {
		        project.SendInfoToLog($"geting {w3toolsTargetPath} from {w3toolsUrl}", true);
		        client.DownloadFile(w3toolsUrl, w3toolsTargetPath);
		        project.SendInfoToLog($"Successfully downloaded {w3toolsFile}", true);
		    }
		    catch (WebException wex)
		    {
		        string errorDetails = wex.Message;
		        if (wex.Response is HttpWebResponse resp2)
		        {
		            errorDetails += $"; HTTP Status: {(int)resp2.StatusCode} {resp2.StatusDescription}";
		        }
		        project.SendWarningToLog($"ERR geting {w3toolsTargetPath} from {w3toolsUrl}: {errorDetails}", true);
		    }
		    catch (Exception ex)
		    {
		        project.SendWarningToLog($"ERR geting {w3toolsTargetPath} from {w3toolsUrl}: {ex.Message}", true);
		    }
		}
		else
		{
		    project.SendInfoToLog($"File {w3toolsFile} already exists, skipping", true);
		}
		
		
		string driverTargetPath = Path.Combine(project.Path, driverFile);
		if (!File.Exists(driverTargetPath))
		{
		    try
		    {
		        project.SendInfoToLog($"geting {driverTargetPath} from {driverUrl}", true);
		        client.DownloadFile(driverUrl, driverTargetPath);
		        project.SendInfoToLog($"Successfully downloaded {driverFile}", true);
		    }
		    catch (WebException wex)
		    {
		        string errorDetails = wex.Message;
		        if (wex.Response is HttpWebResponse resp2)
		        {
		            errorDetails += $"; HTTP Status: {(int)resp2.StatusCode} {resp2.StatusDescription}";
		        }
		        project.SendWarningToLog($"ERR geting {w3toolsTargetPath} from {w3toolsUrl}: {errorDetails}", true);
		    }
		    catch (Exception ex)
		    {
		        project.SendWarningToLog($"ERR geting {driverTargetPath} from {driverUrl}: {ex.Message}", true);
		    }
		}
		else
		{
		    project.SendInfoToLog($"File {driverFile} already exists, skipping", true);
		}		
		
		
    }
}
catch (Exception ex)
{
    project.SendWarningToLog($"ERR {ex.Message}", true);
}

try
{
    string driverFile = "sqliteodbc_w64.exe";
    string driverPath = Path.Combine(project.Path, driverFile);

    if (!File.Exists(driverPath))
    {
        project.SendWarningToLog($"no {driverFile} in {project.Path}", true);
        return 0;
    }
    project.SendInfoToLog($"Found: {driverPath}", true);


    try
    {
        project.SendInfoToLog($"Execute install {driverFile}", true);
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = driverPath,
            UseShellExecute = true 
        };
        using (Process process = Process.Start(startInfo))
        {
            return 0;
        }
    }
    catch (Exception ex)
    {
        project.SendWarningToLog($"Execute err {driverFile}: {ex.Message}", true);
    }

    try
    {
        Process.Start("explorer.exe", $"/select,\"{driverPath}\"");
    }
    catch (Exception ex)
    {
        project.SendWarningToLog($"Err open folder {driverFile}: {ex.Message}", true);
    }
}
catch (Exception ex)
{
    project.SendWarningToLog($"Err: {ex.Message}", true);
}