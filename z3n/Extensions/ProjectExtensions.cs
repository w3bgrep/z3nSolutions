using Leaf.xNet;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
//using static Global.Env.EnvironmentVariables;



namespace z3n
{
    
    public static class ProjectExtensions
    {
        private static readonly object LockObject = new object();
        private static readonly object FileLock = new object();


        public static void L0g(this IZennoPosterProjectModel project, string toLog, [CallerMemberName] string callerName = "", bool show = true, bool thr0w = false ,bool toZp = true)
        {
            new Logger(project).Send(toLog,show:show,thr0w:thr0w,toZp:toZp);
        }
        public static string[] GetErr(this IZennoPosterProjectModel project, Instance instance)
        {
            var error = project.GetLastError();
            var ActionId = error.ActionId.ToString();
            var ActionComment = error.ActionComment;
            var exception = error.Exception;

            if (exception != null)
            {
                var typeEx = exception.GetType();
                string type = typeEx.Name;
                string msg = exception.Message;
                string StackTrace = exception.StackTrace;
                StackTrace = StackTrace.Split('в')[1].Trim();
                string innerMsg = string.Empty;

                var innerException = exception.InnerException;
                if (innerException != null) innerMsg = innerException.Message;


                string toLog = $"{ActionId}\n{type}\n{msg}\n{StackTrace}\n{innerMsg}";

                project.SendErrorToLog(toLog);

                string failReport = $"⛔️\\#fail  \\#{project.Name.EscapeMarkdown()} \\#{project.Variables["acc0"].Value} \n" +
                                    $"Error: `{ActionId.EscapeMarkdown()}` \n" +
                                    $"Type: `{type.EscapeMarkdown()}` \n" +
                                    $"Msg: `{msg.EscapeMarkdown()}` \n" +
                                    $"Trace: `{StackTrace.EscapeMarkdown()}` \n";

                if (!string.IsNullOrEmpty(innerMsg)) failReport += $"Inner: `{innerMsg.EscapeMarkdown()}` \n";
                if (instance.GetType().ToString() != "ZennoLab.CommandCenter.EmptyInstance")
                {
                    if (instance.BrowserType.ToString() == "Chromium") failReport += $"Page: `{instance.ActiveTab.URL.EscapeMarkdown()}`";
                }
                project.Variables["failReport"].Value = failReport;
                try
                {
                    string path = $"{project.Path}.failed\\{project.Variables["projectName"].Value}\\{project.Name} • {project.Variables["acc0"].Value} • {project.LastExecutedActionId} • {ActionId}.jpg";
                    ZennoPoster.ImageProcessingResizeFromScreenshot(instance.Port, path, 50, 50, "percent", true, false);
                }
                catch (Exception e) { project.SendInfoToLog(e.Message); }
                return new string[] { ActionId, type, msg, StackTrace, innerMsg };

            }

            return null;


        }

        #region Vars
        public static void acc0w(this IZennoPosterProjectModel project, object acc0)
        {
            project.Variables["acc0"].Value = acc0?.ToString() ?? string.Empty;
        }
        public static void NullVars(this IZennoPosterProjectModel project, object acc0)
        {
            project.GlobalNull();
            project.Var("acc0", null);

        }

