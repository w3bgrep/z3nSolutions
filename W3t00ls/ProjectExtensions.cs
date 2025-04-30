using Global.WinApi;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading;
using System.IO;

using System.Globalization;

using System.Runtime.CompilerServices;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.CommandCenter;
using ZXing;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace W3t00ls
{
    public static class ProjectExtensions
    {
        private static readonly object LockObject = new object();
        public static void L0g(this IZennoPosterProjectModel project, string toLog, [CallerMemberName] string callerName = "", bool show = true, bool thr0w = false)
        {
            string acc0 = "null";
            string port = "null";
            string totalAge = "null";
            try { acc0 = project.Variables["acc0"].Value; } catch { }
            try { port = project.Variables["instancePort"].Value; } catch { }
            totalAge = project.Age<string>();
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();

            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = project.Variables["projectName"].Value;
            if (toLog == null) toLog = "null";
            string formated = $"⛑  [{acc0}] ⚙  [{port}] ⏱  [{totalAge}] ⛏  [{callerName}].\n          {toLog.Trim()}";
            LogType type = LogType.Info; LogColor color = LogColor.Default;

            if (formated.Contains("!W")) type = LogType.Warning;
            if (formated.Contains("!E")) type = LogType.Error;

            var colorMap = new Dictionary<string, LogColor>
                {
                    { "`.", LogColor.Default },
                    { "`w", LogColor.Gray },
                    { "`y", LogColor.Yellow },
                    { "`o", LogColor.Orange },
                    { "`r", LogColor.Red },
                    { "`p", LogColor.Pink },
                    { "`v", LogColor.Violet },
                    { "`b", LogColor.Blue },
                    { "`lb", LogColor.LightBlue },
                    { "`t", LogColor.Turquoise },
                    { "`g", LogColor.Green },
                    { "!W", LogColor.Orange },
                    { "!E", LogColor.Orange },
                    { "relax", LogColor.LightBlue },
                };

            foreach (var pair in colorMap)
            {
                if (formated.Contains(pair.Key))
                {
                    color = pair.Value;
                    break;
                }

            }

            project.SendToLog(formated, type, show, color);
            if (thr0w) throw new Exception($"{formated}");

        }

        public static bool SetGlobalVar(this IZennoPosterProjectModel project, bool log = false)
        {
            lock (LockObject)
            {
                try
                {
                    var nameSpase = "w3tools";
                    var cleaned = new List<int>();
                    var notDeclared = new List<int>();
                    //var busyAccounts = new List<int>();
                    var busyAccounts = new List<string>();
                    for (int i = int.Parse(project.Variables["rangeStart"].Value); i <= int.Parse(project.Variables["rangeEnd"].Value); i++)
                    {
                        string threadKey = $"Thread{i}";
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
                        project.L0g($"buzy Threads: [{string.Join(" | ", busyAccounts)}]");
                    }
                    int currentThread = int.Parse(project.Variables["acc0"].Value);
                    string currentThreadKey = $"Thread{currentThread}";
                    if (!busyAccounts.Any(x => x.StartsWith($"{currentThread}:"))) //
                    {
                        try
                        {
                            project.GlobalVariables.SetVariable("w3tools", currentThreadKey, project.Variables["projectName"].Value);
                        }
                        catch
                        {
                            project.GlobalVariables["w3tools", currentThreadKey].Value = project.Variables["projectName"].Value;
                        }
                        if (log) project.L0g($"Thread {currentThread} bound to {project.Variables["projectName"].Value}");
                        return true;
                    }
                    else
                    {
                        if (log) project.L0g($"Thread {currentThread} is already busy!");
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
        public static string MathVar(this IZennoPosterProjectModel project, string varName, int input)
        {
            project.Variables[$"{varName}"].Value = (int.Parse(project.Variables[$"{varName}"].Value) + input).ToString();
            return project.Variables[$"{varName}"].Value;
        }
        public static string EscapeMarkdown(string text)
        {
            string[] specialChars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
            foreach (var ch in specialChars)
            {
                text = text.Replace(ch, "\\" + ch);
            }
            return text;
        }
        public static void Sleep(this IZennoPosterProjectModel project, int min = 1, int max = 5)
        {
            Random rnd = new Random();
            Thread.Sleep(rnd.Next(min, max) * 1000);
        }
        public static string InputBox(string message = "input data please", int width = 600, int height = 600)
        {

            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = message;
            form.Width = width;
            form.Height = height;
            System.Windows.Forms.TextBox smsBox = new System.Windows.Forms.TextBox();
            smsBox.Multiline = true;
            smsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            smsBox.Left = 5;
            smsBox.Top = 5;
            smsBox.Width = form.ClientSize.Width - 10;
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += new System.EventHandler((sender, e) => { form.Close(); });
            smsBox.Height = okButton.Top - smsBox.Top - 5;
            form.Controls.Add(smsBox);
            form.Controls.Add(okButton);
            form.ShowDialog();
            return smsBox.Text;
        }
        public static T Age<T>(this IZennoPosterProjectModel project)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Random rnd = new Random();
            long Age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(project.Variables["varSessionId"].Value);


            if (typeof(T) == typeof(string))
            {
                string result = TimeSpan.FromSeconds(Age).ToString();
                return (T)(object)result;
            }
            else if (typeof(T) == typeof(TimeSpan))
            {
                TimeSpan result = TimeSpan.FromSeconds(Age);
                return (T)(object)result;
            }
            else
            {
                return (T)Convert.ChangeType(Age, typeof(T));
            }

        }
        public static T RndAmount<T>(this IZennoPosterProjectModel project, decimal min = 0, decimal max = 0)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Random rnd = new Random();
            if (min == 0) min = decimal.Parse(project.Variables["defaultAmountMin"].Value);
            if (max == 0) max = decimal.Parse(project.Variables["defaultAmountMax"].Value);

            decimal value = min + (max - min) * (decimal)rnd.Next(0, 1000000000) / 1000000000;

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(value.ToString(), typeof(T));

            return (T)Convert.ChangeType(value, typeof(T));

        }

        public static string DecodeQr(HtmlElement element)
        {
            try
            {
                var bitmap = element.DrawPartAsBitmap(0, 0, 200, 200, true);
                var reader = new BarcodeReader();
                var result = reader.Decode(bitmap);
                if (result == null || string.IsNullOrEmpty(result.Text)) return "qrIsNull";
                return result.Text;
            }
            catch (Exception) { return "qrError"; }
        }
        public static string GetExtVer(this IZennoPosterProjectModel project, string extId)
        {
            string securePrefsPath = project.Variables["pathProfileFolder"].Value + @"\Default\Secure Preferences";
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
    }
}
