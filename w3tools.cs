#region using
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Numerics;
using System.Management;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.CompilerServices;

using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;
using ZennoLab.Macros;
using ZennoLab.Emulation;
using ZennoLab.CommandCenter.TouchEvents;
using ZennoLab.CommandCenter.FullEmulation;
using ZennoLab.InterfacesLibrary.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Global.ZennoExtensions;

using Nethereum.ABI;
using Nethereum.ABI.Decoders;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.ABIDeserialisation;
using Nethereum.Contracts;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

using NBitcoin;
using QBitNinja.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ZXing;
using ZXing.QrCode;
using Npgsql;
using Leaf.xNet;

#endregion

namespace w3tools //by @w3bgrep
{
	public static class Migrate
	{
		//ЭТОТ КЛАСС СОДЕРЖИТ УСТАРЕВШИЕ ВЫЗОВЫ
		//  ЕСЛИ ТЫ ИСПОЛЬЗУЕШЬ КАКИЕ-ТО ИЗ НИХ - 
		// замени их теми к которым они обращаются как можно скорее, 
		// в следующих версиях эти вызовы будут удалены

		public static string w3Log(IZennoPosterProjectModel project, string toLog = "", string varName = "a0debug") 
	    {
	        return Loggers.W3Log(project, toLog, varName);
	    }
		public static string w3Query(IZennoPosterProjectModel project, string dbQuery, bool toLog = false)
		{
			return SQL.W3Query(project, dbQuery, toLog);
		}
		public static string simpleGET(IZennoPosterProjectModel project, string url, string proxy = "")
		{
			return Http.W3Get(project, url, proxy);
		}
		public static string getTx(IZennoPosterProjectModel project, string chainRPC = "", string hash = "", int deadline = 60,bool log = false)
		{
			return Leaf.waitTx(project,chainRPC,hash,deadline,log);
		}
		public static T getNative<T>(IZennoPosterProjectModel project, string chainRPC = "", string address = "", string proxy = "",bool log = false)
		{
			return Leaf.balNative<T>(project,chainRPC,address);
		}
		public static T getNonce<T>(IZennoPosterProjectModel project, string chainRPC = "", string address = "", string proxy = "",bool log = false)
		{
			return Leaf.nonce<T>(project,chainRPC,address);
		}		
		public static T getERC20<T>(IZennoPosterProjectModel project, string tokenContract, string chainRPC = "", string address = "", string tokenDecimal = "18", string proxy = "",bool log = false)
		{
			return Leaf.balERC20<T>(project,tokenContract);
		}
	    public static string MMConfirm2(this Instance instance,IZennoPosterProjectModel project, bool log = false )
		{
			return MM.Confirm(instance,project,log);
		}
		public static string MMUnlock(this Instance instance,IZennoPosterProjectModel project,string key = "", bool skipCheck = false)
		{
			return MM.Run(instance,project,key,skipCheck);
		}
		public static void MMConfirm(this Instance instance) //USE MM.Confirm instead
		{
			int i = 0;
			DateTime deadline = DateTime.Now.AddSeconds(60);
			while (!instance.ActiveTab.URL.Contains("nkbihfbeogaeaoehlefnkodbefgpgknn"))
				{
					Thread.Sleep(1000);
					 if (i > 5) throw new Exception(""); i++;
				} 
			i = 0; Thread.Sleep(2000);
			
			while (instance.ActiveTab.URL.Contains("nkbihfbeogaeaoehlefnkodbefgpgknn"))
			{
				if (DateTime.Now > deadline) throw new Exception("");
				try
				{
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "class", "button\\ btn--rounded\\ btn-primary", "regexp", 0),3); Thread.Sleep(2000);
				}
				catch{}
			}
			
		}
		public static string KeplrApprove(this Instance instance,IZennoPosterProjectModel project)
		{
			return Keplr.Approve(instance,project);
		}

		public static string GetOTP(this Instance instance, string keyString, int waitIfTimeLess = 5)
		{
			return OTP.Offline(instance,keyString,waitIfTimeLess);
		}
		public static string FirstMailOTP(IZennoPosterProjectModel project,string email = "", string proxy = "")
		{
		    return OTP.FirstMail(project,email,proxy);
		}
		public static string TelegramMailOTP(IZennoPosterProjectModel project, string email = "", string proxy = "")
		{
		    return OTP.Telegram(project,email,proxy);
		}

	}
    public static class FunctionStorage
    {
        public static ConcurrentDictionary<string, object> Functions = new ConcurrentDictionary<string, object>();
    }
	#region SAFU

    public interface ISAFU
    {
        string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log);
        string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log);
        string HWPass(IZennoPosterProjectModel project, bool log);
    }

    internal class SimpleSAFU : ISAFU
    {
        public string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log)
        {
            if (project.Variables["debug"].Value == "True") log = true;
			if (log) project.SendInfoToLog($"[SimpleSAFU.Encode] input: ['{toEncrypt}'] key: ['{project.Variables["cfgPin"].Value}']");
            if (string.IsNullOrEmpty(toEncrypt)) return string.Empty;
            return Crypto.EncryptAES(toEncrypt, project.Variables["cfgPin"].Value, true);
        }

        public string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log)
        {
            if (project.Variables["debug"].Value == "True") log = true;
			if (log) project.SendInfoToLog($"[SimpleSAFU.Decode] input: ['{toDecrypt}'] key: ['{project.Variables["cfgPin"].Value}']");
            if (string.IsNullOrEmpty(toDecrypt)) return string.Empty;
			try
			{
				return Crypto.DecryptAES(toDecrypt, project.Variables["cfgPin"].Value, true);
			}
			catch (Exception ex)
			{
				project.SendWarningToLog($"[SimpleSAFU.Decode] ERR: [{ex.Message}] key: ['{project.Variables["cfgPin"].Value}']");
				throw; 
			}

        }

        public string HWPass(IZennoPosterProjectModel project, bool log)
        {
            if (project.Variables["debug"].Value == "True") log = true;
			if (log) project.SendInfoToLog($"[SimpleSAFU.HWPass] pass: ['{project.Variables["cfgPin"].Value}']");
            return project.Variables["cfgPin"].Value;
        }
    }

    public static class SAFU
    {
        private static readonly SimpleSAFU _defaultSAFU = new SimpleSAFU();

        public static void Initialize(IZennoPosterProjectModel project)
        {
            if (!FunctionStorage.Functions.ContainsKey("SAFU_Encode") ||
                !FunctionStorage.Functions.ContainsKey("SAFU_Decode") ||
                !FunctionStorage.Functions.ContainsKey("SAFU_HWPass"))
            {
				project.SendInfoToLog("using SAFU simple...");
				FunctionStorage.Functions.TryAdd("SAFU_Encode", (Func<IZennoPosterProjectModel, string, bool, string>)_defaultSAFU.Encode);
				FunctionStorage.Functions.TryAdd("SAFU_Decode", (Func<IZennoPosterProjectModel, string, bool, string>)_defaultSAFU.Decode);
				FunctionStorage.Functions.TryAdd("SAFU_HWPass", (Func<IZennoPosterProjectModel, bool, string>)_defaultSAFU.HWPass);           
			}
        }

        public static string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log = false)
        {
            var encodeFunc = (Func<IZennoPosterProjectModel, string, bool, string>)FunctionStorage.Functions["SAFU_Encode"];
            if (log) project.SendInfoToLog($"SAFU.Encode: toEncrypt = '{toEncrypt}'");
            string result = encodeFunc(project, toEncrypt, log);
            if (log) project.SendInfoToLog($"SAFU.Encode: result = '{result}'");
            return result;
        }

        public static string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log = false)
        {
            var decodeFunc = (Func<IZennoPosterProjectModel, string, bool, string>)FunctionStorage.Functions["SAFU_Decode"];
            if (log)
                project.SendInfoToLog($"SAFU.Decode: Вызов с toDecrypt = '{toDecrypt}'");
            string result = decodeFunc(project, toDecrypt, log);
            if (log)
                project.SendInfoToLog($"SAFU.Decode: Результат = '{result}'");
            return result;
        }

        public static string HWPass(IZennoPosterProjectModel project, bool log = false)
        {
            var hwPassFunc = (Func<IZennoPosterProjectModel, bool, string>)FunctionStorage.Functions["SAFU_HWPass"];
            if (log) project.SendInfoToLog("SAFU.HWPass: call");
            string result = hwPassFunc(project, log);
            if (log) project.SendInfoToLog($"SAFU.HWPass: result = '{result}'");
            return result;
        }
    }

    #endregion
	#region Loggers

	public static class Loggers
	{
		public static string W3Log(IZennoPosterProjectModel project, string toLog = "", string varName = "a0debug",[CallerMemberName] string callerName = "")
		{
			if (toLog == "") toLog = project.Variables[$"{varName}"].Value;
			else project.Variables[$"{varName}"].Value = toLog;
			toLog = Regex.Replace(toLog, @"\s+", " ").Trim();
			var acc0 = project.Variables["acc0"].Value;
			var port = project.Variables["instancePort"].Value;
			var lastAction = project.LastExecutedActionId;
			var elapsed = Time.TotalTime(project);

		// Проверяем стек вызовов
			var stackFrame = new System.Diagnostics.StackFrame(1); // 1 = предыдущий кадр стека
			var callingMethod = stackFrame.GetMethod();
			if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno"))
			{
				callerName = "project"; // Если вызывается из ZennoPoster или проекта
			}


			string formated = $"⛑  [{acc0}] ⚙  [{port}] ⏱  [{elapsed}] ⛏ [{callerName}] at [{lastAction}]\n        {toLog}";
			if (formated.Contains("!W"))   project.SendToLog(formated,LogType.Warning, true, LogColor.Orange);
			else if (formated.Contains("!E"))   project.SendToLog(formated,LogType.Error, true, LogColor.Orange);
			else if (formated.Contains("relax"))   project.SendToLog(formated,LogType.Info, true, LogColor.LightBlue);
			else  project.SendToLog(formated,LogType.Info, true, LogColor.Default);
			return formated;
		}
		public static void W3Debug(IZennoPosterProjectModel project, string log)
		{
			Time.TotalTime(project);
			if(project.Variables["debug"].Value == "True") 
				project.SendToLog($"⚙: {log}",LogType.Info, true, LogColor.Default);
		}
		public static void W3Throw(IZennoPosterProjectModel project, string log)
		{
			Time.TotalTime(project);
			project.SendToLog(log,LogType.Warning, true, LogColor.Orange); 
			throw new Exception($"{log}");
		}
		public static void Report(IZennoPosterProjectModel project)
		{
			string time = project.ExecuteMacro(DateTime.Now.ToString("MM-dd HH:mm"));
			string varLast = project.Variables["a0debug"].Value;
			string report = "";
			
			if (!string.IsNullOrEmpty(project.Variables["failReport"].Value))
			{
			    string encodedFailReport = Uri.EscapeDataString(project.Variables["failReport"].Value);
			    string failUrl = $"https://api.telegram.org/bot{project.Variables["settingsTgLogToken"].Value}/sendMessage?chat_id={project.Variables["settingsTgLogGroup"].Value}&text={encodedFailReport}&reply_to_message_id={project.Variables["settingsTgLogTopic"].Value}&parse_mode=MarkdownV2";
			    Http.W3Get(project, failUrl);
			    if(project.Variables["cfgUpload"].Value == "True" && project.Variables["fail"].Value == "True")
			    {
			        string additionalUrl = $"https://api.telegram.org/bot7923291539:AAFE28RrsnR0nPFY_cjyEboCO8t8Zhg5VNk/sendMessage?chat_id=-1002434878999&text={encodedFailReport}&reply_to_message_id=0&parse_mode=MarkdownV2";
			        Http.W3Get(project, additionalUrl);
			    }
			}
			else
			{
			    report = $"✅️ [{time}]{project.Name} | {varLast}";
			    string successReport = $"✅️  \\#{Tools.EscapeMarkdown(project.Name)} \\#{project.Variables["acc0"].Value} \n" +
			                          $"varLast: [{Tools.EscapeMarkdown(varLast)}] \n";
			    
			    string encodedReport = Uri.EscapeDataString(successReport);
			    project.Variables["REPORT"].Value = encodedReport;
			    string url = $"https://api.telegram.org/bot{project.Variables["settingsTgLogToken"].Value}/sendMessage?chat_id={project.Variables["settingsTgLogGroup"].Value}&text={encodedReport}&reply_to_message_id={project.Variables["settingsTgLogTopic"].Value}&parse_mode=MarkdownV2";
			    Http.W3Get(project, url);
			}
			string toLog = $"✔️ All jobs done. lastMark is: {Regex.Replace(varLast, @"\s+", " ").Trim()}. Elapsed: {Time.Elapsed(project.Variables["varSessionId"].Value)}";
			if (toLog.Contains("fail"))project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Orange);
			else project.SendToLog(toLog.Trim(), LogType.Info, true, LogColor.Green);
		}		
	
	}
	#endregion
	#region OnStart
    public static class OnStart
	{
		public static void InitVariables(IZennoPosterProjectModel project, string author = "")
		{
			if (author == "") author = project.Variables["projectAuthor"].Value;
            project.Variables["varSessionId"].Value = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();
            if (project.Variables["cfgPin"].Value == "") Loggers.W3Throw(project,"PIN IS EMPTY");
			if (project.Variables["DBsqltPath"].Value == "") Loggers.W3Log(project,"!W SQLite path IS EMPTY");
			project.Variables["instancePort"].Value = $"noInstance";
			project.Variables["timeToday"].Value = DateTime.Now.ToString("MM-dd");
			string name = project.ExecuteMacro(project.Name).Split('.')[0];
            var logo = Logo(name,author);
            project.SendInfoToLog(logo,true);
			project.Variables["projectName"].Value = name;
            if (project.Variables["DBmode"].Value == "SQLite") project.Variables["projectTable"].Value = $"_{name}";
            else if (project.Variables["DBmode"].Value == "PostgreSQL") project.Variables["projectTable"].Value = $"projects.{name.ToLower()}";
			SAFU.Initialize(project);
        }
		public static void SetRange(IZennoPosterProjectModel project)
		{
			string accRange = project.Variables["cfgAccRange"].Value;
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
			project.Variables["cfgRangeStart"].Value = $"{rangeS}";
			project.Variables["cfgRangeEnd"].Value = $"{rangeE}";
			project.Variables["range"].Value = range;
		}
		public static void GetGlobalVars(IZennoPosterProjectModel project)
		{
			try 
			{
			    var nameSpase = "w3tools";
			    var cleaned = new List<int>();
			    var notDeclared = new List<int>();
			    var busyAccounts = new List<int>();
			    for (int i = int.Parse(project.Variables["cfgRangeStart"].Value); i <= int.Parse(project.Variables["cfgRangeEnd"].Value); i++)
			    {
			        string threadKey = $"Thread{i}";
			        try 
			        {
			            var globalVar = project.GlobalVariables[nameSpase, threadKey];
			            if (globalVar != null)
			            {
			                if (!string.IsNullOrEmpty(globalVar.Value)) busyAccounts.Add(i);
			                if (project.Variables["cleanGlobal"].Value == "True")
			                {
			                    globalVar.Value = string.Empty;
			                    cleaned.Add(i);
			                }
			            }
			            else notDeclared.Add(i);
			        }
			        catch{notDeclared.Add(i);}
			    }
			    if (project.Variables["cleanGlobal"].Value == "True")Loggers.W3Debug(project,$"GlobalVars cleaned: {string.Join(",", cleaned)}");
			    else project.Variables["busyAccounts"].Value = string.Join(",", busyAccounts);
			}
			catch (Exception ex){Loggers.W3Debug(project,$"⚙  {ex.Message}");}
			
		}
		public static void BindGlobal(IZennoPosterProjectModel project)
		{
			try{project.GlobalVariables.SetVariable("w3tools", $"Thread{project.Variables["acc0"].Value}", project.Variables["projectName"].Value);}
			catch{project.GlobalVariables["w3tools", $"Thread{project.Variables["acc0"].Value}"].Value = project.Variables["projectName"].Value;}
		}
		public static void SetSettingsFromDb(IZennoPosterProjectModel project)
		{
            
            string settings = Db.Settings(project);
			foreach (string varData in settings.Split('\n'))
			{
				string varName = varData.Split('|')[0]; 
				string varValue = varData.Split('|')[1].Trim(); 
				try	{project.Variables[$"{varName}"].Value = varValue;} 
				catch (Exception ex){Loggers.W3Debug(project,$"⚙  {ex.Message}");}
			}
		}
		public static void DisableLogs()
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
		    catch (Exception ex){}
		}
		public static void IntitProjectEvironment(IZennoPosterProjectModel project)
		{
			w3tools.OnStart.DisableLogs();
			w3tools.OnStart.InitVariables(project);
			w3tools.OnStart.SetRange(project);
			w3tools.OnStart.GetGlobalVars(project);
			w3tools.OnStart.SetSettingsFromDb(project);
		}
		public static void BuildAccList(IZennoPosterProjectModel project, string dbQuery, bool log = false)
		{
			
			var tableName = "";
            //ifManual
			if (!string.IsNullOrEmpty(project.Variables["cfgManualAcc0"].Value)) 
			{
				project.Lists["accs"].Clear();
				project.Lists["accs"].Add(project.Variables["cfgManualAcc0"].Value);
				Loggers.W3Debug(project, $@"manual mode on with {project.Variables["cfgManualAcc0"].Value}");
				return;
			}
			//fromDb
			var accsByQuery = "";
			try
			{accsByQuery = SQL.W3Query(project, dbQuery).Trim();
				if (string.IsNullOrWhiteSpace(accsByQuery)) 
				{
				    project.Variables["noAccsToDo"].Value = "True";
				    Loggers.W3Debug(project,$"♻ noAccoutsAvaliable by query [{dbQuery}] ");
				    return; 
				}
			} catch {Loggers.W3Throw(project,dbQuery);}
			//
			
			var availableAccounts = accsByQuery.Split('\n').Select(x => x.Trim().TrimStart(',')).ToHashSet(); 
			Loggers.W3Debug(project, $"Initial availableAccounts: [{string.Join(", ", availableAccounts)}]");

			if (!string.IsNullOrEmpty(project.Variables["requiredSocial"].Value))
			{
			    string[] demanded = project.Variables["requiredSocial"].Value.Split(',');
				Loggers.W3Debug(project, $"Filtring by socials: [{string.Join(", ", demanded)}]");
			    foreach (string social in demanded)
			    {   
			        
					if (project.Variables["DBmode"].Value == "SQLite")  dbQuery = $"SELECT acc0 FROM acc{social.Trim()} WHERE status NOT LIKE '%ok%'";
                    else if (project.Variables["DBmode"].Value == "PostgreSQL") dbQuery = $"SELECT acc0 FROM accounts.{social.Trim().ToLower()} WHERE status NOT LIKE '%ok%'";
                    
					var notOK = SQL.W3Query(project,dbQuery).Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
						Loggers.W3Debug(project, $"Before {social} filter: [{string.Join("|", availableAccounts)}]");
						availableAccounts.ExceptWith(notOK);
						Loggers.W3Debug(project, $"After {social} filter: [{string.Join("|", availableAccounts)}]");
			    }
			}
			Loggers.W3Debug(project, $"after socialCheck [{string.Join("|", availableAccounts)}]");
			// if (!string.IsNullOrEmpty(project.Variables["busyAccounts"].Value))
			// {
			//     availableAccounts.ExceptWith(
			//         project.Variables["busyAccounts"].Value.Split(',').Select(x => x.Trim())
			//     );
			// }
			// Loggers.W3Debug(project, $"after globalVarsCheck [{string.Join("|", availableAccounts)}]");
			project.Lists["accs"].Clear();
			project.Lists["accs"].AddRange(availableAccounts);
			Loggers.W3Debug(project,$"final list [{string.Join("|", project.Lists["accs"])}]");
			return; 
		}
		public static void SetProfile(this Instance instance, IZennoPosterProjectModel project)
		{
			var tableName = "";
            Loggers.W3Debug(project,$"applying webGL");

            var required = "Profile";
            if (project.Variables["DBmode"].Value == "SQLite") tableName = $"acc{required.Trim()}";
            else if (project.Variables["DBmode"].Value == "PostgreSQL") tableName = $"accounts.{required.Trim().ToLower()}";	            

			var jsonObject = JObject.Parse(SQL.W3Query(project,$@"SELECT webGL FROM {tableName} WHERE acc0 = {project.Variables["acc0"].Value}"));
			var mapping = new Dictionary<string, string>
			{
			    {"Renderer", "RENDERER"},
			    {"Vendor", "VENDOR"},
			    {"Version", "VERSION"},
			    {"ShadingLanguageVersion", "SHADING_LANGUAGE_VERSION"},
			    {"UnmaskedRenderer", "UNMASKED_RENDERER_WEBGL"},
			    {"UnmaskedVendor", "UNMASKED_VENDOR"},
			    {"MaxCombinedTextureImageUnits", "MAX_COMBINED_TEXTURE_IMAGE_UNITS"},
			    {"MaxCubeMapTextureSize", "MAX_CUBE_MAP_TEXTURE_SIZE"},
			    {"MaxFragmentUniformVectors", "MAX_FRAGMENT_UNIFORM_VECTORS"},
			    {"MaxTextureSize", "MAX_TEXTURE_SIZE"},
			    {"MaxVertexAttribs", "MAX_VERTEX_ATTRIBS"}
			};
			
			foreach (var pair in mapping)
			{
			    string value = "";
			    if (jsonObject["parameters"]["default"][pair.Value] != null) value = jsonObject["parameters"]["default"][pair.Value].ToString();
			    else if (jsonObject["parameters"]["webgl"][pair.Value] != null)  value = jsonObject["parameters"]["webgl"][pair.Value].ToString();
			    else if (jsonObject["parameters"]["webgl2"][pair.Value] != null) value = jsonObject["parameters"]["webgl2"][pair.Value].ToString();
			    if (!string.IsNullOrEmpty(value)) instance.WebGLPreferences.Set((ZennoLab.InterfacesLibrary.Enums.Browser.WebGLPreference)Enum.Parse(typeof(ZennoLab.InterfacesLibrary.Enums.Browser.WebGLPreference), pair.Key), value);
			
			}
	
			Loggers.W3Debug(project,$"applying settings");
			project.Variables["instancePort"].Value = instance.Port.ToString();
			try 
			{
			    instance.SetWindowSize(1280, 720);
			    project.Profile.AcceptLanguage = "en-US,en;q=0.9";
			    project.Profile.Language = "EN";
			    project.Profile.UserAgentBrowserLanguage = "en-US";
			    instance.UseMedia = false;
			    
			}			
			catch (Exception ex)
			{
				try {project.GlobalVariables[$"w3tools", $"Thread{project.Variables["acc0"].Value}"].Value = null;}catch{}
				project.Variables["acc0"].Value = "";
				Loggers.W3Throw(project,ex.Message);
			}
			
		}
		public static void SetProxy(this Instance instance, IZennoPosterProjectModel project)
		{
			long uTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); 
			string ipLocal = Http.W3Get(project,$"http://api.ipify.org/");
			string proxy = Db.Proxy(project);
            Loggers.W3Debug(project,$"applying proxy {proxy}");
			while (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - uTime < 60)
			{
				instance.SetProxy(proxy, true, true, true, true); Thread.Sleep(2000);
				string ipProxy = Http.W3Get(project,$"http://api.ipify.org/",proxy);
				Loggers.W3Debug(project,$"[{ipLocal}]?[{ipProxy}]");
				project.Variables["ip"].Value = ipProxy;
				project.Variables["proxy"].Value = proxy;
				project.SendInfoToLog(ipProxy);
				if (ipLocal != ipProxy) return;
			}
			Loggers.W3Throw(project,"badProxy");
		}
		public static void SetCookiesFromJson(this Instance instance, IZennoPosterProjectModel project, string filePath = "")
		{
			
			project.Variables["pathCookies"].Value = $"{project.Variables["settingsZenFolder"].Value}accounts\\cookies\\{project.Variables["acc0"].Value}.json";
			if (filePath == "") filePath = project.Variables["pathCookies"].Value;
			Loggers.W3Debug(project,$"applying cookies from  {filePath}");
			try
			{
				var cookies = File.ReadAllText(project.Variables["pathCookies"].Value);
				instance.SetCookie(cookies);
			}
			catch{Loggers.W3Log(project,$"!W noCookiesAvaliable by path {filePath}");}
		}
		public static void SetCookiesFromDB(this Instance instance, IZennoPosterProjectModel project, string filePath = "")
		{
            var required = "Profile";var tableName = "";
            if (project.Variables["DBmode"].Value == "SQLite") tableName = $"acc{required.Trim()}";
            else if (project.Variables["DBmode"].Value == "PostgreSQL") tableName = $"accounts.{required.Trim().ToLower()}";				
                        
            try
			{
				var cookies = SQL.W3Query(project,$@"SELECT cookies FROM {tableName} WHERE acc0 = {project.Variables["acc0"].Value}");
				instance.SetCookie(cookies);
			}
			catch{Loggers.W3Log(project,$"!W noCookiesAvaliable by path {filePath}");}
		}
		public static void ExportCookiesAsJson(IZennoPosterProjectModel project, string filePath = "")
		{
			if (filePath == "") filePath = $"{project.Variables["settingsZenFolder"].Value}accounts\\cookies\\{project.Variables["acc0"].Value}.json";
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

			File.WriteAllText(filePath, project.Variables["accCookies"].Value);
            
            
            var required = "Profile";var tableName = "";
            if (project.Variables["DBmode"].Value == "SQLite") tableName = $"acc{required.Trim()}";
            else if (project.Variables["DBmode"].Value == "PostgreSQL") tableName = $"accounts.{required.Trim().ToLower()}";	            
			SQL.W3Query(project,$"UPDATE {tableName} SET cookies = '{project.Variables["accCookies"].Value.Replace("'", "''")}' WHERE acc0 = {project.Variables["acc0"].Value};");

		}
		public static void SwitchExtentions(this Instance instance, IZennoPosterProjectModel project, string toUse = "")
		{
			Loggers.W3Debug(project,$"switching extentions  {toUse}");

			int i = 0;string extName = "";string outerHtml = "";string extId = "";string extStatus = "enabled";
			for(;;){try{instance.AllTabs[1].Close();Thread.Sleep(1000);}catch{break;}}
			Tab extTab = instance.NewTab("extTab");if (extTab.IsBusy) extTab.WaitDownloading();
			instance.ActiveTab.Navigate("chrome-extension://pbgjpgbpljobkekbhnnmlikbbfhbhmem/index.html", "");
			if (instance.ActiveTab.IsBusy) instance.ActiveTab.WaitDownloading();
			Thread.Sleep(1000);
			if (!instance.ActiveTab.FindElementByAttribute("div", "class", "error-code", "regexp", 0).IsVoid){string path = $"{project.Path}.crx\\One-Click-Extensions-Manager.crx"; instance.InstallCrxExtension(path);
				Loggers.W3Debug(project,$"installing {path}");instance.ActiveTab.Navigate("chrome-extension://pbgjpgbpljobkekbhnnmlikbbfhbhmem/index.html", ""); Thread.Sleep(1000);
			}
			while (!instance.ActiveTab.FindElementByAttribute("button", "class", "ext-name", "regexp", i).IsVoid)
			{
				extName = Regex.Replace(instance.ActiveTab.FindElementByAttribute("button", "class", "ext-name", "regexp", i).GetAttribute("innertext"), @" Wallet", "");
			    outerHtml = instance.ActiveTab.FindElementByAttribute("li", "class", "ext\\ type-normal", "regexp", i).GetAttribute("outerhtml");
			    extId = Regex.Match(outerHtml, @"extension-icon/([a-z0-9]+)").Groups[1].Value;
			    if (outerHtml.Contains("disabled")) extStatus = "disabled";
				if (toUse.Contains(extName) && extStatus == "disabled" || toUse.Contains(extId) && extStatus == "disabled" || !toUse.Contains(extName) && !toUse.Contains(extId) && extStatus == "enabled") instance.ActiveTab.FindElementByAttribute("button", "class", "ext-name", "regexp", i).RiseEvent("click", instance.EmulationLevel);
				i++;
			}
			
			Thread.Sleep(1000); while (true) try { instance.AllTabs[1].Close(); Thread.Sleep(1000); } catch { break;Thread.Sleep(1000); }
		}
		public static void BrowserScanCheck(this Instance instance, IZennoPosterProjectModel project)
		{
			bool set = false;
			string timezoneOffset = "";
			string timezoneName = "";
            var tableName = "";

			var tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{"score", "TEXT DEFAULT '_'"},
				{"WebGL", "TEXT DEFAULT '_'"},
				{"WebGLReport", "INTEGER DEFAULT 0"},
				{"UnmaskedRenderer", "TEXT DEFAULT '_'"},  
				{"Audio", "TEXT DEFAULT '_'"},
				{"ClientRects", "TEXT DEFAULT '_'"},  
				{"WebGPUReport", "TEXT DEFAULT '_'"},  
				{"Fonts", "TEXT DEFAULT '_'"},  
				{"TimeZoneBasedonIP", "TEXT DEFAULT '_'"},  
				{"TimeFromIP", "TEXT DEFAULT '_'"},  
				
			};	
            if (project.Variables["DBmode"].Value == "SQLite") tableName = $"_Browser";
            else if (project.Variables["DBmode"].Value == "PostgreSQL") tableName = $"browser";	
            try{
            SQL.W3MakeTable(project,tableStructure,tableName,schemaName:"accounts");
            }catch{}
            var required = "Browser";
            if (project.Variables["DBmode"].Value == "SQLite") tableName = $"_Browser";
            else if (project.Variables["DBmode"].Value == "PostgreSQL") tableName = $"accounts.browser";	



			while (true)
			{
				instance.ActiveTab.Navigate("https://www.browserscan.net/", "");
				var toParse = "WebGL,WebGLReport, Audio, ClientRects, WebGPUReport,Fonts,TimeZoneBasedonIP,TimeFromIP";
				Thread.Sleep (5000);
				var hardware = instance.ActiveTab.FindElementById("webGL_anchor").ParentElement.GetChildren(false);
				foreach (HtmlElement child in hardware)
				{
					var text = child.GetAttribute("innertext");
					var varName = Regex.Replace(text.Split('\n')[0]," ",""); var varValue = "";
					if (varName == "")continue;
					if (toParse.Contains(varName))  
					{
						project.SendInfoToLog(text);
						try{varValue = text.Split('\n')[2];} catch{Thread.Sleep (2000);continue;}
						SQL.W3Query(project,$"UPDATE {tableName} SET {varName} = '{varValue}' WHERE acc0 = {project.Variables["acc0"].Value};");
					}
				}
				
				var software = instance.ActiveTab.FindElementById("lang_anchor").ParentElement.GetChildren(false);
				foreach (HtmlElement child in software)
				{
					var text = child.GetAttribute("innertext");
					var varName = Regex.Replace(text.Split('\n')[0]," ",""); var varValue = "";
					if (varName == "")continue;
					if (toParse.Contains(varName))  
					{
						if (varName == "TimeZone")continue;
						try{varValue = text.Split('\n')[1];} catch{continue;}
						if (varName == "TimeFromIP") timezoneOffset = varValue;
						if (varName == "TimeZoneBasedonIP") timezoneName = varValue;
						SQL.W3Query(project,$"UPDATE {tableName} SET {varName} = '{varValue}' WHERE acc0 = {project.Variables["acc0"].Value};");
					}
				}
				
				string heToWait = instance.WaitGetValue(() => instance.ActiveTab.FindElementById("anchor_progress"));
				var score = heToWait.Split(' ')[3].Split('\n')[0]; var problems = "";
				
				if (!score.Contains("100%"))
				{
					var problemsHe = instance.ActiveTab.FindElementByAttribute("ul", "fulltagname", "ul", "regexp", 5).GetChildren(false);
					foreach (HtmlElement child in problemsHe)
					{
						var text = child.GetAttribute("innertext");
						var varValue = "";
						var varName = text.Split('\n')[0];
						try	{varValue = text.Split('\n')[1];}catch{continue;};
						problems+= $"{varName}: {varValue}; ";
					}
					problems = problems.Trim();
				
				}
				
				score = $"[{score}] {problems}";
				project.SendInfoToLog(score);

				SQL.W3Query(project,$"UPDATE {tableName} SET score = '{score}' WHERE acc0 = {project.Variables["acc0"].Value};");
				
				
				if (!score.Contains("100%")&& !set)
				{
					var match = System.Text.RegularExpressions.Regex.Match(timezoneOffset, @"GMT([+-]\d{2})");
					if (match.Success)
					{
					    int Offset = int.Parse(match.Groups[1].Value);
					    Loggers.W3Debug(project, $"Setting timezone offset to: {Offset}");
					    
					    instance.TimezoneWorkMode = ZennoLab.InterfacesLibrary.Enums.Browser.TimezoneMode.Emulate;
					    instance.SetTimezone(Offset, 0);
					}
					instance.SetIanaTimezone(timezoneName);
					set = true;
					instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");
					continue;	
				}
				break;
			}
		}
		public static string Logo(string name, string author)
        {
            if (author != "") author = $" script author: @{author}";
            string logo = $@"using w3tools;
            ┌by─┐					
            │    w3bgrep			
            └─→┘
                        ► init {name} ░▒▓█ {author}";
            return logo;
        }
	}
	#endregion	
	#region SQL 
	public class PostgresDB : IDisposable
	{
	    private NpgsqlConnection _conn;
		public void Dispose()
		{
			_conn?.Close();
			_conn?.Dispose();
		}
	    public PostgresDB(string host, string database, string user, string password) 
	    {
	        var (hostname, port) = ParseHostPort(host, 5432);
	        var cs = $"Host={hostname};Port={port};Database={database};Username={user};Password={password};";
	        _conn = new NpgsqlConnection(cs);
	    }	
	    private (string host, int port) ParseHostPort(string input, int defaultPort) 
	    {
	        var parts = input.Split(':');
	        return parts.Length == 2 
	            ? (parts[0], int.Parse(parts[1])) 
	            : (input, defaultPort);
	    }	
        public void open()
        {
            if (_conn.State == System.Data.ConnectionState.Closed)
            {
                _conn.Open();
            }
        }	
        public void close()
        {
            if (_conn.State == System.Data.ConnectionState.Open)
            {
                _conn.Close();
            }
        }
        private void EnsureConnection()
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                throw new InvalidOperationException("Connection is not opened");
        }	
		public int Query(string sql, params NpgsqlParameter[] parameters)
		{
			EnsureConnection();
			
			using (var cmd = new NpgsqlCommand(sql, _conn))
			{
				try
				{
					if (parameters != null)cmd.Parameters.AddRange(parameters);			
					return cmd.ExecuteNonQuery();
				}
				catch (NpgsqlException ex)
				{
					throw new Exception($"SQL Error: {ex.Message} | Query: [{sql}]");
				}
			}
		}
	    public List<string> getAll(string sql, string separator = "|") 
	    {
	        EnsureConnection();
	        var result = new List<string>();
	        
	        using (var cmd = new NpgsqlCommand(sql, _conn))
	        using (var reader = cmd.ExecuteReader()) 
	        {
	            while (reader.Read()) 
	            {
	                var row = new List<string>();
	                for (int i = 0; i < reader.FieldCount; i++)
	                    row.Add(reader[i].ToString());
	                
	                result.Add(string.Join(separator, row));
	            }
	        }
	        return result;
	    }	
	    public string getOne(string sql) 
	    {
	        EnsureConnection();
	        using (var cmd = new NpgsqlCommand(sql, _conn)) 
	        {
	            var result = cmd.ExecuteScalar();
	            return result?.ToString() ?? string.Empty;
	        }
	    }
		public static string pSQL(IZennoPosterProjectModel project, string query, bool log = false, bool ignoreErrors = false, string host ="localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "", [CallerMemberName] string callerName = "")
		{
			if (string.IsNullOrEmpty(dbPswd)) 
            {
                dbPswd = project.Variables["DBpstgrPass"].Value;
                if (string.IsNullOrEmpty(dbPswd)) throw new Exception("PostgreSQL password isNull");
            }

            var db = new PostgresDB(host, dbName, dbUser, dbPswd);
            try 
            {
                db.open();
            }
            catch (Exception ex)
            {
                if (!ignoreErrors) project.SendToLog($"Ошибка при открытии соединения: {ex.Message}", LogType.Warning);
                throw; 
            }
            try 
			{	
                var response = "";
				if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    response = string.Join("\r\n", db.getAll(query)); 
                else 
                    response = db.Query(query).ToString(); 
				if (log) 
				{
					if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) project.SendToLog($"[PstgSQL ▼ ]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]\nRESULT: [{response.Replace('\n','|')}]", LogType.Info, true, LogColor.Gray);
					else 
					{
						if (response != "0") project.SendToLog($"[PstgSQL ▲ ]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]", LogType.Info, true, LogColor.Gray);
					}
				}  
				return response;				
			}
			catch (Exception ex)
			{
				if (!ignoreErrors) project.SendToLog($"{ex.Message}", LogType.Warning);
				return string.Empty;
			}
            finally
            {
                db.close(); 
            }
		}
		public DataTable GetTableStructure(string tableName)
		{
			EnsureConnection();
			
			var query = $@"
				SELECT 
					column_name AS ColumnName,
					data_type AS DataType
				FROM information_schema.columns 
				WHERE table_name = '{tableName}'
				ORDER BY ordinal_position";
			
			using (var cmd = new NpgsqlCommand(query, _conn))
			using (var reader = cmd.ExecuteReader())
			{
				var dt = new DataTable();
				dt.Load(reader);
				return dt;
			}
		}
        public static void pSQLMakeTable (IZennoPosterProjectModel project, Dictionary<string, string> tableStructure, string tableName = "", bool strictMode = false, bool insertData = true, string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "", string schemaName = "projects", bool log = false)
        {
            string query = ""; string dbMode = "PostgreSQL";
           
            
            if (string.IsNullOrEmpty(tableName))
                tableName = project.Variables["projectTable"].Value;

            if (string.IsNullOrEmpty(dbPswd))
            {
                dbPswd = project.Variables["DBpstgrPass"].Value ?? throw new Exception("PostgreSQL password is null");
            }

            if (log) 
            using (var db = new PostgresDB(host, dbName, dbUser, dbPswd)) 
            {
                try
                {
                    db.open();
                    CheckAndCreateTable(db, schemaName, tableName, tableStructure, project, log:log);
                    ManageColumns(db, schemaName, tableName, tableStructure, strictMode, project, log:log);
                    
                    if (tableStructure.ContainsKey("acc0") && !string.IsNullOrEmpty(project.Variables["cfgRangeEnd"].Value))
                    {
                        InsertInitialData(db, schemaName, tableName, project.Variables["cfgRangeEnd"].Value, project, log:log);
                    }

                }
                catch (Exception ex)
                {
                    project.SendToLog($"Ошибка выполнения: {ex.Message}", LogType.Warning);
                    throw;
                }
            }
        }
        private static void CheckAndCreateTable(PostgresDB db, string schemaName, string tableName, Dictionary<string, string> tableStructure, IZennoPosterProjectModel project, bool log = false)
        {

            string tableExists = SQL.W3Query(project,$@"SELECT COUNT(*)
                FROM information_schema.tables
                WHERE table_schema = '{schemaName}'
                AND table_name = '{tableName}';
            ")?.Trim() ?? "0";

            if (tableExists == "0")
            {
				SQL.W3Query(project, $@"
					CREATE TABLE {schemaName}.{tableName} (
						{string.Join(", ", tableStructure.Select(kvp => $"\"{kvp.Key}\" {kvp.Value.Replace("AUTOINCREMENT", "SERIAL")}"))}
					);");
            }
        }
        private static void ManageColumns(PostgresDB db, string schemaName, string tableName, Dictionary<string, string> tableStructure, bool strictMode, IZennoPosterProjectModel project, bool log = false)
        {
            string query = ""; string dbMode = "PostgreSQL";
            query = $@"SELECT column_name
                FROM information_schema.columns
                WHERE table_schema = '{schemaName}'
                AND table_name = '{tableName}';";

            if (log) 
            {
                if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) project.SendToLog($"[PstgSQL ▼]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]", LogType.Info, true, LogColor.Gray);
                else project.SendToLog($"[PstgSQL ▲]: [{query}]", LogType.Info, true, LogColor.Gray);
            }
            var currentColumns = db.getAll(query).Select(c => c.Split('|')[0]).ToList();
            if (strictMode)
            {
                var columnsToRemove = currentColumns.Where(col => !tableStructure.ContainsKey(col)).ToList();
                foreach (var column in columnsToRemove)
                {
                    query = $@"ALTER TABLE {schemaName}.{tableName} DROP COLUMN {column} CASCADE;";
                    if (log) 
                    {
                        if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) project.SendToLog($"[PstgSQL ▼]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]", LogType.Info, true, LogColor.Gray);
                        else project.SendToLog($"[PstgSQL ▲]: [{query}]", LogType.Info, true, LogColor.Gray);
                    }                   
                    db.Query(query);
                }
            }

            foreach (var column in tableStructure)
            {
                query = $@"SELECT COUNT(*)
                    FROM information_schema.columns
                    WHERE table_schema = '{schemaName}'
                    AND table_name = '{tableName}'
                    AND lower(column_name) = lower('{column.Key}');";
                    if (log) 
                    {
                        if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) project.SendToLog($"[PstgSQL ▼]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]", LogType.Info, true, LogColor.Gray);
                        else project.SendToLog($"[PstgSQL ▲]: [{query}]", LogType.Info, true, LogColor.Gray);
                    }                 
                string columnExists = db.getOne(query)?.Trim() ?? "0";
                if (columnExists == "0")
                {
                    query = $@"ALTER TABLE {schemaName}.{tableName} ADD COLUMN {column.Key} {column.Value};";
                    if (log) 
                    {
                        if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) project.SendToLog($"[{dbMode} ▼ ]: [{query}]", LogType.Info, true, LogColor.Gray);
                        else project.SendToLog($"[{dbMode} ▲ ]: [{query}]", LogType.Info, true, LogColor.Gray);
                    }                                         
                    db.Query(query);
                }
            }
        }
		private static void InsertInitialData(PostgresDB db, string schemaName, string tableName, string cfgRangeEnd, IZennoPosterProjectModel project, bool log = false)
		{
			if (!int.TryParse(cfgRangeEnd, out int rangeEnd) || rangeEnd <= 0)
				throw new ArgumentException("cfgRangeEnd must be a positive integer");

			string maxAcc0Query = $@"SELECT COALESCE(MAX(acc0), 0) FROM {schemaName}.{tableName};";
			if (log)
			{
				project.SendToLog($"[PstgSQL ▼]: [{Regex.Replace(maxAcc0Query.Trim(), @"\s+", " ")}]", LogType.Info, true, LogColor.Gray);
			}
			string maxAcc0Str = db.getOne(maxAcc0Query)?.Trim() ?? "0";
			if (!int.TryParse(maxAcc0Str, out int maxAcc0))
			{
				maxAcc0 = 1; 
				project.SendWarningToLog($"Failed to parse max acc0, defaulting to 1");
			}

			if (maxAcc0 < rangeEnd)
			{
				for (int currentAcc0 = maxAcc0 + 1; currentAcc0 <= rangeEnd; currentAcc0++)
				{
					string insertQuery = $@"INSERT INTO {schemaName}.{tableName} (acc0) VALUES ({currentAcc0}) ON CONFLICT DO NOTHING;";
					if (log)
					{
						project.SendToLog($"[PstgSQL ▲]: [{insertQuery}]", LogType.Info, true, LogColor.Gray);
					}
					SQL.W3Query(project, insertQuery); 
				}
			}
			else
			{
				if (log)
				{
					project.SendInfoToLog($"No new data to insert. Max acc0 ({maxAcc0}) is >= rangeEnd ({rangeEnd})");
				}
			}
		}
	}
    public static class SQLite
    {
		public static string lSQL(IZennoPosterProjectModel project, string query, bool log = false, bool ignoreErrors = false)
		{
		    try 
		    {
                string connectionString = $"Dsn=SQLite3 Datasource; database={project.Variables["DBsqltPath"].Value};";
				var response = ZennoPoster.Db.ExecuteQuery(query,null,ZennoLab.InterfacesLibrary.Enums.Db.DbProvider.Odbc,connectionString,"|","\r\n",true);
				if (log) 
				{
					if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) project.SendToLog($"[SQLite ▼ ]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]\nRESULT: [{response.Replace('\n','|')}]", LogType.Info, true, LogColor.Gray);
					else 
					{
						if (response != "0") project.SendToLog($"[SQLite ▲ ]: [{Regex.Replace(query.Trim(), @"\s+", " ")}] RESULT: [{response}]", LogType.Info, true, LogColor.Gray);
						else project.SendToLog($"[SQLite ▲ ]: [{Regex.Replace(query.Trim(), @"\s+", " ")}] RESULT: [{response}]", LogType.Info, true, LogColor.Default);
					}
				} 
				return response;
		    }
		    catch (Exception ex)
		    {   
                if (!ignoreErrors) Loggers.W3Throw(project,$"{ex.Message}: {query}");
                project.SendToLog($"{ex.Message}", LogType.Warning);
				return string.Empty;
		    }
		}

		public static void lSQLMakeTable(IZennoPosterProjectModel project, Dictionary<string, string> tableStructure, string tableName = "", bool strictMode = false)
		{
		    if (tableName == "") tableName = project.Variables["projectTable"].Value;
			string tableExists = lSQL(project, $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';");
		    if (tableExists.Trim() == "0") 
		    {
		        string createTableQuery = $"CREATE TABLE {tableName} (";
		        createTableQuery += string.Join(", ", tableStructure.Select(kvp => $"{kvp.Key} {kvp.Value}"));
		        createTableQuery += ");";
		        Loggers.W3Log(project, createTableQuery);
		        lSQL(project, createTableQuery);
		    }
		    else {
		        string[] currentColumns = lSQL(project, $"SELECT name FROM pragma_table_info('{tableName}');").Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		        if (strictMode)
		        {
					var columnsToRemove = currentColumns.Where(col => !tableStructure.ContainsKey(col)).ToList();
		            foreach (var column in columnsToRemove) lSQL(project, $"ALTER TABLE {tableName} DROP COLUMN {column};");}
			        foreach (var column in tableStructure){string resp = lSQL(project, $"SELECT COUNT(*) FROM pragma_table_info('{tableName}') WHERE name='{column.Key}';");
		            if (resp.Trim() == "0")  lSQL(project, $"ALTER TABLE {tableName} ADD COLUMN {column.Key} {column.Value};");
				}
		    }
		
		    if (tableStructure.ContainsKey("acc0") && !string.IsNullOrEmpty(project.Variables["cfgRangeEnd"].Value))
		    {for (int currentAcc0 = 1; currentAcc0 <= int.Parse(project.Variables["cfgRangeEnd"].Value); currentAcc0++)
		        {lSQL(project, $"INSERT OR IGNORE INTO {tableName} (acc0) VALUES ('{currentAcc0}');");}}
		}

    }   
    public static class SQL
	{
        public static string W3Query(IZennoPosterProjectModel project, string query, bool log = false, bool ignoreErrors = false, string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "")
        {
            string dbMode = project.Variables["DBmode"].Value;
            if (project.Variables["debug"].Value == "True") log = true;        
            if (dbMode == "SQLite") return SQLite.lSQL(project, query, log);
            else if (dbMode == "PostgreSQL") return PostgresDB.pSQL(project, query, log, ignoreErrors, host, dbName, dbUser, dbPswd);
            else return $"unknown DBmode: {dbMode}";
        }
        public static void W3MakeTable(IZennoPosterProjectModel project, Dictionary<string, string> tableStructure, string tableName = "", bool strictMode = false, bool insertData = false, string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "", string schemaName = "projects", bool log = false)
            {
                string dbMode = project.Variables["DBmode"].Value;
                if (project.Variables["debug"].Value == "True") log = true;
                if (log) 
                {
                     project.SendToLog($"[{dbMode} 🕵 ]: checking table {tableName}", LogType.Info, true, LogColor.Gray);
                }      
                if (dbMode == "SQLite") SQLite.lSQLMakeTable(project, tableStructure, tableName, strictMode);
                else if (dbMode == "PostgreSQL") PostgresDB.pSQLMakeTable(project, tableStructure, tableName, strictMode, insertData, host, dbName, dbUser, dbPswd, schemaName, log:log);
                else throw new Exception($"Неподдерживаемый режим базы данных: {dbMode}");
            }
        public static string DBget(IZennoPosterProjectModel project, string req)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            //socials
            if (req == "Twitter")
            {
				if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2fa, emailLogin, emailPass FROM accTwitter WHERE acc0 = {project.Variables["acc0"].Value};");
                else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2fa, emailLogin, emailPass FROM accounts.twitter WHERE acc0 = {project.Variables["acc0"].Value};");
                string[] twitterData = resp.Split('|');
				project.Variables["twitterSTATUS"].Value = twitterData[0].Trim();
				project.Variables["twitterTOKEN"].Value = twitterData[1].Trim();
				project.Variables["twitterLOGIN"].Value = twitterData[2].Trim();
				project.Variables["twitterPASSWORD"].Value = twitterData[3].Trim();
				project.Variables["twitterCODE2FA"].Value = twitterData[4].Trim();
				project.Variables["twitterEMAIL"].Value = twitterData[5].Trim();
				project.Variables["twitterEMAIL_PASSWORD"].Value = twitterData[6].Trim();			
				return project.Variables["twitterSTATUS"].Value;
            }

			if (req == "Discord")
			{

				if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2FA, username, servers FROM accDiscord WHERE acc0 = {project.Variables["acc0"].Value};");
                else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2FA, username, servers FROM accounts.discord WHERE acc0 = {project.Variables["acc0"].Value};");

				string[] discordData = resp.Split('|');
				project.Variables["discordSTATUS"].Value = discordData[0].Trim();
				project.Variables["discordTOKEN"].Value = discordData[1].Trim();
				project.Variables["discordLOGIN"].Value = discordData[2].Trim();
				project.Variables["discordPASSWORD"].Value = discordData[3].Trim();
				project.Variables["discord2FACODE"].Value = discordData[4].Trim();
				project.Variables["discordUSERNAME"].Value = discordData[5].Trim();
				project.Variables["discordSERVERS"].Value = discordData[6].Trim();
                return project.Variables["discordSTATUS"].Value;
			}

			if (req == "Google")
			{
 				if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT status, login, password, code2FA, recoveryEmail, recovery2FA FROM accGoogle WHERE acc0 = {project.Variables["acc0"].Value};");
                else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT status, login, password, code2FA, recoveryEmail, recovery2FA FROM accounts.google WHERE acc0 = {project.Variables["acc0"].Value};");	

				string[] googleData = resp.Split('|');
				project.Variables["googleSTATUS"].Value = googleData[0].Trim();
				project.Variables["googleLOGIN"].Value = googleData[1].Trim();
				project.Variables["googlePASSWORD"].Value = googleData[2].Trim();
				project.Variables["google2FACODE"].Value =googleData[3].Trim();
				project.Variables["googleSECURITY_MAIL"].Value = googleData[4].Trim();
				project.Variables["googleBACKUP_CODES"].Value = googleData[5].Trim();
                return project.Variables["googleSTATUS"].Value;
            }
            return resp;

        }

	}

    public static class Db
    {
        public static string KeyEVM(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"SELECT private256K1 FROM accBlockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"SELECT private256K1 FROM accounts.blockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            return SAFU.Decode(project,resp);
        }
        public static string KeySOL(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"SELECT privateBASE58 FROM accBlockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"SELECT privateBASE58 FROM accounts.blockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            return SAFU.Decode(project,resp);
        }
        public static string Seed(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"SELECT seedBIP39 FROM accBlockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"SELECT seedBIP39 FROM accounts.blockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            return SAFU.Decode(project,resp);
        }   
        public static string AdrEvm(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            if (dbMode == "SQLite") resp = SQL.W3Query(project,$"SELECT publicEVM FROM accBlockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"SELECT publicEVM FROM accounts.blockchain WHERE acc0 = {project.Variables["acc0"].Value}");
            project.Variables["addressEvm"].Value = resp;  return resp;
        }   
        public static string Proxy(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"SELECT proxy FROM accProfile WHERE acc0 = {project.Variables["acc0"].Value}");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"SELECT proxy FROM accounts.profile WHERE acc0 = {project.Variables["acc0"].Value}");
            project.Variables["proxy"].Value = resp;   return resp;
        }   
        public static string NickName(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
			var emailMode = project.Variables["cfgMail"].Value; 
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT nickname, bio FROM accProfile WHERE acc0 = {project.Variables["acc0"].Value};");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT nickname, bio FROM accounts.profile WHERE acc0 = {project.Variables["acc0"].Value};");	

            string[] respData = resp.Split('|');
            project.Variables["accNICKNAME"].Value = respData[0].Trim();
            project.Variables["accBIO"].Value = respData[1].Trim();
			
            return resp;
        }   
        public static string Settings(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
                if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"SELECT var, value FROM accSettings");
                else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"SELECT var, value FROM accounts.settings"); return resp;
        }   
        public static string Email(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
			var emailMode = project.Variables["cfgMail"].Value; 
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT login, icloud FROM accGoogle WHERE acc0 = {project.Variables["acc0"].Value};");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT login, icloud FROM accounts.google WHERE acc0 = {project.Variables["acc0"].Value};");	

            string[] emailData = resp.Split('|');
            project.Variables["emailGOOGLE"].Value = emailData[0].Trim();
            project.Variables["emailICLOUD"].Value = emailData[1].Trim();
			
			if (emailMode == "Google" ) resp = emailData[0].Trim();
			if (emailMode == "Icloud" ) resp = emailData[1].Trim();
            return resp;
        }   
        public static string Twitter(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2fa, emailLogin, emailPass FROM accTwitter WHERE acc0 = {project.Variables["acc0"].Value};");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2fa, emailLogin, emailPass FROM accounts.twitter WHERE acc0 = {project.Variables["acc0"].Value};");
            
            string[] twitterData = resp.Split('|');
            project.Variables["twitterSTATUS"].Value = twitterData[0].Trim();
            project.Variables["twitterTOKEN"].Value = twitterData[1].Trim();
            project.Variables["twitterLOGIN"].Value = twitterData[2].Trim();
            project.Variables["twitterPASSWORD"].Value = twitterData[3].Trim();
            project.Variables["twitterCODE2FA"].Value = twitterData[4].Trim();
            project.Variables["twitterEMAIL"].Value = twitterData[5].Trim();
            project.Variables["twitterEMAIL_PASSWORD"].Value = twitterData[6].Trim();			
            return project.Variables["twitterSTATUS"].Value;
        }  
        public static string Discord(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
				if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2FA, username, servers FROM accDiscord WHERE acc0 = {project.Variables["acc0"].Value};");
                else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT status, token, login, password, code2FA, username, servers FROM accounts.discord WHERE acc0 = {project.Variables["acc0"].Value};");
				
                string[] discordData = resp.Split('|');
				project.Variables["discordSTATUS"].Value = discordData[0].Trim();
				project.Variables["discordTOKEN"].Value = discordData[1].Trim();
				project.Variables["discordLOGIN"].Value = discordData[2].Trim();
				project.Variables["discordPASSWORD"].Value = discordData[3].Trim();
				project.Variables["discord2FACODE"].Value = discordData[4].Trim();
				project.Variables["discordUSERNAME"].Value = discordData[5].Trim();
				project.Variables["discordSERVERS"].Value = discordData[6].Trim();
                return project.Variables["discordSTATUS"].Value;
        }  
        public static string Google(IZennoPosterProjectModel project)
        {
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$@"SELECT status, login, password, code2FA, recoveryEmail, recovery2FA FROM accGoogle WHERE acc0 = {project.Variables["acc0"].Value};");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$@"SELECT status, login, password, code2FA, recoveryEmail, recovery2FA FROM accounts.google WHERE acc0 = {project.Variables["acc0"].Value};");	

            string[] googleData = resp.Split('|');
            project.Variables["googleSTATUS"].Value = googleData[0].Trim();
            project.Variables["googleLOGIN"].Value = googleData[1].Trim();
            project.Variables["googlePASSWORD"].Value = googleData[2].Trim();
            project.Variables["google2FACODE"].Value =googleData[3].Trim();
            project.Variables["googleSECURITY_MAIL"].Value = googleData[4].Trim();
            project.Variables["googleBACKUP_CODES"].Value = googleData[5].Trim();
            return project.Variables["googleSTATUS"].Value;
        }  
 		public static string TwitterTokenUpdate(IZennoPosterProjectModel project)  
		{
			var dbMode = project.Variables["DBmode"].Value;  var resp = "";
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"UPDATE accTwitter SET token = '{project.Variables["twitterTOKEN"].Value}' WHERE acc0 = {project.Variables["acc0"].Value};");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"UPDATE accounts.twitter SET token = '{project.Variables["twitterTOKEN"].Value}' WHERE acc0 = {project.Variables["acc0"].Value};");
            return resp;
		}
		public static string UpdAddressSol(IZennoPosterProjectModel project,string address = "")  
		{
			var dbMode = project.Variables["DBmode"].Value;  var resp = "";
			if (address == "") address = project.Variables["addressSol"].Value;
            if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"UPDATE accBlockchain SET publicSOL = '{address}' WHERE acc0 = {project.Variables["acc0"].Value};");
            else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"UPDATE accounts.blockchain SET publicsol = '{address}' WHERE acc0 = {project.Variables["acc0"].Value};");
            return resp;
		}
		public static string BinanceApiKeys(IZennoPosterProjectModel project)  
		{
            var dbMode = project.Variables["DBmode"].Value; var resp = "";
                if (dbMode == "SQLite")  resp = SQL.W3Query(project,$"SELECT value FROM accSettings WHERE var = 'settingsApiBinance';");
                else if (dbMode == "PostgreSQL") resp = SQL.W3Query(project,$"SELECT value FROM accounts.settings WHERE var = 'settingsApiBinance';"); return resp;
		}
	}    

    #endregion
	#region POST/GET
	public static class Http
	{
				//POST/GET
		public static string W3Get(IZennoPosterProjectModel project, string url, string proxy = "")
		{
			string response = ZennoPoster.HttpGet(url,proxy,"UTF-8",ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,5000,"","Mozilla/5.0",true,5,null,"",false);
			return response;
		}
		public static string W3Post(IZennoPosterProjectModel project, string url, string body, string proxy = "")
		{
			string response = ZennoPoster.HttpPost(url, body, "application/json", proxy, "UTF-8",ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly, 5000, "", "Mozilla/5.0", true, 5, null, "", false);
			return response;
		}

	}
	public static class Leaf
	{
		public static string GET(IZennoPosterProjectModel project, string url, string proxy = "")
		{
			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						return null;
					}
				}
				try
				{
					HttpResponse httpResponse = request.Get(url);
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка запроса: {ex.Message}");
					return null;
				}
			}

			return response;
		}
		public static string POST(IZennoPosterProjectModel project, string url, string body, string proxy = "")
		{
			string response;
			using (var request = new HttpRequest())
			{
				// Настройки запроса, аналогичные оригинальной функции
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				// Настройка прокси, если он указан
				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						return null;
					}
				}

				// Выполнение запроса
				try
				{
					HttpResponse httpResponse = request.Post(url, body, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка запроса: {ex.Message}");
					return null;
				}
			}

			return response;
		}
		public static T nonce<T>(IZennoPosterProjectModel project, string chainRPC = "", string address = "", string proxy = "")
{
    if (address == "") address = project.Variables["addressEvm"].Value;
    if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;
    string jsonBodyGetNonce = $@"{{""jsonrpc"": ""2.0"",""method"": ""eth_getTransactionCount"",""params"": [""{address}"", ""latest""],""id"": 1}}";

    string response;
    using (var request = new HttpRequest())
    {
        request.UserAgent = "Mozilla/5.0";
        request.IgnoreProtocolErrors = true;
        request.ConnectTimeout = 5000;
        if (!string.IsNullOrEmpty(proxy))
        {
            try
            {
                request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
            }
            catch (Exception ex)
            {
                project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
                throw;
            }
        }

        try
        {
            // Исправление: Передаём jsonBodyGetNonce как строку
            HttpResponse httpResponse = request.Post(chainRPC, jsonBodyGetNonce, "application/json");
            response = httpResponse.ToString();
        }
        catch (HttpException ex)
        {
            project.SendErrorToLog($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
            throw;
        }
    }

    var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
    string hexResultNonce = match.Success ? match.Groups[1].Value : "0x0";
    
    if (hexResultNonce == "0x0") 
        return (T)Convert.ChangeType("0", typeof(T));
    
    int transactionCount = Convert.ToInt32(hexResultNonce.TrimStart('0', 'x'), 16);
    
    if (typeof(T) == typeof(string))
        return (T)Convert.ChangeType(transactionCount.ToString(), typeof(T));
    
    return (T)Convert.ChangeType(transactionCount, typeof(T));
}	
		public static T balNative<T>(IZennoPosterProjectModel project, string chainRPC = "", string address = "", string proxy = "",bool log = false)
		{
			if (address == "") address = project.Variables["addressEvm"].Value;
			if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;
			string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_getBalance"", ""params"": [""{address}"", ""latest""], ""id"": 1 }}";
			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"can't parse proxy '{proxy}': {ex.Message}");
						throw;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}
			var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
			string hexResultBalance = match.Success ? match.Groups[1].Value.TrimStart('0', 'x') : "0";
			BigInteger balanceWei = BigInteger.Parse("0" + hexResultBalance, NumberStyles.AllowHexSpecifier);
			decimal balanceNative = (decimal)balanceWei / 1000000000000000000m;
			project.SendInfoToLog($"[Leaf.xNet] {address}: {balanceNative}");
			if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceNative.ToString("0.##################", CultureInfo.InvariantCulture), typeof(T));
			return (T)Convert.ChangeType(balanceNative, typeof(T));
		}
		public static T gasNow<T>(IZennoPosterProjectModel project, string chainRPC = "", string proxy = "",bool log = false)
		{
			if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;
			string jsonBody = @"{""jsonrpc"":""2.0"",""method"":""eth_gasPrice"",""params"":[],""id"":1}";

			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						throw;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
			string hexResultGas = match.Success ? match.Groups[1].Value.TrimStart('0', 'x') : "0";
			
			BigInteger gasWei = BigInteger.Parse("0" + hexResultGas, NumberStyles.AllowHexSpecifier);
			decimal gasGwei = (decimal)gasWei / 1000000000m;
			
			project.SendInfoToLog($"[Leaf.xNet] Gas price: {gasGwei} Gwei");

			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(gasGwei.ToString("0.######", CultureInfo.InvariantCulture), typeof(T));
			
			return (T)Convert.ChangeType(gasGwei, typeof(T));
		}								
		public static T balERC20<T>(IZennoPosterProjectModel project, string tokenContract, string chainRPC = "", string address = "", string tokenDecimal = "18", string proxy = "",bool log = false)
		{
			if (address == "") address = project.Variables["addressEvm"].Value;
			if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;
			
			string data = "0x70a08231000000000000000000000000" + address.Replace("0x", "");
			
			string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						throw;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
			string hexResult = match.Success ? match.Groups[1].Value.TrimStart('0', 'x') : "0";
			
			BigInteger balanceWei = BigInteger.Parse("0" + hexResult, NumberStyles.AllowHexSpecifier);
			decimal decimals = (decimal)Math.Pow(10, double.Parse(tokenDecimal));
			decimal balance = (decimal)balanceWei / decimals;

			project.SendInfoToLog($"[Leaf.xNet] Баланс ERC-20 токена ({tokenContract}) для адреса {address}: {balance}");

			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(balance.ToString("0.##################", CultureInfo.InvariantCulture), typeof(T));
			
			return (T)Convert.ChangeType(balance, typeof(T));
		}					
		public static T balERC721<T>(IZennoPosterProjectModel project, string tokenContract, string chainRPC = "", string address = "", string proxy = "",bool log = false)
		{
			if (address == "") address = project.Variables["addressEvm"].Value;
			if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;

			string functionSelector = "0x70a08231"; // keccak256("balanceOf(address)")[:4]
			string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');
			string data = functionSelector + paddedAddress;

			string jsonBody = $@"{{
				""jsonrpc"": ""2.0"",
				""method"": ""eth_call"",
				""params"": [{{
					""to"": ""{tokenContract}"",
					""data"": ""{data}""
				}}, ""latest""],
				""id"": 1
			}}";

			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						throw;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
			string hexResult = match.Success ? match.Groups[1].Value.TrimStart('0', 'x') : "0";

			BigInteger balance = BigInteger.Parse("0" + hexResult, NumberStyles.AllowHexSpecifier);

			project.SendInfoToLog($"[Leaf.xNet] Баланс токенов ERC-721 для адреса {address} в контракте {tokenContract}: {balance}");

			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(balance.ToString(), typeof(T));

			return (T)Convert.ChangeType(balance, typeof(T));
		}
		public static T balERC1155<T>(IZennoPosterProjectModel project, string tokenContract, string tokenId, string chainRPC = "", string address = "", string proxy = "",bool log = false)
		{
			if (address == "") address = project.Variables["addressEvm"].Value;
			if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;

			string functionSelector = "0x00fdd58e";
			string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');
			string paddedTokenId = BigInteger.Parse(tokenId).ToString("x").PadLeft(64, '0');
			string data = functionSelector + paddedAddress + paddedTokenId;

			string jsonBody = $@"{{
				""jsonrpc"": ""2.0"",
				""method"": ""eth_call"",
				""params"": [{{
					""to"": ""{tokenContract}"",
					""data"": ""{data}""
				}}, ""latest""],
				""id"": 1
			}}";

			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						throw;
					}
				}

				try
				{
					// Исправление: Передаём jsonBody как строку, а не StringContent
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString(); // Получаем тело ответа как строку
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
			string hexResult = match.Success ? match.Groups[1].Value.TrimStart('0', 'x') : "0";
			BigInteger balance = BigInteger.Parse("0" + hexResult, NumberStyles.AllowHexSpecifier);
			if (log) project.SendInfoToLog($"[Leaf.xNet ⇌] balance of ERC-1155 [{tokenContract}:id({tokenId})] on {address}: [{balance}]");

			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(balance.ToString(), typeof(T));
			else if (typeof(T) == typeof(int))
				return (T)(object)(int)balance; // Явное преобразование BigInteger в int
			else if (typeof(T) == typeof(BigInteger))
				return (T)(object)balance; // Возвращаем BigInteger напрямую
			else
        	throw new InvalidOperationException($"!W unsupported type {typeof(T)}");
		}
		public static T chainId<T>(IZennoPosterProjectModel project, string chainRPC = "", string proxy = "",bool log = false)
		{
			if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;
			string jsonBodyGetChainId = @"{""jsonrpc"": ""2.0"",""method"": ""eth_chainId"",""params"": [],""id"": 1}";

			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						throw;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBodyGetChainId, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
			string hexResultChainId = match.Success ? match.Groups[1].Value : "0x0";
			
			if (hexResultChainId == "0x0")
				return (T)Convert.ChangeType("0", typeof(T));
			
			int chainId = Convert.ToInt32(hexResultChainId.TrimStart('0', 'x'), 16);
			
			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(chainId.ToString(), typeof(T));
			
			return (T)Convert.ChangeType(chainId, typeof(T));
		}			
		public static string txReceipt(IZennoPosterProjectModel project, string chainRPC, string txHash, string proxy = "",bool log = false)
		{
			string jsonBody = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionReceipt"",""params"":[""{txHash}""],""id"":1}}";

			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						return null;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
					if (httpResponse.StatusCode != HttpStatusCode.OK)
					{
						project.SendErrorToLog($"Ошибка сервера: {httpResponse.StatusCode}");
						return null;
					}
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка запроса: {ex.Message}");
					return null;
				}
			}

			// Проверяем наличие результата
			if (string.IsNullOrWhiteSpace(response) || response.Contains("\"result\":null"))
				return null;

			return response;
		}	
		public static string txRaw(IZennoPosterProjectModel project, string chainRPC, string txHash, string proxy = "",bool log = false)
		{
			string jsonBody = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionByHash"",""params"":[""{txHash}""],""id"":1}}";

			string response;
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
						return null;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
					if (httpResponse.StatusCode != HttpStatusCode.OK)
					{
						project.SendErrorToLog($"Ошибка сервера: {httpResponse.StatusCode}");
						return null;
					}
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка запроса: {ex.Message}");
					return null;
				}
			}

			// Проверяем наличие результата
			if (string.IsNullOrWhiteSpace(response) || response.Contains("\"result\":null"))
				return null;

			return response;
		}						
		public static string waitTx(IZennoPosterProjectModel project, string chainRPC = "", string hash = "", int deadline = 60,bool log = false)
		{
			if (string.IsNullOrEmpty(hash))  hash = project.Variables["blockchainHash"].Value;
			if (string.IsNullOrEmpty(chainRPC)) chainRPC = project.Variables["blockchainRPC"].Value;
			string jsonRecept = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionReceipt"",""params"":[""{hash}""],""id"":1}}";
			string jsonRaw = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionByHash"",""params"":[""{hash}""],""id"":1}}";
			string response = "";
			project.Variables["a0debug"].Value = "";
			Thread.Sleep(2000);

			DateTime startTime = DateTime.Now;
			TimeSpan timeout = TimeSpan.FromSeconds(deadline);

			while (true)
			{
				if (DateTime.Now - startTime > timeout) throw new Exception($"timeout {deadline}s");
				var logString = "";

				// Проверяем receipt
				try
				{
					response = LeafHttpPost(project, chainRPC, jsonRecept);
					if (!string.IsNullOrEmpty(response))
					{
						project.Json.FromString(response);
					}
				}
				catch
				{
					Thread.Sleep(2000);
					continue;
				}

				try
				{
					string gasUsed = Onchain.HexToString(project.Json.result.gasUsed, "gwei");
					string gasPrice = Onchain.HexToString(project.Json.result.effectiveGasPrice, "gwei");
					string status = Onchain.HexToString(project.Json.result.status);

					if (status == "1") project.Variables["txStatus"].Value = "SUCCSESS";
					else project.Variables["txStatus"].Value = "!W FAIL";
					string result = $"{chainRPC} {hash} [{project.Variables["txStatus"].Value}] gasUsed: {gasUsed}";
					Loggers.W3Debug(project, result);
					return result;
				}
				catch
				{
					project.Variables["txStatus"].Value = "noStatus";
				}

				Thread.Sleep(1000);

				// Проверяем raw транзакцию
				try
				{
					response = LeafHttpPost(project, chainRPC, jsonRaw);
					if (!string.IsNullOrEmpty(response))
					{
						project.Json.FromString(response);
					}
				}
				catch
				{
					Thread.Sleep(2000);
					continue;
				}

				try
				{
					string gas = Onchain.HexToString(project.Json.result.maxFeePerGas, "gwei");
					string gasPrice = Onchain.HexToString(project.Json.result.gasPrice, "gwei");
					string nonce = Onchain.HexToString(project.Json.result.nonce);
					string value = Onchain.HexToString(project.Json.result.value, "eth");
					project.Variables["txStatus"].Value = "PENDING";

					logString = $"[{chainRPC} {hash}] pending  gasLimit:[{gas}] gasNow:[{gasPrice}] nonce:[{nonce}] value:[{value}]";
				}
				catch
				{
					project.Variables["txStatus"].Value = "";
					logString = $"[{chainRPC} {hash}] not found";
				}

				Loggers.W3Debug(project, logString);
				Thread.Sleep(2000);
			}
		}
		private static string LeafHttpPost(IZennoPosterProjectModel project, string url, string jsonBody, string proxy = "",bool log = false)
		{
			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						request.Proxy = ProxyClient.Parse(proxy.Contains("@") ? proxy : $"HTTP://{proxy}");
					}
					catch (Exception ex)
					{
						project.SendErrorToLog($"Ошибка прокси: {ex.Message}");
						throw;
					}
				}

				try
				{
					HttpResponse httpResponse = request.Post(url, jsonBody, "application/json");
					return httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка запроса: {ex.Message}");
					throw;
				}
			}
		}			
		public static string GenerateTweet(IZennoPosterProjectModel project, string content, string bio = "", bool log = false)
		{
			// Очищаем переменную для ответа API
			project.Variables["api_response"].Value = "";

			// Формируем объект для JSON-запроса
			var requestBody = new
			{
				model = "sonar",
				messages = new[]
				{
					new
					{
						role = "system",
						content = string.IsNullOrEmpty(bio)
							? "You are a social media account. Generate tweets that reflect a generic social media persona."
							: $"You are a social media account with the bio: '{bio}'. Generate tweets that reflect this persona, incorporating themes relevant to bio."
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

			// Сериализуем объект в JSON-строку
			string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, Newtonsoft.Json.Formatting.None);

			// Устанавливаем заголовки для запроса
			string[] headers = new string[]
			{
				"Content-Type: application/json",
				$"Authorization: Bearer {project.Variables["settingsApiPerplexity"].Value}"
			};

			// Отправляем POST-запрос к Perplexity API с использованием Leaf
			string response;
			using (var request = new HttpRequest())
			{
				// Настраиваем параметры запроса
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				// Добавляем заголовки
				foreach (var header in headers)
				{
					var parts = header.Split(new[] { ": " }, 2, StringSplitOptions.None);
					if (parts.Length == 2)
					{
						request.AddHeader(parts[0], parts[1]);
					}
				}

				// Отправляем POST-запрос
				try
				{
					HttpResponse httpResponse = request.Post("https://api.perplexity.ai/chat/completions", jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					project.SendErrorToLog($"Ошибка HTTP-запроса: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			// Сохраняем полный ответ в переменную
			project.Variables["api_response"].Value = response;

			// Логируем полный ответ, если log = true
			if (log)
			{
				project.SendInfoToLog($"Full response: {response}");
			}

			// Парсим JSON и извлекаем текст твита
			try
			{
				var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(response);
				string tweetText = jsonResponse["choices"][0]["message"]["content"].ToString();

				// Логируем сгенерированный твит, если log = true
				if (log)
				{
					project.SendInfoToLog($"Generated tweet: {tweetText}");
				}

				return tweetText; // Возвращаем только текст твита
			}
			catch (Exception ex)
			{
				project.SendErrorToLog($"Error parsing response: {ex.Message}");
				throw;
			}
		}
		public static T GetInitiaBalances<T>(IZennoPosterProjectModel project, string chain = "initiation-2",  string address = "", string token = "")
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			
			if (string.IsNullOrEmpty(address)) address = project.Variables["addressInitia"].Value.Trim();

			string url = $"https://celatone-api-prod.alleslabs.dev/v1/initia/{chain}/accounts/{address}/balances";

			string jsonString = Http.W3Get(project, url);

			try
			{
				JArray balances = JArray.Parse(jsonString);
				List<string> balanceList = new List<string>();
				foreach (JObject balance in balances)
				{
					string denom = balance["denom"].ToString();
					string amount = balance["amount"].ToString();
					if (double.TryParse(amount, out double amountValue))
					{
						double amountInMillions = amountValue / 1000000;
						balanceList.Add($"{denom}:{amountInMillions.ToString("0.########", CultureInfo.InvariantCulture)}");
					}
					else
					{
						balanceList.Add($"{denom}:{amount}");
					}
				}

				if (string.IsNullOrEmpty(token))
				{
					// Если токен не указан, возвращаем строку со всеми балансами
					return (T)Convert.ChangeType(string.Join(", ", balanceList), typeof(T));
				}
				else
				{
					// Если токен указан, возвращаем баланс указанного токена
					string balanceToken = balanceList.FirstOrDefault(entry => entry.StartsWith(token + ":"))?.Split(':')[1] ?? "";
					if (typeof(T) == typeof(string))
						return (T)Convert.ChangeType(balanceToken, typeof(T));
					else if (double.TryParse(balanceToken, NumberStyles.Float, CultureInfo.InvariantCulture, out double balanceValue))
						return (T)Convert.ChangeType(balanceValue, typeof(T));
					else
						return default(T);
				}
			}
			catch (Exception ex)
			{
				project.SendInfoToLog(ex.Message);
				return default(T);
			}
		}
	
	}
	#endregion
	#region Socials
	public static class Twitter
	{
		public static void TwitterSetToken(this Instance instance, IZennoPosterProjectModel project, string authToken = "", bool log = false, [CallerMemberName] string caller = "")
		{
			if (project.Variables["debug"].Value == "True") log = true;
			instance.ClearCookie("x.com");
			instance.ClearCookie("twitter.com");
			instance.ClearCache("x.com");
			instance.ClearCache("twitter.com");
			if (authToken == "") authToken = project.Variables["twitterTOKEN"].Value;
			string[] headers = new string[]
			{
			    $"accept: {project.Profile.HTTPAccept}",
			    "accept-encoding: gzip, deflate, br",
			    $"accept-language: {project.Profile.AcceptLanguage}",
			    "sec-ch-ua-mobile: ?0",
			    "sec-ch-ua-platform: \"Windows\"",
			    "sec-fetch-dest: document",
			    "sec-fetch-mode: navigate",
			    "sec-fetch-site: none",
			    "sec-fetch-user: ?1",
			    "upgrade-insecure-requests: 1",
			    $"user-agent: {project.Profile.UserAgent}"
			};
			
			string result = ZennoPoster.HttpGet(
			    "https://x.com/", 
			    instance.GetProxy(),
			    "UTF-8",
			    ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.HeaderAndBody,
			    5000,
			    "",
			    project.Profile.UserAgent,
			    true,
			    5, 
			    headers,
			    "", 
			    false
			);
			
			instance.SetCookie($@".twitter.com	TRUE	/	FALSE	05/18/2033 06:33:20	auth_token	{authToken}	FALSE	TRUE");
			instance.SetCookie($@".x.com	TRUE	/	FALSE	05/18/2033 06:33:20	auth_token	{authToken}	FALSE	TRUE");
			if (log) Loggers.W3Log(project,$"[{caller}].[TwitterTokenSet] {authToken} set");
		}
		public static string TwitterGetStatus(this Instance instance, IZennoPosterProjectModel project,bool log = false, [CallerMemberName] string caller = "")
		{
			if (project.Variables["debug"].Value == "True") log = true;

			DateTime deadline = DateTime.Now.AddSeconds(60);
			string login = project.Variables["twitterLOGIN"].Value; var status = "";
			instance.ActiveTab.Navigate($"https://x.com/{project.Variables["twitterLOGIN"].Value}", "");
			Thread.Sleep(2000);
			instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)"); Thread.Sleep(3000);

			instance.ActiveTab.FindElementByAttribute("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
			instance.ActiveTab.FindElementByAttribute("button", "data-testid", "xMigrationBottomBar", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
			instance.ActiveTab.FindElementByAttribute("button", "innertext", "Refresh", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
			
			while (true)
			{
				Thread.Sleep(2000);

				if (log) Loggers.W3Log(project,$"{instance.ActiveTab.URL}");
				
				if (DateTime.Now > deadline) Loggers.W3Throw(project,"TwitterGetStatus timeout");
				
				if (!instance.ActiveTab.FindElementByAttribute("span", "innertext", "Something\\ went\\ wrong.\\ Try\\ reloading.", "regexp", 0).IsVoid)
				{	
					instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");
					Thread.Sleep(5000);
					continue;}
				
				if (!instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Caution:\s+This\s+account\s+is\s+temporarily\s+restricted", "regexp", 0).IsVoid)
					{status = "restricted";break;}
				if (!instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Account\s+suspended\s+X\s+suspends\s+accounts\s+which\s+violate\s+the\s+X\s+Rules", "regexp", 0).IsVoid)
					{status = "suspended";break;}
				if (!instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Log\ in", "regexp", 0).IsVoid || !instance.ActiveTab.FindElementByAttribute("a", "data-testid", "loginButton", "regexp", 0).IsVoid)
					{status = "unlogged";break;}

				if (!instance.ActiveTab.FindElementByAttribute("*", "innertext", "erify\\ your\\ email\\ address", "regexp", 0).IsVoid ||
					!instance.ActiveTab.FindElementByAttribute("div", "innertext", "We\\ sent\\ your\\ verification\\ code.", "regexp", 0).IsVoid)
					{status = "emailCapcha";break;}
				if (!instance.ActiveTab.FindElementByAttribute("button", "data-testid", "SideNav_AccountSwitcher_Button", "regexp", 0).IsVoid)
					{
						
						while (instance.ActiveTab.FindElementByAttribute("a", "data-testid", "AccountSwitcher_Logout_Button", "regexp", 0).IsVoid)
						{
						    if (DateTime.Now > deadline ) 
							{
								instance.ActiveTab.Navigate($"https://x.com/{project.Variables["twitterLOGIN"].Value}", "");
								Thread.Sleep(2000);
								continue;
							}
							instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
							instance.ActiveTab.FindElementByAttribute("button", "data-testid", "SideNav_AccountSwitcher_Button", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
						    Thread.Sleep(1000);
						}
						Thread.Sleep(1000);	
						if (log) Loggers.W3Log(project,$"[{caller}].[TwitterGetStatus] getting data from logOut button url now is {instance.ActiveTab.URL}");
						string attribute = instance.ActiveTab.FindElementByAttribute("a", "data-testid", "AccountSwitcher_Logout_Button", "regexp", 0).GetAttribute("innertext");
						instance.ActiveTab.FindElementByAttribute("button", "data-testid", "SideNav_AccountSwitcher_Button", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
						
						if (attribute.IndexOf(login, StringComparison.OrdinalIgnoreCase) >= 0)	
							{status = "ok";	break;}
						else
						{
							instance.CloseAllTabs();
							instance.ClearCookie("x.com");instance.ClearCache("x.com");
							instance.ClearCookie("twitter.com");instance.ClearCache("twitter.com");
							if (log) Loggers.W3Log(project,$"string [@{login}] not found in attribute [{attribute}]");
							//Loggers.W3Debug(project,$"string [@{login}] not found in attribute [{attribute}]");
							instance.ActiveTab.Navigate($"https://x.com/{project.Variables["twitterLOGIN"].Value}", "");
							Thread.Sleep(2000);
							status = "unlogged";
							break;
						}
					}
			}
			return status;
		}
		public static string TwitterLogin(this Instance instance, IZennoPosterProjectModel project, bool log = false, [CallerMemberName] string caller = "")
		{
			DateTime deadline = DateTime.Now.AddSeconds(60);
			var status = "";
			var login = project.Variables["twitterLOGIN"].Value;
			
			instance.ActiveTab.Navigate("https://x.com/", "");Thread.Sleep(2000);
			
			instance.ActiveTab.FindElementByAttribute("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
			instance.ActiveTab.FindElementByAttribute("button", "data-testid", "xMigrationBottomBar", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
			Thread.Sleep(2000);
			instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("a", "data-testid", "login", "regexp", 0));
			//1
			string waitEllement = instance.WaitGetValue(() => 
			    instance.ActiveTab.FindElementByAttribute("span", "innertext", "Phone,\\ email,\\ or\\ username", "regexp", 0),30);
			Loggers.W3Debug(project,$"input login found");
			instance.WaitSetValue(() => 
				instance.ActiveTab.GetDocumentByAddress("0").FindElementByAttribute("input:text", "autocomplete", "username", "text", 0),login);
			
			//2
			instance.ClickOut(() => 
				instance.ActiveTab.GetDocumentByAddress("0").FindElementByAttribute("span", "innertext", "Next", "regexp", 1));
			Loggers.W3Debug(project,$"next button is dissapeared");
			
			try
			{var check = instance.WaitGetValue(() => 
			    instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Sorry, we could not find your account')]", 0),2);
				Loggers.W3Debug(project,$"Sorry, we could not find your account");
				status = "notFound"; return status;}
			catch{}
			
			Loggers.W3Debug(project,$"input password");
			instance.WaitSetValue(() => 
				instance.ActiveTab.GetDocumentByAddress("0").FindElementByName("password"),project.Variables["twitterPASSWORD"].Value);
			instance.ClickOut(() => 
				instance.ActiveTab.GetDocumentByAddress("0").FindElementByAttribute("button", "data-testid", "LoginForm_Login_Button", "regexp", 0));
			
			
			try
			{var check = instance.WaitGetValue(() => 
			    instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Wrong password!')]", 0),2);
				Loggers.W3Log(project,$"!Wrong password!");
				status = "wrongPassword"; return status;}
			catch{}
			
			Loggers.W3Debug(project,$"input otp");
			var OTP = instance.GetOTP(project.Variables["twitterCODE2FA"].Value); 
			instance.WaitSetValue(() => 
				instance.ActiveTab.GetDocumentByAddress("0").FindElementByName("text"),OTP);
			
			instance.ClickOut(() => 
				instance.ActiveTab.GetDocumentByAddress("0").FindElementByAttribute("span", "innertext", "Next", "regexp", 1));
			
			try
			{var check = instance.WaitGetValue(() => 
			    instance.ActiveTab.FindElementByAttribute("span", "innertext", "Oops,\\ something\\ went\\ wrong.\\ Please\\ try\\ again\\ later.", "regexp", 0),3);
				Loggers.W3Log(project,$"!W Oops,\\ something\\ went\\ wrong");
				status = "somethingWrong"; return status;}
			catch{}
			
			try
			{var check = instance.WaitGetValue(() => 
			    instance.ActiveTab.FindElementByAttribute("*", "innertext", "Suspicious\\ login\\ prevented", "regexp", 0),1);
				Loggers.W3Log(project,$"!W Suspicious\\ login\\ prevented");
				status = "suspiciousLogin"; return status;}
			catch{}
			
			try
			{var check = instance.WaitGetValue(() => 
			    instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Your account is suspended')]", 0),1);
				Loggers.W3Log(project,$"!W Your account is suspended");
				status = "suspended"; return status;}
			catch{}
			
			instance.ActiveTab.FindElementByAttribute("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
			instance.ActiveTab.FindElementByAttribute("button", "data-testid", "xMigrationBottomBar", "regexp", 0).RiseEvent("click", instance.EmulationLevel);
			
			Loggers.W3Debug(project,$"should be ok");
			status = "ok"; return status;
			Thread.Sleep(3000);


		}
		public static string TwitterSyncToken(this Instance instance, IZennoPosterProjectModel project, bool log = false, [CallerMemberName] string caller = "")
		{
			bool found = false;
			string authTokenTwitter = "";
			string authTokenX = "";
			string tokenTable = project.Variables["twitterTOKEN"].Value;
			
			var twitterCookies = project.Profile.CookieContainer.Get(".twitter.com");
			var xCookies = project.Profile.CookieContainer.Get(".x.com");
			
			foreach (var cookie in twitterCookies)
			{
			    if (cookie.Name == "auth_token")
			    {
			        authTokenTwitter = cookie.Value;
			        found = true;break;
			    }
			}
			
			foreach (var cookie in xCookies)
			{
			    if (cookie.Name == "auth_token")
			    {
			        authTokenX = cookie.Value;
			        found = true;break;
			    }
			}
			
			if (found)
			{
			    string chosenToken = !string.IsNullOrEmpty(authTokenX) ? authTokenX : authTokenTwitter; 
				project.Variables["twitterTOKEN"].Value = chosenToken;
				Db.TwitterTokenUpdate(project);
			    project.Variables["a0debug"].Value = $"token extracted {chosenToken} from {(!string.IsNullOrEmpty(authTokenX) ? ".x.com" : ".twitter.com")}";
			    if (authTokenTwitter != authTokenX)
			    {
			        string expirationDate = "05/18/2033 06:33:20"; 
			        if (!string.IsNullOrEmpty(authTokenX))
						instance.SetCookie($@".twitter.com	TRUE	/	FALSE	{expirationDate}	auth_token	{authTokenX}	FALSE	TRUE");
			        else if (!string.IsNullOrEmpty(authTokenTwitter))
						instance.SetCookie($@".x.com	TRUE	/	FALSE	{expirationDate}	auth_token	{authTokenTwitter}	FALSE	TRUE");
					return chosenToken; 
			    }
			}
			return "";
		}		
		public static string TwitterFullCheck(this Instance instance, IZennoPosterProjectModel project, bool log = false, [CallerMemberName] string caller = "")
		{
			Db.Twitter(project);
			if (project.Variables["debug"].Value == "True") log = true;
			bool tokenSet = false;
			DateTime deadline = DateTime.Now.AddSeconds(60);
			var status = "";
			while (true)
			{
				if (DateTime.Now > deadline) Loggers.W3Throw(project,"twitter timeout");
				status = instance.TwitterGetStatus(project);
				if (log) Loggers.W3Log(project,status);
				if (status == "unlogged")
				{
					if (tokenSet == false)
					{
						instance.TwitterSetToken(project);
						tokenSet = true;
						Thread.Sleep(3000);
						continue;
					}
					else
					{
						status = instance.TwitterLogin(project);
						if (status != "ok") break;
						Thread.Sleep(3000);
						continue;
					}
				}
				if (log) Loggers.W3Log(project,status);
				break;
			}
			Thread.Sleep(3000);
			if (status == "ok")
			{
				var token = instance.TwitterSyncToken(project);	
				if (log) Loggers.W3Log(project,token);				
			}
			return status;
		}		
	}
	public static class Google
	{
		public static string GoogleCheckLogin(this Instance instance, IZennoPosterProjectModel project, bool log = false, [CallerMemberName] string caller = "")
		{
			instance.ActiveTab.Navigate("https://myaccount.google.com/", "");
			var status = "";		
			try
			{
				string heToWait = instance.WaitGetValue(() =>  instance.ActiveTab.FindElementByAttribute("a", "href", "https://accounts.google.com/SignOutOptions\\?", "regexp", 0), 5,"aria-label");
				var currentAcc = heToWait.Split('\n')[1];			
				if (currentAcc.IndexOf(project.Variables["googleLOGIN"].Value, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					Loggers.W3Debug (project,$"{currentAcc} is correct and logined");
					status = "ok";	
					return status;
				}	
				else 
				{
					Loggers.W3Log (project,$"!W {currentAcc} is InCorrect. MustBe {project.Variables["googleLOGIN"].Value}");
					status = "wrong";	
					return status;
				}	
			}
			catch
			{
					Loggers.W3Debug (project,$"account area not found");
					status = "undefined";	
			}				
			try
			{
				string heToWait = instance.WaitGetValue(() =>  instance.ActiveTab.FindElementByAttribute("a", "class", "h-c-header__cta-li-link\\ h-c-header__cta-li-link--primary\\ button-standard-mobile", "regexp", 1), 5,"aria-label");
				if (heToWait == "Go to your Google Account")
				{
					status = "unlogged";
					Loggers.W3Debug (project,$"Go to your Google Account. Go to login");
					instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("a", "class", "h-c-header__cta-li-link\\ h-c-header__cta-li-link--primary\\ button-standard-mobile", "regexp", 1));
				}
			}
			catch
			{
				status = "unknown";
				project.SendInfoToLog("no ontop buttons found");
			}
			return status;
		}		
		public static string GoogleFullCheck(this Instance instance, IZennoPosterProjectModel project, bool log = false, [CallerMemberName] string caller = "")
		{
			Db.Google(project);
			var status = "";
			while (true)
			{
				status =instance.GoogleCheckLogin(project);
				if (status == "ok") return status;
				if (status == "wrong") 
				{
						instance.CloseAllTabs();
						instance.ClearCookie("google.com");
						instance.ClearCookie("google.com");
						//instance.SetCookiesFromDB(project);
						continue;
				}
				break;
			}
			
				
						
			while (true)
			try
			{
				var userContainer = instance.WaitGetValue(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0));
				if (userContainer.IndexOf(project.Variables["googleLOGIN"].Value, StringComparison.OrdinalIgnoreCase) >= 0)	
				{
					//toDo = "Auth";
					instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0));
					try{instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Continue", "regexp", 0),2);}catch{}	
					Loggers.W3Debug (project,$"{userContainer} is correct and logined");
					status = "ok";	
					return status;
				}
				else
				{
					instance.CloseAllTabs();
					instance.ClearCookie("google.com");
					instance.ClearCookie("google.com");
					//instance.SetCookiesFromDB(project);
					Loggers.W3Log (project,$"!W {userContainer} is Wrong. MustBe {project.Variables["googleLOGIN"].Value}");
					status = "wrong";	
					continue;
				}
			}
			catch
			{
				Loggers.W3Log(project,$"no loggined Accounts detected");	
				try	{
					instance.WaitSetValue(() => instance.ActiveTab.FindElementById("identifierId"),project.Variables["googleLOGIN"].Value);
					instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Next", "regexp", 0),5);
					status = "unlogged";
				}catch{}
				
				try	{
					string Capcha = instance.WaitGetValue(() => instance.ActiveTab.FindElementByAttribute("div", "innertext", "Verify\\ it’s\\ you", "regexp", 0),5);
					status = "capcha";						
					SQL.W3Query(project,$@"UPDATE {project.Variables["projectTable"].Value} SET
					status = '!W fail.Google Capcha or Locked',
					cooldown = {Time.cd(24)},
					last = '{DateTime.Now.ToString("MM/dd-HH:mm")}' WHERE acc0 = {project.Variables["acc0"].Value};",true);
					SQL.W3Query(project,$@"UPDATE accGoogle SET
					status = '!WCapcha',
					cooldown = {Time.cd(24)},
					last = '{DateTime.Now.ToString("MM/dd-HH:mm")}' WHERE acc0 = {project.Variables["acc0"].Value};",true);
					
				}catch{}
				
				if (status == "capcha") throw new Exception("CAPCHA");
				try	{
					string BadBrowser= instance.WaitGetValue(() => instance.ActiveTab.FindElementByAttribute("div", "innertext", "Try\\ using\\ a\\ different\\ browser.\\ If\\ you’re\\ already\\ using\\ a\\ supported\\ browser,\\ you\\ can\\ try\\ again\\ to\\ sign\\ in.", "regexp", 0),1);
					status = "BadBrowser";
					SQL.W3Query(project,$@"UPDATE {project.Variables["projectTable"].Value} SET
					status = '!W fail.Google BadBrowser',
					cooldown = {Time.cd(24)},
					last = '{DateTime.Now.ToString("MM/dd-HH:mm")}' WHERE acc0 = {project.Variables["acc0"].Value};",true);
					SQL.W3Query(project,$@"UPDATE accGoogle SET
					status = '!BadBrowser',
					cooldown = {Time.cd(24)},
					last = '{DateTime.Now.ToString("MM/dd-HH:mm")}' WHERE acc0 = {project.Variables["acc0"].Value};",true);
					throw new Exception("BadBrowser");
				}catch{}
				if (status == "BadBrowser") throw new Exception("BadBrowser");
				
				if (!instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "-1", "regexp", 0).IsVoid)
				{		
					var userContainer = instance.WaitGetValue(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "-1", "regexp", 0));	
					if (userContainer.IndexOf(project.Variables["googleLOGIN"].Value, StringComparison.OrdinalIgnoreCase) >= 0)	
					{
						Loggers.W3Log(project,$"Signed Out acc detected [{userContainer}]");	
						instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "-1", "regexp", 0));
					}
					else
					{
						instance.CloseAllTabs();
						instance.ClearCookie("google.com");
						instance.ClearCookie("google.com");
						//instance.SetCookiesFromDB(project);
						Loggers.W3Log (project,$"!W {userContainer} is Wrong. MustBe {project.Variables["googleLOGIN"].Value}");
						status = "wrong";	
						continue;
					}
				}	
				try
				{
					Loggers.W3Debug (project,$"input pass {project.Variables["googlePASSWORD"].Value}");
					instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("Passwd"),project.Variables["googlePASSWORD"].Value,5);
					instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Next", "regexp", 0));
				}catch{}
				
				try
				{
					instance.WaitSetValue(() => instance.ActiveTab.FindElementById("totpPin"),instance.GetOTP(project.Variables["google2FACODE"].Value));	
					instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Next", "regexp", 0));
				}catch{}
				
				try	{
					string BadBrowser= instance.WaitGetValue(() => instance.ActiveTab.FindElementByAttribute("div", "innertext", "To\\ continue,\\ you’ll\\ need\\ to\\ verify\\ that\\ it’s\\ you", "regexp", 0),2);
					status = "verify";
					SQL.W3Query(project,$@"UPDATE {project.Variables["projectTable"].Value} SET
					status = '!W Google verify',
					cooldown = {Time.cd(24)},
					last = '{DateTime.Now.ToString("MM/dd-HH:mm")}' WHERE acc0 = {project.Variables["acc0"].Value};",true);
					SQL.W3Query(project,$@"UPDATE accGoogle SET
					status = '!W verify demanded',
					cooldown = {Time.cd(24)},
					last = '{DateTime.Now.ToString("MM/dd-HH:mm")}' WHERE acc0 = {project.Variables["acc0"].Value};",true);
					return status;
				}catch{}	
					
				try{string attribute = instance.WaitGetValue(() =>	instance.ActiveTab.FindElementByAttribute("*", "innertext", "error\\nAdd\\ a\\ recovery\\ phone", "regexp", 0),5);
				instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Cancel", "regexp", 0));}catch{}
				
				try{instance.WaitClick(() =>instance.ActiveTab.FindElementByAttribute("span", "innertext", "Not\\ now", "regexp", 0));}catch{}
				status = "mustBeOk";	
				return status;
			}

		}				
		public static string GoogleAuth(this Instance instance, IZennoPosterProjectModel project, bool log = false, [CallerMemberName] string caller = "")
		{
			try
			{
				var userContainer = instance.WaitGetValue(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0));
				Loggers.W3Debug(project,$"container:{userContainer} catched");	
				if (userContainer.IndexOf(project.Variables["googleLOGIN"].Value, StringComparison.OrdinalIgnoreCase) >= 0)	
				{
					Loggers.W3Debug(project,$"correct user found: {project.Variables["googleLOGIN"].Value}");	
					instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0));
					instance.ClickOut(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0));
					try
					{
						instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Continue", "regexp", 0));
						Loggers.W3Debug(project,$"{project.Variables["googleLOGIN"].Value} Auth SUCCESSFUL");
						return "SUCCESS";
					}catch{return "SUCCESS. No confirmation after clic was required";}					
				}
				else
				{
					Loggers.W3Log(project,$"!Wrong account [{userContainer}]. Expected: {project.Variables["googleLOGIN"].Value}. Cleaning");
					instance.CloseAllTabs();
					instance.ClearCookie("google.com");
					instance.ClearCookie("google.com");
					instance.SetCookiesFromDB(project);
					//W3Throw(project,"!W mixedAccounts [google]");
					return "FAIL. Wrong account";
				}
			}
			catch
			{
				return "FAIL. No loggined Users Found";
			}
		}		
	}

    #endregion
	#region Time
	
	public static class Time
	{

		public static string UnixNow()
		{
			return ((long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds)).ToString();
		}
		public static string Elapsed(string start)
		{
			string result = $"{TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(start))}";
			return result;
		}
		public static int TimeElapsed(IZennoPosterProjectModel project, string varName = "varSessionId")
		{
		    var start = project.Variables[$"{varName}"].Value;
		    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		    long startTime = long.Parse(start);
		    int difference = (int)(currentTime - startTime);
		    
		    return difference;
		}
		public static string cd(object input = null)
		{
		    DateTime utcNow = DateTime.UtcNow;
		    
		    if (input == null)
		    {
		        DateTime todayEnd = utcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
		        return ((int)(todayEnd - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
		    }
		    else if (input is decimal || input is int)  
		    {
				decimal minutes = Convert.ToDecimal(input);
				int secondsToAdd = (int)Math.Round(minutes * 60); 
				return ((int)(utcNow - new DateTime(1970, 1, 1)).TotalSeconds + secondsToAdd).ToString();
		    }
		    else if (input is string timeString)
		    {
		        TimeSpan parsedTime = TimeSpan.Parse(timeString);
		        DateTime futureTime = utcNow.Add(parsedTime);
		        return ((int)(futureTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
		    }
		    
		    throw new ArgumentException("Неподдерживаемый тип входного параметра");
		}	
		public static string TotalTime(IZennoPosterProjectModel project)
		{
			var elapsedMinutes = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(project.Variables["varSessionId"].Value)) / 60.0;
            return TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - long.Parse(project.Variables["varSessionId"].Value)).ToString();
		}
		public static void RndSleep(IZennoPosterProjectModel project, int min = 0,  int max = 0, bool log = false)
		{
			//int min = 0;  int max = 0; bool log = false;
			var rnd = new Random();
			if (min == 0) min = int.Parse(project.Variables["delayMin"].Value);
			if (max == 0) min = int.Parse(project.Variables["delayMax"].Value);
			int sleep = rnd.Next(min,max);
			if (log) project.SendInfoToLog($"sleep {sleep}s");
			Thread.Sleep(sleep*1000);			
		}
	}
	#endregion
	#region Tx
	public static class Onchain
	{
		
		public static string HexToString(string hexValue, string convert = "")
		{
		    try
		    {
		        hexValue = hexValue?.Replace("0x", "").Trim();
		        if (string.IsNullOrEmpty(hexValue)) return "0";
		        BigInteger number = BigInteger.Parse("0" + hexValue, NumberStyles.AllowHexSpecifier);
		        switch(convert.ToLower())
		        {
		            case "gwei":
		                decimal gweiValue = (decimal)number / 1000000000m;
		                return gweiValue.ToString("0.#########", CultureInfo.InvariantCulture);
		            case "eth":
		                decimal ethValue = (decimal)number / 1000000000000000000m;
		                return ethValue.ToString("0.##################", CultureInfo.InvariantCulture);
		            default:
		                return number.ToString();
		        }
		    }
		    catch
		    {
		        return "0";
		    }
		}	
		
		public static string SendLegacy(string chainRpc, string contractAddress, string encodedData, decimal value, string walletKey, int speedup = 1)
		{
			try 
			{
				var web3 = new Nethereum.Web3.Web3(chainRpc);
				try
				{
					var chainIdTask = web3.Eth.ChainId.SendRequestAsync();
					chainIdTask.Wait();
					int chainId = (int)chainIdTask.Result.Value;				
					string fromAddress = new Nethereum.Signer.EthECKey(walletKey).GetPublicAddress();	
					try
					{
						var gasPriceTask = web3.Eth.GasPrice.SendRequestAsync();
						gasPriceTask.Wait();
						BigInteger gasPrice = gasPriceTask.Result.Value;
						
						BigInteger baseGasPrice = gasPrice / 100 + gasPrice;
						BigInteger adjustedGasPrice = baseGasPrice / 100 * speedup + gasPrice;
						
						var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
						{
							To = contractAddress,
							From = fromAddress,
							Data = encodedData,
							Value = new Nethereum.Hex.HexTypes.HexBigInteger((BigInteger)(value * 1000000000000000000m))
						};
						
						try
						{
							var gasEstimateTask = web3.Eth.Transactions.EstimateGas.SendRequestAsync(transactionInput);
							gasEstimateTask.Wait();
							var gasEstimate = gasEstimateTask.Result;
							var gasLimit = gasEstimate.Value + (gasEstimate.Value / 2);
							
							var blockchain = new Blockchain(walletKey, chainId, chainRpc);
							string hash = blockchain.SendTransaction(contractAddress, value, encodedData, gasLimit, adjustedGasPrice).Result;
							
							return hash;
						}
						catch (Exception ex)
						{
							throw new Exception($"Ошибка на стадии оценки газа или отправки: {ex.Message}");
						}
					}
					catch (Exception ex)
					{
						throw new Exception($"Ошибка на стадии получения цены газа: {ex.Message}");
					}
				}
				catch (Exception ex)
				{
					throw new Exception($"Ошибка на стадии получения chainId: {ex.Message}");
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Ошибка отправки транзакции: {ex.Message}");
			}
		}
		public static string SendTx1559(string chainRpc, string contractAddress, string encodedData, decimal value, string walletKey, int speedup = 1)
		{
		    
			var web3 = new Nethereum.Web3.Web3(chainRpc);
			var chainIdTask = web3.Eth.ChainId.SendRequestAsync(); chainIdTask.Wait();
			int chainId = (int)chainIdTask.Result.Value;
			string fromAddress = new Nethereum.Signer.EthECKey(walletKey).GetPublicAddress();
			//
			BigInteger _value = (BigInteger)(value * 1000000000000000000m);
			//
			BigInteger gasLimit = 0;  BigInteger priorityFee = 0;  BigInteger maxFeePerGas = 0;  BigInteger baseGasPrice = 0;
			try 
		    {   var gasPriceTask = web3.Eth.GasPrice.SendRequestAsync();gasPriceTask.Wait();
		        baseGasPrice = gasPriceTask.Result.Value / 100 + gasPriceTask.Result.Value;
		        priorityFee =  baseGasPrice / 100 * speedup + gasPriceTask.Result.Value;
		        maxFeePerGas =  baseGasPrice / 100 * speedup + gasPriceTask.Result.Value;}
		     catch (Exception ex)  {throw new Exception($"failedEstimateGas: {ex.Message}");}
		       
			try 
		    {   var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
		        {
		            To = contractAddress,
		            From = fromAddress,
		            Data = encodedData,
		            //Value = new Nethereum.Hex.HexTypes.HexBigInteger((BigInteger)value),
					Value = new Nethereum.Hex.HexTypes.HexBigInteger((BigInteger)_value),
		            MaxPriorityFeePerGas = new Nethereum.Hex.HexTypes.HexBigInteger(priorityFee),
		            MaxFeePerGas = new Nethereum.Hex.HexTypes.HexBigInteger(maxFeePerGas),
		            Type = new Nethereum.Hex.HexTypes.HexBigInteger(2) 
		        };
		        
		        var gasEstimateTask = web3.Eth.Transactions.EstimateGas.SendRequestAsync(transactionInput);
		        gasEstimateTask.Wait();
		        var gasEstimate = gasEstimateTask.Result;
		        gasLimit = gasEstimate.Value + (gasEstimate.Value / 2);
			}
		    catch (AggregateException ae)
			{
			    if (ae.InnerException is Nethereum.JsonRpc.Client.RpcResponseException rpcEx)
			    {
			        var error = $"Code: {rpcEx.RpcError.Code}, Message: {rpcEx.RpcError.Message}, Data: {rpcEx.RpcError.Data}";
			        throw new Exception($"FailedSimulate RPC Error: {error}");
			    }
			    throw;
			}
			try 
		    {	
		        var blockchain = new Blockchain(walletKey, chainId, chainRpc);
				string hash = blockchain.SendTransactionEIP1559(contractAddress, value, encodedData, gasLimit, maxFeePerGas, priorityFee).Result;
		        return hash;
		    }
		    catch (Exception ex)
		    {
		        throw new Exception($"FailedSend: {ex.Message}");
		    }
		}	
	
		public static string GazZip(IZennoPosterProjectModel project, string chainTo, decimal value, string chainRPC = "") //refuel GazZip
		
		{
			
			// 0x010066 Sepolia | 0x01019e Soneum
			
			
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; 
			Random rnd = new Random();
			var accountAddress = Db.AdrEvm(project); 
			
			if ( chainRPC == "")
			{
				string chainList = @"https://mainnet.era.zksync.io,
				https://linea-rpc.publicnode.com,
				https://arb1.arbitrum.io/rpc,
				https://optimism.llamarpc.com,
				https://scroll.blockpi.network/v1/rpc/public,
				https://rpc.taiko.xyz,
				https://base.blockpi.network/v1/rpc/public,
				https://rpc.zora.energy";
				
				
				bool found = false; 
				foreach (string RPC in chainList.Split(','))
				{
				    chainRPC = RPC.Trim();
					var native = Leaf.balNative<decimal>(project, RPC, accountAddress);
					var required = value + 0.00015m;
				    if (native > required)
				    {
				        project.SendInfoToLog($"CHOSEN: rpc:[{chainRPC}] native:[{native}]", true);
						found = true; break;
				    }
					project.SendInfoToLog($"rpc:[{chainRPC}] native:[{native}] lower than [{required}]");
				    Thread.Sleep(1000);
				}
				
				
				if (!found)
				{
				    return $"fail: no balance over {value}ETH found by all Chains";
				}
			}
			
			else 
			{
				var native = Leaf.balNative<decimal>(project, chainRPC, accountAddress);
				Loggers.W3Debug(project,$"rpc:[{chainRPC}] native:[{native}]");
				if (native < value + 0.0002m)
				{
					return $"fail: no balance over {value}ETH found on {chainRPC}";
				}
			}
			
			string functionName = "transfer";// withdraw
			
			string[] types = { };
			object[] values = { };
			
			try
			{
			    string dataEncoded = chainTo;//0x010066 for Sepolia//0x01019e Soneum
			    string txHash = SendTx1559(
			        chainRPC,
			        "0x391E7C679d29bD940d63be94AD22A25d25b5A604",//gazZip
			        dataEncoded,
			        value,  // value в ETH
			        Db.KeyEVM(project),
			        3          // speedup %
			    );
			    Thread.Sleep(1000);
			    project.Variables["blockchainHash"].Value = txHash;
			}
			catch (Exception ex){project.SendWarningToLog($"{ex.Message}",true);throw;}
			string result = Leaf.waitTx(project,chainRPC);
			return result;
		}
		public static string ApproveMax(IZennoPosterProjectModel project, string contract, string spender, string chainRPC = "")
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			if (chainRPC == "" ) chainRPC = project.Variables["blockchainRPC"].Value;

			string abi = @"[{""inputs"":[{""name"":""spender"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""}]";

			string[] types = { "address", "uint256" };
			object[] values = { spender, BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935") }; // max uint256

			try
			{
				string txHash = SendLegacy(
					chainRPC,
					contract,
					w3tools.Encoder.EncodeTransactionData(abi, "approve", types, values),
					0,
					Db.KeyEVM(project),
					3
				);
				
				project.Variables["blockchainHash"].Value = txHash;
			}
			catch (Exception ex){Loggers.W3Throw(project,$"!W:{ex.Message}");}

			Loggers.W3Log(project,$"[APPROVE] {contract} for spender {spender}...");
			return Leaf.waitTx(project);
		}
		public static string ApproveCancel(IZennoPosterProjectModel project, string contract, string spender, string chainRPC = "")
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			if (chainRPC == "" ) chainRPC = project.Variables["blockchainRPC"].Value;

			string abi = @"[{""inputs"":[{""name"":""spender"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""}]";

			string[] types = { "address", "uint256" };
			object[] values = { spender, BigInteger.Parse("0") }; // max uint256

			try
			{
				string txHash = SendLegacy(
					chainRPC,
					contract,
					w3tools.Encoder.EncodeTransactionData(abi, "approve", types, values),
					0,
					Db.KeyEVM(project),
					3
				);
				
				project.Variables["blockchainHash"].Value = txHash;
			}
			catch (Exception ex){Loggers.W3Throw(project,$"!W:{ex.Message}");}

			Loggers.W3Log(project,$"[APPROVE] {contract} for spender {spender}...");
			return Leaf.waitTx(project);
		}
		public static string WrapNative(IZennoPosterProjectModel project, string contract, decimal value, string chainRPC = "")
		{
		    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		    
		    if (chainRPC == "") chainRPC = project.Variables["blockchainRPC"].Value;
		    
		    string abi = @"[{""inputs"":[],""name"":""deposit"",""outputs"":[],""stateMutability"":""payable"",""type"":""function""}]";
		    
		    string[] types = { };
		    object[] values = { };
		    
		    try
		    {
		        string txHash = SendLegacy(
		            chainRPC,
		            contract,
		            w3tools.Encoder.EncodeTransactionData(abi, "deposit", types, values),
		            value,
		            Db.KeyEVM(project),
		            3
		        );
		        
		        project.Variables["blockchainHash"].Value = txHash;
		    }
		    catch (Exception ex){Loggers.W3Throw(project,$"!W:{ex.Message}");}
		    
		    Loggers.W3Log(project,$"[WRAP] {value} native to {contract}...");
			return Leaf.waitTx(project);
		}
	}
	#endregion
	#region CEX
	public static class Binance
	{ 
        public static string GetUserAsset(IZennoPosterProjectModel project, string coin = "")
		{
			string[] keys = Db.BinanceApiKeys(project).Split(';');
			
			var apiKey = keys[0];
			var secretKey = keys[1];
			var proxy = keys[2];
			var method = "/sapi/v3/asset/getUserAsset";
			var hash = "";
			
			
			string parameters = $"timestamp={Time.UnixNow()}";
			
			byte[] secretkeyBytes = Encoding.UTF8.GetBytes(secretKey);
			using (HMACSHA256 hmacsha256 = new HMACSHA256(secretkeyBytes))
			{
			    byte[] inputBytes = Encoding.UTF8.GetBytes(parameters);
			    byte[] hashValue = hmacsha256.ComputeHash(inputBytes);
				hash = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
			}
			
			
			string jsonBody = $@"{parameters}&signature={hash}";
			
			string[] headers = new string[] {
			    "User-Agent: Mozilla/4.0 (compatible; PHP Binance API)",
				$"X-MBX-APIKEY: {apiKey}",
				"Content-Type: application/x-www-form-urlencoded"
			};
			string url = $"https://api.binance.com{method}";
			
			string response = ZennoPoster.HttpPost(
			    url,
			    Encoding.UTF8.GetBytes(jsonBody),
			    "application/x-www-form-urlencoded",
			    proxy, 
			    "UTF-8",
			    ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
			    30000,
			    "",
			    "Mozilla/4.0",
			    true,
			    5,
			    headers,
			    "",
			    true
			);
			
			project.Json.FromString(response);
			
			var balanceList = "";
			foreach (var item in project.Json)
			{
			    string asset = item.asset;
			    string free = item.free;
				balanceList += $"{asset}:{free}\n"; 
			}
			
			balanceList.Trim();
			//return balanceList;

            if (coin == "") return $"{balanceList}";
            if (!balanceList.Contains(coin)) return $"NoCoinFound: {coin}";

            string tiker = "", balance = "";
            foreach(string asset in balanceList.Split('\n'))
            {
                tiker = asset.Split(':')[0];
                balance = asset.Split(':')[1];
                if (tiker == coin) break;
            }
            
            return $"{balance}";

		}
		public static string Withdraw(IZennoPosterProjectModel project, string amount, string network, string coin = "ETH", string address = "")
		{
			string[] keys = Db.BinanceApiKeys(project).Split(';');	
			var apiKey = keys[0]; var secretKey = keys[1]; var proxy = keys[2];
			
			if (address == "") address = Db.AdrEvm(project);
			var parameters = $"timestamp={Time.UnixNow()}&coin={coin}&network={network}&address={address}&amount={amount}";
			
			var hash = "";
			byte[] secretkeyBytes = Encoding.UTF8.GetBytes(secretKey);
			using (HMACSHA256 hmacsha256 = new HMACSHA256(secretkeyBytes))
			{
			    byte[] inputBytes = Encoding.UTF8.GetBytes(parameters);
			    byte[] hashValue = hmacsha256.ComputeHash(inputBytes);
				hash = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
			}
			
			string jsonBody = $@"{parameters}&signature={hash}";
			project.SendInfoToLog(jsonBody);
			
			string[] headers = new string[] {
			    "User-Agent: Mozilla/4.0 (compatible; PHP Binance API)",
				$"X-MBX-APIKEY: {apiKey}",
				"Content-Type: application/x-www-form-urlencoded"
			};
			
			string response = ZennoPoster.HttpPost( $"https://api.binance.com/sapi/v1/capital/withdraw/apply",
			    Encoding.UTF8.GetBytes(jsonBody), "application/x-www-form-urlencoded",
				proxy, "UTF-8", ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly, 30000, "", "Mozilla/4.0", true, 5, headers, "", true );
			
			return response;
		}
        public static string GetWithdrawHistory(IZennoPosterProjectModel project, string searchId = "")
        {
			string[] keys = Db.BinanceApiKeys(project).Split(';');
			
			var apiKey = keys[0];
			var secretKey = keys[1];
			var proxy = keys[2];
            
            string parameters = $"timestamp={Time.UnixNow()}";
            string hash = "";
            
            byte[] secretkeyBytes = Encoding.UTF8.GetBytes(secretKey);
            using (HMACSHA256 hmacsha256 = new HMACSHA256(secretkeyBytes))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(parameters);
                byte[] hashValue = hmacsha256.ComputeHash(inputBytes);
                hash = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
            
            string[] headers = new string[] {
                "User-Agent: Mozilla/4.0 (compatible; PHP Binance API)",
                $"X-MBX-APIKEY: {apiKey}",
                "Content-Type: application/x-www-form-urlencoded"
            };
            
            string url = $"https://api.binance.com/sapi/v1/capital/withdraw/history?{parameters}&signature={hash}";
            
            string response = ZennoPoster.HttpGet(
                url,
                proxy, 
                "UTF-8",
                ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                30000,
                "",
                "Mozilla/4.0",
                true,
                5,
                headers,
                "",
                true
            );
            project.SendInfoToLog(response);
            project.Json.FromString(response);
            
            var historyList = "";
			foreach (var item in project.Json)
			{
				string id = item.id;
				string amount = item.amount;
				string coin = item.coin;
				string status = item.status.ToString(); // явное преобразование числового status в строку
				historyList += $"{id}:{amount}:{coin}:{status}\n";
			}
            
            historyList = historyList.Trim();
            
            if (searchId == "") return historyList;
            if (!historyList.Contains(searchId)) return $"NoIdFound: {searchId}";
            
            string foundId = "", foundAmount = "", foundCoin = "", foundStatus = "";
            foreach(string withdrawal in historyList.Split('\n'))
            {
                var parts = withdrawal.Split(':');
                foundId = parts[0];
                foundAmount = parts[1];
                foundCoin = parts[2];
                foundStatus = parts[3];
                if (foundId == searchId) break;
            }
            
            return $"{foundAmount}:{foundCoin}:{foundStatus}";
        }
	}
	#endregion
	#region Wallets
	
	public static class MM
	{
		public static string Confirm(this Instance instance,IZennoPosterProjectModel project, bool log = false )
		{
			instance.UseFullMouseEmulation = false;
			DateTime urlChangeDeadline = DateTime.Now.AddSeconds(60);
			int attemptCount = 0;

			if (log)project.SendInfoToLog("Waiting for MetaMask URL to appear...");
			while (!instance.ActiveTab.URL.Contains("nkbihfbeogaeaoehlefnkodbefgpgknn"))
			{
				Thread.Sleep(1000); attemptCount++;
				if (log)project.SendInfoToLog($"Attempt {attemptCount}: Current URL is {instance.ActiveTab.URL}");
				if (attemptCount > 5)
				{
					if (log)project.SendErrorToLog("Failed to load MetaMask URL within 6 seconds");
					throw new Exception("Timeout waiting for MetaMask URL");
				}
			}

			if (log)project.SendInfoToLog($"{instance.ActiveTab.URL} detected, pausing for 2 seconds...");
			Thread.Sleep(2000); 

			HtmlElement allert = instance.ActiveTab.FindElementByAttribute("div", "class", "mm-box\\ mm-banner-base\\ mm-banner-alert\\ mm-banner-alert--severity-danger", "regexp", 0);
			HtmlElement simulation = instance.ActiveTab.FindElementByAttribute("div", "data-testid", "simulation-details-layout", "regexp", 0);
			HtmlElement detail = instance.ActiveTab.FindElementByAttribute("div", "class", "transaction-detail", "regexp", 0);

			if (log)project.SendInfoToLog($"{Regex.Replace(simulation.GetAttribute("innertext").Trim(), @"\s+", " ")}");
			if (log)project.SendInfoToLog($"{Regex.Replace(detail.GetAttribute("innertext").Trim(), @"\s+", " ")}");
			if (!allert.IsVoid) 
			{
						var error = Regex.Replace(allert.GetAttribute("innertext").Trim(), @"\s+", " ");
						while (!instance.ActiveTab.FindElementByAttribute("button", "data-testid", "page-container-footer-cancel", "regexp", 0).IsVoid)
						{
							instance.ActiveTab.Touch.SwipeBetween(600, 400, 600, 300);
							instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "page-container-footer-cancel", "regexp", 0));
						}
						Loggers.W3Throw(project,error);
			}
			if (log)project.SendInfoToLog("Starting button click loop on MetaMask page...");
			while (instance.ActiveTab.URL.Contains("nkbihfbeogaeaoehlefnkodbefgpgknn"))
			{
				if (DateTime.Now > urlChangeDeadline)
				{
					if (log)project.SendErrorToLog("Operation timed out after 60 seconds");
					throw new Exception("Timeout exceeded while interacting with MetaMask");
				}
				try
				{
					if (log)project.SendInfoToLog("Attempting to find and click the confirm button...");
					instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "class", "button btn--rounded btn-primary", "regexp", 0), 3);
					if (log)project.SendInfoToLog("Button clicked successfully");
					Thread.Sleep(2000);
				}
				catch (Exception ex)
				{
					if (log)project.SendWarningToLog($"Failed to click button: {ex.Message}");
				}
			}
			if (log)project.SendInfoToLog("MetaMask interaction completed, URL has changed");
			instance.UseFullMouseEmulation = true;
			return "done";
		}
		public static string Run(this Instance instance,IZennoPosterProjectModel project,string key = "", bool skipCheck = false)
		{
			instance.UseFullMouseEmulation = false;
            string address = "";
			while (true)
			{
				instance.CloseExtraTabs();
				instance.ActiveTab.Navigate("chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html", "");
				instance.CloseExtraTabs(); var toDo = ""; Thread.Sleep(3000);
				DateTime deadline = DateTime.Now.AddSeconds(60);//if (DateTime.Now < deadline ) Thread.Sleep(1000);
				var password = SAFU.HWPass(project);
				
				while (true)
				{Thread.Sleep(3000);
				if (!instance.ActiveTab.FindElementByAttribute("div", "class", "error-code", "regexp", 0).IsVoid) 
					{toDo = "install,import";break;}
				else if (!instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-options-menu-button", "regexp", 0).IsVoid) 
					{toDo = "checkAddress";break;}
				else if (!instance.ActiveTab.FindElementByAttribute("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0).IsVoid) 
					{toDo = "import";break;}
				else if (!instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0).IsVoid) 
					{toDo = "unlock";break;}
				}
				Loggers.W3Log(project,toDo);
				
				if ( toDo.Contains("install")) 
				{
					string path = $"{project.Path}.crx\\MetaMask 11.16.0.crx"; 
					instance.InstallCrxExtension(path);
					Loggers.W3Log(project,$"installing {path}"); 
				}
				if (toDo.Contains("import")) 
				{
					string welcomeURL = $"chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html#onboarding/welcome"; 
					while (true)
					{
						if (instance.ActiveTab.URL == welcomeURL) break;
						if (DateTime.Now < deadline ) Thread.Sleep(1000);
						else
						{
							instance.CloseExtraTabs();
							instance.ActiveTab.Navigate("chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html", "");
							break;
						}
					}
                    if (key == "") key = Db.KeyEVM(project);
                    else skipCheck = true;
					
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0));
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("span", "innertext", "I\\ agree\\ to\\ MetaMask\'s\\ Terms\\ of\\ use", "regexp", 1),10,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "aria-label", "Close", "regexp", 0));
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "onboarding-create-wallet", "regexp", 0),10,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "metametrics-no-thanks", "regexp", 0),10,0);
					instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "data-testid", "create-password-new", "regexp", 0),password);
					instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "data-testid", "create-password-confirm", "regexp", 0),password);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("span", "innertext", "I\\ understand\\ that\\ MetaMask\\ cannot\\ recover\\ this\\ password\\ for\\ me.\\ Learn\\ more", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "create-password-wallet", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "secure-wallet-later", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("label", "class", "skip-srp-backup-popover__label", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "skip-srp-backup", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "onboarding-complete-done", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "pin-extension-next", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "pin-extension-done", "regexp", 0),5,0);
					Thread.Sleep(1000); while (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid)
					{instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "popover-close", "regexp", 0));}
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-menu-icon", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "multichain-account-menu-popover-action-button", "regexp", 0),5,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("span", "style", "mask-image:\\ url\\(\"./images/icons/import.svg\"\\);", "regexp", 0),5,0);
					instance.WaitSetValue(() => instance.ActiveTab.FindElementById("private-key-box"), key);
					instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "import-account-confirm-button", "regexp", 0),5,0);
					toDo = "checkAddress";
				}
				if ( toDo == "unlock") 
				{
					//pass
					instance.WaitSetValue(() => instance.ActiveTab.FindElementById("password"),password,3,0);
					instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0));
					if (!instance.ActiveTab.FindElementByAttribute("p", "innertext", "Incorrect password", "text", 0).IsVoid) 
					{
					    instance.CloseAllTabs(); instance.UninstallExtension("nkbihfbeogaeaoehlefnkodbefgpgknn"); 
					    project.Variables["a0debug"].Value = $"wallet fuckup"; project.SendWarningToLog(Loggers.W3Log(project),true); throw new Exception("wrongPassword");
					}	
					toDo = "checkAddress";
				}
				
				if ( toDo == "checkAddress") 
				{
						while (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid)
						{
							try	{instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "popover-close", "regexp", 0),2,0);}
							catch{instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).RiseEvent("click", instance.EmulationLevel);};
						}
						
						//addres
						try{
						instance.WaitSetValue(() => instance.ActiveTab.FindElementById("password"),password,3,0);
					    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0));}
						catch{}
						
						instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-options-menu-button", "regexp", 0),5,0);
						instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-list-menu-details", "regexp", 0),5,0);
						address = instance.WaitGetValue(() =>    instance.ActiveTab.FindElementByAttribute("button", "data-testid", "address-copy-button-text", "regexp", 0));
						
                        if (!skipCheck)
						if(!String.Equals(address,project.Variables["addressEvm"].Value,StringComparison.OrdinalIgnoreCase))
						{
						    instance.CloseAllTabs(); instance.UninstallExtension("nkbihfbeogaeaoehlefnkodbefgpgknn"); 
						    Loggers.W3Log(project,$"!WrongWallet expected: {project.Variables["addressEvm"].Value}. InWallet {address}"); continue;//throw new Exception("!WrongWallet");
						}	
				}
				instance.UseFullMouseEmulation = true;
				return address;
			}
		}

	}
	public static class Keplr
	{
		public static string Approve(this Instance instance,IZennoPosterProjectModel project)
		{
			string extId = "dmkamcknogkgcdfhhbddcghachkejeap";

			instance.UseFullMouseEmulation = false;
			DateTime deadline = DateTime.Now.AddSeconds(10);
			
			while (true)
			{
				if (DateTime.Now > deadline) throw new Exception("no kepl tab");
				if (instance.ActiveTab.URL.Contains(extId)) break;
			}
			Loggers.W3Debug(project,"Keplr tab detected");
			
			try
			{
				instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Approve", "regexp", 0),5,2);
				instance.ClickOut(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Approve", "regexp", 0),2,2);
				Loggers.W3Debug(project,"Approve button detected");
			} 
			catch
			{
				Loggers.W3Debug(project,"!W No Approve button");
			}
			instance.UseFullMouseEmulation = true;
			
			deadline = DateTime.Now.AddSeconds(10);
			while (true)
			{
				if (DateTime.Now > deadline) Loggers.W3Throw(project,"Keplr tab stucked");
				if (!instance.ActiveTab.URL.Contains(extId)) break;
			}
			Loggers.W3Debug(project,"Keplr tab closed");
			return "done";
		}
		public static void KeplrClick(this Instance instance, HtmlElement he)
		{
			int x = int.Parse(he.GetAttribute("leftInTab"));int y = int.Parse(he.GetAttribute("topInTab"));
			x = x - 450;instance.Click(x, x, y, y, "Left", "Normal");Thread.Sleep(1000);
			return;
		}
		public static string KeplrCheck(this Instance instance)
		{
			instance.CloseExtraTabs(); 
			//Tab exTab = instance.NewTab("keplr"); 
			instance.ActiveTab.Navigate($"chrome-extension://dmkamcknogkgcdfhhbddcghachkejeap/popup.html#/", "");var toDo = "";
			int i = 1;
			DateTime deadline = DateTime.Now.AddSeconds(15);
			while (true)
			{
				if (DateTime.Now > deadline) throw new Exception($"!W cant't check KeplrState");
				Thread.Sleep(1000);
				if (!instance.ActiveTab.FindElementByAttribute("div", "class", "error-code", "regexp", 0).IsVoid) return "install";
				else if (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0).IsVoid)  return "import";
				else if (!instance.ActiveTab.FindElementByAttribute("input:password", "tagname", "input", "regexp", 0).IsVoid)  return "inputPass";
				else if (!instance.ActiveTab.FindElementByAttribute("div", "innertext", "Copy\\ Address", "regexp", 0).IsVoid)  return "setSourse"; 

			}
			return "unknown";				
		}
		public static void KeplrImportSeed(this Instance instance,IZennoPosterProjectModel project)
		{
			var WalletPassword = SAFU.HWPass(project);
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0),comment:"Import\\ an\\ existing\\ wallet");
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Use\\ recovery\\ phrase\\ or\\ private\\ key", "regexp", 0));
			string seedPhrase = Db.Seed(project);
			int index = 0;	
			foreach(string word in seedPhrase.Split(' ')) 
				{ 
					instance.ActiveTab.FindElementByAttribute("input", "fulltagname", "input:", "regexp", index).SetValue(word, "Full", false);
					index++;
				}
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 1));
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("name"),"seed");
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("password"),WalletPassword,2,Throw:false);
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("confirmPassword"),WalletPassword,2,Throw:false);
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Next", "regexp", 0));
			
			string check = instance.WaitGetValue(() => 
			instance.ActiveTab.FindElementByAttribute("div", "innertext", "Select\\ All", "regexp", 0));
			int j = 0;	while (!instance.ActiveTab.FindElementByAttribute("div", "innertext", "Select\\ All", "regexp", j).IsVoid) j++;
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("div", "innertext", "Select\\ All", "regexp", j-1));

			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Save", "regexp", 0));
			while  (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).IsVoid)
			{
				instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).RiseEvent("click", instance.EmulationLevel); 
				Thread.Sleep(2000);
			}
			instance.CloseExtraTabs();
		}
		public static void KeplrImportPkey(this Instance instance,IZennoPosterProjectModel project)
		{
			var WalletPassword = SAFU.HWPass(project);
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0));
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Use\\ recovery\\ phrase\\ or\\ private\\ key", "regexp", 0));
			var key = Db.KeyEVM(project);
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Private\\ key", "regexp", 1));
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "tagname", "input", "regexp", 0),key);
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 1));
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("name"),"pkey");
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("password"),WalletPassword,2,Throw:false);
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("confirmPassword"),WalletPassword,2,Throw:false);
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Next", "regexp", 0));
			
			string check = instance.WaitGetValue(() => 
			instance.ActiveTab.FindElementByAttribute("div", "innertext", "Select\\ All", "regexp", 0));
			
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Save", "regexp", 0));
			while  (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).IsVoid)
			{
				instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).RiseEvent("click", instance.EmulationLevel); 
				Thread.Sleep(2000);
			}
			
		}
		public static void KeplrSetSource(this Instance instance,IZennoPosterProjectModel project, string source)
		{
			//var source  = "pkey";//"seed"

			while (true)
			{
				instance.CloseExtraTabs();
				instance.ActiveTab.Navigate("chrome-extension://dmkamcknogkgcdfhhbddcghachkejeap/popup.html#/wallet/select", "");
				string heToWait = instance.WaitGetValue(() => 
					instance.ActiveTab.FindElementByAttribute("button", "innertext", "Add\\ Wallet", "regexp", 0)
				);
				
				var imported = instance.KeplrPrune(project);
				project.SendInfoToLog(imported);
				if (imported.Contains("seed") && imported.Contains("pkey")) 
				{
					instance.KeplrClick(instance.LastHe(("div", "innertext", source, "regexp", 0)));
					project.SendInfoToLog($"sourse set to {source}");
					return;
				}
				else 	
				{
					project.SendInfoToLog("not all wallets imported");
					instance.KeplrClick(instance.ActiveTab.FindElementByAttribute("button", "innertext", "Add\\ Wallet", "regexp", 0));
					instance.KeplrImportPkey(project);
					continue;
				}
			}	
		}
		public static void KeplrInstallExt(this Instance instance,IZennoPosterProjectModel project)
		{
			string path = $"{project.Path}.crx\\keplr0.12.169.crx";
			instance.InstallCrxExtension(path);Thread.Sleep(2000);
		}
		public static void KeplrUnlock(this Instance instance,IZennoPosterProjectModel project)
		{
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "tagname", "input", "regexp", 0),SAFU.HWPass(project));
			instance.KeplrClick(instance.ActiveTab.FindElementByAttribute("button", "innertext", "Unlock", "regexp", 0));
			if (!instance.ActiveTab.FindElementByAttribute("div", "innertext", "Invalid\\ password", "regexp", 0).IsVoid) 
			{
				instance.CloseAllTabs(); instance.UninstallExtension("dmkamcknogkgcdfhhbddcghachkejeap"); 
				Loggers.W3Throw(project,$"!WrongPassword");
			}			
		}
		public static string KeplrPrune(this Instance instance,IZennoPosterProjectModel project)
		{
			instance.UseFullMouseEmulation = true;
			int i = 0;
			var imported = "";
			while (true)
			{
				var dotBtn = instance.ActiveTab.FindElementByAttribute("path", "d", "M10.5 6C10.5 5.17157 11.1716 4.5 12 4.5C12.8284 4.5 13.5 5.17157 13.5 6C13.5 6.82843 12.8284 7.5 12 7.5C11.1716 7.5 10.5 6.82843 10.5 6ZM10.5 12C10.5 11.1716 11.1716 10.5 12 10.5C12.8284 10.5 13.5 11.1716 13.5 12C13.5 12.8284 12.8284 13.5 12 13.5C11.1716 13.5 10.5 12.8284 10.5 12ZM10.5 18C10.5 17.1716 11.1716 16.5 12 16.5C12.8284 16.5 13.5 17.1716 13.5 18C13.5 18.8284 12.8284 19.5 12 19.5C11.1716 19.5 10.5 18.8284 10.5 18Z", "text", i);
				
				if (dotBtn.IsVoid) break;
				var tile = dotBtn.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement.ParentElement;
				project.SendInfoToLog(tile.InnerText);
				if (tile.InnerText.Contains("pkey") )
				{
					imported += "pkey"; 
					i++;
					continue;
				}
				if (tile.InnerText.Contains("seed")) 				
				{
					imported += "seed"; 
					i++;
					continue;
				}
				instance.KeplrClick(dotBtn);
				instance.KeplrClick(instance.LastHe(("div", "innertext", "Delete\\ Wallet", "regexp", 0)));
				instance.WaitSetValue(() => 	instance.ActiveTab.FindElementByName("password"), SAFU.HWPass(project));
				instance.KeplrClick(instance.ActiveTab.FindElementByAttribute("button", "type", "submit", "regexp", 0));
				i++;
			}
			return imported;
		}
		public static string KeplrMain(this Instance instance,IZennoPosterProjectModel project, string source, bool log = false)
		{
		//var source = "pkey"; //seed | pkey
			while (true)
			{
				var kState = instance.KeplrCheck();
				if (log) project.SendInfoToLog(kState);
				if (kState == "install") 
				{
					instance.KeplrInstallExt(project);
					continue;
				}
				if (kState == "import") 
				{
					instance.KeplrImportSeed(project);
					continue;
				}
				if (kState == "inputPass") 
				{
					instance.KeplrUnlock(project);
					continue;
				}
				if (kState == "setSourse") 
				{
					instance.KeplrSetSource(project,source);
					break;
				}
			}
			return $"Keplr set from {source}";
		}	
	}





	#endregion	
	#region Tools&Vars
	public static class OTP
	{
		public static string Offline(this Instance instance, string keyString, int waitIfTimeLess = 5)
		{
		    var key = OtpNet.Base32Encoding.ToBytes(keyString);
		    var otp = new OtpNet.Totp(key);
		    string code = otp.ComputeTotp();
		    int remainingSeconds = otp.RemainingSeconds();
		
		    if (remainingSeconds <= waitIfTimeLess)
		    {
		        Thread.Sleep(remainingSeconds * 1000 + 1);
		        code = otp.ComputeTotp();
		    }
		
		    return code;
		}
		public static string FirstMail(IZennoPosterProjectModel project,string email = "", string proxy = "")
		{
		    string encodedLogin = Uri.EscapeDataString(project.Variables["settingsFmailLogin"].Value);
		    string encodedPass = Uri.EscapeDataString(project.Variables["settingsFmailPass"].Value);
			if (email == "") email = project.Variables["googleLOGIN"].Value;
		    string url = $"https://api.firstmail.ltd/v1/mail/one?username={encodedLogin}&password={encodedPass}";
		
		    string[] headers = new string[]
		    {
		        $"accept: application/json",
		        "accept-encoding: gzip, deflate, br",
		        $"accept-language: {project.Profile.AcceptLanguage}",
		        "sec-ch-ua-mobile: ?0",
		        "sec-ch-ua-platform: \"Windows\"",
		        "sec-fetch-dest: document",
		        "sec-fetch-mode: navigate",
		        "sec-fetch-site: none",
		        "sec-fetch-user: ?1",
		        "upgrade-insecure-requests: 1",
		        $"user-agent: {project.Profile.UserAgent}",
		        $"X-API-KEY: {project.Variables["settingsApiFirstMail"].Value}"
		    };
		
		    string result = ZennoPoster.HttpGet(url, proxy, "UTF-8", ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly, 5000, "", project.Profile.UserAgent, true, 5, headers, "", false);
			Loggers.W3Debug(project,result);
		    project.Json.FromString(result);
		
		    string deliveredTo = project.Json.to[0];
		    string text = project.Json.text;
			string html = project.Json.html;
			string otp = "";
					
		    if (!deliveredTo.Contains(email)) throw new Exception($"Fmail: Email {email} not found in last message");
		    else
		    {
		        Match match = Regex.Match(text, @"\b\d{6}\b");	
		        if (match.Success) return  match.Value;
				match = Regex.Match(html, @"\b\d{6}\b");	
		        if (match.Success) return  match.Value;
		        else throw new Exception("Fmail: OTP not found in message with correct email");
		    }
		}
		public static string Telegram(IZennoPosterProjectModel project, string email = "", string proxy = "")
		{
		    if (email == "") email = project.Variables["login"].Value;
		    string url = $"https://api.telegram.org/bot{project.Variables["settingsTgLogToken"].Value}/getUpdates?chat_id={project.Variables["settingsTgMailGroup"].Value}&limit=1&offset=-1";
		    
		    string result = Http.W3Get(project, url, proxy);
		    string text = project.ExecuteMacro(result);
		    
		    var messageIdGroups = ZennoLab.Macros.TextProcessing.Regex(text, @"(?<=\{""message_id"":).*?(?=,"")", "0");
		    if (messageIdGroups.Count == 0) throw new Exception("Message ID not found");
		    
		    var emailGroups = ZennoLab.Macros.TextProcessing.Regex(text, email, "0");
		    if (emailGroups.Count == 0) throw new Exception($"gmailBot: Email {email} not found in last message");
		    
		    var otpGroups = ZennoLab.Macros.TextProcessing.Regex(text, @"(?<!\d)\d{6}(?!\d)", "0");
		    if (otpGroups.Count == 0) throw new Exception("gmailBot: OTP not found in message with correct email");
		    
		    string deleteUrl = $"https://api.telegram.org/bot{project.Variables["settingsTgLogToken"].Value}/deleteMessage?chat_id={project.Variables["settingsTgMailGroup"].Value}&message_id={messageIdGroups[0].FirstOrDefault()}";
		    Http.W3Get(project, deleteUrl, proxy);
		    
		    return otpGroups[0].FirstOrDefault();
		}

	}
	public static class Tools
	{
		private static readonly object LockObject = new object();
		public static bool SetGlobalVar(IZennoPosterProjectModel project, bool log = false)
		{
			lock (LockObject) 
			{
				try
				{
					var nameSpase = "w3tools";
					var cleaned = new List<int>();
					var notDeclared = new List<int>();
					var busyAccounts = new List<int>();
					for (int i = int.Parse(project.Variables["cfgRangeStart"].Value); i <= int.Parse(project.Variables["cfgRangeEnd"].Value); i++)
					{
						string threadKey = $"Thread{i}";
						try
						{
							var globalVar = project.GlobalVariables[nameSpase, threadKey];
							if (globalVar != null)
							{
								if (!string.IsNullOrEmpty(globalVar.Value)) busyAccounts.Add(i);
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
						Loggers.W3Log(project, $"!W cleanGlobal is [on] Cleaned: {string.Join(",", cleaned)}");
					}
					else
					{
						project.Variables["busyAccounts"].Value = string.Join(",", busyAccounts);
						Loggers.W3Log(project, $"!Buzy now: {string.Join(",", busyAccounts)}");
					}
					int currentThread = int.Parse(project.Variables["acc0"].Value);
					string currentThreadKey = $"Thread{currentThread}";
					if (!busyAccounts.Contains(currentThread))
					{
						try
						{
							project.GlobalVariables.SetVariable("w3tools", currentThreadKey, project.Variables["projectName"].Value);
						}
						catch
						{
							project.GlobalVariables["w3tools", currentThreadKey].Value = project.Variables["projectName"].Value;
						}
						if (log) Loggers.W3Log(project, $"Thread {currentThread} bound to {project.Variables["projectName"].Value}");
						return true;
					}
					else
					{
						if (log) Loggers.W3Log(project, $"Thread {currentThread} is already busy!");
						return false;
					}
				}
				catch (Exception ex)
				{
					if (log) Loggers.W3Log(project, $"⚙  {ex.Message}");
					throw; // Пробрасываем исключение дальше для обработки вызывающим кодом
				}
			}
		}
		public static void IncreaseVar(IZennoPosterProjectModel project, string varName)
		{
			project.Variables[$"{varName}"].Value = (int.Parse(project.Variables[$"{varName}"].Value) + 1).ToString();
			return;
		}
		public static string MathVar(IZennoPosterProjectModel project, string varName, int input)
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
		public static string RandomFromSettings(IZennoPosterProjectModel project)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Random rnd = new Random();
			
			var min = decimal.Parse(project.Variables["amountBridgeMin"].Value);
			var max = decimal.Parse(project.Variables["amountBridgeMax"].Value);
			
			decimal value = min + (max - min) * (decimal)rnd.Next(0, 1000000000) / 1000000000;
			return value.ToString("0.000000");
		}
		public static string RndAmount(string max, int percent)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Random rnd = new Random();
			double value = double.Parse(max);
			var amount = (value * 0.001337 * rnd.Next(1, percent)).ToString("0.000000");
			return amount;
		}
		public static T RandomAmount<T>(IZennoPosterProjectModel project)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Random rnd = new Random();
			
			var min = decimal.Parse(project.Variables["amountBridgeMin"].Value);
			var max = decimal.Parse(project.Variables["amountBridgeMax"].Value);
			
			decimal value = min + (max - min) * (decimal)rnd.Next(0, 1000000000) / 1000000000;
			
			if (typeof(T) == typeof(string))
		        return (T)Convert.ChangeType(value.ToString(), typeof(T));
		    
		    return (T)Convert.ChangeType(value, typeof(T));
			
		}
		public static string MultiplyEmail(string email, int count = 1)
		{
		    var results = new HashSet<string>();
		    var parts = email.Split('@');
		    var username = parts[0];
		    var random = new Random();
		    
		    int maxPossibleDots = username.Length - 2;
		    if (count > maxPossibleDots)
		    {
		        count = maxPossibleDots;
		    }
		    
		    while (results.Count < count)
		    {
		        var position = random.Next(1, username.Length - 1); 
		        var newEmail = username.Insert(position, ".") + "@" + parts[1];
		        results.Add(newEmail);
		    }
		    
		    return string.Join(",", results);
		}
	}
	public static class Browser
	{		
		private static readonly object LockObject = new object();
		private static HtmlElement TryGetDirectElement(Func<ZennoLab.CommandCenter.HtmlElement> elementFunc)
		{
			try
			{
				var element = elementFunc();
				if (element != null && !element.IsVoid)
				{
					var secondCall = elementFunc();
					if (ReferenceEquals(element, secondCall)) return element;
				}
			}
			catch (Exception)
			{}
			return null;
		}

		public static HtmlElement LastHe(this Instance instance, (string, string, string, string, int) obj)
		{
			string tag = obj.Item1;
			string attribute = obj.Item2;
			string pattern = obj.Item3;
			string mode = obj.Item4;
			int pos = obj.Item5;
			int index = 0;
			
			while (true)
			{
				HtmlElement he = instance.ActiveTab.FindElementByAttribute(tag, attribute, pattern, mode, index);
				if (he.IsVoid) 
				{
					he = instance.ActiveTab.FindElementByAttribute(tag, attribute, pattern, mode, index - 1);
					return he;
				}
				index++;
			}
		}

		public static void WaitClick(this Instance instance, Func<ZennoLab.CommandCenter.HtmlElement> elementSearch, int maxWaitSeconds = 10, int delay = 1, string comment = "",bool Throw = true)
		{
			DateTime functionStart = DateTime.Now;
			HtmlElement directElement = TryGetDirectElement(elementSearch);
			bool isDirectElement = directElement != null;

			while (true)
			{
				if ((DateTime.Now - functionStart).TotalSeconds > maxWaitSeconds)
					if (Throw) throw new TimeoutException($"{comment} not found in {maxWaitSeconds}s");
					else return;

				HtmlElement element;
				if (isDirectElement) element = directElement;
				else element = elementSearch();

				if (!element.IsVoid)
				{
					Thread.Sleep(delay * 1000);
					element.RiseEvent("click", instance.EmulationLevel);
					break;
				}

				Thread.Sleep(500);
			}
		}

		public static void ClickOut(this Instance instance, Func<ZennoLab.CommandCenter.HtmlElement> elementSearch, int maxWaitSeconds = 10, int disappearThresholdSeconds = 2, string comment = "")
		{
		    DateTime functionStart = DateTime.Now; 
		    DateTime lastSeenTime = DateTime.Now;
		    while (true)
		    {
		        if ((DateTime.Now - functionStart).TotalSeconds > maxWaitSeconds)
		            throw new TimeoutException($"{comment} did not disappear within {maxWaitSeconds}s");
		        var element = elementSearch();
		        if (!element.IsVoid) 
		        {
		            lastSeenTime = DateTime.Now; 
		            element.RiseEvent("click", instance.EmulationLevel); 
		        }
		        else 
		        {
		            if ((DateTime.Now - lastSeenTime).TotalSeconds > disappearThresholdSeconds)
		                break; 
		        }
		        Thread.Sleep(500); 
		    }
		}
		public static void RemoveElement(this Instance instance, Func<ZennoLab.CommandCenter.HtmlElement> elementSearch)
		{
			HtmlElement he = elementSearch();
			HtmlElement heParent = he.ParentElement;heParent.RemoveChild(he);
		}
		public static void WaitSetValue(this Instance instance, Func<ZennoLab.CommandCenter.HtmlElement> elementSearch, string value, int maxWaitSeconds = 10, int delay = 1, string comment = "",bool Throw = true)
		{
		    DateTime functionStart = DateTime.Now;
		    
		    while (true)
		    {
				if ((DateTime.Now - functionStart).TotalSeconds > maxWaitSeconds)
					if (Throw) throw new TimeoutException($"{comment} not found in {maxWaitSeconds}s");
					else return;
		            
		        var element = elementSearch();
		        
		        if (!element.IsVoid)
		        {
		            Thread.Sleep(delay * 1000);
		            instance.WaitFieldEmulationDelay();
		            element.SetValue(value, "Full", false);
		            break;
		        }
		        
		        Thread.Sleep(500);
		    }
		}
		public static string WaitGetValue(this Instance instance, Func<ZennoLab.CommandCenter.HtmlElement> elementSearch, int maxWaitSeconds = 10, string atr = "innertext", int delayBeforeGetSeconds = 1, string comment = "")
		{
		    DateTime functionStart = DateTime.Now;
		    
		    while (true)
		    {
		        if ((DateTime.Now - functionStart).TotalSeconds > maxWaitSeconds)
		            throw new TimeoutException($"{comment} not found in {maxWaitSeconds}s");
		            
		        var element = elementSearch();
		        
		        if (!element.IsVoid)
		        {
		            Thread.Sleep(delayBeforeGetSeconds * 1000);
		            return element.GetAttribute(atr);
		        }
		        
		        Thread.Sleep(500);
		    }
		}
		public static void CFv2(this Instance instance)
		{
			Random rnd = new Random(); string strX = ""; string strY = ""; Thread.Sleep(3000);
			HtmlElement he1 = instance.ActiveTab.FindElementById("cf-turnstile");
			HtmlElement he2 = instance.ActiveTab.FindElementByAttribute("div", "outerhtml", "<div><input type=\"hidden\" name=\"cf-turnstile-response\"", "regexp", 4);
			if (he1.IsVoid && he2.IsVoid) return;
			else if (!he1.IsVoid)
			{
				strX = he1.GetAttribute("leftInbrowser");	strY = he1.GetAttribute("topInbrowser");
			}
			else if (!he2.IsVoid)
			{
				strX = he2.GetAttribute("leftInbrowser");	strY = he2.GetAttribute("topInbrowser");
			}

			int rndX = rnd.Next(23, 26); int x = (int.Parse(strX) + rndX);
			int rndY = rnd.Next(27, 31); int y = (int.Parse(strY) + rndY);
			Thread.Sleep(rnd.Next(4, 5) * 1000); 
			instance.WaitFieldEmulationDelay();
			instance.Click(x, x, y, y, "Left", "Normal");
			Thread.Sleep(rnd.Next(3, 4) * 1000);
			
		}
		public static void CloseExtraTabs(this Instance instance)
		{
			for (; ; ){try{instance.AllTabs[1].Close();Thread.Sleep(1000);}catch{break;}}Thread.Sleep(1000);
		}	
		public static List<(HtmlElement Element, string TestId, string TestIdValue)> parseTestId(Instance instance, IZennoPosterProjectModel project, HtmlElement parent, bool log = false)
		{
			var wall = parent.GetChildren(true).ToList();
			project.SendInfoToLog($"Total: {wall.Count}");

			var allElements = new List<(HtmlElement Element, string TestId, string TestIdValue)>();

			foreach (HtmlElement he in wall)
			{
				var testId = he.GetAttribute("data-testid");
				if (testId != "") 
				{
					var heText = Regex.Replace(he.InnerText, "\n", "|");
					allElements.Add((he, testId, heText));
				}
			}

			if (log)
			{
				string logOutput = string.Join("\n", allElements.Select(x => $"{x.TestId} [{x.TestIdValue}]"));
				project.SendInfoToLog(logOutput);
			}

			return allElements;
		}	
		public static void CtrlV(this Instance instance, string ToPaste)
		{
			lock(LockObject) {System.Windows.Forms.Clipboard.SetText(ToPaste);instance.ActiveTab.KeyEvent("v","press","ctrl");}
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
		    catch (Exception){return "qrError";}
		}
	}
	#endregion
	public static class StyleConverter
	{
	    public static string ConvertStyle(string text, string inputStyle, string outputStyle)
	    {
	        string intermediate;
	        switch (inputStyle)
	        {
	            case "PascalCase":
	                intermediate = ToSnakeCase(text);
	                break;
	            case "UPPER_CASE":
	                intermediate = text.ToLower();
	                break;
	            case "camelCase":
	                intermediate = ToSnakeCase(text);
	                break;
	            case "kebab-case":
	                intermediate = text.Replace('-', '_');
	                break;
	            case "Title Case":
	                intermediate = text.Replace(' ', '_').ToLower();
	                break;
	            default:
	                intermediate = text;
	                break;
	        }
	        switch (outputStyle)
	        {
	            case "PascalCase":
	                return ToPascalCase(intermediate);
	            case "UPPER_CASE":
	                return intermediate.ToUpper();
	            case "camelCase":
	                return ToCamelCase(intermediate);
	            case "snake_case":
	                return intermediate;
	            case "kebab-case":
	                return intermediate.Replace('_', '-');
	            case "Title Case":
	                return string.Join(" ", intermediate.Split('_')
	                    .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
	            default:
	                throw new ArgumentException($"Неподдерживаемый стиль: {outputStyle}");
	        }
	    }
	
	    private static string ToSnakeCase(string text)
	    {
	        return Regex.Replace(text, "(?<!^)(?=[A-Z])", "_").ToLower();
	    }
	
	    private static string ToPascalCase(string text)
	    {
	        return string.Join("", text.Split('_')
	            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
	    }
	
	    private static string ToCamelCase(string text)
	    {
	        string pascal = ToPascalCase(text);
	        return char.ToLower(pascal[0]) + pascal.Substring(1);
	    }
	}
	
	#region AES
	public static class Crypto
    {
        #region AES
        public static string EncryptAES(string phrase, string key, bool hashKey = true)
        {
            if (phrase == null || key == null)
                return null;

            var keyArray = HexStringToByteArray(hashKey ? HashMD5(key) : key);
            var toEncryptArray = Encoding.UTF8.GetBytes(phrase);
            byte[] result;

            using (var aes = new AesCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                var cTransform = aes.CreateEncryptor();
                result = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                aes.Clear();
            }
            return ByteArrayToHexString(result);
        }

        public static string DecryptAES(string hash, string key, bool hashKey = true)
        {
            if (hash == null || key == null)
                return null;

            var keyArray = HexStringToByteArray(hashKey ? HashMD5(key) : key);
            var toEncryptArray = HexStringToByteArray(hash);

            var aes = new AesCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = aes.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            aes.Clear();
            return Encoding.UTF8.GetString(resultArray);
        }
        #endregion

        #region Helpers
        internal static string ByteArrayToHexString(byte[] inputArray)
        {
            if (inputArray == null)
                return null;
            var o = new StringBuilder("");
            for (var i = 0; i < inputArray.Length; i++)
                o.Append(inputArray[i].ToString("X2"));
            return o.ToString();
        }

        internal static byte[] HexStringToByteArray(string inputString)
        {
            if (inputString == null)
                return null;

            if (inputString.Length == 0)
                return new byte[0];

            if (inputString.Length%2 != 0)
                throw new Exception("Hex strings have an even number of characters and you have got an odd number of characters!");

            var num = inputString.Length/2;
            var bytes = new byte[num];
            for (var i = 0; i < num; i++)
            {
                var x = inputString.Substring(i*2, 2);
                try
                {
                    bytes[i] = Convert.ToByte(x, 16);
                }
                catch (Exception ex)
                {
                    throw new Exception("Part of your \"hex\" string contains a non-hex value.", ex);
                }
            }
            return bytes;
        }
        #endregion

        #region MD5
        public static string HashMD5(string phrase)
        {
            if (phrase == null)
                return null;
            var encoder = new UTF8Encoding();
            var md5Hasher = new MD5CryptoServiceProvider();
            var hashedDataBytes = md5Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToHexString(hashedDataBytes);
        }
        #endregion
    }
	#endregion

	#region Nethereum
	public class Blockchain
    {
        public static object SyncObject = new object();
        
        public string walletKey;
        public int chainId;
        public string jsonRpc;
        
        public Blockchain(string walletKey, int chainId, string jsonRpc)
        {
            this.walletKey = walletKey;
            this.chainId = chainId;
            this.jsonRpc = jsonRpc;
        }
        
        public Blockchain(string jsonRpc) : this("", 0, jsonRpc)
        {}
        
        public Blockchain() {}
		
		public string GetAddressFromPrivateKey(string privateKey)
        {
            if (!privateKey.StartsWith("0x")) privateKey = "0x" + privateKey;
            var account = new Account(privateKey);
            return account.Address;
        }
		
	
		public async Task<string> GetBalance()
        {
            var account = new Account(walletKey, chainId);
            var web3 = new Web3(account, jsonRpc);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            var bnbAmount = Web3.Convert.FromWei(balance.Value);
            return bnbAmount.ToString();
        }
		
		public async Task<string> ReadContract(string contractAddress, string functionName, string abi, params object[] parameters)
		{
			var web3 = new Web3(jsonRpc);
			web3.TransactionManager.UseLegacyAsDefault = true;
			var contract = web3.Eth.GetContract(abi, contractAddress);
			var function = contract.GetFunction(functionName);
			var result = await function.CallAsync<object>(parameters);
			
			// Добавляем проверку на тип результата для структуры
			if (result is Tuple<BigInteger, BigInteger, BigInteger, BigInteger> structResult)
			{
				return $"0x{structResult.Item1.ToString("X")},{structResult.Item2.ToString("X")},{structResult.Item3.ToString("X")},{structResult.Item4.ToString("X")}";
			}
			
			// Оставляем существующие проверки
			if (result is BigInteger bigIntResult) return "0x" + bigIntResult.ToString("X");
			else if (result is bool boolResult) return boolResult.ToString().ToLower();
			else if (result is string stringResult) return stringResult;
			else if (result is byte[] byteArrayResult) return "0x" + BitConverter.ToString(byteArrayResult).Replace("-", "");
			else return result?.ToString() ?? "null";
		}

        
        public async Task<string> SendTransaction(string addressTo, decimal amount, string data, BigInteger gasLimit, BigInteger gasPrice)
        {
            var account = new Account(walletKey, chainId);
            var web3 = new Web3(account, jsonRpc);
            web3.TransactionManager.UseLegacyAsDefault = true;
            var transaction = new TransactionInput();
            transaction.From = account.Address;
            transaction.To = addressTo;
            transaction.Value = Web3.Convert.ToWei(amount).ToHexBigInteger();
            transaction.Data = data;
            transaction.Gas = new HexBigInteger(gasLimit);
            transaction.GasPrice = new HexBigInteger(gasPrice);
            var hash = await web3.TransactionManager.SendTransactionAsync(transaction);
            return hash;
        }
		
		public async Task<string> SendTransactionEIP1559(string addressTo, decimal amount, string data, BigInteger gasLimit, BigInteger maxFeePerGas, BigInteger maxPriorityFeePerGas)
		{
		    var account = new Account(walletKey, chainId);
		    var web3 = new Web3(account, jsonRpc);
		    var transaction = new TransactionInput
		    {
		        From = account.Address,
		        To = addressTo,
		        Value = Web3.Convert.ToWei(amount).ToHexBigInteger(),
		        Data = data,
		        Gas = new HexBigInteger(gasLimit),
		        MaxFeePerGas = new HexBigInteger(maxFeePerGas),
		        MaxPriorityFeePerGas = new HexBigInteger(maxPriorityFeePerGas),
		        Type = new HexBigInteger(2) // EIP-1559 транзакция
		    };
		    
		    var hash = await web3.TransactionManager.SendTransactionAsync(transaction);
		    return hash;
		}

		
		//btc
		public static string GenerateMnemonic(string wordList = "English", int wordCount = 12)
		{
			Wordlist _wordList;
			WordCount _wordCount;
		
			switch (wordList)
			{
				case "English":
					_wordList = Wordlist.English;
					break;
		
				case "Japanese":
					_wordList = Wordlist.Japanese;
					break;
		
				case "Chinese Simplified":
					_wordList = Wordlist.ChineseSimplified;
					break;
		
				case "Chinese Traditional":
					_wordList = Wordlist.ChineseTraditional;
					break;
		
				case "Spanish":
					_wordList = Wordlist.Spanish;
					break;
		
				case "French":
					_wordList = Wordlist.French;
					break;
		
				case "Portuguese":
					_wordList = Wordlist.PortugueseBrazil;
					break;
		
				case "Czech":
					_wordList = Wordlist.Czech;
					break;
		
				default:
					_wordList = Wordlist.English;
					break;
			}
			
			switch (wordCount)
			{
				case 12:
					_wordCount = WordCount.Twelve;
					break;
		
				case 15:
					_wordCount = WordCount.Fifteen;
					break;
		
				case 18:
					_wordCount = WordCount.Eighteen;
					break;
		
				case 21:
					_wordCount = WordCount.TwentyOne;
					break;
		
				case 24:
					_wordCount = WordCount.TwentyFour;
					break;
		
				default:
					_wordCount = WordCount.Twelve;
					break;
			}
		
			Mnemonic mnemo = new Mnemonic(_wordList, _wordCount);
		
			return mnemo.ToString();
		}
		
		public static Dictionary<string, string> MnemonicToAccountEth(string words, int amount)
        {
            string password = "";
            var accounts = new Dictionary<string, string>();

            var wallet = new Wallet(words, password);

            for (int i = 0; i < amount; i++)
            {
                var recoveredAccount = wallet.GetAccount(i);

                accounts.Add(recoveredAccount.Address, recoveredAccount.PrivateKey);
            }

            return accounts;
        }
		
		public static Dictionary<string, string> MnemonicToAccountBtc(string mnemonic, int amount, string walletType = "Bech32")
        {
			Func<string, string> GenerateAddress = null;

            switch (walletType)
            {
                case "P2PKH compress":
                    GenerateAddress = PrivateKeyToP2WPKHCompress;
                    break;

                case "P2PKH uncompress":
                    GenerateAddress = PrivateKeyToP2PKHUncompress;
                    break;

                case "P2SH":
                    GenerateAddress = PrivateKeyToP2SH;
                    break;

                case "Bech32":
                    GenerateAddress = PrivateKeyToBech32;
                    break;

                default:
					GenerateAddress = PrivateKeyToBech32;
                    break;
            }

            var mnemo = new Mnemonic(mnemonic);
            var hdroot = mnemo.DeriveExtKey();
			string derive = "m/84'/0'/0'/0"; // m / purpose' / coin_type' / account' / change / address_index
			
			var addresses = new Dictionary<string,string>();
			
			for (int i = 0; i < amount; i++)
            {
                string keyPath = derive + "/" + i;
                var pKey = hdroot.Derive(new NBitcoin.KeyPath(keyPath));
                string privateKey = pKey.PrivateKey.ToHex();

                string address = GenerateAddress?.Invoke(privateKey);

                addresses.Add(address, privateKey);
            }
			
            return addresses;
        }
		
		private static string PrivateKeyToBech32(string privateKey)
        {
            var privKey = KeyConverter(privateKey, true);
            var wifCompressed = new BitcoinSecret(privKey, Network.Main);

            var bech32 = wifCompressed.PubKey.GetAddress(ScriptPubKeyType.Segwit, Network.Main);

            return bech32.ToString();
        }

        private static string PrivateKeyToP2WPKHCompress(string privateKey)
        {
            var privKey = KeyConverter(privateKey, true);
            var wifCompressed = new BitcoinSecret(privKey, Network.Main);

            var P2PKHCompressed = wifCompressed.PubKey.GetAddress(ScriptPubKeyType.Segwit, Network.Main);

            return P2PKHCompressed.ToString();
        }

        private static string PrivateKeyToP2PKHUncompress(string privateKey)
        {
            var privKey = KeyConverter(privateKey, false);
            var wifUncompressed = new BitcoinSecret(privKey, Network.Main);

            var P2PKHUncompressed = wifUncompressed.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);

            return P2PKHUncompressed.ToString();
        }

        private static string PrivateKeyToP2SH(string privateKey)
        {
            var privKey = KeyConverter(privateKey, true);
            var wifUncompressed = new BitcoinSecret(privKey, Network.Main);

            var P2PKHUncompressed = wifUncompressed.PubKey.GetAddress(ScriptPubKeyType.SegwitP2SH, Network.Main);

            return P2PKHUncompressed.ToString();
        }
		
		private static Key KeyConverter(string privateKey, bool compress)
        {
            Key key;
            var byteKey = privateKey.HexToByteArray();

            if (compress)
            {
                key = new Key(byteKey);
            }

            else
            {
                key = new Key(byteKey, -1, false);
            }

            return key;
        }
		
		public static string GetEthAccountBalance(string address, string jsonRpc)
        {
            var web3 = new Web3(jsonRpc);

            var balance = web3.Eth.GetBalance.SendRequestAsync(address).Result;
			return balance.Value.ToString();
        }
		
		public static string GetBtcAccountBalance(string address)
        {
            var network = Network.Main;
            var client = new QBitNinjaClient(network);
            var dest = BitcoinAddress.Create(address, network);
            var balance = client.GetBalance(dest, true).Result;

            long money = new Money(0);

            if (balance.Operations.Count != 0)
            {
                foreach (var operations in balance.Operations)
                {
                    money += operations.Amount;
                }
            }
            
            return money.ToString();
        }
		
		
		
		
    }
	public class Function
	{
		public static string[] GetFuncInputTypes(string abi, string functionName)
        {
            var deserialize = new ABIJsonDeserialiser();
            var abiFunctions = deserialize.DeserialiseContract(abi).Functions;
            int paramsAmount = abiFunctions.Where(n => n.Name == functionName).SelectMany(p => p.InputParameters, (n, p) => new { Type = p.Type }).Count();
            var inputTypes = abiFunctions.Where(n => n.Name == functionName).SelectMany(p => p.InputParameters, (n, p) => new { Type = p.Type });
            string[] types = new string[paramsAmount];
            var typesList = new List<string>();
            foreach (var item in inputTypes) typesList.Add(item.Type);
            types = typesList.ToArray();
            return types;
        }
		
		public static Dictionary<string, string> GetFuncInputParameters(string abi, string functionName)
        {
            var deserialize = new ABIJsonDeserialiser();
            var abiFunctions = deserialize.DeserialiseContract(abi).Functions;
            var parameters = abiFunctions.Where(n => n.Name == functionName).SelectMany(p => p.InputParameters, (n, p) => new { Name = p.Name, Type = p.Type });
            return parameters.ToDictionary(p => p.Name, p => p.Type);
        }

        public static Dictionary<string, string> GetFuncOutputParameters(string abi, string functionName)
        {
            var deserialize = new ABIJsonDeserialiser();
            var abiFunctions = deserialize.DeserialiseContract(abi).Functions;
            var parameters = abiFunctions.Where(n => n.Name == functionName).SelectMany(p => p.OutputParameters, (n, p) => new { Name = p.Name, Type = p.Type });
            return parameters.ToDictionary(p => p.Name, p => p.Type);
        }
		
		public static string GetFuncAddress(string abi, string functionName)
		{
			var deserialize = new ABIJsonDeserialiser();
            var abiFunctions = deserialize.DeserialiseContract(abi).Functions;
            var address = abiFunctions.Where(n => n.Name == functionName).Select(f => f.Sha3Signature).First();
            return address;
		}

	}
	public class Decoder
	{
		public static Dictionary<string, string> AbiDataDecode(string abi, string functionName, string data)
		{
		    var decodedData = new Dictionary<string, string>();
		    if (data.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) data = data.Substring(2);
		    if (data.Length < 64) data = data.PadLeft(64, '0'); // Если данные короче 64 символов, дополняем их нулями слева
		    List<string> dataChunks = SplitChunks(data).ToList();
		    Dictionary<string, string> parametersList = Function.GetFuncOutputParameters(abi, functionName);	
		    for (int i = 0; i < parametersList.Count && i < dataChunks.Count; i++)
		    {
		        string key = parametersList.Keys.ElementAt(i);
		        string type = parametersList.Values.ElementAt(i);
		        string value = TypeDecode(type, dataChunks[i]);
		        decodedData.Add(key, value);
		    }	
		    return decodedData;
		}
		
		private static IEnumerable<string> SplitChunks(string data)
		{
		    int chunkSize = 64;
		    for (int i = 0; i < data.Length; i += chunkSize) yield return i + chunkSize <= data.Length ? data.Substring(i, chunkSize) : data.Substring(i).PadRight(chunkSize, '0');
		}
		
		private static string TypeDecode(string type, string dataChunk)
        {
            string decoded = string.Empty;

            var decoderAddr = new AddressTypeDecoder();
            var decoderBool = new BoolTypeDecoder();
            var decoderInt = new IntTypeDecoder();

            switch (type)
            {
                case "address": decoded = decoderAddr.Decode<string>(dataChunk);
                    break;
                case "uint256": decoded = decoderInt.DecodeBigInteger(dataChunk).ToString();
                    break;
                case "uint8": decoded = decoderInt.Decode<int>(dataChunk).ToString();
                    break;
                case "bool": decoded = decoderBool.Decode<bool>(dataChunk).ToString();
                    break;
                default: break;
            }
            return decoded;
        }
	}
	public class Encoder
	{
		public static string EncodeTransactionData(string abi, string functionName, string[] types, object[] values)
        {
            string funcAddress = Function.GetFuncAddress(abi, functionName);
            string encodedParams = EncodeParams(types, values);
            string encodedData = "0x" + funcAddress + encodedParams;
            return encodedData;
        }
		
		public static string EncodeParam(string type, object value)
        {
            var abiEncode = new ABIEncode();
            string result = abiEncode.GetABIEncoded(new ABIValue(type, value)).ToHex();
            return result;
        }

		public static string EncodeParams(string[] types, object[] values)
		{
		    var abiEncode = new ABIEncode();
		    var parameters = new ABIValue[types.Length];
		    for (int i = 0; i < types.Length; i++)
		    {
		        parameters[i] = new ABIValue(types[i], values[i]);
		    }
		    return abiEncode.GetABIEncoded(parameters).ToHex();
		}

        public static string EncodeParams(Dictionary<string, string> parameters)
        {
            var abiEncode = new ABIEncode();
            string result = string.Empty;
            foreach (var item in parameters) result += abiEncode.GetABIEncoded(new ABIValue(item.Value, item.Key)).ToHex();
            return result;
        }
	}
	public class Converter
	{
		public static object[] ValuesToArray(params dynamic[] inputValues)
        {
            int valuesAmount = inputValues.Length;
            var valuesList = new List<object>();
            foreach (var item in inputValues) valuesList.Add(item);
            object[] values = new object[valuesAmount];
            values = valuesList.ToArray();
            return values;
        }
	}

	#endregion

	#region hana/GRAPSQL example
	public static class HanaGarden
	{
		private static readonly string GRAPHQL_URL = "https://hanafuda-backend-app-520478841386.us-central1.run.app/graphql";
		private static readonly string API_KEY = "AIzaSyDipzN0VRfTPnMGhQ5PSzO27Cxm3DohJGY";

		private static string ExecuteGraphQLQuery(IZennoPosterProjectModel project, string query, string variables = null)
		{
			// Получаем токен и проверяем его
			string token = project.Variables["TOKEN_CURRENT"].Value.Trim();
			
			if (string.IsNullOrEmpty(token))
			{
				project.SendErrorToLog("Token is empty or null");
				return null;
			}

			// Форматируем заголовки, убедившись что токен передается корректно
			string[] headers = new string[] {
				"Content-Type: application/json",
				$"Authorization: Bearer {token.Trim()}"
			};

			// Форматируем GraphQL запрос, удаляя лишние пробелы и табуляции
			query = query.Replace("\t", "").Replace("\n", " ").Replace("\r", "").Trim();
			
			//string jsonBody = JsonConvert.SerializeObject(new { query = query });
			string jsonBody;
			if (variables != null)
			{
				jsonBody = JsonConvert.SerializeObject(new { query = query, variables = JsonConvert.DeserializeObject(variables) });
			}
			else
			{
				jsonBody = JsonConvert.SerializeObject(new { query = query });
			}


			
			Loggers.W3Debug(project,$"Request headers: {string.Join(", ", headers)}");
			Loggers.W3Debug(project,$"Request body: {jsonBody}");

			try 
			{
				string response = ZennoPoster.HttpPost(
					GRAPHQL_URL,
					Encoding.UTF8.GetBytes(jsonBody),
					"application/json",
					project.Variables["proxy"].Value,
					"UTF-8",
					ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
					30000,
					"",
					"HANA/v1",
					true,
					5,
					headers,
					"",
					true
				);

				Loggers.W3Debug(project,$"Response received: {response}");
				return response;
			}
			catch (Exception ex)
			{
				project.SendErrorToLog($"GraphQL request failed: {ex.Message}");
				return null;
			}
		}
		public static string RefreshToken(IZennoPosterProjectModel project, string currentToken)
		{
			string url = $"https://securetoken.googleapis.com/v1/token?key={API_KEY}";
			
			string jsonBody = JsonConvert.SerializeObject(new
			{
				grant_type = "refresh_token",
				refresh_token = currentToken
			});
		
			Loggers.W3Debug(project,$"Refreshing token. Request body: {jsonBody}");
		
			string[] headers = new string[] {
				"Content-Type: application/json"
			};
		
			try
			{
				string response = ZennoPoster.HttpPost(
					url,
					Encoding.UTF8.GetBytes(jsonBody),
					"application/json",
					project.Variables["proxy"].Value,
					"UTF-8",
					ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
					30000,
					"",
					"Firebase/v1",
					true,
					5,
					headers,
					"",
					true
				);
		
				Loggers.W3Debug(project,$"Refresh token response: {response}");
				
				dynamic tokenData = JObject.Parse(response);
				string newToken = tokenData.access_token;
				
				// Сохраняем новый токен в переменную проекта
				project.Variables["TOKEN_CURRENT"].Value = newToken;
				
				return newToken;
			}
			catch (Exception ex)
			{
				project.SendErrorToLog($"Failed to refresh token: {ex.Message}");
				return null;
			}
		}




		private static dynamic GetUserInfo(IZennoPosterProjectModel project)
		{
			string query = @"
			query CurrentUser {
				currentUser {
					id
					sub
					name
					totalPoint
					evmAddress {
						userId
						address
					}
				}
			}";

			string response = ExecuteGraphQLQuery(project, query);
			return JObject.Parse(response);
				}// Получение информации о картах пользователя
		public static string GetUserYakuInfo(IZennoPosterProjectModel project)
		{
			string query = @"
			query GetYakuList {
				getYakuListForCurrentUser {
					cardId
					group
				}
			}";
			
			return ExecuteGraphQLQuery(project, query);
		}
		public static string GetUserYakuInfo2(IZennoPosterProjectModel project)
		{
			string query = @"
			query GetMasterData {
				masterData {
					yaku {
					cardId
					group
					}
				}
			}";
						
			return ExecuteGraphQLQuery(project, query);
		}

		// Получение информации о саде
		public static string GetGardenInfo(IZennoPosterProjectModel project)
		{
			project.SendInfoToLog("Getting garden info...");
			string query = @"
			query GetGardenForCurrentUser {
				getGardenForCurrentUser {
					id
					inviteCode
					gardenDepositCount
					gardenStatus {
						id
						activeEpoch
						growActionCount
						gardenRewardActionCount
					}
					gardenMembers {
						id
						sub
						name
						iconPath
						depositCount
					}
				}
			}";
			
			return ExecuteGraphQLQuery(project, query);
		}
		
		public static void ProcessGarden(IZennoPosterProjectModel project)
		{
				try
				{
					// Получаем и обновляем токен
					string currentToken = project.Variables["TOKEN_CURRENT"].Value;
					project.SendInfoToLog($"Initial token: {currentToken}");
			
					string refreshedToken = RefreshToken(project, currentToken);
					if (string.IsNullOrEmpty(refreshedToken))
					{
						project.SendErrorToLog("Failed to refresh token");
						return;
					}
			
				project.SendInfoToLog($"Successfully refreshed token: {refreshedToken}");
				
				// Получаем информацию о саде
				project.SendInfoToLog("Getting garden info...");
				string gardenResponse = ExecuteGraphQLQuery(project, @"
					query GetGardenForCurrentUser {
						getGardenForCurrentUser {
							id
							inviteCode
							gardenDepositCount
							gardenStatus {
								id
								activeEpoch
								growActionCount
								gardenRewardActionCount
							}
							gardenMembers {
								id
								sub
								name
								iconPath
								depositCount
							}
						}
					}");
		
				project.SendInfoToLog($"Garden response received: {gardenResponse.Substring(0, Math.Min(100, gardenResponse.Length))}...");
		
				if (string.IsNullOrEmpty(gardenResponse))
				{
					project.SendErrorToLog("Garden response is empty!");
					return;
				}
		
				dynamic gardenData = JObject.Parse(gardenResponse);
		
				if (gardenData.data == null || gardenData.data.getGardenForCurrentUser == null)
				{
					project.SendErrorToLog($"Invalid garden data structure: {gardenResponse}");
					return;
				}
		
				dynamic gardenStatus = gardenData.data.getGardenForCurrentUser.gardenStatus;
				dynamic gardenMembers = gardenData.data.getGardenForCurrentUser.gardenMembers;
		
				// Проверяем наличие необходимых данных
				if (gardenStatus == null)
				{
					project.SendErrorToLog("Garden status is null!");
					return;
				}
		
				int totalGrows = (int)gardenStatus.growActionCount;
				int totalRewards = (int)gardenStatus.gardenRewardActionCount;
		
				project.SendInfoToLog($"Found actions - Grows: {totalGrows}, Rewards: {totalRewards}");
		
				string accountName = "Unknown";
				string accountId = "Unknown";
		
				if (gardenMembers != null && gardenMembers.Count > 0)
				{
					accountName = gardenMembers[0].name;
					accountId = gardenMembers[0].id;
				}
		
				project.SendInfoToLog($"Processing account: {accountName} (ID: {accountId})");
		
			
				
				//grow
				string growQuery = @"
				mutation {
					executeGrowAction(withAll: true) {
						baseValue
						leveragedValue
						totalValue
						multiplyRate
						limit
					}
				}";
				
				project.SendInfoToLog($"Executing grow all action");
				string growResponse = ExecuteGraphQLQuery(project, growQuery);
				project.SendInfoToLog($"Grow response: {growResponse}");
				
				dynamic growData = JObject.Parse(growResponse);
				if (growData.data != null && growData.data.executeGrowAction != null)
				{
					var result = growData.data.executeGrowAction;
					project.SendInfoToLog($"Grow results: Base={result.baseValue}, " +
										$"Leveraged={result.leveragedValue}, " +
										$"Total={result.totalValue}, " +
										$"Rate={result.multiplyRate}, " +
										$"Limit={result.limit}");
				}

					
				// Получаем обновленные очки
				string userInfoResponse = ExecuteGraphQLQuery(project, @"
					query CurrentUser {
						currentUser {
							totalPoint
						}
					}");
				
				dynamic userInfo = JObject.Parse(userInfoResponse);
				int totalPoints = (int)userInfo.data.currentUser.totalPoint;
				
				project.SendInfoToLog($"Grow action completed. Current Total Points: {totalPoints}");
				
				int delay = new Random().Next(1000, 5000);
				project.SendInfoToLog($"Waiting for {delay}ms before next action");
				Thread.Sleep(delay);			
				
				
				// Получение наград
				if (totalRewards > 0)
				{
					project.SendInfoToLog($"Starting reward collection. Total rewards: {totalRewards}");
					
					string rewardQuery = @"
					mutation executeGardenRewardAction($limit: Int!) {
						executeGardenRewardAction(limit: $limit) {
							data { cardId, group }
							isNew
						}
					}";
		
					int steps = (int)Math.Ceiling(totalRewards / 10.0);
					project.SendInfoToLog($"Will process rewards in {steps} steps");
		
					for (int i = 0; i < steps; i++)
					{
						try
						{
							project.SendInfoToLog($"Processing rewards step {i + 1} of {steps}");
							string variables = @"{""limit"": 10}";
							string rewardResponse = ExecuteGraphQLQuery(project, rewardQuery, variables);
							project.SendInfoToLog($"Reward response: {rewardResponse}");
		
							dynamic rewardData = JObject.Parse(rewardResponse);
		
							foreach (var reward in rewardData.data.executeGardenRewardAction)
							{
								if ((bool)reward.isNew)
								{
									project.SendInfoToLog($"New card received: ID {reward.data.cardId}, Group: {reward.data.group}");
								}
							}
		
							delay = new Random().Next(1000, 5000);
							project.SendInfoToLog($"Waiting for {delay}ms before next reward collection");
							Thread.Sleep(delay);
						}
						catch (Exception ex)
						{
							project.SendErrorToLog($"Error during reward collection: {ex.Message}\nStack trace: {ex.StackTrace}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				project.SendErrorToLog($"Major error in garden processing: {ex.Message}\nStack trace: {ex.StackTrace}");
			}
		}
		

		// Выполнение всех доступных действий роста
		public static string ExecuteGrowAll(IZennoPosterProjectModel project)
		{
			string query = @"
			mutation {
				executeGrowAction(withAll: true) {
					baseValue
					leveragedValue
					totalValue
					multiplyRate
					limit
				}
			}";
			
			return ExecuteGraphQLQuery(project, query);
		}
		
		// Получение текущих очков пользователя
		public static string GetUserPoints(IZennoPosterProjectModel project)
		{
			string query = @"
			query CurrentUser {
				currentUser {
					totalPoint
				}
			}";
			
			return ExecuteGraphQLQuery(project, query);
		}
		
		// Получение наград с указанным лимитом
		public static string CollectRewards(IZennoPosterProjectModel project, int limit)
		{
			string query = @"
			mutation executeGardenRewardAction($limit: Int!) {
				executeGardenRewardAction(limit: $limit) {
					data { 
						cardId
						group 
					}
					isNew
				}
			}";
			
			string variables = $"{{\"limit\": {limit}}}";
			return ExecuteGraphQLQuery(project, query, variables);
		}
		
		


		
		
		

	}
	public static class HanaAPI
	{
		private static readonly string GRAPHQL_URL = "https://hanafuda-backend-app-520478841386.us-central1.run.app/graphql";
		
		public static string GetSchemaInfo(IZennoPosterProjectModel project)
		{
			string introspectionQuery = @"
			query {
				__schema {
					types {
						name
						fields {
							name
							type {
								name
								kind
							}
						}
					}
					mutationType {
						fields {
							name
							type {
								name
							}
							args {
								name
								type {
									name
								}
							}
						}
					}
				}
			}";
	
			string[] headers = new string[] {
				"Content-Type: application/json",
				$"Authorization: Bearer {project.Variables["TOKEN_CURRENT"].Value}"
			};
	
			string jsonBody = JsonConvert.SerializeObject(new { query = introspectionQuery });
	
			return ZennoPoster.HttpPost(
				GRAPHQL_URL,
				Encoding.UTF8.GetBytes(jsonBody),
				"application/json",
				"",
				"UTF-8",
				ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
				30000,
				"",
				"HANA/v1",
				true,
				5,
				headers,
				"",
				true
			);
		}
	}
	#endregion
}