        public static int Range(this IZennoPosterProjectModel project, string accRange = null, string output = null, bool log = false)
        {
            if (string.IsNullOrEmpty(accRange)) accRange = project.Variables["cfgAccRange"].Value;
            if (string.IsNullOrEmpty(accRange)) throw new Exception("range is not provided by input or project setting [cfgAccRange]");
            int rangeS, rangeE;
            string range;

            if (accRange.Contains(","))
            {
                range = accRange;
                var rangeParts = accRange.Split(',').Select(int.Parse).ToArray();
                rangeS = rangeParts.Min();
                rangeE = rangeParts.Max();
            }
            else if (accRange.Contains("-"))
            {
                var rangeParts = accRange.Split('-').Select(int.Parse).ToArray();
                rangeS = rangeParts[0];
                rangeE = rangeParts[1];
                range = string.Join(",", Enumerable.Range(rangeS, rangeE - rangeS + 1));
            }
            else
            {
                rangeE = int.Parse(accRange);
                rangeS = int.Parse(accRange);
                range = accRange;
            }
            project.Variables["rangeStart"].Value = $"{rangeS}";
            project.Variables["rangeEnd"].Value = $"{rangeE}";
            project.Variables["range"].Value = range;
            return rangeE;
            //project.L0g($"{rangeS}-{rangeE}\n{range}");
        }
        public static int VarCounter(this IZennoPosterProjectModel project, string varName, int input)
        {
            project.Variables[$"{varName}"].Value = (int.Parse(project.Variables[$"{varName}"].Value) + input).ToString();
            return int.Parse(project.Variables[varName].Value);
        }
        public static string Var(this IZennoPosterProjectModel project, string Var)
        {
            string value = string.Empty;
            try
            {
                value = project.Variables[Var].Value;
            }
            catch (Exception e)
            {
                project.SendInfoToLog(e.Message);               
            }
            if (value == string.Empty)
            { }// project.L0g($"no Value from [{Var}] `w");

            return value;
        }
        public static string Var(this IZennoPosterProjectModel project, string var, object value)
        {
            try
            {
               project.Variables[var].Value = value.ToString();
            }
            catch (Exception e)
            {
                project.SendInfoToLog(e.Message);
            }
            return string.Empty;
        }
        public static string VarRnd(this IZennoPosterProjectModel project, string Var)
        {
            string value = string.Empty;
            try
            {
                value = project.Variables[Var].Value;
            }
            catch (Exception e)
            {
                project.SendInfoToLog(e.Message);
            }
            if (value == string.Empty) project.L0g($"no Value from [{Var}] `w");

            if (value.Contains("-"))
            {
                var min = int.Parse(value.Split('-')[0].Trim());
                var max = int.Parse(value.Split('-')[1].Trim());
                return new Random().Next(min, max).ToString();
            }
            return value.Trim();
        }
        public static decimal VarsMath(this IZennoPosterProjectModel project, string varA, char operation, string varB, string varRslt = "a_")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            decimal a = decimal.Parse(project.Var(varA));
            decimal b = decimal.Parse(project.Var(varB));
            decimal result;
            switch (operation) 
            {
                case '+':

                    result = a + b;
                    break;
                case '-':
                    result = a - b;
                    break;
                case '*':
                    result = a * b;
                    break;
                case '/':
                    result = a / b;
                    break;
                default:
                    throw new Exception($"unsuppoted operation {operation}");
            }
            try { project.Var(varRslt, $"{result}"); } catch { }
            return result;
        }

        #endregion

        #region GlobalVars

        public static bool GlobalSet(this IZennoPosterProjectModel project, bool log = false)
        {
            string nameSpase = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyTitleAttribute>()
            ?.Title ?? "Unknown";

            var cleaned = new List<int>();
            var notDeclared = new List<int>();
            var busyAccounts = new List<string>();


            lock (LockObject)
            {
                try
                {

                    for (int i = 1; i <= int.Parse(project.Variables["rangeEnd"].Value); i++)
                    {
                        string threadKey = $"acc{i}";
                        try
                        {
                            var globalVar = project.GlobalVariables[nameSpase, threadKey];
                            if (globalVar != null)
                            {
                                if (!string.IsNullOrEmpty(globalVar.Value))
                                {
                                    //busyAccounts.Add(i);
                                    busyAccounts.Add($"{i}:{globalVar.Value}");
                                }
                                if (project.Variables["cleanGlobal"].Value == "True")
                                {
                                    globalVar.Value = string.Empty;
                                    cleaned.Add(i);
                                }
                            }
                            else notDeclared.Add(i);
                        }
                        catch { notDeclared.Add(i); }
                    }
                    if (project.Variables["cleanGlobal"].Value == "True")
                    {

                        project.L0g($"!W cleanGlobal is [on] Cleaned: {string.Join(",", cleaned)}");
                    }
                    else
                    {
                        project.L0g($"buzy Accounts: [{string.Join(" | ", busyAccounts)}]");
                    }
                    int currentThread = int.Parse(project.Variables["acc0"].Value);
                    string currentThreadKey = $"acc{currentThread}";
                    if (!busyAccounts.Any(x => x.StartsWith($"{currentThread}:"))) //
                    {
                        try
                        {
                            project.GlobalVariables.SetVariable(nameSpase, currentThreadKey, project.Variables["projectName"].Value);
                        }
                        catch
                        {
                            project.GlobalVariables[nameSpase, currentThreadKey].Value = project.Variables["projectName"].Value;
                        }
                        if (log) project.L0g($"{currentThreadKey} bound to {project.Variables["projectName"].Value}");
                        return true;
                    }
                    else
                    {
                        if (log) project.L0g($"{currentThreadKey} is already busy!");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    if (log) project.L0g($"⚙  {ex.Message}");
                    throw;
                }
            }
        }
        public static void GlobalGet(this IZennoPosterProjectModel project)
        {
            string nameSpase = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyTitleAttribute>()
            ?.Title ?? "Unknown";
            var cleaned = new List<int>();
            var notDeclared = new List<int>();
            var busyAccounts = new List<string>();

            try
            {

                for (int i = int.Parse(project.Variables["rangeStart"].Value); i <= int.Parse(project.Variables["rangeEnd"].Value); i++)
                {
                    string threadKey = $"acc{i}";
                    try
                    {
                        var globalVar = project.GlobalVariables[nameSpase, threadKey];
                        if (globalVar != null)
                        {
                            if (!string.IsNullOrEmpty(globalVar.Value)) busyAccounts.Add(i.ToString());
                            if (project.Variables["cleanGlobal"].Value == "True")
                            {
                                globalVar.Value = string.Empty;
                                cleaned.Add(i);
                            }
                        }
                        else notDeclared.Add(i);
                    }
                    catch { notDeclared.Add(i); }
                }
                if (project.Variables["cleanGlobal"].Value == "True") project.L0g($"GlobalVars cleaned: {string.Join(",", cleaned)}");
                else project.Variables["busyAccounts"].Value = string.Join(",", busyAccounts);
            }
            catch (Exception ex) { project.L0g($"⚙  {ex.Message}"); }

        }
        public static void GlobalNull(this IZennoPosterProjectModel project)
        {
            string nameSpase = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyTitleAttribute>()
            ?.Title ?? "Unknown";
            try
            {               
                project.GlobalVariables[nameSpase, $"acc{project.Variables["acc0"].Value}"].Value = null;
                project.Var("acc0", null);
            }
            catch { }

        }

        #endregion




        public static bool ChooseSingleAcc(this IZennoPosterProjectModel project)
        {
            var listAccounts = project.Lists["accs"];
            string pathProfiles = project.Var("profiles_folder");

        check:
            if (listAccounts.Count == 0)
            {
                project.Variables["noAccsToDo"].Value = "True";
                project.SendToLog($"♻ noAccoutsAvaliable", LogType.Info, true, LogColor.Turquoise);
                project.Variables["acc0"].Value = "";
                return false;
                throw new Exception($"TimeToChill");
            }

            int randomAccount = new Random().Next(0, listAccounts.Count);
            string acc0 = listAccounts[randomAccount];
            project.Var("acc0", acc0);
            listAccounts.RemoveAt(randomAccount);
            if (!project.GlobalSet())
                goto check;
            project.Var("pathProfileFolder", $"{pathProfiles}accounts\\profilesFolder\\{acc0}");
            project.Var("pathCookies", $"{pathProfiles}accounts\\cookies\\{acc0}.json");
            project.L0g($"`working with: [acc{acc0}] accs left: [{listAccounts.Count}]");
            return true;
        }
        public static string GetNewCreds(this IZennoPosterProjectModel project, string dataType)
        {
            string pathFresh = $"{project.Path}.data\\fresh\\{dataType}.txt";
            string pathUsed = $"{project.Path}.data\\used\\{dataType}.txt";

            lock (FileLock)
            {
                try
                {
                    if (!File.Exists(pathFresh))
                    {
                        project.SendWarningToLog($"File not found: {pathFresh}");
                        return null;
                    }

                    var freshAccs = File.ReadAllLines(pathFresh).ToList();
                    project.SendInfoToLog($"Loaded {freshAccs.Count} accounts from {pathFresh}");

                    if (freshAccs.Count == 0)
                    {
                        project.SendInfoToLog($"No accounts available in {pathFresh}");
                        return string.Empty;
                    }

                    string creds = freshAccs[0];
                    freshAccs.RemoveAt(0);

                    File.WriteAllLines(pathFresh, freshAccs);
                    File.AppendAllText(pathUsed, creds + Environment.NewLine);

                    return creds;
                }
                catch (Exception ex)
                {
                    project.SendWarningToLog($"Error processing files for {dataType}: {ex.Message}");
                    return null;
                }
            }

        }       
        public static Dictionary<string, string> TblMap(this IZennoPosterProjectModel project, string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''")
        {
            if (string.IsNullOrEmpty(dynamicToDo)) dynamicToDo = project.Variables["cfgToDo"].Value;


            var tableStructure = new Dictionary<string, string>
        {
            { "acc0", "INTEGER PRIMARY KEY" }
        };
            foreach (string name in staticColumns)
            {
                if (!tableStructure.ContainsKey(name))
                {
                    tableStructure.Add(name, defaultType);
                }
            }
            string[] toDoItems = (dynamicToDo ?? "").Split(',');
            foreach (string taskId in toDoItems)
            {
                string trimmedTaskId = taskId.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedTaskId) && !tableStructure.ContainsKey(trimmedTaskId))
                {
                    tableStructure.Add(trimmedTaskId, defaultType);
                }
            }
            return tableStructure;
        }


    }
}
