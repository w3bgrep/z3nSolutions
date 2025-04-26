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
using ZennoLab.InterfacesLibrary.Enums.Http;
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ZXing;
using ZXing.QrCode;
using Npgsql;
using Leaf.xNet;
using W3t00ls;

#endregion

namespace w3tools //by @w3bgrep
{

    public static class Migrate
	{
		//ЭТОТ КЛАСС СОДЕРЖИТ УСТАРЕВШИЕ ВЫЗОВЫ
		//  ЕСЛИ ТЫ ИСПОЛЬЗУЕШЬ КАКИЕ-ТО ИЗ НИХ - 
		// замени их теми к которым они обращаются как можно скорее, 
		// в следующих версиях эти вызовы будут удалены
        public static void W3Throw(IZennoPosterProjectModel project, string log)
        {
            Loggers.l0g(project, log, thr0w:true);
            //return;
        }
        public static void W3Log(IZennoPosterProjectModel project, string toLog = "", string varName = "a0debug", [CallerMemberName] string callerName = "")
        {
            Loggers.l0g(project, toLog, varName);
        }
		public static void w3Log(IZennoPosterProjectModel project, string toLog = "", string varName = "a0debug") 
	    {
	        Loggers.l0g(project, toLog, varName);
	    }
		public static string w3Query(IZennoPosterProjectModel project, string dbQuery, bool toLog = false)
		{
			return SQL.W3Query(project, dbQuery, toLog);
		}
		public static string simpleGET(IZennoPosterProjectModel project, string url, string proxy = "")
		{
			return Http.W3Get(project, url, proxy);
		}
		public static string TelegramMailOTP(IZennoPosterProjectModel project, string email = "", string proxy = "")
		{
		    return OTP.Telegram(project,email,proxy);
		}

	}
    public class Traffic
  {
    public static object SyncObject = new object();
    public enum SearchType
    {
      Contains, Exact
    }
    public static Random r = new Random();

    private static TrafficItem SearchRequest(Instance instance, string url, string typeRequest, SearchType searchType, int number, int minLength)
    {
      //instance.UseTrafficMonitoring = true;
      List < TrafficItem > urls = new List < TrafficItem > ();
      var traffic = instance.ActiveTab.GetTraffic();
      foreach(var t in traffic)
      {
        if (searchType == SearchType.Contains)
        {
          if (t.Url.Contains(url) && t.Method.Contains(typeRequest))
          {
            if (t.Url.Length > minLength) urls.Add(t);
          }
        }
        else if (searchType == SearchType.Exact)
        {
          if (t.Url == url && t.Method.Contains(typeRequest))
          {
            if (t.Url.Length > minLength) urls.Add(t);
          }
        }
      }
      if (urls.Count == 0) throw new Exception("искомый урл не найден - " + url);
      else
      {
        try
        {
          return urls[number];
        } catch
        {
          throw new Exception($"нет урл по вашему номеру совпадения. всего было найдено {urls.Count} урл");
        }
      }
    }

    private static void RequestHeadersInGetPost(string RequestHeaders, out string Cookies, out string UserAgent, out string[] Headers)
    {
      List < string > headers = new List < string > ();
      List < string > cookies = new List < string > ();
      string cook = "";
      string peremenDlySikla = "";
      string ua = "";
      headers.AddRange(RequestHeaders.Split('\n')); 
      for (int i = 0; i < headers.Count; i++)
      {
        if (headers[i].Contains("Cookie"))
        {
          lock(SyncObjects.ListSyncer)
          {
            cook = headers[i].Replace("Cookie: ", ""); 
            headers.RemoveAt(i);
          }
          i--;
          cookies.AddRange(cook.Split(';')); 
        }
        else if (headers[i].Contains("User-Agent"))
        {
          lock(SyncObjects.ListSyncer)
          {
            ua = headers[i].Replace("User-Agent: ", "").Trim(' ', '\r', '\n'); 
            headers.RemoveAt(i);
          }
          i--;
        }
      }
      cookies.UdalenieDubleiAndPustyhStrokIzSpiska();
      cook = ""; //очищаем переменную кук
      foreach(string s in cookies)
      cook += s + "; ";
      if (cook != "")
      {
        string obrezka = cook.Substring(cook.Length - 2, 1);
        if (cook.Substring(cook.Length - 2, 1) == ";") cook = cook.Remove(cook.Length - 2);
      }
      Cookies = cook; //возвращаем куки с помощью out Cookies
      UserAgent = ua; //возвращаем user-agent с помощью out UserAgent
      Headers = headers.ToArray();
      for (int i = 0; i < Headers.Length; i++) //удаляем переносы строк, которые создает список
      {
        Headers[i] = Headers[i].Trim('\r', '\n');
      }
    }
    private static string RequestHeadersInGetPost(string RequestHeaders, out string Cookies, out string UserAgent)
    {
      List < string > headers = new List < string > ();
      List < string > cookies = new List < string > (); //список для кук
      string cook = "";
      string peremenDlySikla = "";
      string ua = "";
      headers.AddRange(RequestHeaders.Split('\n')); 
      for (int i = 0; i < headers.Count; i++)
      {
        //если в заголовке содержатся куки
        if (headers[i].Contains("Cookie"))
        {
          lock(SyncObjects.ListSyncer)
          {
            cook = headers[i].Replace("Cookie: ", "");
            headers.RemoveAt(i);
          }
          i--;
          cookies.AddRange(cook.Split(';'));
        }
        else if (headers[i].Contains("User-Agent"))
        {
          lock(SyncObjects.ListSyncer)
          {
            ua = headers[i].Replace("User-Agent: ", "").Trim(' ', '\r', '\n');
            headers.RemoveAt(i);
          }
          i--;
        }
      }
      cookies.UdalenieDubleiAndPustyhStrokIzSpiska();
      cook = ""; //очищаем переменную кук
      foreach(string s in cookies)
      cook += s + "; ";
      if (cook != "")
      {
        string obrezka = cook.Substring(cook.Length - 2, 1);
        if (cook.Substring(cook.Length - 2, 1) == ";") cook = cook.Remove(cook.Length - 2);
      }
      Cookies = cook; //возвращаем куки с помощью out Cookies
      UserAgent = ua; //возвращаем user-agent с помощью out UserAgent

      string Headers = "";
      for (int i = 0; i < headers.Count; i++)
      {
        Headers += headers[i].Trim('\r', '\n') + Environment.NewLine;
      }
      return Headers;
    }

    private static void UrlAndBodyRequest(TrafficItem s, out string url, out string typeMethod, out string body)
    {
      url = s.Url;
      typeMethod = s.Method;
      body = System.Text.Encoding.UTF8.GetString(s.ResponseBody);
    }

    public static void GetHeaders(Instance instance, string url, string typeRequest, SearchType searchType, int number, int minLength, out string Cookies, out string UserAgent, out string[] Headers, out string urlR, out string typeMethod, out string body)
    {
      TrafficItem t = Traffic.SearchRequest(instance, url, typeRequest, searchType, number, minLength);
      Traffic.RequestHeadersInGetPost(t.RequestHeaders, out Cookies, out UserAgent, out Headers);
      Traffic.UrlAndBodyRequest(t, out urlR, out typeMethod, out body);
    }
    public static void GetHeaders(Instance instance, string url, string typeRequest, SearchType searchType, int number, int minLength, ILocalVariable Headers, ILocalVariable Cookies, ILocalVariable UserAgent, ILocalVariable urlR, ILocalVariable typeMethod, ILocalVariable body)
    {
      TrafficItem t = SearchRequest(instance, url, typeRequest, searchType, number, minLength);
      string s = RequestHeadersInGetPost(t.RequestHeaders, out string cookies, out string userAgent);
      Headers.Value = s;
      Cookies.Value = cookies;
      UserAgent.Value = userAgent;
      UrlAndBodyRequest(t, out string z1, out string z2, out string z3);
      urlR.Value = z1;
      typeMethod.Value = z2;
      body.Value = z3;
    }


  }
    public static class ListExtension
    {
        public static void UdalenieDubleiAndPustyhStrokIzSpiska(this List < string > Spisok)
        {
        string dlySikla = "";
        List < string > timeList = new List < string > ();
        List < string > spisok = Spisok.ToList();
        timeList.AddRange(spisok.Distinct().ToList().Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList());
        lock(SyncObjects.ListSyncer)
        {
            Spisok.Clear();
            Spisok.AddRange(timeList);
        }
        }
    }
	
	#region Loggers
	public static class Loggers
	{
		public static void W3Debug(IZennoPosterProjectModel project, string log)
		{
			Time.TotalTime(project);
			if(project.Variables["debug"].Value == "True") 
				project.SendToLog($"⚙: {log}",LogType.Info, true, LogColor.Default);
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
		public static void l0g(IZennoPosterProjectModel project, string toLog = "", string varName = "a0debug", [CallerMemberName] string callerName = "", LogType logType = LogType.Info, LogColor logColor = LogColor.Default, bool show = true, bool thr0w = false)
		{
			if (toLog == "") toLog = project.Variables[$"{varName}"].Value;
			else project.Variables[$"{varName}"].Value = toLog;
			
			//toLog = Regex.Replace(toLog, @"\s+", " ").Trim();
			var acc0 = project.Variables["acc0"].Value;
			var port = project.Variables["instancePort"].Value;
			var lastAction = project.LastExecutedActionId;
			var inSec = $"{(project.LastExecutedActionElapsedTime / 1000.0).ToString("0.000", CultureInfo.InvariantCulture)}s";
			var elapsed = Time.TotalTime(project);
			var stackFrame = new System.Diagnostics.StackFrame(1); 
			var callingMethod = stackFrame.GetMethod();
			


			if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = project.Variables["projectName"].Value;

			
			string formated = $"⛑  [{acc0}] ⚙  [{port}] ⏱  [{elapsed}] ⛏ [{callerName}]. LastAction [{lastAction}] took {inSec}]\n        {toLog.Trim()}";
			
			if (logType == LogType.Info && logColor == LogColor.Default)
			{
				if (formated.Contains("!W")) 
				{
					logType = LogType.Warning;
					logColor = LogColor.Orange;
				}
				else if (formated.Contains("!E")) 
				{
					logType = LogType.Error;
					logColor = LogColor.Orange;
				}
				else if (formated.Contains("relax")) 
				{
					logType = LogType.Info;
					logColor = LogColor.LightBlue;
				}
				else if (Time.TimeElapsed(project) > 60 * 30)
				{
					logType = LogType.Info;
					logColor = LogColor.Yellow;
				}


			}
			
			project.SendToLog(formated, logType, show, logColor);
            if (thr0w) throw new Exception($"{formated}");
		}
	}
	#endregion

	#region OnStart
    public static class OnStart	{
		public static void InitVariables(IZennoPosterProjectModel project, string author = "")
		{
			w3tools.OnStart.DisableLogs();
			if (author == "") author = project.Variables["projectAuthor"].Value;
            project.Variables["varSessionId"].Value = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();
            if (project.Variables["cfgPin"].Value == "") Loggers.l0g(project,"PIN IS EMPTY",thr0w:true);
			if (project.Variables["DBsqltPath"].Value == "") Loggers.l0g(project,"!W SQLite path IS EMPTY");
			project.Variables["instancePort"].Value = $"noInstance";
			project.Variables["timeToday"].Value = DateTime.Now.ToString("MM-dd");
			string name = project.ExecuteMacro(project.Name).Split('.')[0];
            var logo = Logo(name,author);
            project.SendInfoToLog(logo,true);
			project.Variables["projectName"].Value = name;
            if (project.Variables["DBmode"].Value == "SQLite") project.Variables["projectTable"].Value = $"_{name.ToLower()}";
            else if (project.Variables["DBmode"].Value == "PostgreSQL") project.Variables["projectTable"].Value = $"projects.{name.ToLower()}";
			SAFU.Initialize(project);
			SetRange(project);
        }
		private static void SetRange(IZennoPosterProjectModel project)
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
			project.Variables["rangeStart"].Value = $"{rangeS}";
			project.Variables["rangeEnd"].Value = $"{rangeE}";
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
			    for (int i = int.Parse(project.Variables["rangeStart"].Value); i <= int.Parse(project.Variables["rangeEnd"].Value); i++)
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
		private static void DisableLogs()
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
		public static void FilterAccList(IZennoPosterProjectModel project, List<string> dbQueries, bool log = false)
		{
			// Ручной режим
			if (!string.IsNullOrEmpty(project.Variables["acc0Forced"].Value)) 
			{
				project.Lists["accs"].Clear();
				project.Lists["accs"].Add(project.Variables["acc0Forced"].Value);
				if (log) Loggers.l0g(project, $@"manual mode on with {project.Variables["acc0Forced"].Value}");
				return;
			}

			// Получаем все аккаунты из всех запросов
			var allAccounts = new HashSet<string>();
			foreach (var query in dbQueries)
			{
				try
				{
					var accsByQuery = SQL.W3Query(project, query).Trim();
					if (!string.IsNullOrWhiteSpace(accsByQuery))
					{
						var accounts = accsByQuery.Split('\n').Select(x => x.Trim().TrimStart(','));
						allAccounts.UnionWith(accounts);
					}
				}
				catch 
				{
					if (log) Loggers.l0g(project, query,thr0w:true);
				}
			}

			if (allAccounts.Count == 0)
			{
				project.Variables["noAccsToDo"].Value = "True";
				if (log) Loggers.l0g(project, $"♻ noAccountsAvailable by queries [{string.Join(" | ", dbQueries)}]");
				return;
			}

			if (log) Loggers.l0g(project, $"Initial availableAccounts: [{string.Join(", ", allAccounts)}]");

			// Фильтрация по социальным сетям
			if (!string.IsNullOrEmpty(project.Variables["requiredSocial"].Value))
			{
				string[] demanded = project.Variables["requiredSocial"].Value.Split(',');
				if (log) Loggers.l0g(project, $"Filtering by socials: [{string.Join(", ", demanded)}]");
				
				foreach (string social in demanded)
				{   
                    string tableName = social.Trim().ToLower();
                    if (project.Variables["DBmode"].Value != "SQLite") tableName = $"accounts.{tableName}";
					var notOK = SQL.W3Query(project, $"SELECT acc0 FROM {tableName} WHERE status NOT LIKE '%ok%'",log)
						.Split('\n')
						.Select(x => x.Trim())
						.Where(x => !string.IsNullOrEmpty(x));
					allAccounts.ExceptWith(notOK);
					if (log) Loggers.l0g(project, $"After {social} filter: [{string.Join("|", allAccounts)}]");
				}
			}

			// Финальное заполнение списка
			project.Lists["accs"].Clear();
			project.Lists["accs"].AddRange(allAccounts);
			if (log) Loggers.l0g(project, $"final list [{string.Join("|", project.Lists["accs"])}]");
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
				
                Loggers.l0g(project, ex.Message, thr0w:true);
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
            Loggers.l0g(project, "badProxy", thr0w:true);
			
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
			catch{Loggers.l0g(project,$"!W noCookiesAvaliable by path {filePath}");}
		}
		public static void SetCookiesFromDB(this Instance instance, IZennoPosterProjectModel project, string tableName ="profile", string schemaName = "accounts")
		{
			string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;			           
            try
			{
				var cookies = SQL.W3Query(project,$@"SELECT cookies FROM {table} WHERE acc0 = {project.Variables["acc0"].Value}");
				instance.SetCookie(cookies);
			}
			catch{Loggers.l0g(project,$"!W noCookiesAvaliable by query");}
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
			if (project.Variables["skipBrowserScan"].Value == "True") 
            {
                Loggers.l0g(project,"BrowserCheck skipped");
                return;
            }
            bool set = false;
			string timezoneOffset = "";
			string timezoneName = "";
            var tableName = "browser";

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

            if (project.Variables["DBmode"].Value != "SQLite") tableName = $"accounts.{tableName}";
			if (project.Variables["makeTable"].Value == "True") SQL.W3MakeTable(project,tableStructure,tableName);
            
            var required = "Browser";

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
		private static string Logo(string name, string author)
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
	public static class SQL
	{
        public static string W3Query(IZennoPosterProjectModel project, string query, bool log = false, bool throwOnEx = false)
        {
            string dbMode = project.Variables["DBmode"].Value;
            if (project.Variables["debug"].Value == "True") log = true;
            if (dbMode == "SQLite") return SQLite.lSQL(project, query, log);
            else if (dbMode == "PostgreSQL") return PostgresDB.pSQL(project, query, log, throwOnEx);
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
                if (dbMode == "SQLite") return ;//SQLite.lSQLMakeTable(project, tableStructure, tableName, strictMode);
                else if (dbMode == "PostgreSQL") PostgresDB.pSQLMakeTable(project, tableStructure, tableName, strictMode, insertData, host, dbName, dbUser, dbPswd, schemaName, log:log);
                else throw new Exception($"Неподдерживаемый режим базы данных: {dbMode}");
            }	
	}
   
	public class C00kies
	{
		private readonly Instance _instance;
		private readonly IZennoPosterProjectModel _project;
		public C00kies(IZennoPosterProjectModel project, Instance instance)
		{
			_project = project;
			_instance = instance;
		}
		public string c00kies(string domainFilter = "")
		{
			if (domainFilter == ".") domainFilter = _instance.ActiveTab.MainDomain;
			var cookieContainer = _project.Profile.CookieContainer;
			var cookieList = new List<object>();

			foreach (var domain in cookieContainer.Domains)
			{
				if (string.IsNullOrEmpty(domainFilter) || domain.Contains(domainFilter))
				{
					var cookies = cookieContainer.Get(domain);
					cookieList.AddRange(cookies.Select(cookie => new
					{
						domain = cookie.Host,
						expirationDate = cookie.Expiry == DateTime.MinValue ? (double?)null : new DateTimeOffset(cookie.Expiry).ToUnixTimeSeconds(),
						hostOnly = !cookie.IsDomain,
						httpOnly = cookie.IsHttpOnly,
						name = cookie.Name,
						path = cookie.Path,
						sameSite = cookie.SameSite.ToString(),
						secure = cookie.IsSecure,
						session = cookie.IsSession,
						storeId = (string)null,
						value = cookie.Value,
						id = cookie.GetHashCode()
					}));
				}
			}
			string cookiesJson = Global.ZennoLab.Json.JsonConvert.SerializeObject(cookieList, Global.ZennoLab.Json.Formatting.Indented);

			cookiesJson = cookiesJson.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");
			_project.Json.FromString(cookiesJson);
			return cookiesJson;
		}
		public string c00kiesJGet(bool log = false)
		{
			string jsCode = @"
			var cookies = document.cookie.split('; ').map(function(cookie) {
				var parts = cookie.split('=');
				var name = parts[0];
				var value = parts.slice(1).join('=');
				return {
					'domain': window.location.hostname,
					'name': name,
					'value': value,
					'path': '/', 
					'expirationDate': null, 
					'hostOnly': true,
					'httpOnly': false,
					'secure': window.location.protocol === 'https:',
					'session': false,
					'sameSite': 'Unspecified',
					'storeId': null,
					'id': 1
				};
			});
			return JSON.stringify(cookies);
			";

			string jsonResult = _instance.ActiveTab.MainDocument.EvaluateScript(jsCode).ToString();
			if (log) Loggers.l0g(_project,jsonResult);
			JArray cookiesArray = JArray.Parse(jsonResult);
			var escapedJson = jsonResult.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(" ", "").Replace("'", "''").Trim();
			_project.Json.FromString(jsonResult);
			return escapedJson;
		}
		public void c00kiesJSet(string cookiesJson, bool log = false){
			try
			{
				JArray cookies = JArray.Parse(cookiesJson);

				var uniqueCookies = cookies
					.GroupBy(c => new { Domain = c["domain"].ToString(), Name = c["name"].ToString() })
					.Select(g => g.Last())
					.ToList();

				string currentDomain = _instance.ActiveTab.Domain;
				//long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				//long weekFromNowUnixTime = currentUnixTime + (7 * 24 * 60 * 60);
				//string weekFromNow = DateTimeOffset.UtcNow.AddYears(1).ToString("R");

				string[] domainParts = currentDomain.Split('.');
        		string parentDomain = "." + string.Join(".", domainParts.Skip(domainParts.Length - 2));


				string jsCode = "";
				int cookieCount = 0;
				foreach (JObject cookie in uniqueCookies)
				{
					string domain = cookie["domain"].ToString();
					string name = cookie["name"].ToString();
					string value = cookie["value"].ToString();

					if (domain == currentDomain || domain == "." + currentDomain)
					{
						string path = cookie["path"]?.ToString() ?? "/";
						string expires;

						if (cookie["expirationDate"] != null && cookie["expirationDate"].Type != JTokenType.Null)
						{
							double expValue = double.Parse(cookie["expirationDate"].ToString());
							if (expValue < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) expires = DateTimeOffset.UtcNow.AddYears(1).ToString("R");
							else expires = DateTimeOffset.FromUnixTimeSeconds((long)expValue).ToString("R");
						}
						else 
							expires = DateTimeOffset.UtcNow.AddYears(1).ToString("R");

						jsCode += $"document.cookie = '{name}={value}; domain={parentDomain}; path={path}; expires={expires}'; Secure';\n";
						cookieCount++;
					}
				}
				if (log) Loggers.l0g(_project,$"Found cookies for {currentDomain}: [{cookieCount}] runingJs...\n + {jsCode}");

				if (!string.IsNullOrEmpty(jsCode))
				{
					_instance.ActiveTab.MainDocument.EvaluateScript(jsCode);
				}
				else     Loggers.l0g(_project,$"!W No cookies Found for {currentDomain}");
			}
			catch (Exception ex)
			{
				Loggers.l0g(_project,$"!W cant't parse JSON: [{cookiesJson}]  {ex.Message}");
			}
		}
	}

    public static class Db
    {
        public static string KeyEVM(IZennoPosterProjectModel project, string tableName ="blockchain_private", string schemaName = "accounts")
        {
			string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = SQL.W3Query(project,$"SELECT secp256k1 FROM {table} WHERE acc0 = {project.Variables["acc0"].Value}");
            return SAFU.Decode(project,resp);
        }
        public static string KeySOL(IZennoPosterProjectModel project, string tableName ="blockchain_private", string schemaName = "accounts")
        {
            string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;			
            var  resp = SQL.W3Query(project,$"SELECT base58 FROM {table} WHERE acc0 = {project.Variables["acc0"].Value}");
            return SAFU.Decode(project,resp);
        }
        public static string Seed(IZennoPosterProjectModel project, string tableName ="blockchain_private", string schemaName = "accounts")
        {
            string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;			
            var resp = SQL.W3Query(project,$"SELECT bip39 FROM {table} WHERE acc0 = {project.Variables["acc0"].Value}");
            return SAFU.Decode(project,resp);
        }   
        public static string AdrEvm(IZennoPosterProjectModel project, string tableName ="blockchain_public", string schemaName = "accounts")
        {
            string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;	
            var resp = SQL.W3Query(project,$"SELECT evm FROM {table} WHERE acc0 = {project.Variables["acc0"].Value}");
            project.Variables["addressEvm"].Value = resp;  return resp;
        }   		
        public static string Proxy(IZennoPosterProjectModel project, string tableName ="profile", string schemaName = "accounts")
        {
            string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = SQL.W3Query(project,$"SELECT proxy FROM {table} WHERE acc0 = {project.Variables["acc0"].Value}");
            project.Variables["proxy"].Value = resp;
			try {project.Variables["proxyLeaf"].Value = resp.Replace("//", "").Replace("@", ":");} catch{}
			return resp;
        }   
        public static string Settings(IZennoPosterProjectModel project, string tableName ="settings", string schemaName = "accounts")
        {
			string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
			return SQL.W3Query(project,$"SELECT var, value FROM {table}");
        }   
        public static string Google(IZennoPosterProjectModel project, string tableName ="google", string schemaName = "accounts")
        {
            string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
			
			var resp = SQL.W3Query(project,$@"SELECT status, login, password, code2FA, recoveryEmail, recovery2FA FROM {table} WHERE acc0 = {project.Variables["acc0"].Value};");
            
            string[] googleData = resp.Split('|');
            project.Variables["googleSTATUS"].Value = googleData[0].Trim();
            project.Variables["googleLOGIN"].Value = googleData[1].Trim();
            project.Variables["googlePASSWORD"].Value = googleData[2].Trim();
            project.Variables["google2FACODE"].Value =googleData[3].Trim();
            project.Variables["googleSECURITY_MAIL"].Value = googleData[4].Trim();
            project.Variables["googleBACKUP_CODES"].Value = googleData[5].Trim();
            return project.Variables["googleSTATUS"].Value;
        }  
 		public static void TwitterTokenUpdate(IZennoPosterProjectModel project, string tableName ="twitter", string schemaName = "accounts")  
		{
			string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
			var resp = SQL.W3Query(project,$"UPDATE {table} SET token = '{project.Variables["twitterTOKEN"].Value}' WHERE acc0 = {project.Variables["acc0"].Value};");

		}
		public static void UpdAddressSol(IZennoPosterProjectModel project,string address = "", string tableName ="blockchain_public", string schemaName = "accounts")  
		{
			string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
			if (address == "") address = project.Variables["addressSol"].Value;
			SQL.W3Query(project,$"UPDATE {table} SET sol = '{address}' WHERE acc0 = {project.Variables["acc0"].Value};");

		}
		public static string BinanceApiKeys(IZennoPosterProjectModel project, string tableName ="settings", string schemaName = "accounts")  
		{
            string table = (project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
			return SQL.W3Query(project,$"SELECT value FROM {tableName} WHERE var = 'settingsApiBinance';");
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
	public class OnChain
	{
		private readonly IZennoPosterProjectModel _project;
        private readonly L0g _log;
		private readonly bool _logShow;
		public OnChain(IZennoPosterProjectModel project, bool log = false)
		{
			_project = project;
            _log = new L0g(_project);
			_logShow = log;
		}

		public void BalLog(string address, string balance, string rpc, string contract = null, [CallerMemberName] string callerName = "", bool log = false)
		{	
			if (!_logShow && !log) return;
			var stackFrame = new System.Diagnostics.StackFrame(1); 
			var callingMethod = stackFrame.GetMethod();
			if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
			_log.Send( $"[ ⛽  {callerName}] [{address}] balance {contract} is\n		  [{balance}] by [{rpc}]");
		}

		public string Rpc(string chain)
		{
            chain = chain.ToLower();
			switch (chain)
			{
				//ethNative
                case "ethereum": return "https://ethereum-rpc.publicnode.com";
				case "arbitrum": return "https://arbitrum-one.publicnode.com";
                case "base": return "https://base-rpc.publicnode.com";
 				case "blast": return "https://rpc.blast.io";
 				case "linea": return "https://rpc.linea.build"; 
          		case "manta": return "https://pacific-rpc.manta.network/http";                    
				case "optimism": return "https://optimism-rpc.publicnode.com";
				case "scroll": return "https://rpc.scroll.io";
				case "soneium": return "https://rpc.soneium.org";
				case "taiko": return "https://rpc.mainnet.taiko.xyz";
				case "zksync": return "https://mainnet.era.zksync.io";
 				case "zora": return "https://rpc.zora.energy";               
                //nonEthEvm
				case "avalanche": return "https://avalanche-c-chain.publicnode.com";
				case "bsc": return "https://bsc-rpc.publicnode.com";
 				case "gravity": return "https://rpc.gravity.xyz";
				case "fantom": return "https://rpc.fantom.network";                               
				case "opbnb": return "https://opbnb-mainnet-rpc.bnbchain.org";
				case "polygon": return "https://polygon-rpc.com";
                //Testnets
				case "sepolia": return "https://ethereum-sepolia-rpc.publicnode.com";
				//nonEvm
				case "aptos": return "https://fullnode.mainnet.aptoslabs.com/v1";
				case "movement": return "https://mainnet.movementnetwork.xyz/v1";

				default:
					throw new ArgumentException("No RPC for: " + chain);
			}
		}
		public string[] RpcArr(string chains)
        {
            string rpcs = null;
            string[] toAdd = chains.Split(',');
            foreach (string chain in toAdd) rpcs += Rpc(chain.Trim())+"\n";
            return rpcs.Trim().Split('\n');
        }
		public T FloorDecimal<T>(decimal value, int? decimalPlaces = null)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			int effectiveDecimalPlaces = decimalPlaces ?? 18; 

			if (effectiveDecimalPlaces < 0)
				throw new ArgumentException("Decimal places must be non-negative", nameof(decimalPlaces));

			try
			{
				string valueStr = value.ToString(CultureInfo.InvariantCulture);
				int actualDecimalPlaces = 0;
				if (valueStr.Contains("."))
				{
					actualDecimalPlaces = valueStr.Split('.')[1].Length;
				}

				effectiveDecimalPlaces = Math.Min(effectiveDecimalPlaces, actualDecimalPlaces);

				if (effectiveDecimalPlaces > 28) // decimal type supports up to 28-29 digits
				{
					_project.SendWarningToLog($"Requested decimal places ({effectiveDecimalPlaces}) exceeds decimal type limit. Adjusting to 28.");
					effectiveDecimalPlaces = 28;
				}

				decimal multiplier = (decimal)Math.Pow(10, effectiveDecimalPlaces);
				decimal flooredValue = Math.Floor(value * multiplier) / multiplier;

				if (typeof(T) == typeof(string))
				{
					string format = "0." + new string('#', effectiveDecimalPlaces);
					return (T)Convert.ChangeType(flooredValue.ToString(format, CultureInfo.InvariantCulture), typeof(T));
				}
				if (typeof(T) == typeof(int))
					return (T)Convert.ChangeType((int)flooredValue, typeof(T));
				if (typeof(T) == typeof(double))
					return (T)Convert.ChangeType((double)flooredValue, typeof(T));
				return (T)Convert.ChangeType(flooredValue, typeof(T));
			}
			catch (OverflowException ex)
			{
				_project.SendWarningToLog($"Overflow error while flooring {value} to {effectiveDecimalPlaces} decimal places: {ex.Message}");
				return (T)Convert.ChangeType(value, typeof(T)); // Return original value as fallback
			}
			catch (Exception ex)
			{
				_project.SendWarningToLog($"Error while flooring {value} to {effectiveDecimalPlaces} decimal places: {ex.Message}");
				return (T)Convert.ChangeType(value, typeof(T)); // Return original value as fallback
			}
		}
		public T GasPrice<T>(string chainRPC = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(chainRPC)) 
				chainRPC = _project.Variables["blockchainRPC"].Value;

			string jsonBody = @"{""jsonrpc"":""2.0"",""method"":""eth_gasPrice"",""params"":[],""id"":1}";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					try
					{
						string[] proxyArray = proxy.Split(':');
						string username = proxyArray[1];
						string password = proxyArray[2];
						string host = proxyArray[3];
						int port = int.Parse(proxyArray[4]);
						request.Proxy = new HttpProxyClient(host, port, username, password);
					}
					catch (Exception ex)
					{
						_project.SendErrorToLog($"Ошибка парсинга прокси '{proxy}': {ex.Message}");
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
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var json = JObject.Parse(response);
			string hexResultGas = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
			BigInteger gasWei = BigInteger.Parse("0" + hexResultGas, NumberStyles.AllowHexSpecifier);
			decimal gasGwei = (decimal)gasWei / 1000000000m;

			if (log) Loggers.l0g(_project, $"Gas price: {gasGwei} Gwei");

			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(gasGwei.ToString("0.######", CultureInfo.InvariantCulture), typeof(T));
			return (T)Convert.ChangeType(gasGwei, typeof(T));
		}
		public T NativeEVM<T>(string chainRPC = null, string address = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(address)) 
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
				address = SQL.W3Query(_project,$"SELECT evm FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}
			if (string.IsNullOrEmpty(chainRPC)) chainRPC = _project.Variables["blockchainRPC"].Value;

			string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_getBalance"", ""params"": [""{address}"", ""latest""], ""id"": 1 }}";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var json = JObject.Parse(response);
			string hexBalance = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
			BigInteger balanceWei = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);
			decimal balance = (decimal)balanceWei / 1000000000000000000m;

			string balanceString = FloorDecimal<string>(balance, int.Parse("18")); 
			BalLog( address,  balanceString,  chainRPC, log:log);
			if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));			
		}
        public T BalERC20<T>(string tokenContract, string chainRPC = null, string address = null, string tokenDecimal = "18", string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) 
            {
                string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
                address = SQL.W3Query(_project, $"SELECT evm FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            }
            if (string.IsNullOrEmpty(chainRPC)) chainRPC = _project.Variables["blockchainRPC"].Value;

            string data = "0x70a08231000000000000000000000000" + address.Replace("0x", "");
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexBalance = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
            BigInteger balanceWei = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);
            decimal decimals = (decimal)Math.Pow(10, double.Parse(tokenDecimal));
            decimal balance = (decimal)balanceWei / decimals;

			string balanceString = FloorDecimal<string>(balance, int.Parse(tokenDecimal)); 
			BalLog( address,  balanceString,  chainRPC, tokenContract,log:log);
			if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));		

        }
        public T BalERC721<T>(string tokenContract, string chainRPC = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) 
            {
                string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
                address = SQL.W3Query(_project, $"SELECT evm FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            }
            if (string.IsNullOrEmpty(chainRPC)) chainRPC = _project.Variables["blockchainRPC"].Value;

            string functionSelector = "0x70a08231";
            string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');
            string data = functionSelector + paddedAddress;
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexBalance = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
            BigInteger balance = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);

            if (log) _log.Send($"[Leaf.xNet] Баланс токенов ERC-721 для адреса {address} в контракте {tokenContract}: {balance}");

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(balance.ToString(), typeof(T));

            return (T)Convert.ChangeType(balance, typeof(T));
        }
        public T BalERC1155<T>(string tokenContract, string tokenId, string chainRPC = null, string address = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(address)) 
            {
                string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
                address = SQL.W3Query(_project, $"SELECT evm FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            }
            if (string.IsNullOrEmpty(chainRPC)) chainRPC = _project.Variables["blockchainRPC"].Value;

            string functionSelector = "0x00fdd58e";
            string paddedAddress = address.Replace("0x", "").ToLower().PadLeft(64, '0');
            string paddedTokenId = BigInteger.Parse(tokenId).ToString("x").PadLeft(64, '0');
            string data = functionSelector + paddedAddress + paddedTokenId;
            string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""eth_call"", ""params"": [{{ ""to"": ""{tokenContract}"", ""data"": ""{data}"" }}, ""latest""], ""id"": 1 }}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexBalance = json["result"]?.ToString()?.TrimStart('0', 'x') ?? "0";
            BigInteger balance = BigInteger.Parse("0" + hexBalance, NumberStyles.AllowHexSpecifier);

            if (log) _log.Send($"[Leaf.xNet ⇌] balance of ERC-1155 [{tokenContract}:id({tokenId})] on {address}: [{balance}]");

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(balance.ToString(), typeof(T));
            else if (typeof(T) == typeof(int))
                return (T)(object)(int)balance;
            else if (typeof(T) == typeof(BigInteger))
                return (T)(object)balance;
            else
                throw new InvalidOperationException($"!W unsupported type {typeof(T)}");
        }
        public T ChainId<T>(string chainRPC = null, string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(chainRPC)) chainRPC = _project.Variables["blockchainRPC"].Value;
            string jsonBody = @"{""jsonrpc"": ""2.0"",""method"": ""eth_chainId"",""params"": [],""id"": 1}";

            string response;
            using (var request = new HttpRequest())
            {
                request.UserAgent = "Mozilla/5.0";
                request.IgnoreProtocolErrors = true;
                request.ConnectTimeout = 5000;

                if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
                if (!string.IsNullOrEmpty(proxy))
                {
                    string[] proxyArray = proxy.Split(':');
                    string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
                    request.Proxy = new HttpProxyClient(host, port, username, password);
                }

                try
                {
                    HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
                    response = httpResponse.ToString();
                }
                catch (HttpException ex)
                {
                    _project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
                    throw;
                }
            }

            var json = JObject.Parse(response);
            string hexResult = json["result"]?.ToString() ?? "0x0";
            if (hexResult == "0x0")
                return (T)Convert.ChangeType("0", typeof(T));

            int chainId = Convert.ToInt32(hexResult.TrimStart('0', 'x'), 16);

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(chainId.ToString(), typeof(T));

            return (T)Convert.ChangeType(chainId, typeof(T));
        }
		public T NonceEVM<T>(string chainRPC = null, string address = null, string proxy = null, bool log = false)
		{
			if (string.IsNullOrEmpty(address)) 
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
				address = SQL.W3Query(_project,$"SELECT evm FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}
			if (string.IsNullOrEmpty(chainRPC)) chainRPC = _project.Variables["blockchainRPC"].Value;

			string jsonBody = $@"{{""jsonrpc"": ""2.0"",""method"": ""eth_getTransactionCount"",""params"": [""{address}"", ""latest""],""id"": 1}}";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var match = Regex.Match(response, @"""result""\s*:\s*""([^""]+)""");
			string hexResultNonce = match.Success ? match.Groups[1].Value : "0x0";
			
			if (hexResultNonce == "0x0") 
				return (T)Convert.ChangeType("0", typeof(T));
			
			int transactionCount = Convert.ToInt32(hexResultNonce.TrimStart('0', 'x'), 16);
			if (log) _log.Send( $"{address} nonce now {transactionCount}");
			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(transactionCount.ToString(), typeof(T));			
			return (T)Convert.ChangeType(transactionCount, typeof(T));
		}
		public T NativeSOL<T>(string chainRPC = null, string address = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(address)) 
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
				address = SQL.W3Query(_project,$"SELECT sol FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}
			if (string.IsNullOrEmpty(chainRPC)) chainRPC = "https://api.mainnet-beta.solana.com";

			string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getBalance"", ""params"": [""{address}""], ""id"": 1 }}";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);		
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Post(chainRPC, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var json = JObject.Parse(response);
			string tokenDecimal = json["result"]?["value"]?.ToString() ?? "0";			


			decimal balance = decimal.Parse(tokenDecimal) / 1000000000m;

			string balanceString = FloorDecimal<string>(balance, int.Parse(tokenDecimal));
			BalLog( address,  balanceString,  chainRPC,log:log);

			if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));		

		}
		public T TokenSPL<T>(string tokenMint, string address = null, int floor = 0, string rpc = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(address)) 
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
				address = SQL.W3Query(_project,$"SELECT sol FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}
			if (string.IsNullOrEmpty(rpc)) rpc = "https://api.mainnet-beta.solana.com";

			string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""getTokenAccountsByOwner"", ""params"": [""{address}"", {{""mint"": ""{tokenMint}""}}, {{""encoding"": ""jsonParsed""}}], ""id"": 1 }}";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);        
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}
			var json = JObject.Parse(response);
			var tokenAccounts = json["result"]?["value"] as JArray;
			string lamports = tokenAccounts != null && tokenAccounts.Count > 0 
				? tokenAccounts[0]?["account"]?["data"]?["parsed"]?["info"]?["tokenAmount"]?["amount"]?.ToString() ?? "0"
				: "0";
				
			int decimals = tokenAccounts != null && tokenAccounts.Count > 0 
				? int.Parse(tokenAccounts[0]?["account"]?["data"]?["parsed"]?["info"]?["tokenAmount"]?["decimals"]?.ToString() ?? "0")
				: 0;
			decimal balance = decimal.Parse(lamports) / (decimal)Math.Pow(10, decimals);
		
			string balanceString = FloorDecimal<string>(balance, decimals);
			BalLog( address,  balanceString,  rpc, tokenMint);

			if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(balanceString, typeof(T));
            return (T)Convert.ChangeType(balance, typeof(T));

		}
		public T NativeSUI<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(address)) 
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
				address = SQL.W3Query(_project,$"SELECT sui FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}
			if (string.IsNullOrEmpty(rpc)) rpc = "https://fullnode.mainnet.sui.io";

			string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""suix_getBalance"", ""params"": [""{address}"", ""0x2::sui::SUI""], ""id"": 1 }}";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);        
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var json = JObject.Parse(response);
			string mist = json["result"]?["totalBalance"]?.ToString() ?? "0";
			decimal balanceSui = decimal.Parse(mist) / 1000000000m;
			if (log) _log.Send( $"{address}: {balanceSui} SUI");

			if (typeof(T) == typeof(string))
				return (T)Convert.ChangeType(balanceSui.ToString("0.##################"), typeof(T));
			return (T)Convert.ChangeType(balanceSui, typeof(T));
		}
		public T TokenSUI<T>(string coinType, string address = null, string rpc = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(address)) 
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
				address = SQL.W3Query(_project,$"SELECT sui FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}
			if (string.IsNullOrEmpty(rpc)) rpc = "https://fullnode.mainnet.sui.io";

			string jsonBody = $@"{{ ""jsonrpc"": ""2.0"", ""method"": ""suix_getBalance"", ""params"": [""{address}"", ""{coinType}""], ""id"": 1 }}";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);        
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Post(rpc, jsonBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var json = JObject.Parse(response);
			string mist = json["result"]?["totalBalance"]?.ToString() ?? "0";
			decimal balance = decimal.Parse(mist) / 1000000m;
			if (log) _log.Send( $"{address}: {balance} TOKEN ({coinType})");


			if (typeof(T) == typeof(string)) return FloorDecimal<T>(balance, int.Parse(mist)); 
            return (T)Convert.ChangeType(balance, typeof(T));

		}

		public T NativeAPT<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			if (string.IsNullOrEmpty(address))
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";
				address = SQL.W3Query(_project, $"SELECT apt FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}

			if (string.IsNullOrEmpty(rpc))
				rpc = "https://fullnode.mainnet.aptoslabs.com/v1";

			string url = $"{rpc}/view";
			string coinType = "0x1::aptos_coin::AptosCoin";
			string requestBody = $@"{{
				""function"": ""0x1::coin::balance"",
				""type_arguments"": [""{coinType}""],
				""arguments"": [""{address}""]
			}}";

			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;
				request.AddHeader("Content-Type", "application/json");

				// Настройка прокси, если указан
				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1];
					string password = proxyArray[2];
					string host = proxyArray[3];
					int port = int.Parse(proxyArray[4]);
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Post(url, requestBody, "application/json");
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTpreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			JArray json;
			try
			{
				json = JArray.Parse(response);
			}
			catch (Exception ex)
			{
				_project.SendErrorToLog($"Failed to parse JSON response: {ex.Message}");
				if (typeof(T) == typeof(string)) return (T)(object)"0";
				return (T)(object)0m;
			}

			string octas = json[0]?.ToString() ?? "0";
			decimal balance;
			try
			{
				balance = decimal.Parse(octas) / 100000000m; // 8 decimals
			}
			catch (Exception ex)
			{
				_project.SendErrorToLog($"Failed to parse balance: {ex.Message}");
				if (typeof(T) == typeof(string)) return (T)(object)"0";
				return (T)(object)0m;
			}

			string balanceString = FloorDecimal<string>(balance, 8);
			BalLog(address, balanceString, rpc ,log:log);

			if (typeof(T) == typeof(string)) return (T)(object)balanceString;
			return (T)Convert.ChangeType(balance, typeof(T));
		}

		public T TokenAPT<T>(string coinType, string address = null, string rpc = null, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(address)) 
			{
				string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : "") + "blockchain_public";	
				address = SQL.W3Query(_project,$"SELECT evm FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
			}
			if (string.IsNullOrEmpty(rpc)) rpc = "https://fullnode.mainnet.aptoslabs.com/v1";

			string url = $"{rpc}/accounts/{address}/resource/0x1::coin::CoinStore<{coinType}>";
			string response;

			using (var request = new HttpRequest())
			{
				request.UserAgent = "Mozilla/5.0";
				request.IgnoreProtocolErrors = true;
				request.ConnectTimeout = 5000;

				if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
				if (!string.IsNullOrEmpty(proxy))
				{
					string[] proxyArray = proxy.Split(':');
					string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);        
					request.Proxy = new HttpProxyClient(host, port, username, password);
				}

				try
				{
					HttpResponse httpResponse = request.Get(url);
					response = httpResponse.ToString();
				}
				catch (HttpException ex)
				{
					_project.SendErrorToLog($"Err HTTPreq: {ex.Message}, Status: {ex.Status}");
					throw;
				}
			}

			var json = JObject.Parse(response);
			string octas = json["data"]?["coin"]?["value"]?.ToString() ?? "0";
			decimal balance = decimal.Parse(octas) / 1000000m; // Предполагаем 6 decimals, как для USDC
			if (log) _log.Send( $"{address}: {balance} TOKEN ({coinType})");
			if (typeof(T) == typeof(string)) return FloorDecimal<T>(balance, int.Parse(octas)); 
            return (T)Convert.ChangeType(balance, typeof(T));

		}
		public string Send1559(string chainRpc, string contractAddress, string encodedData, decimal value, string walletKey, int speedup = 1)
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
		public string GZ(string chainTo, decimal value, string chainRPC = null, bool log = false) //refuel GazZip
		
		{
			
			// 0x010066 Sepolia | 0x01019e Soneum | 0x01000e BNB | 0x0100f0 Gravity | 0x010169 Zero
			string txHash = null;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; 
			Random rnd = new Random();
			var accountAddress = Db.AdrEvm(_project); 
			
			if (string.IsNullOrEmpty(chainRPC))
			{
				string chainList = @"https://mainnet.era.zksync.io,
				https://linea-rpc.publicnode.com,
				https://arb1.arbitrum.io/rpc,
				https://optimism-rpc.publicnode.com,
				https://scroll.blockpi.network/v1/rpc/public,
				https://rpc.taiko.xyz,
				https://base.blockpi.network/v1/rpc/public,
				https://rpc.zora.energy";
				
				
				bool found = false; 
				foreach (string RPC in chainList.Split(','))
				{
				    chainRPC = RPC.Trim();
					var native = NativeEVM<decimal>(chainRPC);
					var required = value + 0.00015m;
				    if (native > required)
				    {
				        if (log) _log.Send($"CHOSEN: rpc:[{chainRPC}] native:[{native}]");
						found = true; break;
				    }
					if (log) _log.Send($"rpc:[{chainRPC}] native:[{native}] lower than [{required}]");
				    Thread.Sleep(1000);
				}
				
				
				if (!found)
				{
				    return $"fail: no balance over {value}ETH found by all Chains";
				}
			}
			
			else 
			{
				var native = NativeEVM<decimal>(chainRPC);
				if (log) _log.Send($"rpc:[{chainRPC}] native:[{native}]");
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
			    string dataEncoded = chainTo;//0x010066 for Sepolia | 0x01019e Soneum | 0x01000e BNB
			    txHash = Send1559(
			        chainRPC,
			        "0x391E7C679d29bD940d63be94AD22A25d25b5A604",//gazZip
			        dataEncoded,
			        value,  // value в ETH
			        Db.KeyEVM(_project),
			        3          // speedup %
			    );
			    Thread.Sleep(1000);
			    _project.Variables["blockchainHash"].Value = txHash;
			}
			catch (Exception ex){_project.SendWarningToLog($"{ex.Message}",true);throw;}
			
			if (log) _log.Send(txHash);
			WaitTransaction(chainRPC,txHash);
			return txHash;
		}
		public string WaitTransaction(string chainRPC = null, string hash = null, int deadline = 60, string proxy = "", bool log = false)
			{
				// Установка значений по умолчанию из переменных проекта, если параметры пустые
				if (string.IsNullOrEmpty(hash)) hash = _project.Variables["blockchainHash"].Value;
				if (string.IsNullOrEmpty(chainRPC)) chainRPC = _project.Variables["blockchainRPC"].Value;

				// JSON-запросы для получения receipt и raw транзакции
				string jsonReceipt = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionReceipt"",""params"":[""{hash}""],""id"":1}}";
				string jsonRaw = $@"{{""jsonrpc"":""2.0"",""method"":""eth_getTransactionByHash"",""params"":[""{hash}""],""id"":1}}";

				// Инициализация HTTP-запроса
				string response;
				using (var request = new HttpRequest())
				{
					request.UserAgent = "Mozilla/5.0";
					request.IgnoreProtocolErrors = true;
					request.ConnectTimeout = 5000;

					if (proxy == "+") proxy = _project.Variables["proxyLeaf"].Value;
					if (!string.IsNullOrEmpty(proxy))
					{
						string[] proxyArray = proxy.Split(':');
						string username = proxyArray[1]; string password = proxyArray[2]; string host = proxyArray[3]; int port = int.Parse(proxyArray[4]);
						request.Proxy = new HttpProxyClient(host, port, username, password);
					}

					// Таймер для отслеживания дедлайна
					DateTime startTime = DateTime.Now;
					TimeSpan timeout = TimeSpan.FromSeconds(deadline);
					

					// Основной цикл ожидания транзакции
					while (true)
					{
						if (DateTime.Now - startTime > timeout)
							throw new Exception($"timeout {deadline}s");

						string logString = "";

						// Проверка receipt транзакции
						try
						{
							HttpResponse httpResponse = request.Post(chainRPC, jsonReceipt, "application/json");
							response = httpResponse.ToString();

							if (httpResponse.StatusCode != HttpStatusCode.OK)
							{
								_project.SendErrorToLog($"Ошибка сервера (receipt): {httpResponse.StatusCode}");
								Thread.Sleep(2000);
								continue;
							}

							if (string.IsNullOrWhiteSpace(response) || response.Contains("\"result\":null"))
							{
								_project.Variables["txStatus"].Value = "noStatus";
							}
							else
							{
								_project.Json.FromString(response);
								try
								{
									string gasUsed = Onchain.HexToString(_project.Json.result.gasUsed, "gwei");
									string gasPrice = Onchain.HexToString(_project.Json.result.effectiveGasPrice, "gwei");
									string status = Onchain.HexToString(_project.Json.result.status);

									_project.Variables["txStatus"].Value = status == "1" ? "SUCCSESS" : "!W FAIL";
									string result = $"{chainRPC} {hash} [{_project.Variables["txStatus"].Value}] gasUsed: {gasUsed}";
									Loggers.W3Debug(_project, result);
									return result;
								}
								catch
								{
									_project.Variables["txStatus"].Value = "noStatus";
								}
							}
						}
						catch (HttpException ex)
						{
							_project.SendErrorToLog($"Ошибка запроса (receipt): {ex.Message}");
							Thread.Sleep(2000);
							continue;
						}

						// Проверка raw транзакции
						try
						{
							HttpResponse httpResponse = request.Post(chainRPC, jsonRaw, "application/json");
							response = httpResponse.ToString();

							if (httpResponse.StatusCode != HttpStatusCode.OK)
							{
								_project.SendErrorToLog($"Ошибка сервера (raw): {httpResponse.StatusCode}");
								Thread.Sleep(2000);
								continue;
							}

							if (string.IsNullOrWhiteSpace(response) || response.Contains("\"result\":null"))
							{
								_project.Variables["txStatus"].Value = "";
								logString = $"[{chainRPC} {hash}] not found";
							}
							else
							{
								_project.Json.FromString(response);
								try
								{
									string gas = Onchain.HexToString(_project.Json.result.maxFeePerGas, "gwei");
									string gasPrice = Onchain.HexToString(_project.Json.result.gasPrice, "gwei");
									string nonce = Onchain.HexToString(_project.Json.result.nonce);
									string value = Onchain.HexToString(_project.Json.result.value, "eth");
									_project.Variables["txStatus"].Value = "PENDING";

									logString = $"[{chainRPC} {hash}] pending  gasLimit:[{gas}] gasNow:[{gasPrice}] nonce:[{nonce}] value:[{value}]";
								}
								catch
								{
									_project.Variables["txStatus"].Value = "";
									logString = $"[{chainRPC} {hash}] not found";
								}
							}
						}
						catch (HttpException ex)
						{
							_project.SendErrorToLog($"Ошибка запроса (raw): {ex.Message}");
							Thread.Sleep(2000);
							continue;
						}

						Loggers.W3Debug(_project, logString);
						Thread.Sleep(3000); // Задержка перед следующей итерацией
					}
				}
			}

	}
	public static class Leaf
	{
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
	public class X
	{
		private readonly IZennoPosterProjectModel _project;
		private readonly Instance _instance;
		private readonly bool _log;
		public X(IZennoPosterProjectModel project, Instance instance, bool log = false)
		{
			_project = project;
			_instance = instance;
			_log = log;
		}
        private void XcredsFromDb(string tableName ="twitter", string schemaName = "accounts", bool log = false)
        {
            
			log = _project.Variables["debug"].Value == "True";
			string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
			var q = $@"SELECT status, token, login, password, code2fa, emailLogin, emailPass FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value};";
			var resp = SQL.W3Query(_project,q,log);
			   
            string[] twitterData = resp.Split('|');
            _project.Variables["twitterSTATUS"].Value = twitterData[0].Trim();
            _project.Variables["twitterTOKEN"].Value = twitterData[1].Trim();
            _project.Variables["twitterLOGIN"].Value = twitterData[2].Trim();
            _project.Variables["twitterPASSWORD"].Value = twitterData[3].Trim();
            _project.Variables["twitterCODE2FA"].Value = twitterData[4].Trim();
            _project.Variables["twitterEMAIL"].Value = twitterData[5].Trim();
            _project.Variables["twitterEMAIL_PASSWORD"].Value = twitterData[6].Trim();			
        } 
        private void XupdateDb(string toUpd, bool log = false)
		{
			string tableName ="twitter"; string schemaName = "accounts";
			log = _project.Variables["debug"].Value == "True";
			string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
			if (log) Loggers.l0g(_project,toUpd);
			var Q = $@"UPDATE {table} SET {toUpd.Trim().TrimEnd(',')}, last = '{Time.Now("short")}' WHERE acc0 = {_project.Variables["acc0"].Value};";
			SQL.W3Query(_project,Q,log); 
		}
		private string XcheckState(bool log = false)
		{
			log = _project.Variables["debug"].Value == "True";
			DateTime start = DateTime.Now;
			DateTime deadline = DateTime.Now.AddSeconds(60);
			string login = _project.Variables["twitterLOGIN"].Value; 
			_instance.ActiveTab.Navigate($"https://x.com/{login}", "");
			var status = "";

			while (string.IsNullOrEmpty(status))
			{
				Thread.Sleep(5000);
				if (log) Loggers.l0g(_project,$"{DateTime.Now- start}s check... URLNow:[{_instance.ActiveTab.URL}]" );
				if (DateTime.Now > deadline) Loggers.l0g(_project,"!W TwitterGetStatus timeout",thr0w:true);

				else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Caution:\s+This\s+account\s+is\s+temporarily\s+restricted", "regexp", 0).IsVoid) 
					status = "restricted";
				else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Account\s+suspended\s+X\s+suspends\s+accounts\s+which\s+violate\s+the\s+X\s+Rules", "regexp", 0).IsVoid)
					status = "suspended";
				else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", @"Log\ in", "regexp", 0).IsVoid || !_instance.ActiveTab.FindElementByAttribute("a", "data-testid", "loginButton", "regexp", 0).IsVoid)
					status = "login";

				else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", "erify\\ your\\ email\\ address", "regexp", 0).IsVoid || 
					!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "We\\ sent\\ your\\ verification\\ code.", "regexp", 0).IsVoid)
						status = "emailCapcha";
				else if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "SideNav_AccountSwitcher_Button", "regexp", 0).IsVoid)
				{
					var check = _instance.ActiveTab.FindElementByAttribute("button", "data-testid", "SideNav_AccountSwitcher_Button", "regexp", 0).FirstChild.FirstChild.GetAttribute("data-testid");
					if (check == $"UserAvatar-Container-{login}") status = "ok";
					else {
						status = "mixed";
						Loggers.l0g(_project,$"!W {status}. Detected  [{check}] instead [UserAvatar-Container-{login}] {DateTime.Now- start}" );
					}
				}
				else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Something\\ went\\ wrong.\\ Try\\ reloading.", "regexp", 0).IsVoid)
				{	
					_instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");
					//Thread.Sleep(5000);
					continue;
				}
			}
			if (log) Loggers.l0g(_project,$"{status} {DateTime.Now- start}" );
			return status;
		}
		private void XsetToken()
		{
			var token = _project.Variables["twitterTOKEN"].Value; 
			string jsCode = _project.ExecuteMacro($"document.cookie = \"auth_token={token}; domain=.x.com; path=/; expires=${DateTimeOffset.UtcNow.AddYears(1).ToString("R")}; Secure\";\r\nwindow.location.replace(\"https://x.com\")");
			_instance.ActiveTab.MainDocument.EvaluateScript(jsCode);
		}
		private string XgetToken()
		{
			var cook = new C00kies(_project, _instance); 
			var cookJson= cook.c00kies(".");
			JArray toParse = JArray.Parse(cookJson);
			int i = 0; var token = "";
			while (token == "")
			{
				if (toParse[i]["name"].ToString() == "auth_token") token = toParse[i]["value"].ToString();
				i++;
			}
			_project.Variables["twitterTOKEN"].Value = token;
			Db.TwitterTokenUpdate(_project);
			return token;
		}
		private string Xlogin()
		{
			DateTime deadline = DateTime.Now.AddSeconds(60);
			var status = "";
			var login = _project.Variables["twitterLOGIN"].Value;
			
			_instance.ActiveTab.Navigate("https://x.com/", "");Thread.Sleep(2000);
			_instance.LMB(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0),deadline:1,thr0w:false);
			_instance.LMB(("button", "data-testid", "xMigrationBottomBar", "regexp", 0),deadline:0,thr0w:false);
			_instance.LMB(("a", "data-testid", "login", "regexp", 0));
			_instance.SetHe(("input:text", "autocomplete", "username", "text", 0),login,deadline:30);
			_instance.LMB(("span", "innertext", "Next", "regexp", 1),"clickOut");
			
			if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Sorry, we could not find your account')]", 0).IsVoid) return "NotFound"; 

			_instance.WaitSetValue(() => 
				_instance.ActiveTab.GetDocumentByAddress("0").FindElementByName("password"),_project.Variables["twitterPASSWORD"].Value);
			
			_instance.LMB(("button", "data-testid", "LoginForm_Login_Button", "regexp", 0),"clickOut");

			if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Wrong password!')]", 0).IsVoid) return "WrongPass";

			var codeOTP = OTP.Offline(_project.Variables["twitterCODE2FA"].Value); 
			_instance.WaitSetValue(() => 
				_instance.ActiveTab.GetDocumentByAddress("0").FindElementByName("text"),codeOTP);
			
			_instance.LMB(("span", "innertext", "Next", "regexp", 1),"clickOut");
			
			if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Your account is suspended')]", 0).IsVoid) return "Suspended";
			if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Oops,\\ something\\ went\\ wrong.\\ Please\\ try\\ again\\ later.", "regexp", 0).IsVoid) return "SomethingWentWrong";
			if (! _instance.ActiveTab.FindElementByAttribute("*", "innertext", "Suspicious\\ login\\ prevented", "regexp", 0).IsVoid)return "SuspiciousLogin";
			
			_instance.LMB(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0),deadline:1,thr0w:false);
			_instance.LMB(("button", "data-testid", "xMigrationBottomBar", "regexp", 0),deadline:0,thr0w:false);
			XgetToken();
			return "ok";
		}
		public string Xload(bool log = false)
		{
			XcredsFromDb(log:log);
			bool tokenUsed = false;
			DateTime deadline = DateTime.Now.AddSeconds(60);
			check:

			if (DateTime.Now > deadline) Loggers.l0g(_project,"!W Xload timeout",thr0w:true);
			var status = XcheckState(log:true);

			//if (status == "ok") return status;
			if (status == "login" && !tokenUsed) 
			{
				XsetToken();
				tokenUsed = true;
				Thread.Sleep(3000);
			}
			else if (status == "login" && tokenUsed) 
			{
				var login = Xlogin();
				if (log) Loggers.l0g(_project,login);
				Thread.Sleep(3000);
			}
			if (status == "restricted" || status == "suspended" || status == "emailCapcha" || status == "mixed" || status == "ok")
			{
			  XupdateDb($"status = '{status}'");
			  return status;
			}
			if (log) Loggers.l0g(_project,status);
			goto check;	
		}

	}
	public class Discord
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly bool _log;
        public Discord(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _log = log;
        }
        private void DScredsFromDb(string tableName ="discord", string schemaName = "accounts", bool log = false)
        {
            
            log = _project.Variables["debug"].Value == "True";
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = SQL.W3Query(_project,$@"SELECT status, token, login, password, code2FA, username, servers FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value};");
            string[] discordData = resp.Split('|');
            _project.Variables["discordSTATUS"].Value = discordData[0].Trim();
            _project.Variables["discordTOKEN"].Value = discordData[1].Trim();
            _project.Variables["discordLOGIN"].Value = discordData[2].Trim();
            _project.Variables["discordPASSWORD"].Value = discordData[3].Trim();
            _project.Variables["discord2FACODE"].Value = discordData[4].Trim();
            _project.Variables["discordUSERNAME"].Value = discordData[5].Trim();
            _project.Variables["discordSERVERS"].Value = discordData[6].Trim();
        } 
        public void DSupdateDb(string toUpd, bool log = false)
        {
            string tableName ="discord"; string schemaName = "accounts";
            log = _project.Variables["debug"].Value == "True";
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            if (log) Loggers.l0g(_project,toUpd);
            var Q = $@"UPDATE {table} SET {toUpd.Trim().TrimEnd(',')}, last = '{Time.Now("short")}' WHERE acc0 = {_project.Variables["acc0"].Value};";
            SQL.W3Query(_project,Q,log); 
        }
        private void DSsetToken()
        {
            var jsCode = "function login(token) {\r\n    setInterval(() => {\r\n        document.body.appendChild(document.createElement `iframe`).contentWindow.localStorage.token = `\"${token}\"`\r\n    }, 50);\r\n    setTimeout(() => {\r\n        location.reload();\r\n    }, 1000);\r\n}\r\n    login(\'discordTOKEN\');\r\n".Replace("discordTOKEN",_project.Variables["discordTOKEN"].Value);
            _instance.ActiveTab.MainDocument.EvaluateScript(jsCode);
        }
        private string DSgetToken()
        {
            var token = _instance.ActiveTab.MainDocument.EvaluateScript("return (webpackChunkdiscord_app.push([[\'\'],{},e=>{m=[];for(let c in e.c)m.push(e.c[c])}]),m).find(m=>m?.exports?.default?.getToken!== void 0).exports.default.getToken();\r\n");
            return token;
        }
        private string DSlogin()
        {
            _project.SendInfoToLog("DLogin");
            DateTime deadline = DateTime.Now.AddSeconds(60);
            _instance.CloseExtraTabs();
            _instance.SetHe(("input:text", "aria-label", "Email or Phone Number", "text", 0),_project.Variables["discordLOGIN"].Value);
            _instance.SetHe(("input:password", "aria-label", "Password", "text", 0),_project.Variables["discordPASSWORD"].Value);
            _instance.LMB(("button", "type", "submit", "regexp", 0));

            while (_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Are\\ you\\ human\\?", "regexp", 0).IsVoid && 
                _instance.ActiveTab.FindElementByAttribute("input:text", "autocomplete", "one-time-code", "regexp", 0).IsVoid) Thread.Sleep(1000);

            if (!_instance.ActiveTab.FindElementByAttribute("div", "innertext", "Are\\ you\\ human\\?", "regexp", 0).IsVoid){
                if ((_project.Variables["humanNear"].Value) != "True") return "capcha";
                else _instance.WaitForUserAction(100, "dsCap");
            }
            _instance.SetHe(("input:text", "autocomplete", "one-time-code", "regexp", 0),OTP.Offline(_project.Variables["discord2FACODE"].Value));
            _instance.LMB(("button", "type", "submit", "regexp", 0));
            Thread.Sleep(3000);	
            return "ok";
        }
        public string DSload(bool log = false)
        {
            DScredsFromDb();

            string state = null;
            var emu = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;
            bool tokenUsed = false;
            _instance.ActiveTab.Navigate("https://discord.com/channels/@me", "");

            start:
            state = null;
            while (string.IsNullOrEmpty(state))
            {
                _instance.LMB(("button", "innertext", "Continue\\ in\\ Browser", "regexp", 0),thr0w:false);
                if (!_instance.ActiveTab.FindElementByAttribute("input:text", "aria-label", "Email or Phone Number", "text", 0).IsVoid)state = "login";	
                if (!_instance.ActiveTab.FindElementByAttribute("section", "aria-label", "User\\ area", "regexp", 0).IsVoid)state = "logged";	
            }

            Loggers.l0g(_project,state);


            if (state == "login" && !tokenUsed){
                DSsetToken();
                tokenUsed = true;
                //Thread.Sleep(5000);					
                goto start;
            }

            else if (state == "login" && tokenUsed){
                var login = DSlogin();
                if (login == "ok"){
                    Thread.Sleep(5000);
                    goto start;		
                }
                else if (login == "capcha") 
                    Loggers.l0g(_project,"!W capcha");
                    _instance.UseFullMouseEmulation = emu;	
                    state = "capcha";
            }

            else if (state == "logged"){
                state = _instance.ActiveTab.FindElementByAttribute("div", "class", "avatarWrapper__", "regexp", 0).FirstChild.GetAttribute("aria-label");
                Loggers.l0g(_project,state);
                var token = DSgetToken();
                DSupdateDb ($"token = '{token}', status = 'ok'");
                _instance.UseFullMouseEmulation = emu;
            }
            return state;

        }
        public string DSservers()
        {
            _instance.UseFullMouseEmulation = true;
            var folders = new List<HtmlElement>();
            var servers = new List<string>();
            var list = _instance.ActiveTab.FindElementByAttribute("div", "aria-label", "Servers", "regexp", 0).GetChildren(false).ToList();
            foreach (HtmlElement item in list)
            {
                
                if (item.GetAttribute("class").Contains("listItem")) 
                {
                    var server = item.FindChildByTag("div",1).FirstChild.GetAttribute("data-dnd-name");
                    servers.Add(server);
                }
                
                if (item.GetAttribute("class").Contains("wrapper")) 
                {
                    _instance.WaitClick(() => item);
                    var FolderServer = item.FindChildByTag("ul",0).GetChildren(false).ToList();
                    //_project.SendInfoToLog(FolderServer.Count.ToString());
                    foreach(HtmlElement itemInFolder in FolderServer)
                    {
                        var server = itemInFolder.FindChildByTag("div",1).FirstChild.GetAttribute("data-dnd-name");
                        servers.Add(server);
                    }
                }

            }

            string result = string.Join(" | ",servers);
            DSupdateDb ($"servers = '{result}'");
            //_project.SendInfoToLog(servers.Count.ToString());
            //_project.SendInfoToLog(string.Join(" | ",servers));
            return result;
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
					Loggers.l0g (project,$"!W {currentAcc} is InCorrect. MustBe {project.Variables["googleLOGIN"].Value}");
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
					Loggers.l0g (project,$"!W {userContainer} is Wrong. MustBe {project.Variables["googleLOGIN"].Value}");
					status = "wrong";	
					continue;
				}
			}
			catch
			{
				Loggers.l0g(project,$"no loggined Accounts detected");	
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
				
				if (status == "capcha")
				{
					var table = "";
					if (project.Variables["DBmode"].Value == "SQLite") table = $"accGoogle";
					else if (project.Variables["DBmode"].Value == "PostgreSQL") table = $"accounts.google";					 
					SQL.W3Query(project,$@"UPDATE {table} SET status = 'CAPCHA', cooldown = {Time.cd(24 * 60)}, last = '{DateTime.Now.ToString("MM/dd-HH:mm")}' WHERE acc0 = {project.Variables["acc0"].Value};",true);	
					throw new Exception("CAPCHA");
				} 
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
						Loggers.l0g(project,$"Signed Out acc detected [{userContainer}]");	
						instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "-1", "regexp", 0));
					}
					else
					{
						instance.CloseAllTabs();
						instance.ClearCookie("google.com");
						instance.ClearCookie("google.com");
						//instance.SetCookiesFromDB(project);
						Loggers.l0g (project,$"!W {userContainer} is Wrong. MustBe {project.Variables["googleLOGIN"].Value}");
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
					instance.WaitSetValue(() => instance.ActiveTab.FindElementById("totpPin"),OTP.Offline(project.Variables["google2FACODE"].Value));	
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
            //bool log = true;
            try
            {
                var userContainer = instance.ReadHe(("div", "data-authuser", "0", "regexp", 0));
                if (log) Loggers.l0g(project,$"container:{userContainer} catched");	
                if (userContainer.IndexOf(project.Variables["googleLOGIN"].Value, StringComparison.OrdinalIgnoreCase) >= 0)	
                {
                    if (log) Loggers.l0g(project,$"correct user found: {project.Variables["googleLOGIN"].Value}");	
                    instance.LMB(("div", "data-authuser", "0", "regexp", 0),delay:3);
                    Thread.Sleep(5000);
                    if (!instance.ActiveTab.FindElementByAttribute("div", "data-authuser", "0", "regexp", 0).IsVoid)
                    {
                        while (true) instance.LMB(("div", "data-authuser", "0", "regexp", 0),"clickOut",deadline:5, delay:3);
                    }
                    try
                    {
                        instance.LMB(("button", "innertext", "Continue", "regexp", 0),deadline:2,delay:1);
                        return "SUCCESS with continue";
                    }
                    catch
                    {
                        return "SUCCESS. without confirmation";
                    }					
                }
                else
                {
                    Loggers.l0g(project,$"!Wrong account [{userContainer}]. Expected: {project.Variables["googleLOGIN"].Value}. Cleaning");
                    instance.CloseAllTabs();
                    instance.ClearCookie("google.com");
                    instance.ClearCookie("google.com");
                    instance.SetCookiesFromDB(project);
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
			catch (Exception ex){Loggers.l0g(project,$"!W:{ex.Message}",thr0w:true);}

			Loggers.l0g(project,$"[APPROVE] {contract} for spender {spender}...");
            return project.Variables["blockchainHash"].Value;

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
			catch (Exception ex){Loggers.l0g(project,$"!W:{ex.Message}",thr0w:true);}


			Loggers.l0g(project,$"[APPROVE] {contract} for spender {spender}...");
            return project.Variables["blockchainHash"].Value;
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
			catch (Exception ex){Loggers.l0g(project,$"!W:{ex.Message}",thr0w:true);}
		    
		    Loggers.l0g(project,$"[WRAP] {value} native to {contract}...");
           return project.Variables["blockchainHash"].Value;
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
	public class OKXApi
	{
		private readonly IZennoPosterProjectModel _project;
        private readonly string[] _apiKeys;
        

		public OKXApi(IZennoPosterProjectModel project)
		{
			_project = project;
            _apiKeys = okxKeys();
		}
		public string[] okxKeys()  
		{
			string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : null) + "settings";
			var key = SQL.W3Query(_project,$"SELECT value FROM {table} WHERE var = 'okx_apikey';",true);
			var secret = SQL.W3Query(_project,$"SELECT value FROM {table} WHERE var = 'okx_secret';");
			var passphrase = SQL.W3Query(_project,$"SELECT value FROM {table} WHERE var = 'okx_passphrase';");
			string[] result = new string[] {key,secret,passphrase};
			return result;
		}
		private string MapNetwork(string chain, bool log)
		{
			if (log) Loggers.l0g(_project, "Mapping network: " + chain);
            chain = chain.ToLower();
			switch (chain)
			{
				case "arbitrum": return "Arbitrum One";
				case "ethereum": return "ERC20";
                case "base": return "Base";
				case "bsc": return "BSC";
				case "avalanche": return "Avalanche C-Chain";
				case "polygon": return "Polygon";
				case "optimism": return "Optimism";
				case "trc20": return "TRC20";
				case "zksync": return "zkSync Era";
				case "aptos": return "Aptos";
				default:
					if (log) Loggers.l0g(_project, "Unsupported network: " + chain);
					throw new ArgumentException("Unsupported network: " + chain);
			}
		}
		private string CalculateHmacSha256ToBaseSignature(string message, string key)
		{
			var keyBytes = Encoding.UTF8.GetBytes(key);
			var messageBytes = Encoding.UTF8.GetBytes(message);
			using (var hmacSha256 = new HMACSHA256(keyBytes))
			{
				var hashBytes = hmacSha256.ComputeHash(messageBytes);
				return Convert.ToBase64String(hashBytes);
			}
		}
		
		private string OKXPost(string url, object body, string proxy = null, bool log = false)
		{

			//var ApiKeys = okxKeys();
			string apiKey = _apiKeys[0];
			string secretKey = _apiKeys[1];
			string passphrase = _apiKeys[2];

			var jsonBody = JsonConvert.SerializeObject(body);
			string timestamp =  DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            _project.SendInfoToLog(jsonBody);
			// Prepare signature
			//string url = "/api/v5/asset/withdrawal";
			string message = timestamp + "POST" + url + jsonBody;
			string signature = CalculateHmacSha256ToBaseSignature(message, secretKey);

			// Send HTTP request
			var result = ZennoPoster.HttpPost(
				"https://www.okx.com" + url,
				jsonBody,
				"application/json",
				proxy,
				"UTF-8",
				ResponceType.BodyOnly,
				10000,
				"",
				_project.Profile.UserAgent,
				true,
				5,
				new string[]
				{
					"Content-Type: application/json",
					"OK-ACCESS-KEY: " + apiKey,
					"OK-ACCESS-SIGN: " + signature,
					"OK-ACCESS-TIMESTAMP: " + timestamp,
					"OK-ACCESS-PASSPHRASE: " + passphrase
				},
				"",
				false//,
				//false,
				//_project.Profile.CookieContainer
			);
			_project.Json.FromString(result);
			if (log) Loggers.l0g(_project, "Received response: " + result);
			return result;			
		}
		private string OKXGet(string url, string proxy = null, bool log = false)
		{
			//var ApiKeys = okxKeys();
			string apiKey = _apiKeys[0];
			string secretKey = _apiKeys[1];
			string passphrase = _apiKeys[2];


			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");			
			string message = timestamp + "GET" + url;
			string signature = CalculateHmacSha256ToBaseSignature(message, secretKey);

			var jsonResponse = ZennoPoster.HttpGet(			
				"https://www.okx.com" + url,
				proxy,
				"UTF-8",
				//ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.
				ResponceType.BodyOnly,
				10000,
				"",
				_project.Profile.UserAgent,
				true,
				5,
				new string[]
				{
					"Content-Type: application/json",
					"OK-ACCESS-KEY: " + apiKey,
					"OK-ACCESS-SIGN: " + signature,
					"OK-ACCESS-TIMESTAMP: " + timestamp,
					"OK-ACCESS-PASSPHRASE: " + passphrase
				},
				"",
				false
			);

			if (log) Loggers.l0g(_project, "OKX response:\n" + jsonResponse);
			_project.Json.FromString(jsonResponse);
			return jsonResponse;
		}

		public List<string> OKXGetSubAccs(string proxy = null, bool log = false)
		{
			var jsonResponse = OKXGet("/api/v5/users/subaccount/list",log:log);
			
			var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
			string msg = response.msg;
			string code = response.code;
			var subsList = new List<string>();

			if (code != "0") throw new Exception("OKXGetSubMax: Err [{code}]; Сообщение [{msg}]");
			else
			{	
				var dataArray = response.data;
				if (dataArray != null)
				{
					foreach (var item in dataArray)
					{
						string subAcct = item.subAcct;                   // "subName"
						string label = item.label;             
						subsList.Add($"{subAcct}");
						if (log) Loggers.l0g(_project, $"found: {subAcct}:{label}");
					}
				}

			}
			return subsList;
		}
		public List<string> OKXGetSubMax(string accName, string proxy = null, bool log = false)
		{
			var jsonResponse = OKXGet($"/api/v5/account/subaccount/max-withdrawal?subAcct={accName}",log:log);
			
			var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
			string msg = response.msg;
			string code = response.code;
			var balanceList = new List<string>();
			
			if (code != "0") throw new Exception("OKXGetSubMax: Err [{code}]; Сообщение [{msg}]");
			else
			{
				var dataArray = response.data;
				if (dataArray != null)
				{
					foreach (var item in dataArray)
					{
						string ccy = item.ccy;                   // "EGLD"
						string maxWd = item.maxWd;               // "0.22193226"
						balanceList.Add($"{ccy}:{maxWd}");
						Loggers.l0g(_project, $"Currency: {ccy}, Max Withdrawal: {maxWd}");
					}
				}
			}
			return balanceList;
		}
		public List<string> OKXGetSubTrading(string accName, string proxy = null, bool log = false)
		{
			var jsonResponse = OKXGet($"/api/v5/account/subaccount/balances?subAcct={accName}",log:log);
			
			var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
			string msg = response.msg;
			string code = response.code;
			var balanceList = new List<string>();
			
			if (code != "0") throw new Exception("OKXGetSubMax: Err [{code}]; Сообщение [{msg}]");
			else
			{
				var dataArray = response.data;
				if (dataArray != null)
				{
					foreach (var item in dataArray)
					{
						string adjEq = item.adjEq;                   // "EGLD"

						balanceList.Add($"{adjEq}");
						Loggers.l0g(_project, $"adjEq: {adjEq}");
					}
				}
			}
			return balanceList;
		}
		public List<string> OKXGetSubFunding(string accName, string proxy = null, bool log = false)
		{
			var jsonResponse = OKXGet($"/api/v5/asset/subaccount/balances?subAcct={accName}",log:log);
			
			var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
			string msg = response.msg;
			string code = response.code;
			var balanceList = new List<string>();
			
			if (code != "0") throw new Exception("Err [{code}]; Сообщение [{msg}]");
			else
			{
				var dataArray = response.data;
				if (dataArray != null)
				{
					foreach (var item in dataArray)
					{
						string ccy = item.ccy;
						string availBal = item.availBal;                    // "EGLD"
						balanceList.Add($"{ccy}:{availBal}");
						Loggers.l0g(_project, $"{ccy}:{availBal}");
					}
				}
			}
			return balanceList;
		}
		public List<string> OKXGetSubsBal(string proxy = null, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var subs = OKXGetSubAccs();
            _project.SendInfoToLog(subs.Count.ToString());
            
            var balanceList = new List<string>();
            
            foreach(string sub in subs){
                
                var balsFunding = OKXGetSubFunding(sub,log:true);
                foreach (string bal in balsFunding)
                {
                    if(string.IsNullOrEmpty(bal)) continue;
                    _project.SendInfoToLog($"balsFunding [{bal}]");	
                    string ccy = bal.Split(':')[0]?.ToString();
                    string maxWd = bal.Split(':')[1]?.ToString();
                    if(!string.IsNullOrEmpty(maxWd))
                        try{
                            if(double.Parse(maxWd) > 0)	{	
                                balanceList.Add($"{sub}:{ccy}:{maxWd}");
                                Thread.Sleep(1000);
                            }
                        }
                        catch{
                            _project.SendInfoToLog($"failed to add [{maxWd}]$[{ccy}] from [{sub}] to main");
                        }		
                }
                
                var balsTrading = OKXGetSubMax(sub,log:true);
                foreach (string bal in balsTrading)
                {
                    _project.SendInfoToLog($"balsTrading [{bal}]");	
                    string ccy = bal.Split(':')[0]?.ToString();
                    string maxWd = bal.Split(':')[1]?.ToString();
                    if(!string.IsNullOrEmpty(maxWd))
                        try{
                            if(double.Parse(maxWd) > 0)	{	
                                balanceList.Add($"{sub}:{ccy}:{maxWd}");
                                Thread.Sleep(1000);
                            }
                        }
                        catch{
                            _project.SendInfoToLog($"failed to add [{maxWd}]$[{ccy}] from [{sub}] to main");
                        }		
                }	
            }
            return balanceList;
        }

		public void OKXWithdraw( string toAddress, string currency, string chain, decimal amount, decimal fee, string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			string network = MapNetwork(chain, log);
			var body = new
			{
				amt = amount.ToString("G", CultureInfo.InvariantCulture),
				fee = fee.ToString("G", CultureInfo.InvariantCulture),
				dest = "4",
				ccy = currency,
				chain = currency + "-" + network,
				toAddr = toAddress
			};
			var jsonResponse = OKXPost("/api/v5/asset/withdrawal",body, proxy, log);

			if (log) Loggers.l0g(_project, $"processing response {jsonResponse} ");

			var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
			string msg = response.msg;
			string code = response.code;

			if (code != "0") throw new Exception("Err [{code}]; Сообщение [{msg}]");
			else
			{
				if (log) Loggers.l0g(_project, $"Refueled {toAddress} for {amount}");
			}
			_project.Json.FromString(jsonResponse);
		}
		private void OKXSubToMain( string fromSubName, string currency, decimal amount, string accountType = "6", string proxy = null, bool log = false)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			
			string strAmount = amount.ToString("G", CultureInfo.InvariantCulture);
			
			var body = new
			{
				ccy = currency,
				type = "2",
				amt = strAmount,
				from = accountType, //18 tradinng |6 funding
				to = "6",
				subAcct = fromSubName
			};
			var jsonResponse = OKXPost("/api/v5/asset/transfer",body, proxy, log);

			if (log) Loggers.l0g(_project, $"processing response {jsonResponse} ");

			var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
			string msg = response.msg;
			string code = response.code;

			if (code != "0") throw new Exception("Err [{code}]; Сообщение [{msg}] amt:[{strAmount}] ccy:[{currency}]");
			else
			{
				if (log) Loggers.l0g(_project, jsonResponse);
			}
			
		}
		public void OKXCreateSub(string subName, string accountType = "1", string proxy = null, bool log = false)
		{
			var body = new
			{
				subAcct = subName,
				type = accountType
			};
			var jsonResponse = OKXPost("/api/v5/users/subaccount/create-subaccount",body, proxy, log);

			if (log) Loggers.l0g(_project, $"processing response {jsonResponse} ");

			var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
			string msg = response.msg;
			string code = response.code;

			if (code != "0") throw new Exception($"Err [{code}]; Сообщение [{msg}]");
			else
			{
				if (log) Loggers.l0g(_project, jsonResponse);
			}
			
		}

		public void OKXDrainSubs()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			var subs = OKXGetSubAccs();
			_project.SendInfoToLog(subs.Count.ToString());

			foreach(string sub in subs){
				
				var balsFunding = OKXGetSubFunding(sub,log:true);
				foreach (string bal in balsFunding)
				{
					if(string.IsNullOrEmpty(bal)) continue;
					_project.SendInfoToLog($"balsFunding [{bal}]");	
					string ccy = bal.Split(':')[0]?.ToString();
					string maxWd = bal.Split(':')[1]?.ToString();
					if(!string.IsNullOrEmpty(maxWd))
						try{
							if(decimal.Parse(maxWd) > 0)	{
								decimal amount = decimal.Parse(maxWd);
								_project.SendInfoToLog($"sending {maxWd}${ccy} from {sub} to main");		
								OKXSubToMain(sub,ccy,amount,"6",log:true);
								Thread.Sleep(500);
							}
						}
						catch{
							_project.SendInfoToLog($"failed to send [{maxWd}]$[{ccy}] from [{sub}] to main");
						}		
				}
				
				var balsTrading = OKXGetSubMax(sub,log:true);
				foreach (string bal in balsTrading)
				{
					_project.SendInfoToLog($"balsTrading [{bal}]");	
					string ccy = bal.Split(':')[0]?.ToString();
					string maxWd = bal.Split(':')[1]?.ToString();
					if(!string.IsNullOrEmpty(maxWd))
						try{
							if(decimal.Parse(maxWd) > 0)	{
								decimal amount = decimal.Parse(maxWd);
								_project.SendInfoToLog($"sending {maxWd}${ccy} from {sub} to main");		
								OKXSubToMain(sub,ccy,amount,"18",log:true);
							}
						}
						catch{
							_project.SendInfoToLog($"failed to send [{maxWd}]$[{ccy}] from [{sub}] to main");
						}		
				}	
			}


		}
		public void OKXAddMaxSubs()
		{
			int i = 1;
			while (true)
			{
				
				try{
					OKXCreateSub($"sub{i}t{Time.Now("unix")}");
					i++;
					Thread.Sleep(1500);
				}
				catch{
					_project.SendInfoToLog($"{i} subs added");
					break;
				}
			}
		}
        public T OKXPrice<T>(string pair, string proxy = null, bool log = false)
        {
   				var jsonResponse = OKXGet($"/api/v5/market/ticker?instId={pair}",log:log);
	            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;			
				var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
				string msg = response.msg;
				string code = response.code;
				string last = null;

                if (code != "0") throw new Exception("Err [{code}]; Сообщение [{msg}]");
                else
                {
                    var dataArray = response.data;
                    if (dataArray != null)
                    {
                        foreach (var item in dataArray)
                        {
                            string lastPrice = item.last;
                            if (!string.IsNullOrEmpty(lastPrice)){
                                last = lastPrice;
                                Loggers.l0g(_project, $"{pair}:{lastPrice}");
                                break;
                            }
                        }
                    }
                }
                decimal price = decimal.Parse(last);
                if (typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(price.ToString("0.##################"), typeof(T));
                return (T)Convert.ChangeType(price, typeof(T));      
        }
	}
	#endregion

	#region Wallets

	public class Wall3t
	{
		private readonly IZennoPosterProjectModel _project;
		private readonly Instance _instance;
		private readonly L0g _log;
		private readonly bool _logShow;
        private readonly string _pass;
		private readonly Sql _sql;

		public Wall3t(IZennoPosterProjectModel project, Instance instance, bool log = false)
		{
			_project = project;
			_instance = instance;
            _log = new L0g(_project);
			_logShow = log;
			_sql = new Sql(_project);
            _pass = SAFU.HWPass(_project);
		}
		public void Switch(string toUse = "", bool log = false)
		{
			if (log)Loggers.l0g(_project,$"switching extentions  {toUse}");
			var em = _instance.UseFullMouseEmulation;

			int i = 0;string extName = "";string outerHtml = "";string extId = "";string extStatus = "enabled";
			string path = $"{_project.Path}.crx\\One-Click-Extensions-Manager.crx";
			var managerId = "pbgjpgbpljobkekbhnnmlikbbfhbhmem";
			
			
			var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
			if (!extListString.Contains(managerId)) 
			{
				if (log)Loggers.l0g(_project,"Ext Manager Install");
				_instance.InstallCrxExtension(path);
				
			}
			
			while (_instance.ActiveTab.URL != "chrome-extension://pbgjpgbpljobkekbhnnmlikbbfhbhmem/index.html"){
				_instance.ActiveTab.Navigate("chrome-extension://pbgjpgbpljobkekbhnnmlikbbfhbhmem/index.html", "");
				_instance.CloseExtraTabs();
				if (log)Loggers.l0g(_project,$"URL is correct {_instance.ActiveTab.URL}");
			}

			while (!_instance.ActiveTab.FindElementByAttribute("button", "class", "ext-name", "regexp", i).IsVoid)
			{
				if (log)Loggers.l0g(_project,$"Cheking ext no {i}");
				extName = Regex.Replace(_instance.ActiveTab.FindElementByAttribute("button", "class", "ext-name", "regexp", i).GetAttribute("innertext"), @" Wallet", "");
			    outerHtml = _instance.ActiveTab.FindElementByAttribute("li", "class", "ext\\ type-normal", "regexp", i).GetAttribute("outerhtml");
			    extId = Regex.Match(outerHtml, @"extension-icon/([a-z0-9]+)").Groups[1].Value;
			    if (outerHtml.Contains("disabled")) extStatus = "disabled";
				if (toUse.Contains(extName) && extStatus == "disabled" || toUse.Contains(extId) && extStatus == "disabled" || !toUse.Contains(extName) && !toUse.Contains(extId) && extStatus == "enabled") 
				_instance.LMB(("button", "class", "ext-name", "regexp", i));
				i++;
			}
			
			_instance.CloseExtraTabs();
			_instance.UseFullMouseEmulation = em;
			if (log)Loggers.l0g(_project,$"Enabled  {toUse}");

		}
		public void WalLog(string tolog = "",  [CallerMemberName] string callerName = "", bool log = false)
		{	
			if (!_logShow && !log) return;
			var stackFrame = new System.Diagnostics.StackFrame(1); 
			var callingMethod = stackFrame.GetMethod();
			if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
			_log.Send( $"[ 💰  {callerName}] [{tolog}] ");
		}
		
        //MetaMask
		public void MMLaunch (string key = null)
		{
			var em = _instance.UseFullMouseEmulation;
			_instance.UseFullMouseEmulation = false;
			string address = "";
			bool skipCheck = false;
			var extId = "nkbihfbeogaeaoehlefnkodbefgpgknn";
			string path = $"{_project.Path}.crx\\MetaMask 11.16.0.crx";
			string sourse = "pkey"; //pkey | seed
			var password = SAFU.HWPass(_project);
			DateTime deadline = DateTime.Now.AddSeconds(60);

			install:
			var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
			if (!extListString.Contains(extId)) 
			{
				_instance.InstallCrxExtension(path);
				_instance.CloseExtraTabs();
			}
			_instance.ActiveTab.Navigate("chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html", "");
			check:
			string state = null; 

			while (string.IsNullOrEmpty(state))
			{

				if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-options-menu-button", "regexp", 0).IsVoid) state = "mainPage";
				else if (!_instance.ActiveTab.FindElementByAttribute("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0).IsVoid) state = "initPage";
				else if (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0).IsVoid) state = "passwordPage";

			}

			Loggers.l0g(_project,state);

			if (state == "initPage") 
			{
				MMimport();
				goto check;
			}

			if (state == "passwordPage") 
			{
				try {
					MMUnlock();
					goto check;
				}
				catch{
					goto install;
				}
			}

			if ( state == "mainPage") 
			{
				try {
					address = MMChkAddress();
				}
				catch{
					goto install;
				}
			}
			_instance.UseFullMouseEmulation = em;
			//return address;
		}
		public void MMimport (string key = null)
		{
			var password = SAFU.HWPass(_project);
			DateTime deadline = DateTime.Now.AddSeconds(60);
			
			string welcomeURL = $"chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html#onboarding/welcome"; 
			while (!_instance.ActiveTab.URL.Contains("#onboarding/welcome"))
			{
				if (DateTime.Now > deadline ) throw new Exception("timeout");
				_instance.CloseExtraTabs();
				_instance.ActiveTab.Navigate("chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html", "");
				Thread.Sleep(1000);
			}
			if (string.IsNullOrEmpty(key)) key = Db.KeyEVM(_project);
			
			
			_instance.LMB(("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0));
			_instance.LMB(("span", "innertext", "I\\ agree\\ to\\ MetaMask\'s\\ Terms\\ of\\ use", "regexp", 1));
			_instance.LMB(("button", "aria-label", "Close", "regexp", 0));
			_instance.LMB(("button", "data-testid", "onboarding-create-wallet", "regexp", 0));
			_instance.LMB(("button", "data-testid", "metametrics-no-thanks", "regexp", 0));
			_instance.SetHe(("input:password", "data-testid", "create-password-new", "regexp", 0),password);
			_instance.SetHe(("input:password", "data-testid", "create-password-confirm", "regexp", 0),password);
			_instance.LMB(("span", "innertext", "I\\ understand\\ that\\ MetaMask\\ cannot\\ recover\\ this\\ password\\ for\\ me.\\ Learn\\ more", "regexp", 0));
			_instance.LMB(("button", "data-testid", "create-password-wallet", "regexp", 0));
			_instance.LMB(("button", "data-testid", "secure-wallet-later", "regexp", 0));
			_instance.LMB(("label", "class", "skip-srp-backup-popover__label", "regexp", 0));
			_instance.LMB(("button", "data-testid", "skip-srp-backup", "regexp", 0));
			_instance.LMB(("button", "data-testid", "onboarding-complete-done", "regexp", 0));
			_instance.LMB(("button", "data-testid", "pin-extension-next", "regexp", 0));
			_instance.LMB(("button", "data-testid", "pin-extension-done", "regexp", 0));
			Thread.Sleep(1000); 
			while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid) 
			try{_instance.LMB(("button", "data-testid", "popover-close", "regexp", 0));}
			catch{_instance.LMB(("button", "innertext", "Got\\ it", "regexp", 0));}
				
			_instance.LMB(("button", "data-testid", "account-menu-icon", "regexp", 0));
			_instance.LMB(("button", "data-testid", "multichain-account-menu-popover-action-button", "regexp", 0));
			_instance.LMB(("span", "style", "mask-image:\\ url\\(\"./images/icons/import.svg\"\\);", "regexp", 0));
			_instance.WaitSetValue(() => _instance.ActiveTab.FindElementById("private-key-box"), key);
			_instance.LMB(("button", "data-testid", "import-account-confirm-button", "regexp", 0));
			Thread.Sleep(1000); 
		}
		public void MMUnlock (bool log = false)
		{
			var password = SAFU.HWPass(_project);
			_instance.WaitSetValue(() => _instance.ActiveTab.FindElementById("password"),password);
			_instance.LMB(("button", "data-testid", "unlock-submit", "regexp", 0));
			if (!_instance.ActiveTab.FindElementByAttribute("p", "innertext", "Incorrect password", "text", 0).IsVoid) 
			{
				_instance.CloseAllTabs(); 
				_instance.UninstallExtension("nkbihfbeogaeaoehlefnkodbefgpgknn"); 
				Loggers.l0g(_project,"! WrongPassword",thr0w:true);
			}

		}
		public string MMChkAddress (bool skipCheck = false)
		{
			while (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid) 
			try{_instance.LMB(("button", "data-testid", "popover-close", "regexp", 0));}
			catch{_instance.LMB(("button", "innertext", "Got\\ it", "regexp", 0));}
			_instance.LMB(("button", "data-testid", "account-options-menu-button", "regexp", 0));
			_instance.LMB(("button", "data-testid", "account-list-menu-details", "regexp", 0));
			string address = _instance.ReadHe(("button", "data-testid", "address-copy-button-text", "regexp", 0));
			
			if (!skipCheck)
				if(!String.Equals(address,_project.Variables["addressEvm"].Value,StringComparison.OrdinalIgnoreCase))
				{
					_instance.CloseAllTabs(); 
					_instance.UninstallExtension("nkbihfbeogaeaoehlefnkodbefgpgknn"); 
					Loggers.l0g(_project,"! WrongAddress",thr0w:true);
				}
			return address;
		}
		public string MMConfirm(bool log = false)
		{
			var me = _instance.UseFullMouseEmulation;
            _instance.UseFullMouseEmulation = false;
			DateTime urlChangeDeadline = DateTime.Now.AddSeconds(60);
			int attemptCount = 0;

			if (log)_project.SendInfoToLog("Waiting for MetaMask URL to appear...");
			while (!_instance.ActiveTab.URL.Contains("nkbihfbeogaeaoehlefnkodbefgpgknn"))
			{
				Thread.Sleep(1000); attemptCount++;
				if (log)_project.SendInfoToLog($"Attempt {attemptCount}: Current URL is {_instance.ActiveTab.URL}");
				if (attemptCount > 5)
				{
					if (log)_project.SendErrorToLog("Failed to load MetaMask URL within 6 seconds");
					throw new Exception("Timeout waiting for MetaMask URL");
				}
			}

			if (log)_project.SendInfoToLog($"{_instance.ActiveTab.URL} detected, pausing for 2 seconds...");
			Thread.Sleep(2000); 

			HtmlElement allert = _instance.ActiveTab.FindElementByAttribute("div", "class", "mm-box\\ mm-banner-base\\ mm-banner-alert\\ mm-banner-alert--severity-danger", "regexp", 0);
			HtmlElement simulation = _instance.ActiveTab.FindElementByAttribute("div", "data-testid", "simulation-details-layout", "regexp", 0);
			HtmlElement detail = _instance.ActiveTab.FindElementByAttribute("div", "class", "transaction-detail", "regexp", 0);

			if (log)_project.SendInfoToLog($"{Regex.Replace(simulation.GetAttribute("innertext").Trim(), @"\s+", " ")}");
			if (log)_project.SendInfoToLog($"{Regex.Replace(detail.GetAttribute("innertext").Trim(), @"\s+", " ")}");
			if (!allert.IsVoid) 
			{
						var error = Regex.Replace(allert.GetAttribute("innertext").Trim(), @"\s+", " ");
						while (!_instance.ActiveTab.FindElementByAttribute("button", "data-testid", "page-container-footer-cancel", "regexp", 0).IsVoid)
						{
							_instance.ActiveTab.Touch.SwipeBetween(600, 400, 600, 300);
							_instance.WaitClick(() =>  _instance.ActiveTab.FindElementByAttribute("button", "data-testid", "page-container-footer-cancel", "regexp", 0));
						}
                        
						Loggers.l0g(_project,error,thr0w:true);
			}
			if (log)_project.SendInfoToLog("Starting button click loop on MetaMask page...");
			while (_instance.ActiveTab.URL.Contains("nkbihfbeogaeaoehlefnkodbefgpgknn"))
			{
				if (DateTime.Now > urlChangeDeadline)
				{
					if (log)_project.SendErrorToLog("Operation timed out after 60 seconds");
					throw new Exception("Timeout exceeded while interacting with MetaMask");
				}
				try
				{
					if (log)_project.SendInfoToLog("Attempting to find and click the confirm button...");
					_instance.WaitClick(() => _instance.ActiveTab.FindElementByAttribute("button", "class", "button btn--rounded btn-primary", "regexp", 0), 3);
					if (log)_project.SendInfoToLog("Button clicked successfully");
					Thread.Sleep(2000);
				}
				catch (Exception ex)
				{
					if (log)_project.SendWarningToLog($"Failed to click button: {ex.Message}");
				}
			}
			if (log)_project.SendInfoToLog("MetaMask interaction completed, URL has changed");
			_instance.UseFullMouseEmulation = me;
			return "done";
		}

		//Rabby
		public void RBLaunch (bool log = false)
		{
			if (log)Loggers.l0g(_project,"RBInstall");
			var em = _instance.UseFullMouseEmulation;
			_instance.UseFullMouseEmulation = true;
			if (RBInstall ()) RBImport();
			else RBUnlock();
			_instance.CloseExtraTabs();
			_instance.UseFullMouseEmulation = em;

		}
		private bool RBInstall (bool log = false)
		{
			string path = $"{_project.Path}.crx\\Rabby0.93.24.crx";
			var extId = "acmacodkjbdgmoleebolmdjonilkdbch";
			var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
			if (!extListString.Contains(extId)) 
			{
				if (log)Loggers.l0g(_project,"RBInstall");
				_instance.InstallCrxExtension(path);
				return true;
			}
			return false;
		}
		private void RBImport (bool log = false)
		{
			if (log)Loggers.l0g(_project,"RBImport");
			_instance.LMB(("button", "innertext", "I\\ already\\ have\\ an\\ address", "regexp", 0));
			_instance.LMB(("img", "src", "chrome-extension://acmacodkjbdgmoleebolmdjonilkdbch/generated/svgs/d5409491e847b490e71191a99ddade8b.svg", "regexp", 0));
			var key = Db.KeyEVM(_project);
			_instance.WaitSetValue(() => _instance.ActiveTab.FindElementById("privateKey"),key);
			_instance.LMB(("button", "innertext", "Confirm", "regexp", 0));
			var password = SAFU.HWPass(_project);
			_instance.WaitSetValue(() => _instance.ActiveTab.FindElementById("password"),password);
			_instance.WaitSetValue(() => _instance.ActiveTab.FindElementById("confirmPassword"),password);
			_instance.LMB(("button", "innertext", "Confirm", "regexp", 0));
			_instance.LMB(("button", "innertext", "Get\\ Started", "regexp", 0));
		}
		private void RBUnlock (bool log = false)
		{
			if (log)Loggers.l0g(_project,"RBUnlock");
			_instance.ActiveTab.Navigate("chrome-extension://acmacodkjbdgmoleebolmdjonilkdbch/index.html#/unlock", "");
			var password = SAFU.HWPass(_project);
			_instance.UseFullMouseEmulation = true;
			unlock:
			if( _instance.ActiveTab.URL == "chrome-extension://acmacodkjbdgmoleebolmdjonilkdbch/offscreen.html") {
				_instance.ActiveTab.Close();
				_instance.ActiveTab.Navigate("chrome-extension://acmacodkjbdgmoleebolmdjonilkdbch/index.html#/unlock", "");
				goto unlock;
			}
			else
			{
				_instance.WaitSetValue(() => 	_instance.ActiveTab.FindElementById("password"),password);
				_instance.LMB(("button", "innertext", "Unlock", "regexp", 0));
			}
		}
		
        //BagPack
		public void BPLaunch (bool log = false)
		{
			if (log)Loggers.l0g(_project,"RBInstall");
			var em = _instance.UseFullMouseEmulation;
			_instance.UseFullMouseEmulation = false;
			if (BPInstall (log)) BPImport(log);
			else BPUnlock(log);
			BPCheck(log);
			_instance.CloseExtraTabs();
			_instance.UseFullMouseEmulation = em;
		}
		public bool BPInstall (bool log = false)
		{
			string path = $"{_project.Path}.crx\\Backpack0.10.94.crx";
			var extId = "aflkmfhebedbjioipglgcbcmnbpgliof";
			var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
			if (!extListString.Contains(extId)) 
			{
				if (log)Loggers.l0g(_project,"BPInstall");
				_instance.InstallCrxExtension(path);
				return true;
			}
			return false;
		}
		public bool BPImport (bool log = false)
		{
			if (log)Loggers.l0g(_project,"BPImport");
			var key = Db.KeySOL(_project);
			var password = SAFU.HWPass(_project);
			_instance.CloseExtraTabs();
			_instance.ActiveTab.Navigate("chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/options.html?onboarding=true", "");
			
			waitEl:
			if (!_instance.ActiveTab.FindElementByAttribute("p", "innertext", "Already\\ setup", "regexp", 0).IsVoid) return false;
			else if (!_instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ Wallet", "regexp", 0).IsVoid)
			{ 
				_instance.LMB(("button", "innertext", "Import\\ Wallet", "regexp", 0));
				_instance.LMB(("div", "class", "_dsp-flex\\ _ai-stretch\\ _fd-row\\ _fb-auto\\ _bxs-border-box\\ _pos-relative\\ _mih-0px\\ _miw-0px\\ _fs-0\\ _btc-889733467\\ _brc-889733467\\ _bbc-889733467\\ _blc-889733467\\ _w-10037\\ _pt-1316333121\\ _pr-1316333121\\ _pb-1316333121\\ _pl-1316333121\\ _gap-1316333121", "regexp", 0));
				_instance.LMB(("button", "innertext", "Import\\ private\\ key", "regexp", 0));
				_instance.SetHe(("textarea", "fulltagname", "textarea", "regexp", 0),key);
				_instance.LMB(("button", "innertext", "Import", "regexp", 0));
				_instance.SetHe(("input:password", "placeholder", "Password", "regexp", 0),password);
				_instance.SetHe(("input:password", "placeholder", "Confirm\\ Password", "regexp", 0),password);
				_instance.LMB(("input:checkbox", "class", "PrivateSwitchBase-input\\ ", "regexp", 0));
				_instance.LMB(("button", "innertext", "Next", "regexp", 0));
				_instance.LMB(("button", "innertext", "Open\\ Backpack", "regexp", 0));
				return true;
			}
			else goto waitEl;

		}
		public void BPUnlock (bool log = false)
		{
			if (log)Loggers.l0g(_project,$"[BackPack] unlocking");
			var password = SAFU.HWPass(_project);			
			if (_instance.ActiveTab.URL != "chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html")
			 _instance.ActiveTab.Navigate("chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html", "");
			_instance.CloseExtraTabs();
			try {
				_instance.SetHe(("input:password", "fulltagname", "input:password", "regexp", 0),password);
				_instance.LMB(("button", "innertext", "Unlock", "regexp", 0));			
			}
			catch{
				if (!_instance.ActiveTab.FindElementByAttribute("path", "d", "M12 5v14", "text", 0).IsVoid)	return;
				else throw;
			}

		}
		public void BPCheck (bool log = false)
		{
			if (log) Loggers.l0g(_project,$"[BackPack] getting address...");
			getA:

			_instance.CloseExtraTabs();
			try{
			while (_instance.ActiveTab.FindElementByAttribute("button", "class", "is_Button\\ ", "regexp", 0).IsVoid) 
				_instance.LMB(("path", "d", "M12 5v14", "text", 0),deadline:2);
			var publicSOL =	 _instance.ReadHe(("p", "class", "MuiTypography-root\\ MuiTypography-body1", "regexp", 0 ),"last");
			_instance.LMB(("button", "aria-label", "TabsNavigator,\\ back", "regexp", 0));
			_project.Variables["addressSol"].Value = publicSOL;
			Db.UpdAddressSol(_project);
			}
			catch{goto getA;}
		}
 		public void BPApprove (bool log = false)
		{
			if (log) Loggers.l0g(_project,$"[BackPack] Approve...");
            
			
			try{
            _instance.LMB(("div", "innertext", "Approve", "regexp", 0),"last");
            _instance.CloseExtraTabs();
			return;
            }
            catch{			
            _instance.SetHe(("input:password", "fulltagname", "input:password", "regexp", 0),_pass);
            _instance.LMB(("button", "innertext", "Unlock", "regexp", 0));
			}
            _instance.LMB(("div", "innertext", "Approve", "regexp", 0),"last");
            _instance.CloseExtraTabs();
		}
       
        //Razor
		public void RZRLaunch (bool log = false)
		{
			if (log)Loggers.l0g(_project,"RZRLaunch");
			var em = _instance.UseFullMouseEmulation;
			_instance.UseFullMouseEmulation = false;
			if (RZRInstall (log)) RZRImport(log);
			else RZRUnlock(log);
			_instance.CloseExtraTabs();
			_instance.UseFullMouseEmulation = em;
		}
		public bool RZRInstall (bool log = false)
		{
			string path = $"{_project.Path}.crx\\Razor2.0.9.crx";
			var extId = "fdcnegogpncmfejlfnffnofpngdiejii";
			var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
			if (!extListString.Contains(extId)) 
			{
				if (log)Loggers.l0g(_project,"RZRInstall");
				_instance.InstallCrxExtension(path);
				return true;
			}
			return false;
		}
		public bool RZRImport (bool log = false)
		{
			if (log)Loggers.l0g(_project,"RZRImport");
			var key = Db.KeySOL(_project);
			var password = SAFU.HWPass(_project);
			_instance.CloseExtraTabs();
            Tab walTab = _instance.NewTab("wal");
            walTab.SetActive();
            walTab.Navigate("chrome-extension://fdcnegogpncmfejlfnffnofpngdiejii/index.html#/account/initialize/import/private-key", "");
            
            try{
                RZRUnlock();
                return true;
            }
            catch{}

            _instance.WaitSetValue(() => 	_instance.ActiveTab.FindElementByName("name"),"pkey");
            _instance.WaitSetValue(() => 	_instance.ActiveTab.FindElementByName("privateKey"),key);
            _instance.LMB(("button", "innertext", "Proceed", "regexp", 0));

            _instance.WaitSetValue(() => 	_instance.ActiveTab.FindElementByName("password"),password);
            _instance.WaitSetValue(() => 	_instance.ActiveTab.FindElementByName("repeatPassword"),password);
            _instance.LMB(("button", "innertext", "Proceed", "regexp", 0));

            _instance.LMB(("button", "innertext", "Done", "regexp", 0));
            
            return true;
		}
		public void RZRUnlock (bool log = false)
		{
			if (log)Loggers.l0g(_project,$"[RZRUnlock]");
			var password = SAFU.HWPass(_project);			
			try 
            {
                _instance.WaitSetValue(() => _instance.ActiveTab.FindElementByName("password"),password,deadline:3);
				_instance.LMB(("button", "innertext", "Unlock", "regexp", 0));
                return;
			}
			catch
            {
                try{
                    Tab walTab = _instance.NewTab("wal");
                    walTab.SetActive();
                    walTab.Navigate("chrome-extension://fdcnegogpncmfejlfnffnofpngdiejii/index.html", "");
                    _instance.WaitSetValue(() => _instance.ActiveTab.FindElementByName("password"),password,deadline:3);
				    _instance.LMB(("button", "innertext", "Unlock", "regexp", 0));	
                    return;

                }
                catch{
                    throw;
                }
            }

		}
		public void RZRCheck (bool log = false)
		{

		}
        //OKX
		public string OKXGetWallets(string mode = null, string choose = null)
		{
			_instance.ActiveTab.Navigate("chrome-extension://mcohilncbfahbmgdjkbpemcciiolgcge/home.html#/wallet/management-home-page?fromHome=1", "");
			var pKeys = new List<string>();
			var sKeys = new List<string>();
			string active = null;
			var wList = _instance.ActiveTab.FindElementByAttribute("div", "class", "okui-virtual-list-holder-inner", "regexp", 0).GetChildren(false).ToList();
			bool set = false;
			foreach(HtmlElement he in wList)
			{
				
				if (he.InnerText.Contains("0x")) pKeys.Add((he.InnerText.Split('\n')[0]) + ": " + he.InnerText.Split('\n')[1]) ;
				if (he.InnerText.Contains("Account")) sKeys.Add((he.InnerText.Split('\n')[0]) + ": " + he.InnerText.Split('\n')[1]) ;
				if (he.InnerHtml.Contains("okd-checkbox-circle"))   active =((he.InnerText.Split('\n')[0]) + ": " + he.InnerText.Split('\n')[1]) ;
				if (choose != null) 
				{
					if (he.InnerText.Contains(choose)) 
					{
						_instance.UseFullMouseEmulation = true;
						_instance.WaitClick(() => he);
						set = true;
						//return "";
					}
					
				}
			}
			if (choose != null)
			{
				if(!set)throw new Exception("no key");
				return active;
			} 
			// if (_log) _project.SendInfoToLog(string.Join("\n", pKeys));
			// if (_log) _project.SendInfoToLog(string.Join("\n", sKeys));
			// if (_log) _project.SendInfoToLog(active);
			if (mode == "pKeys") return string.Join("\n", pKeys);
			if (mode == "sKeys") return string.Join("\n", sKeys);
			return active;
		}
		public void OKXImport(string sourse = "pkey", string chainMode = "EVM") //seed|pkey //EVM|Aptos
		{

			var password = SAFU.HWPass(_project);
			_instance.ActiveTab.Navigate("chrome-extension://mcohilncbfahbmgdjkbpemcciiolgcge/home.html#/wallet-add/import-with-seed-phrase-and-private-key", "");
			try{
			_instance.LMB(("button", "innertext", "Import\\ wallet", "regexp", 0), deadline:3);
			_instance.LMB(("i", "class", "icon\\ iconfont\\ okx-wallet-plugin-futures-grid-20\\ _wallet-icon__icon__core_", "regexp", 0), thr0w:false);
			}
			catch{}
			if( sourse== "pkey")
			{
				_instance.LMB(("div", "class", "okui-tabs-pane\\ okui-tabs-pane-sm\\ okui-tabs-pane-grey\\ okui-tabs-pane-segmented", "regexp", 1));
				
				var key = Db.KeyEVM(_project); 
				_instance.SetHe(("textarea", "class", "okui-input-input\\ input-textarea", "regexp", 0),key);

				_instance.LMB(("button", "innertext", "Confirm", "regexp", 0),deadline:20);
				if (chainMode == "Aptos") _instance.LMB(("span", "innertext", "Aptos\\ network", "regexp", 0));
				_instance.LMB(("button", "class", "okui-btn\\ btn-lg\\ btn-fill-highlight\\ block\\ chains-choose-network-modal__confirm-button", "regexp", 0));
			}
			if( sourse== "seed")
			{
				string seedPhrase = Db.Seed(_project);
				int index = 0;	
				foreach(string word in seedPhrase.Split(' ')) 
					{ 
						_instance.ActiveTab.FindElementByAttribute("input", "class", "mnemonic-words-inputs__container__input", "regexp", index).SetValue(word, "Full", false);
						index++;
					}
				_instance.LMB(("button", "type", "submit", "regexp", 0));

			}
			try{
			_instance.LMB(("button", "data-testid", "okd-button", "regexp", 0));
			_instance.WaitSetValue(() => 	_instance.ActiveTab.GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildByAttribute("input:password", "data-testid", "okd-input", "regexp", 0),password);
			_instance.WaitSetValue(() => 	_instance.ActiveTab.GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildByAttribute("input:password", "data-testid", "okd-input", "regexp", 1),password);
			_instance.LMB(("button", "data-testid", "okd-button", "regexp", 0));
			_instance.LMB(("button", "data-testid", "okd-button", "regexp", 0));
			}
			catch{}
		}

        //Zer
		public void ZERLaunch (bool log = false)
		{
			if (log)Loggers.l0g(_project,"RZRLaunch");
			var em = _instance.UseFullMouseEmulation;
			_instance.UseFullMouseEmulation = false;
			if (ZERInstall (log:log)) ZERImport(log:log);
			else ZERUnlock(log:false); ZERCheck(log:log);
			_instance.CloseExtraTabs();
			_instance.UseFullMouseEmulation = em;
		}
		public bool ZERInstall (bool log = false)
		{
			string path = $"{_project.Path}.crx\\Zerion1.21.3.crx";
			var extId = "klghhnkeealcohjjanjjdaeeggmfmlpl";
			var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
			if (!extListString.Contains(extId)) 
			{
				WalLog();
				_instance.InstallCrxExtension(path);
				return true;
			}
			return false;
		}
		public bool ZERImport (string sourse = "pkey", string refCode = null, bool log = false)
		{
			if (string.IsNullOrWhiteSpace(refCode))refCode = SQL.W3Query(_project,$@"SELECT referralCode
			FROM projects.zerion
			WHERE referralCode != '_' 
			AND TRIM(referralCode) != ''
			ORDER BY RANDOM()
			LIMIT 1;");

			var inputRef =true;
			_instance.LMB(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import", "regexp", 0));
			if (sourse == "pkey")
			{
				_instance.LMB(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/private-key", "regexp", 0));
				string key = Db.KeyEVM(_project);
				_instance.ActiveTab.FindElementByName("key").SetValue(key, "Full", false);
			}
			else if (sourse == "seed")
			{
				_instance.LMB(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/mnemonic", "regexp", 0));
				string seedPhrase = Db.Seed(_project);
				int index = 0;	
				foreach(string word in seedPhrase.Split(' ')) 
					{ 
						_instance.ActiveTab.FindElementById($"word-{index}").SetValue(word, "Full", false);
						index++;
					}
			}
			_instance.LMB(("button", "innertext", "Import\\ wallet", "regexp", 0));	
			_instance.SetHe(("input:password", "fulltagname", "input:password", "text", 0),_pass);
			_instance.LMB(("button", "class", "_primary", "regexp", 0));
			_instance.SetHe(("input:password", "fulltagname", "input:password", "text", 0),_pass);
			_instance.LMB(("button", "class", "_primary", "regexp", 0));
			if (inputRef)
			{
				_instance.LMB(("button", "innertext", "Enter\\ Referral\\ Code", "regexp", 0),refCode);
				_instance.WaitSetValue(() => _instance.ActiveTab.FindElementByName("referralCode"),refCode);
				_instance.LMB(("button", "class", "_regular", "regexp", 0));
			}
			return true;
		}
		public void ZERUnlock (bool log = false)
		{
			_instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview", "");

			string active = null;
			try{
				active = _instance.ReadHe(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0));				
			}
			catch{
				_instance.SetHe(("input:password", "fulltagname", "input:password", "text", 0),_pass);
				_instance.LMB(("button", "class", "_primary", "regexp", 0));
				active = _instance.ReadHe(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0));
			}
			WalLog(active);
		}
		public string ZERCheck (bool log = false)
		{
			_instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/wallet-select", "");
			var active = _instance.ReadHe(("button", "class", "_wallet", "regexp", 0)).Replace("Explore Rewards","").Replace("\n"," ");
			WalLog(active);
			return active;
		}

	}
	
	public static class Keplr
	{
		public static string KeplrApprove(this Instance instance,IZennoPosterProjectModel project)
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
				if (DateTime.Now > deadline)  Loggers.l0g(project,"Keplr tab stucked",thr0w:true);
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
		public static void KeplrImportPkey(this Instance instance,IZennoPosterProjectModel project,bool temp = false)
		{
			var walletPassword = SAFU.HWPass(project);
			var key = new Key().ToHex(); var walletName = "temp";
			if (!temp) key = Db.KeyEVM(project);
			if (!temp) walletName = "pkey";
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ an\\ existing\\ wallet", "regexp", 0));
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Use\\ recovery\\ phrase\\ or\\ private\\ key", "regexp", 0));
			
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Private\\ key", "regexp", 1));
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "tagname", "input", "regexp", 0),key);
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 1));
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("name"), walletName);
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("password"),walletPassword,2,Throw:false);
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByName("confirmPassword"),walletPassword,2,Throw:false);
			instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Next", "regexp", 0));
			
			string check = instance.WaitGetValue(() => 
			instance.ActiveTab.FindElementByAttribute("div", "innertext", "Select\\ All", "regexp", 0));
			
			instance.WaitClick(() => 	instance.ActiveTab.FindElementByAttribute("button", "innertext", "Save", "regexp", 0));
			while  (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).IsVoid)
			{
				instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0).RiseEvent("click", instance.EmulationLevel); 
				Thread.Sleep(2000);
			}
			instance.CloseExtraTabs();
		}
		public static void KeplrSetSource(this Instance instance,IZennoPosterProjectModel project, string source)
		{
			//var source  = "pkey";//"seed"

			while (true)
			{
				Thread.Sleep(1000);
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
			string path = $"{project.Path}.crx\\keplr0.12.223.crx";//keplr0.12.169.crx"
			instance.InstallCrxExtension(path);Thread.Sleep(2000);
		}//0.12.223
		public static void KeplrUnlock(this Instance instance,IZennoPosterProjectModel project)
		{
			instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "tagname", "input", "regexp", 0),SAFU.HWPass(project));
			instance.KeplrClick(instance.ActiveTab.FindElementByAttribute("button", "innertext", "Unlock", "regexp", 0));
			if (!instance.ActiveTab.FindElementByAttribute("div", "innertext", "Invalid\\ password", "regexp", 0).IsVoid) 
			{
				instance.CloseAllTabs(); instance.UninstallExtension("dmkamcknogkgcdfhhbddcghachkejeap"); 
                Loggers.l0g(project,$"!WrongPassword",thr0w:true);

			}			
		}
		public static string KeplrPrune(this Instance instance,IZennoPosterProjectModel project, bool keepTemp = false)
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
				if(keepTemp)
				{
					if (tile.InnerText.Contains("temp")) 				
					{
						imported += "temp"; 
						i++;
						continue;
					}
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
				
				if (log) Loggers.l0g(project,kState);
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
	public class Easy

	{
		private readonly IZennoPosterProjectModel _project;
		private readonly Instance _instance;
		private readonly bool _log;
        private readonly Random _random = new Random();

		public Easy(IZennoPosterProjectModel project, bool log = false)
		{
			_project = project;
			_log = log;
		}

		public string Ref(string refCode = null, bool log = false)
		{
			if (string.IsNullOrEmpty(refCode)) refCode = _project.Variables["cfgRefCode"].Value;
			if (string.IsNullOrEmpty(refCode)||refCode == "_" ) refCode = SQL.W3Query(_project,$@"SELECT refcode FROM {_project.Variables["projectTable"].Value}
			WHERE refcode != '_' 
			AND TRIM(refcode) != ''
			ORDER BY RANDOM()
			LIMIT 1;",log);
			return refCode;
		}


        public T EasyRandomise<T>(object value, decimal percent = 1m, int decimalPlaces = 5)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (percent < 0)
                throw new ArgumentException("Percent must be non-negative", nameof(percent));
            if (decimalPlaces < 0)
                throw new ArgumentException("Decimal places must be non-negative", nameof(decimalPlaces));

            decimal number;
            if (value is int intValue)
                number = intValue;
            else if (value is double doubleValue)
                number = (decimal)doubleValue;
            else if (value is decimal decimalValue)
                number = decimalValue;
            else
                throw new ArgumentException("Value must be int, double, or decimal", nameof(value));

            // Вычисляем диапазон рандомизации (±percent)
            decimal range = number * (percent / 100m);
            decimal randomAdjustment = (decimal)(_random.NextDouble() * (double)(range * 2) - (double)range);
            decimal result = number + randomAdjustment;

            // Округляем до указанного числа знаков
            result = Math.Round(result, decimalPlaces, MidpointRounding.AwayFromZero);

            // Форматируем результат
            if (typeof(T) == typeof(string))
            {
                string format = "0." + new string('#', decimalPlaces);
                return (T)Convert.ChangeType(result.ToString(format, CultureInfo.InvariantCulture), typeof(T));
            }
            if (typeof(T) == typeof(int))
                return (T)Convert.ChangeType((int)result, typeof(T));
            if (typeof(T) == typeof(double))
                return (T)Convert.ChangeType((double)result, typeof(T));
            return (T)Convert.ChangeType(result, typeof(T));
        }


	}

    public static class Time
    {

        public static string UnixNow()
        {
            return ((long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds)).ToString();
        }
        public static string Now(string format = "unix") // unix|iso
        {
            if (format == "unix") return ((long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds)).ToString();	//Unix Epoch
            else if (format == "iso") return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601 
            else if (format == "short") return DateTime.UtcNow.ToString("MM-ddTHH:mm"); 		
            throw new ArgumentException("Invalid format. Use 'unix' or 'iso'.");
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
        public static string cd(object input = null, string o = "unix")
        {
            DateTime utcNow = DateTime.UtcNow;
            if (input == null)
            {
                DateTime todayEnd = utcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                if (o == "unix") return ((int)(todayEnd - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                else if (o == "iso") return todayEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601
            }
            else if (input is decimal || input is int)  
            {
                decimal minutes = Convert.ToDecimal(input);
                int secondsToAdd = (int)Math.Round(minutes * 60); 
                DateTime futureTime = utcNow.AddSeconds(secondsToAdd);
                if (o == "unix") return ((int)(futureTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                else if (o == "iso") return futureTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601
            }
            else if (input is string timeString)
            {
                TimeSpan parsedTime = TimeSpan.Parse(timeString);
                DateTime futureTime = utcNow.Add(parsedTime);
                if (o == "unix") return ((int)(futureTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                else if (o == "iso") return futureTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601
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
	public static class OTP
	{
		public static string Offline(string keyString, int waitIfTimeLess = 5)
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
		
		    string result = ZennoPoster.HttpGet(
				url,
				proxy, 
				"UTF-8", 
				ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly, 
				5000, 
				"", 
				project.Profile.UserAgent, 
				true, 
				5, 
				headers, 
				"", 
				false);

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
						Loggers.l0g(project, $"!W cleanGlobal is [on] Cleaned: {string.Join(",", cleaned)}");
					}
					else
					{
						Loggers.l0g(project, $"buzy Threads: [{string.Join(" | ", busyAccounts)}]");
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
						if (log) Loggers.l0g(project, $"Thread {currentThread} bound to {project.Variables["projectName"].Value}");
						return true;
					}
					else
					{
						if (log) Loggers.l0g(project, $"Thread {currentThread} is already busy!");
						return false;
					}
				}
				catch (Exception ex)
				{
					if (log) Loggers.l0g(project, $"⚙  {ex.Message}");
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
		public static HtmlElement GetHe(this Instance instance, object obj, string method = "")
		{
			
            
            Type tupleType = obj.GetType();
			int tupleLength = tupleType.GetFields().Length;
			switch (tupleLength)
			{
				case 2:
					                    
                    string value = tupleType.GetField("Item1").GetValue(obj).ToString();
					method = tupleType.GetField("Item2").GetValue(obj).ToString();
					if (method == "id")
					{
						HtmlElement he = instance.ActiveTab.FindElementById(value);
						if (he.IsVoid) throw new Exception($"no element by {method}='{value}'");
						return he;
					}
					else if (method == "name")
					{
						HtmlElement he = instance.ActiveTab.FindElementByName(value);
						if (he.IsVoid) throw new Exception($"no element by {method}='{value}'");
						return he;
					}
					else
					{
						throw new Exception($"unsupported method for tupple1 {method}");
					}

				case 5:

					string tag = tupleType.GetField("Item1").GetValue(obj).ToString();
					string attribute = tupleType.GetField("Item2").GetValue(obj).ToString();
					string pattern = tupleType.GetField("Item3").GetValue(obj).ToString();
					string mode = tupleType.GetField("Item4").GetValue(obj).ToString();
					
					object posObj = tupleType.GetField("Item5").GetValue(obj);
					int pos;
					if (!int.TryParse(posObj.ToString(), out pos)) throw new ArgumentException("5th element of Tupple must be (int).");

					if (method == "last")
					{
						int index = 0;
						while (true)
						{
							HtmlElement he = instance.ActiveTab.FindElementByAttribute(tag, attribute, pattern, mode, index);
							if (he.IsVoid)
							{
								he = instance.ActiveTab.FindElementByAttribute(tag, attribute, pattern, mode, index - 1);
								if (he.IsVoid)
								{
									throw new Exception(string.Format("no element by: tag='{0}', attribute='{1}', pattern='{2}', mode='{3}'.", tag, attribute, pattern, mode));
								}
								return he;
							}
							index++;
						}
					}
					else
					{
						HtmlElement he = instance.ActiveTab.FindElementByAttribute(tag, attribute, pattern, mode, pos);
						if (he.IsVoid)
						{
							throw new Exception(string.Format("no element by: tag='{0}', attribute='{1}', pattern='{2}', mode='{3}', pos={4}.", tag, attribute, pattern, mode, pos));
						}
						return he;
					}
				default:
					throw new ArgumentException(string.Format("unsupported Tupple: {0}.", tupleLength));
			}
		}
//new
		public static void LMB(this Instance instance, object obj, string method = "", int deadline = 10, int delay = 1, string comment = "", bool thr0w = true)
		{
			DateTime functionStart = DateTime.Now;
			string lastExceptionMessage = "";

			while (true)
			{
				if ((DateTime.Now - functionStart).TotalSeconds > deadline)
				{
					if (thr0w) throw new TimeoutException($"{comment} not found in {deadline}s: {lastExceptionMessage}");
					else return;
				}
				
                try
				{
					HtmlElement he = instance.GetHe(obj, method);
					Thread.Sleep(delay * 1000);
					he.RiseEvent("click", instance.EmulationLevel);
					break;
				}
				catch (Exception ex)
				{
					lastExceptionMessage = ex.Message;
				}
				Thread.Sleep(500);
			}

            if (method == "clickOut")
            {
				if ((DateTime.Now - functionStart).TotalSeconds > deadline)
				{
					if (thr0w) throw new TimeoutException($"{comment} not found in {deadline}s: {lastExceptionMessage}");
					else return;
				}
				while (true)
                {
                    try
                    {
                        HtmlElement he = instance.GetHe(obj, method);
                        Thread.Sleep(delay * 1000);
                        he.RiseEvent("click", instance.EmulationLevel);
                        continue;
                    }
                    catch 
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }

            }

		}


        public static string ReadHe(this Instance instance, object obj, string method = "", int deadline = 10, string atr = "innertext", int delay = 1, string comment = "", bool thr0w = true)
        {
            DateTime functionStart = DateTime.Now;
            string lastExceptionMessage = "";

            while (true)
            {
                if ((DateTime.Now - functionStart).TotalSeconds > deadline)
                {
                    if (method == "!")
                    {
                        // Элемент не найден в течение дедлайна — это успех
                        return null; // или можно вернуть "not found" для явности
                    }
                    else if (thr0w)
                    {
                        throw new TimeoutException($"{comment} not found in {deadline}s: {lastExceptionMessage}");
                    }
                    else
                    {
                        return null;
                    }
                }

                try
                {
                    HtmlElement he = instance.GetHe(obj, method); // Передаём method в GetHe, если он там обрабатывается
                    if (method == "!")
                    {
                        // Элемент найден, а не должен быть — выбрасываем исключение
                        throw new Exception($"{comment} element detected when it should not be: {atr}='{he.GetAttribute(atr)}'");
                    }
                    else
                    {
                        // Обычное поведение: элемент найден, возвращаем атрибут
                        Thread.Sleep(delay * 1000);
                        return he.GetAttribute(atr);
                    }
                }
                catch (Exception ex)
                {
                    lastExceptionMessage = ex.Message;
                    if (method == "!" && ex.Message.Contains("no element by"))
                    {
                        // Элемент не найден — это нормально, продолжаем ждать
                    }
                    else if (method != "!")
                    {
                        // Обычное поведение: элемент не найден, записываем ошибку и ждём
                    }
                    else
                    {
                        // Неожиданная ошибка при method = "!", пробрасываем её
                        throw;
                    }
                }

                Thread.Sleep(500);
            }
        }
		public static void SetHe(this Instance instance, object obj, string value, string method = "id", int deadline = 10, int delay = 1, string comment = "", bool thr0w = true)
		{
			DateTime functionStart = DateTime.Now;
			string lastExceptionMessage = "";

			while (true)
			{
				if ((DateTime.Now - functionStart).TotalSeconds > deadline)
				{
					if (thr0w) throw new TimeoutException($"{comment} not found in {deadline}s: {lastExceptionMessage}");
					else return;
				}

				try
				{
					HtmlElement he = instance.GetHe(obj, method);
					Thread.Sleep(delay * 1000);
					instance.WaitFieldEmulationDelay(); // Mimics WaitSetValue behavior
					he.SetValue(value, "Full", false);
					break;
				}
				catch (Exception ex)
				{
					lastExceptionMessage = ex.Message;
				}

				Thread.Sleep(500);
			}
		}
		
//old
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
			public static void WaitSetValue(this Instance instance, Func<ZennoLab.CommandCenter.HtmlElement> elementSearch, string value, int deadline = 10, int delay = 1, string comment = "",bool Throw = true)
		{
		    DateTime functionStart = DateTime.Now;
			HtmlElement directElement = TryGetDirectElement(elementSearch);
			bool isDirectElement = directElement != null;
		    
		    while (true)
		    {
				if ((DateTime.Now - functionStart).TotalSeconds > deadline)
					if (Throw) throw new TimeoutException($"{comment} not found in {deadline}s");
					else return;
		            
				HtmlElement element;
				if (isDirectElement) element = directElement;
				else element = elementSearch();
		        
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
		public static string WaitGetValue(this Instance instance, Func<ZennoLab.CommandCenter.HtmlElement> elementSearch, int deadline = 10, string atr = "innertext", int delayBeforeGetSeconds = 1, string comment = "")
		{
		    DateTime functionStart = DateTime.Now;
		    
		    while (true)
		    {
		        if ((DateTime.Now - functionStart).TotalSeconds > deadline)
		            throw new TimeoutException($"{comment} not found in {deadline}s");
		            
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
		public static string CF(this Instance instance,int deadline = 60, bool strict = false)
		{
			DateTime timeout = DateTime.Now.AddSeconds(deadline);
			while (true)
			{
				if (DateTime.Now > timeout) throw new Exception($"!W CF timeout");
				Random rnd = new Random(); 
				
				Thread.Sleep(rnd.Next(3, 4) * 1000);

				var token = instance.ReadHe(("cf-turnstile-response","name"),atr:"value");
				if (!string.IsNullOrEmpty(token)) return token;

				string strX = ""; string strY = ""; 
				
				try 
				{
					var cfBox = instance.GetHe(("cf-turnstile","id"));	
					strX = cfBox.GetAttribute("leftInbrowser");	strY = cfBox.GetAttribute("topInbrowser");
				}
				catch
				{
					var cfBox = instance.GetHe(("div", "outerhtml", "<div><input type=\"hidden\" name=\"cf-turnstile-response\"", "regexp", 4));
					strX = cfBox.GetAttribute("leftInbrowser");	strY = cfBox.GetAttribute("topInbrowser");
				}
				
				int x = (int.Parse(strX) + rnd.Next(23, 26));
				int y = (int.Parse(strY) + rnd.Next(27, 31));
				instance.Click(x, x, y, y, "Left", "Normal");

			}
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

	public static class Cnvrt
	{
		public static string ConvertFormat(IZennoPosterProjectModel project, string toProcess, string input, string output, bool log = false)
		{
			try
			{
				input = input.ToLower();
				output = output.ToLower();

				string[] supportedFormats = { "hex", "base64", "bech32", "bytes", "text" };
				if (!supportedFormats.Contains(input))
				{
					throw new ArgumentException($"Неподдерживаемый входной формат: {input}. Поддерживаемые форматы: {string.Join(", ", supportedFormats)}");
				}
				if (!supportedFormats.Contains(output))
				{
					throw new ArgumentException($"Неподдерживаемый выходной формат: {output}. Поддерживаемые форматы: {string.Join(", ", supportedFormats)}");
				}

				byte[] bytes;
				switch (input)
				{
					case "hex":
						string hex = toProcess.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? toProcess.Substring(2) : toProcess;
						hex = hex.PadLeft(64, '0');
						if (!System.Text.RegularExpressions.Regex.IsMatch(hex, @"^[0-9a-fA-F]+$"))
						{
							throw new ArgumentException("Входная строка не является валидной hex-строкой");
						}
						bytes = new byte[hex.Length / 2];
						for (int i = 0; i < hex.Length; i += 2)
						{
							bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
						}
						break;

					case "base64":
						bytes = Convert.FromBase64String(toProcess);
						break;

					case "bech32":
						var (hrp, data) = DecodeBech32(toProcess);
						if (hrp != "init")
						{
							throw new ArgumentException($"Ожидался Bech32-адрес с префиксом 'init', но получен префикс '{hrp}'");
						}
						bytes = ConvertBits(data, 5, 8, false);
						if (bytes.Length != 32)
						{
							throw new ArgumentException($"Bech32-адрес должен декодироваться в 32 байта, но получено {bytes.Length} байт");
						}
						break;

					case "bytes":
						if (!System.Text.RegularExpressions.Regex.IsMatch(toProcess, @"^[0-9a-fA-F]+$"))
						{
							throw new ArgumentException("Входная строка не является валидной hex-строкой для байтов");
						}
						bytes = new byte[toProcess.Length / 2];
						for (int i = 0; i < toProcess.Length; i += 2)
						{
							bytes[i / 2] = Convert.ToByte(toProcess.Substring(i, 2), 16);
						}
						break;

					case "text":
						bytes = System.Text.Encoding.UTF8.GetBytes(toProcess);
						break;

					default:
						throw new ArgumentException($"Неизвестный входной формат: {input}");
				}

				string result;
				switch (output)
				{
					case "hex":
						result = "0x" + BitConverter.ToString(bytes).Replace("-", "").ToLower();
						break;

					case "base64":
						result = Convert.ToBase64String(bytes);
						break;

					case "bech32":
						if (bytes.Length != 32)
						{
							throw new ArgumentException($"Для Bech32 требуется 32 байта, но получено {bytes.Length} байт");
						}
						byte[] data5Bit = ConvertBits(bytes, 8, 5, true);
						result = EncodeBech32("init", data5Bit);
						break;

					case "bytes":
						result = BitConverter.ToString(bytes).Replace("-", "").ToLower();
						break;

					case "text":
						result = System.Text.Encoding.UTF8.GetString(bytes);
						break;

					default:
						throw new ArgumentException($"Неизвестный выходной формат: {output}");
				}

				if (log )project.SendInfoToLog($"convert success: {toProcess} ({input}) -> {result} ({output})");
				return result;
			}
			catch (Exception ex)
			{
				project.SendErrorToLog($"Ошибка при преобразовании: {ex.Message}");
				return null;
			}
		}
		private static (string hrp, byte[] data) DecodeBech32(string bech32)
		{
			// Проверяем, что строка валидная
			if (string.IsNullOrEmpty(bech32) || bech32.Length > 1023)
			{
				throw new ArgumentException("Невалидная Bech32-строка");
			}

			// Разделяем на префикс (hrp) и данные
			int separatorIndex = bech32.LastIndexOf('1');
			if (separatorIndex < 1 || separatorIndex + 7 > bech32.Length)
			{
				throw new ArgumentException("Невалидный формат Bech32: отсутствует разделитель '1'");
			}

			string hrp = bech32.Substring(0, separatorIndex);
			string dataPart = bech32.Substring(separatorIndex + 1);

			// Проверяем контрольную сумму
			if (!VerifyChecksum(hrp, dataPart))
			{
				throw new ArgumentException("Невалидная контрольная сумма Bech32");
			}

			// Преобразуем символы данных в 5-битные значения
			byte[] data = new byte[dataPart.Length - 6]; // Убираем 6 байт контрольной суммы
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)Bech32Charset.IndexOf(dataPart[i]);
				if (data[i] == 255)
				{
					throw new ArgumentException($"Невалидный символ в Bech32: {dataPart[i]}");
				}
			}

			return (hrp, data);
		}
		private static string EncodeBech32(string hrp, byte[] data)
		{
			// Добавляем контрольную сумму
			string checksum = CreateChecksum(hrp, data);
			string combined = string.Concat(data.Select(b => Bech32Charset[b])) + checksum;
			return hrp + "1" + combined;
		}
		private static byte[] ConvertBits(byte[] data, int fromBits, int toBits, bool pad)
		{
			int acc = 0;
			int bits = 0;
			var result = new List<byte>();
			int maxv = (1 << toBits) - 1;
			int maxAcc = (1 << (fromBits + toBits - 1)) - 1;

			foreach (var value in data)
			{
				acc = ((acc << fromBits) | value) & maxAcc;
				bits += fromBits;
				while (bits >= toBits)
				{
					bits -= toBits;
					result.Add((byte)((acc >> bits) & maxv));
				}
			}

			if (pad && bits > 0)
			{
				result.Add((byte)((acc << (toBits - bits)) & maxv));
			}
			else if (bits >= fromBits || ((acc << (toBits - bits)) & maxv) != 0)
			{
				throw new ArgumentException("Невозможно преобразовать биты без потерь");
			}

			return result.ToArray();
		}
		private static readonly string Bech32Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
		private static bool VerifyChecksum(string hrp, string data)
		{
			var values = new List<byte>();
			foreach (char c in hrp.ToLower())
			{
				values.Add((byte)c);
			}
			values.Add(0);
			foreach (char c in data)
			{
				int v = Bech32Charset.IndexOf(c);
				if (v == -1) return false;
				values.Add((byte)v);
			}
			return Polymod(values) == 1;
		}
		private static string CreateChecksum(string hrp, byte[] data)
		{
			var values = new List<byte>();
			foreach (char c in hrp.ToLower())
			{
				values.Add((byte)c);
			}
			values.Add(0);
			values.AddRange(data);
			values.AddRange(new byte[] { 0, 0, 0, 0, 0, 0 }); // 6 байт для контрольной суммы
			int polymod = Polymod(values) ^ 1;
			var checksum = new char[6];
			for (int i = 0; i < 6; i++)
			{
				checksum[i] = Bech32Charset[(polymod >> (5 * (5 - i))) & 31];
			}
			return new string(checksum);
		}
		private static int Polymod(List<byte> values)
		{
			int chk = 1;
			int[] generator = { 0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3 };
			foreach (byte value in values)
			{
				int b = chk >> 25;
				chk = (chk & 0x1ffffff) << 5 ^ value;
				for (int i = 0; i < 5; i++)
				{
					if (((b >> i) & 1) != 0)
					{
						chk ^= generator[i];
					}
				}
			}
			return chk;
		}
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

	public class Galxe

	{
		private readonly IZennoPosterProjectModel _project;
		private readonly Instance _instance;
		private readonly bool _log;

		private readonly string GRAPHQL_URL = "https://graphigo.prd.galaxy.eco/query";

		public Galxe(IZennoPosterProjectModel project, Instance instance, bool log = false)
		{
			_project = project;
			_instance = instance;
			_log = log;
		}

		public List<HtmlElement> ParseTasks(string type = "tasksUnComplete", bool log = false) //tasksComplete|tasksUnComplete|reqComplete|reqUnComplete|refComplete|refUnComplete
		{
			string sectionName = null;
			var reqComplete = new List<HtmlElement>();
			var reqUnComplete = new List<HtmlElement>();

			var tasksComplete = new List<HtmlElement>();
			var tasksUnComplete = new List<HtmlElement>();

			var refComplete = new List<HtmlElement>();
			var refUnComplete = new List<HtmlElement>();

			var dDone = "M10 19a9 9 0 1 0 0-18 9 9 0 0 0 0 18m3.924-10.576a.6.6 0 0 0-.848-.848L9 11.652 6.924 9.575a.6.6 0 0 0-.848.848l2.5 2.5a.6.6 0 0 0 .848 0z";

			var sectionList = _instance.ActiveTab.FindElementByAttribute("div", "class", "mb-20", "regexp", 0).GetChildren(false).ToList();

			foreach( HtmlElement section in sectionList)
			{
				sectionName = null;
				var taskList = section.GetChildren(false).ToList();
				foreach (HtmlElement taskTile in taskList)
				{
					if (taskTile.GetAttribute("class") == "flex justify-between") {
						sectionName = taskTile.InnerText.Replace("\n"," ");
						_project.SendInfoToLog(sectionName);
						continue;
					}
					if (sectionName.Contains("Requirements"))
					{
						var taskText = taskTile.InnerText.Replace("View Detail","").Replace("\n",";");
						if (taskTile.InnerHtml.Contains(dDone)) reqComplete.Add(taskTile); 
						else reqUnComplete.Add(taskTile); 	
					}
					else if (sectionName.Contains("Get") && !sectionName.Contains("Referral") )
					{
						var taskText = taskTile.InnerText.Replace("View Detail","").Replace("\n",";");
						if (taskTile.InnerHtml.Contains(dDone)) tasksComplete.Add(taskTile); 
						else tasksUnComplete.Add(taskTile); 	
					}
					else if (sectionName.Contains("Referral") )
					{
						var taskText = taskTile.InnerText.Replace("View Detail","").Replace("\n",";");
						if (taskTile.InnerHtml.Contains(dDone)) refComplete.Add(taskTile); 
						else refUnComplete.Add(taskTile); 	
					}
					
				}	
			}

			_project.SendInfoToLog($"requirements done/!done {reqComplete.Count}/{reqUnComplete.Count}");
			_project.SendInfoToLog($"tasks done/!done {tasksComplete.Count}/{tasksUnComplete.Count}");
			_project.SendInfoToLog($"refs counted {refComplete.Count}");

			switch (type) //tasksComplete|tasksUnComplete|reqComplete|reqUnComplete|refComplete|refUnComplete
            {
                case "tasksComplete": return tasksComplete; 
                case "tasksUnComplete": return tasksUnComplete; 
                case "reqComplete": return reqComplete; 
                case "reqUnComplete": return reqUnComplete; 
				case "refComplete": return refComplete; 
                case "refUnComplete": return refUnComplete; 
                default: return tasksUnComplete; 
            }
            return null; 

		}

		public string BasicUserInfo(string token, string address)
			{
				// GraphQL-запрос с исправленным полем injectiveAddress
				string query = @"
					query BasicUserInfo($address: String!) {
						addressInfo(address: $address) {
							id
							username
							address
							evmAddressSecondary {
								address
								__typename
							}
							userLevel {
								level {
									name
									logo
									minExp
									maxExp
									__typename
								}
								exp
								gold
								__typename
							}
							ggInviteeInfo {
								questCount
								ggCount
								__typename
							}
							ggInviteCode
							ggInviter {
								id
								username
								__typename
							}
							isBot
							solanaAddress
							aptosAddress
							starknetAddress
							bitcoinAddress
							suiAddress
							xrplAddress
							tonAddress
							displayNamePref
							email
							twitterUserID
							twitterUserName
							githubUserID
							githubUserName
							discordUserID
							discordUserName
							telegramUserID
							telegramUserName
							enableEmailSubs
							subscriptions
							isWhitelisted
							isInvited
							isAdmin
							accessToken
							humanityType
							participatedCampaigns {
								totalCount
								__typename
							}
							__typename
						}
					}";

				// Переменные для запроса с динамическим адресом
				string variables = $"{{\"address\": \"EVM:{address}\"}}";

				// Проверка токена
				if (string.IsNullOrEmpty(token))
				{
					_project.SendErrorToLog("Token is empty or null");
					return null;
				}
				//token = null;
				// Формируем заголовки (только необходимые)
				string[] headers = new string[]
				{
					"Content-Type: application/json",
					$"Authorization: {token}"
				};

				// Форматируем запрос (удаляем лишние пробелы и переносы строк)
				query = query.Replace("\t", "").Replace("\n", " ").Replace("\r", "").Trim();

				// Формируем тело запроса
				string jsonBody = JsonConvert.SerializeObject(new
				{
					operationName = "BasicUserInfo",
					query = query,
					variables = JsonConvert.DeserializeObject(variables)
				});

				_project.SendInfoToLog($"Request headers: {string.Join(", ", headers)}");
				_project.SendInfoToLog($"Request body: {jsonBody}");

				try
				{
					string response = ZennoPoster.HttpPost(
						GRAPHQL_URL,
						Encoding.UTF8.GetBytes(jsonBody),
						"application/json",
						_project.Variables["proxy"].Value,
						"UTF-8",
						ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
						30000,
						"",
						"Galaxy/v1",
						true,
						5,
						headers,
						"",
						true
					);

					_project.SendInfoToLog($"Response received: {response.Substring(0, Math.Min(100, response.Length))}...");
					_project.Json.FromString(response);
					return response;
				}
				catch (Exception ex)
				{
					_project.SendErrorToLog($"GraphQL request failed: {ex.Message}");
					return null;
				}
			}

		public string GetLoyaltyPoints(string alias, string address)
		{
			// GraphQL-запрос SpaceAccessQuery
			string query = @"
				query SpaceAccessQuery($id: Int, $alias: String, $address: String!) {
					space(id: $id, alias: $alias) {
						id
						addressLoyaltyPoints(address: $address) {
							points
							rank
							__typename
						}
						__typename
					}
				}";

			// Переменные для запроса
			string variables = $"{{\"alias\": \"{alias}\", \"address\": \"{address.ToLower()}\"}}";

			// Формируем заголовки (аналогично Google Apps Script)
			string[] headers = new string[]
			{
				"Content-Type: application/json",
				"Accept: */*",
				"Authority: graphigo.prd.galaxy.eco",
				"Origin: https://galxe.com",
				"User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
			};

			// Форматируем запрос (удаляем лишние пробелы и переносы строк)
			query = query.Replace("\t", "").Replace("\n", " ").Replace("\r", "").Trim();

			// Формируем тело запроса
			string jsonBody = JsonConvert.SerializeObject(new
			{
				operationName = "SpaceAccessQuery",
				query = query,
				variables = JsonConvert.DeserializeObject(variables)
			});

			_project.SendInfoToLog($"Request headers: {string.Join(", ", headers)}");
			_project.SendInfoToLog($"Request body: {jsonBody}");

			try
			{
				string response = ZennoPoster.HttpPost(
					"https://graphigo.prd.galaxy.eco/query", // URL эндпоинта
					Encoding.UTF8.GetBytes(jsonBody),
					"application/json",
					_project.Variables["proxy"].Value,
					"UTF-8",
					ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
					30000,
					"",
					"Galaxy/v1",
					true,
					5,
					headers,
					"",
					true
				);

				_project.SendInfoToLog($"Response received: {response.Substring(0, Math.Min(100, response.Length))}...");
				_project.Json.FromString(response);
				return response;
			}
			catch (Exception ex)
			{
				_project.SendErrorToLog($"GraphQL request failed: {ex.Message}");
				return null;
			}
		}

	}

    public class Url
    {
        //private readonly string Url; 
        private readonly Instance _instance;


		public Url(Instance instance)
		{
			_instance = instance;
		}

        public void Stargate (string srcChain, string dstChain, string srcToken = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE", string dstToken = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE")
        {
            
            string url = "https://stargate.finance/bridge?" + $"srcChain={srcChain}" + $"&srcToken={srcToken}" + $"&dstChain={dstChain}"+ $"&dstToken={dstToken}";
             _instance.ActiveTab.Navigate(url, "");
           
        }
        public void Relay (string fromChainId, string to, string toCurrency = "0x0000000000000000000000000000000000000000", string fromCurrency = "0x0000000000000000000000000000000000000000")
        {

            string url = $"https://relay.link/bridge/{to}?fromChainId={fromChainId}&toCurrency={toCurrency}&fromCurrency={fromCurrency}";
            _instance.ActiveTab.Navigate(url, "");

        }

    }

	public class DataImporter
	{
		private readonly IZennoPosterProjectModel _project;
		private readonly Instance _instance;
		private readonly string _schema;



		public DataImporter(IZennoPosterProjectModel project, Instance instance, string schema = "accounts")
		{
			_project = project;
			_instance = instance;
			_schema = schema;
		}

		public bool ImportAll()
		{
			//OnStart.InitVariables(_project); 

			if (_project.Variables["cfgConfirmRebuildDB"].Value != "True")
			{
				_project.SendInfoToLog("Database rebuild not confirmed, skipping import", true);
				return false;
			}
			var import = _project.Variables["toImport"].Value;
			if (!ImportStart()) return false;

			CreateSchema(true);
			if (import.Contains("settings")) ImportSettings();
			if (import.Contains("proxy")) ImportProxy();

			if (import.Contains("evm")) ImportKeys("evm");
			if (import.Contains("sol")) ImportKeys("sol");
			if (import.Contains("seed")) ImportKeys("seed");
			if (import.Contains("deps")) ImportDepositAddresses();

			if (import.Contains("google")) ImportGoogle();
			if (import.Contains("icloud")) ImportIcloud();
			if (import.Contains("twitter")) ImportTwitter();
			if (import.Contains("discord")) ImportDiscord();

			if (import.Contains("nickname")) ImportNickname();
			if (import.Contains("bio")) ImportBio();

			if (import.Contains("webgl")) ImportWebGL(_instance);
			ImportDone();
			return true;
		}

		public void CreateSchema(bool log = false)
		{
			string defaultColumn = "TEXT DEFAULT ''";
			string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";

			if (!string.IsNullOrEmpty(schemaName))
			{
				SQL.W3Query(_project, "CREATE SCHEMA IF NOT EXISTS accounts;", true);
				if (log) _project.SendInfoToLog("Schema 'accounts' created or already exists", true);
			}

			// Profile
			string tableName = schemaName + "profile";
			var tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{"nickname", defaultColumn},
				{"bio", defaultColumn},
				{"cookies", defaultColumn},
				{"webgl", defaultColumn},
				{"timezone", defaultColumn},
				{"proxy", defaultColumn}
			};
			SQL.W3MakeTable(_project, tableStructure, tableName, false, schemaName:"");
			if (log) _project.SendInfoToLog($"Table {tableName} created", true);

			// Twitter
			tableName = schemaName + "twitter";
			tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{"cooldown", "INTEGER DEFAULT 0"},
				{"status", defaultColumn},
				{"last", defaultColumn},
				{"token", defaultColumn},
				{"login", defaultColumn},
				{"password", defaultColumn},
				{"code2fa", defaultColumn},
				{"email", defaultColumn},
				{"email_pass", defaultColumn},
				{"recovery2fa", defaultColumn}
			};
			SQL.W3MakeTable(_project, tableStructure, tableName, false, schemaName:"");
			if (log) _project.SendInfoToLog($"Table {tableName} created", true);

			// Discord
			tableName = schemaName + "discord";
			tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{"cooldown", "INTEGER DEFAULT 0"},
				{"status", defaultColumn},
				{"last", defaultColumn},
				{"servers", defaultColumn},
				{"roles", defaultColumn},
				{"token", defaultColumn},
				{"login", defaultColumn},
				{"password", defaultColumn},
				{"code2fa", defaultColumn}
			};
			SQL.W3MakeTable(_project, tableStructure, tableName, false, schemaName:"");
			if (log) _project.SendInfoToLog($"Table {tableName} created", true);

			// Google
			tableName = schemaName + "google";
			tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{"cooldown", "INTEGER DEFAULT 0"},
				{"status", defaultColumn},
				{"last", defaultColumn},
				{"login", defaultColumn},
				{"password", defaultColumn},
				{"recovery_email", defaultColumn},
				{"code2fa", defaultColumn},
				{"recovery2fa", defaultColumn},
				{"icloud", defaultColumn}
			};
			SQL.W3MakeTable(_project, tableStructure, tableName, false, schemaName:"");
			if (log) _project.SendInfoToLog($"Table {tableName} created", true);

			// Blockchain Private
			tableName = schemaName + "blockchain_private";
			tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{"secp256k1", defaultColumn},
				{"base58", defaultColumn},
				{"bip39", defaultColumn}
			};
			SQL.W3MakeTable(_project, tableStructure, tableName, false, schemaName:"");
			if (log) _project.SendInfoToLog($"Table {tableName} created", true);

			// Blockchain Public
			tableName = schemaName + "blockchain_public";
			tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{"evm", defaultColumn},
				{"sol", defaultColumn},
				{"apt", defaultColumn},
				{"sui", defaultColumn},
				{"osmo", defaultColumn},
				{"xion", defaultColumn},
				{"ton", defaultColumn},
				{"taproot", defaultColumn}
			};
			SQL.W3MakeTable(_project, tableStructure, tableName, false, schemaName:"accounts");
			if (log) _project.SendInfoToLog($"Table {tableName} created", true);

			// Settings (без schemaName)
			tableName = schemaName + "settings";
			tableStructure = new Dictionary<string, string>
			{
				{"var", "TEXT PRIMARY KEY"},
				{"value", "TEXT DEFAULT ''"}
			};
			SQL.W3MakeTable(_project, tableStructure, tableName, false, schemaName:"");
			if (log) _project.SendInfoToLog($"Table {tableName} created", true);
		}
		private string ImportData(string tableName, string formTitle, string[] availableFields, Dictionary<string, string> columnMapping, string message = "Select format (one field per box):")
		{
			string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
			string table = schemaName + tableName;
			int lineCount = 0;

			// Создание формы
			System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = formTitle;
			form.Width = 800;
			form.Height = 700;
			form.TopMost = true; // Форма поверх всех окон
			form.Location = new System.Drawing.Point(108, 108);				

			List<string> selectedFormat = new List<string>();
			System.Windows.Forms.TextBox formatDisplay = new System.Windows.Forms.TextBox();
			System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();

			// Метка для выбора формата
			System.Windows.Forms.Label formatLabel = new System.Windows.Forms.Label();
			formatLabel.Text = message;
			formatLabel.AutoSize = true;
			formatLabel.Left = 10;
			formatLabel.Top = 10;
			form.Controls.Add(formatLabel);

			// Создаём ComboBox в строку
			System.Windows.Forms.ComboBox[] formatComboBoxes = new System.Windows.Forms.ComboBox[availableFields.Length - 1]; // -1 из-за пустой строки
			int spacing = 5;
			int totalSpacing = spacing * (formatComboBoxes.Length - 1);
			int comboWidth = (form.ClientSize.Width - 20 - totalSpacing) / formatComboBoxes.Length;
			for (int i = 0; i < formatComboBoxes.Length; i++)
			{
				formatComboBoxes[i] = new System.Windows.Forms.ComboBox();
				formatComboBoxes[i].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
				formatComboBoxes[i].Items.AddRange(availableFields);
				formatComboBoxes[i].SelectedIndex = 0;
				formatComboBoxes[i].Left = 10 + i * (comboWidth + spacing);
				formatComboBoxes[i].Top = 30;
				formatComboBoxes[i].Width = comboWidth;
				formatComboBoxes[i].SelectedIndexChanged += (s, e) =>
				{
					selectedFormat.Clear();
					foreach (var combo in formatComboBoxes)
					{
						if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
							selectedFormat.Add(combo.SelectedItem.ToString());
					}
					formatDisplay.Text = string.Join(":", selectedFormat);
				};
				form.Controls.Add(formatComboBoxes[i]);
			}

			// Поле для отображения текущего формата
			formatDisplay.Left = 10;
			formatDisplay.Top = 60;
			formatDisplay.Width = form.ClientSize.Width - 20;
			formatDisplay.ReadOnly = true;
			form.Controls.Add(formatDisplay);

			// Метка и поле для ввода данных
			System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
			dataLabel.Text = "Input data (one per line, matching format):";
			dataLabel.AutoSize = true;
			dataLabel.Left = 10;
			dataLabel.Top = 90;
			form.Controls.Add(dataLabel);

			dataInput.Left = 10;
			dataInput.Top = 110;
			dataInput.Width = form.ClientSize.Width - 20;
			dataInput.Multiline = true;
			dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			form.Controls.Add(dataInput);

			// Кнопка "OK"
			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "OK";
			okButton.Width = form.ClientSize.Width - 10;
			okButton.Height = 25;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
			okButton.Top = form.ClientSize.Height - okButton.Height - 5;
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);
			dataInput.Height = okButton.Top - dataInput.Top - 5;

			form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом
			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };
			// Показываем форму
			form.ShowDialog();
			// Если форма закрыта крестиком, прерываем выполнение
			if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				_project.SendInfoToLog($"Import to {tableName} cancelled by user", true);
				return "0";
			}

			// Собираем формат из ComboBox
			selectedFormat.Clear();
			foreach (var combo in formatComboBoxes)
			{
				if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
					selectedFormat.Add(combo.SelectedItem.ToString());
			}

			// Проверка введённых данных
			if (string.IsNullOrEmpty(dataInput.Text) || selectedFormat.Count == 0)
			{
				_project.SendWarningToLog("Data or format cannot be empty");
				return "0";
			}

			string[] lines = dataInput.Text.Trim().Split('\n');
			_project.SendInfoToLog($"Parsing [{lines.Length}] {tableName} data lines", true);

			// Обработка строк
			for (int acc0 = 1; acc0 <= lines.Length; acc0++)
			{
				string line = lines[acc0 - 1].Trim();
				if (string.IsNullOrWhiteSpace(line))
				{
					_project.SendWarningToLog($"Line {acc0} is empty", false);
					continue;
				}
				if (formTitle.Contains("proxy"))
				{
					try
					{
						string dbQuery = $@"UPDATE {table} SET proxy = '{line.Replace("'", "''")}' WHERE acc0 = {acc0};";
						SQL.W3Query(_project, dbQuery, true);
						lineCount++;
					}
					catch (Exception ex)
					{
						_project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}", false);
					}
				}
				else
				{
					string[] data_parts = line.Split(':');
					Dictionary<string, string> parsed_data = new Dictionary<string, string>();

					for (int i = 0; i < selectedFormat.Count && i < data_parts.Length; i++)
					{
						parsed_data[selectedFormat[i]] = data_parts[i].Trim();
					}

					// Формируем SQL-запрос на основе маппинга
					var queryParts = new List<string>();
					foreach (var field in columnMapping.Keys)
					{
						string value = parsed_data.ContainsKey(field) ? parsed_data[field].Replace("'", "''") : "";
						if (field == "CODE2FA" && value.Contains('/'))
							value = value.Split('/').Last();
						queryParts.Add($"{columnMapping[field]} = '{value}'");
					}
					//queryParts.Add("cooldown = 0");

					try
					{
						string dbQuery = $@"UPDATE {table} SET {string.Join(", ", queryParts)} WHERE acc0 = {acc0};";
						SQL.W3Query(_project, dbQuery, true);
						lineCount++;
					}
					catch (Exception ex)
					{
						_project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}", false);
					}
				}
			}

			_project.SendInfoToLog($"[{lineCount}] records added to [{table}]", true);
			return lineCount.ToString();
		}
		private string ImportTwitter()
		{
			string[] twitterFields = new string[] { "", "LOGIN", "PASSWORD", "EMAIL", "EMAIL_PASSWORD", "TOKEN", "CODE2FA", "RECOVERY_SEED" };
			var twitterMapping = new Dictionary<string, string>
			{
				{ "LOGIN", "login" },
				{ "PASSWORD", "password" },
				{ "EMAIL", "email" },
				{ "EMAIL_PASSWORD", "email_pass" },
				{ "TOKEN", "token" },
				{ "CODE2FA", "code2fa" },
				{ "RECOVERY_SEED", "recovery2fa" }
			};
			return ImportData("twitter", "Import Twitter Data", twitterFields, twitterMapping);
		}
		private string ImportDiscord()
		{
			string[] discordFields = new string[] { "", "LOGIN", "PASSWORD", "TOKEN", "CODE2FA" };
			var discordMapping = new Dictionary<string, string>
			{
				{ "LOGIN", "login" },
				{ "PASSWORD", "password" },
				{ "TOKEN", "token" },
				{ "CODE2FA", "code2fa" }
			};
			return ImportData("discord", "Import Discord Data", discordFields, discordMapping);
		}
		private string ImportGoogle()
		{
			string[] googleFields = new string[] { "", "LOGIN", "PASSWORD", "RECOVERY_EMAIL", "CODE2FA", "RECOVERY_SEED" };
			var googleMapping = new Dictionary<string, string>
			{
				{ "LOGIN", "login" },
				{ "PASSWORD", "password" },
				{ "RECOVERY_EMAIL", "recovery_email" },
				{ "CODE2FA", "code2fa" },
				{ "RECOVERY_SEED", "recovery2fa" }
			};
			return ImportData("google", "Import Google Data", googleFields, googleMapping);
		}
		private string ImportIcloud()
		{
			string[] fields = new string[] { "ICLOUD", "" };
			var mapping = new Dictionary<string, string>
			{
				{ "ICLOUD", "icloud" },
			};
			return ImportData("google", "Import Icloud", fields, mapping);
		}
		private string ImportNickname()
		{
			string[] fields = new string[] { "NICKNAME", "" };
			var mapping = new Dictionary<string, string>
			{
				{ "NICKNAME", "nickname" },
			};
			return ImportData("profile", "Import nickname", fields, mapping);
		}
		private string ImportBio()
		{
			string[] fields = new string[] { "BIO", "" };
			var mapping = new Dictionary<string, string>
			{
				{ "BIO", "bio" },
			};
			return ImportData("profile", "Import Bio", fields, mapping);
		}
		private string ImportProxy()
		{
			string[] fields = new string[] { "PROXY", "" };
			var mapping = new Dictionary<string, string>
			{
				{ "PROXY", "proxy" },
			};
			return ImportData("profile", "Import proxy ", fields, mapping, message:"Proxy format: http://login1:pass1@111.111.111.111:1111" );
		}
		private string ImportKeys(string keyType)
		{
			var acc0 = _project.Variables["acc0"];
			int rangeEnd = int.Parse(_project.Variables["rangeEnd"].Value);
			var blockchain = new Blockchain();

			string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
			string privateTable = schemaName + "blockchain_private";
			string publicTable = schemaName + "blockchain_public";
			acc0.Value = "1";

			// Создание формы
			System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = $"Import {keyType} keys";
			form.Width = 420;
			form.Height = 700;
			form.TopMost = true; // Форма поверх всех окон
			form.Location = new System.Drawing.Point(108, 108);			

			System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
			dataLabel.Text = $"Input {keyType} keys (one per line):";
			dataLabel.AutoSize = true;
			dataLabel.Left = 10;
			dataLabel.Top = 10;
			form.Controls.Add(dataLabel);

			System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();
			dataInput.Left = 10;
			dataInput.Top = 30;
			dataInput.Width = form.ClientSize.Width - 20;
			dataInput.Multiline = true;
			dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			dataInput.MaxLength = 1000000;
			form.Controls.Add(dataInput);

			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "OK";
			okButton.Width = form.ClientSize.Width - 10;
			okButton.Height = 25;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
			okButton.Top = form.ClientSize.Height - okButton.Height - 5;
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);
			dataInput.Height = okButton.Top - dataInput.Top - 5;

			form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

			form.ShowDialog();

			if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				_project.SendInfoToLog("Import cancelled by user", true);
				return "0";
			}

			if (string.IsNullOrEmpty(dataInput.Text))
			{
				_project.SendWarningToLog("Input could not be empty");
				return "0"; // Возвращаем "0", так как обработка не началась
			}

			string[] lines = dataInput.Text.Trim().Split('\n');
			_project.SendInfoToLog($"Parsing [{lines.Length}] strings", false);

			for (int i = 0; i < lines.Length && int.Parse(acc0.Value) <= rangeEnd; i++)
			{
				string key = lines[i].Trim();
				if (string.IsNullOrWhiteSpace(key)) continue;

				try
				{
					switch (keyType)
					{
						case "seed":
							string encodedSeed = SAFU.Encode(_project, key);
							SQL.W3Query(_project, $@"UPDATE {privateTable} SET bip39 = '{encodedSeed}' WHERE acc0 = {acc0.Value};", true);
							break;

						case "evm":
							string privateKey;
							string address;

							if (key.Split(' ').Length > 1) // Простая проверка на мнемонику
							{
								var mnemonicObj = new Mnemonic(key);
								var hdRoot = mnemonicObj.DeriveExtKey();
								var derivationPath = new NBitcoin.KeyPath("m/44'/60'/0'/0/0");
								privateKey = hdRoot.Derive(derivationPath).PrivateKey.ToHex();
							}
							else
							{
								privateKey = key;
							}

							string encodedEvmKey = SAFU.Encode(_project, privateKey);
							address = blockchain.GetAddressFromPrivateKey(privateKey);
							SQL.W3Query(_project, $@"UPDATE {privateTable} SET secp256k1 = '{encodedEvmKey}' WHERE acc0 = {acc0.Value};", true);
							SQL.W3Query(_project, $@"UPDATE {publicTable} SET evm = '{address}' WHERE acc0 = {acc0.Value};", true);
							break;

						case "sol":
							string encodedSolKey = SAFU.Encode(_project, key);
							SQL.W3Query(_project, $@"UPDATE {privateTable} SET base58 = '{encodedSolKey}' WHERE acc0 = {acc0.Value};", true);
							break;

						default:
							_project.SendWarningToLog($"Unknown key type: {keyType}");
							return lines.Length.ToString();
					}

					acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
				}
				catch (Exception ex)
				{
					_project.SendWarningToLog($"Error processing record {acc0.Value}: {ex.Message}", false);
					acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
				}
			}

			return lines.Length.ToString();
		}
		public string ImportAddressesOld()
		{
			var acc0 = _project.Variables["acc0"];
			int rangeEnd = int.Parse(_project.Variables["rangeEnd"].Value);

			string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
			string tableName = schemaName + "blockchain_public";

			acc0.Value = "1";

			// Создание формы
			System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = "chose type and input keys devided by new line, than ckick OK. Close window to continue";
			
			form.Width = 1008;
			form.Height = 1008;
			form.TopMost = true;
			form.Location = new System.Drawing.Point(108, 108);

			System.Windows.Forms.Label typeLabel = new System.Windows.Forms.Label();
			typeLabel.Text = "Select address type:";
			typeLabel.AutoSize = true;
			typeLabel.Left = 10;
			typeLabel.Top = 10;
			form.Controls.Add(typeLabel);

			System.Windows.Forms.ComboBox typeComboBox = new System.Windows.Forms.ComboBox();
			typeComboBox.Left = 10;
			typeComboBox.Top = 30;
			typeComboBox.Width = form.ClientSize.Width - 20;
			typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			var addressTypes = new[] { "evm", "sol", "apt", "sui" };
			typeComboBox.Items.AddRange(addressTypes);
			typeComboBox.SelectedIndex = 0;
			form.Controls.Add(typeComboBox);

			System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
			dataLabel.Text = "Input addresses (one per line):";
			dataLabel.AutoSize = true;
			dataLabel.Left = 10;
			dataLabel.Top = 60;
			form.Controls.Add(dataLabel);

			System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();
			dataInput.Left = 10;
			dataInput.Top = 80;
			dataInput.Width = form.ClientSize.Width - 20;
			dataInput.Multiline = true;
			dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			dataInput.MaxLength = 1000000;
			form.Controls.Add(dataInput);

			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "OK";
			okButton.Width = form.ClientSize.Width - 20;
			okButton.Height = 25;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
			okButton.Top = form.ClientSize.Height - okButton.Height - 5;
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);
			dataInput.Height = okButton.Top - dataInput.Top - 5;

			string placeholder = "Chose type and input keys devided by new line, than ckick OK. Close window to continue\r\nВыбери тип адреса и вставь адсеса в форму каждый с новой строки. Нажми ОК. Повтори для нужных типов адресов и закрой окно";
			// Установка плейсхолдера
			if (string.IsNullOrEmpty(dataInput.Text)) // Если поле пустое, показываем плейсхолдер
			{
				dataInput.Text = placeholder;
				dataInput.ForeColor = System.Drawing.Color.Gray;
				//dataInput.Font = new System.Drawing.Font("Lucida Console", 10, System.Drawing.FontStyle.Bold);
			}
			dataInput.Enter += (s, e) => { if (dataInput.Text == placeholder) { dataInput.Text = ""; dataInput.ForeColor = System.Drawing.Color.Black; } };
			dataInput.Leave += (s, e) => { if (string.IsNullOrEmpty(dataInput.Text)) { dataInput.Text = placeholder; dataInput.ForeColor = System.Drawing.Color.Gray; } };			

			form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

			show:
			form.ShowDialog();

			if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				_project.SendInfoToLog("Import cancelled by user", true);
				return "0";
			}

			if (string.IsNullOrEmpty(dataInput.Text))
			{
				_project.SendWarningToLog("Input could not be empty");
				goto show;
			}

			string selectedType = typeComboBox.SelectedItem.ToString();
			string[] lines = dataInput.Text.Trim().Split('\n');
			_project.SendInfoToLog($"Parsing [{lines.Length}] addresses for {selectedType}", false);

			for (int i = 0; i < lines.Length && int.Parse(acc0.Value) <= rangeEnd; i++)
			{
				string address = lines[i].Trim();
				// Пустые строки записываются как '' без пропуска
				try
				{
					_project.SendInfoToLog($"Processing acc0 = {acc0.Value}, address = '{address}'", false);
					SQL.W3Query(_project, $@"UPDATE {tableName} SET {selectedType} = '{address}' WHERE acc0 = {acc0.Value};", true);
					acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
				}
				catch (Exception ex)
				{
					_project.SendWarningToLog($"Error processing record {acc0.Value} for {selectedType}: {ex.Message}", false);
					acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
				}
			}
			_project.SendInfoToLog($"Imported {lines.Length} strings", true);
			goto show;
		}

        public string ImportAddresses()
        {
            var acc0 = _project.Variables["acc0"];
            int rangeEnd = int.Parse(_project.Variables["rangeEnd"].Value);
            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
            string tableName = schemaName + "blockchain_public";

            acc0.Value = "1";

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Import Addresses";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            // Поле для ввода имени столбца
            System.Windows.Forms.Label columnLabel = new System.Windows.Forms.Label();
            columnLabel.Text = "Column name (e.g., evm, sol):";
            columnLabel.AutoSize = true;
            columnLabel.Left = 10;
            columnLabel.Top = 10;
            form.Controls.Add(columnLabel);

            System.Windows.Forms.TextBox columnInput = new System.Windows.Forms.TextBox();
            columnInput.Left = 10;
            columnInput.Top = 30;
            columnInput.Width = form.ClientSize.Width - 20;
            columnInput.Text = "input address label here ex: evm | apt |sol ";//_project.Variables["addressType"].Value; // Предполагаем, что переменная существует
            form.Controls.Add(columnInput);

            // Поле для ввода адресов
            System.Windows.Forms.Label addressLabel = new System.Windows.Forms.Label();
            addressLabel.Text = "Addresses (one per line):";
            addressLabel.AutoSize = true;
            addressLabel.Left = 10;
            addressLabel.Top = 60;
            form.Controls.Add(addressLabel);

            System.Windows.Forms.TextBox addressInput = new System.Windows.Forms.TextBox();
            addressInput.Left = 10;
            addressInput.Top = 80;
            addressInput.Width = form.ClientSize.Width - 20;
            addressInput.Multiline = true;
            addressInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            addressInput.MaxLength = 1000000;
            form.Controls.Add(addressInput);

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 20;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            addressInput.Height = okButton.Top - addressInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            // Проверка ввода
            if (string.IsNullOrEmpty(columnInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Column name or addresses cannot be empty");
                return "0";
            }

            // Формирование имени столбца
            string columnName = columnInput.Text.ToLower();

            // Создание таблицы (если нужно)
            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {columnName, "TEXT DEFAULT ''"}
            };
            SQL.W3MakeTable(_project, tableStructure, tableName);

            // Обработка адресов
            string[] lines = addressInput.Text.Trim().Split('\n');
            int lineCount = 0;

            for (int i = 0; i < lines.Length && int.Parse(acc0.Value) <= rangeEnd; i++)
            {
                string address = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(address))
                {
                    _project.SendWarningToLog($"Line {acc0.Value} is empty");
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                    continue;
                }

                try
                {
                    _project.SendInfoToLog($"Processing acc0 = {acc0.Value}, address = '{address}'", false);
                    SQL.W3Query(_project, $@"UPDATE {tableName} SET {columnName} = '{address}' WHERE acc0 = {acc0.Value};", true);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                    lineCount++;
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing record {acc0.Value} for {columnName}: {ex.Message}", false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
            }

            _project.SendInfoToLog($"[{lineCount}] strings added to [{tableName}]", true);
            return lineCount.ToString();
        }
		public string ImportDepositAddresses()
		{
			string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : null) + "cex_deps";
			// Создание формы
			System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = "Import Deposit Addresses";
			form.Width = 420;
			form.Height = 700;
			form.TopMost = true; // Форма поверх всех окон
			form.Location = new System.Drawing.Point(108, 108);		

			// Поле для ввода CHAIN
			System.Windows.Forms.Label chainLabel = new System.Windows.Forms.Label();
			chainLabel.Text = "Chain (e.g., ETH, BSC):";
			chainLabel.AutoSize = true;
			chainLabel.Left = 10;
			chainLabel.Top = 10;
			form.Controls.Add(chainLabel);

			System.Windows.Forms.TextBox chainInput = new System.Windows.Forms.TextBox();
			chainInput.Left = 10;
			chainInput.Top = 30;
			chainInput.Width = form.ClientSize.Width - 20;
			chainInput.Text = _project.Variables["depositChain"].Value; // Текущее значение из переменной
			form.Controls.Add(chainInput);

			// Поле для ввода CEX
			System.Windows.Forms.Label cexLabel = new System.Windows.Forms.Label();
			cexLabel.Text = "CEX (e.g., binance, kucoin):";
			cexLabel.AutoSize = true;
			cexLabel.Left = 10;
			cexLabel.Top = 60;
			form.Controls.Add(cexLabel);

			System.Windows.Forms.TextBox cexInput = new System.Windows.Forms.TextBox();
			cexInput.Left = 10;
			cexInput.Top = 80;
			cexInput.Width = form.ClientSize.Width - 20;
			cexInput.Text = _project.Variables["depositCEX"].Value; // Текущее значение из переменной
			form.Controls.Add(cexInput);

			// Поле для ввода адресов
			System.Windows.Forms.Label addressLabel = new System.Windows.Forms.Label();
			addressLabel.Text = "Deposit addresses (one per line):";
			addressLabel.AutoSize = true;
			addressLabel.Left = 10;
			addressLabel.Top = 110;
			form.Controls.Add(addressLabel);

			System.Windows.Forms.TextBox addressInput = new System.Windows.Forms.TextBox();
			addressInput.Left = 10;
			addressInput.Top = 130;
			addressInput.Width = form.ClientSize.Width - 20;
			addressInput.Multiline = true;
			addressInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			form.Controls.Add(addressInput);

			// Кнопка "OK"
			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "OK";
			okButton.Width = form.ClientSize.Width - 10;
			okButton.Height = 25;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
			okButton.Top = form.ClientSize.Height - okButton.Height - 5;
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);
			addressInput.Height = okButton.Top - addressInput.Top - 5;
			form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

			form.ShowDialog();

			if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				_project.SendInfoToLog("Import cancelled by user", true);
				return "0";
			}

			// Проверка ввода
			if (string.IsNullOrEmpty(chainInput.Text) || string.IsNullOrEmpty(cexInput.Text) || string.IsNullOrEmpty(addressInput.Text))
			{
				_project.SendWarningToLog("Chain, CEX, or addresses cannot be empty");
				return "0";
			}

			// Формирование имени столбца
			string CHAIN = chainInput.Text.ToLower();
			string CEX = cexInput.Text.ToLower();
			string columnName = $"{CEX}_{CHAIN}";

			// Создание таблицы
			var tableStructure = new Dictionary<string, string>
			{
				{"acc0", "INTEGER PRIMARY KEY"},
				{columnName, "TEXT DEFAULT ''"}
			};
			SQL.W3MakeTable(_project, tableStructure, table); // Используем полное имя с схемой

			// Обработка адресов
			string[] lines = addressInput.Text.Trim().Split('\n');
			int lineCount = 0;

			for (int acc0 = 1; acc0 <= lines.Length; acc0++) // Начинаем с 1, как в других методах
			{
				string line = lines[acc0 - 1].Trim();
				if (string.IsNullOrWhiteSpace(line))
				{
					_project.SendWarningToLog($"Line {acc0} is empty");
					continue;
				}

				try
				{
					SQL.W3Query(_project, $@"UPDATE {table} SET
						{columnName} = '{line}'
						WHERE acc0 = {acc0};");
					lineCount++;
				}
				catch (Exception ex)
				{
					_project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}");
					continue;
				}
			}

			_project.SendInfoToLog($"[{lineCount}] strings added to [{table}]", true);
			return lineCount.ToString();
		}
		private void ImportWebGL(Instance instance, string tableName = "profile")
		{
			
			_project.SendWarningToLog("This function works only on ZennoPoster versions below 7.8", true);
			_instance.CanvasRenderMode = ZennoLab.InterfacesLibrary.Enums.Browser.CanvasMode.Allow;

			string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
			string table = schemaName + tableName;

			// Создание формы для выбора вендора
			System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = "Import WebGL Data";
			form.Width = 420;
			form.Height = 200; // Компактная форма, так как только выбор вендора
			form.TopMost = true; // Форма поверх всех окон
			form.Location = new System.Drawing.Point(108, 108);					

			System.Windows.Forms.Label vendorLabel = new System.Windows.Forms.Label();
			vendorLabel.Text = "Select WebGL Vendor:";
			vendorLabel.AutoSize = true;
			vendorLabel.Left = 10;
			vendorLabel.Top = 10;
			form.Controls.Add(vendorLabel);

			System.Windows.Forms.ComboBox vendorComboBox = new System.Windows.Forms.ComboBox();
			vendorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			vendorComboBox.Items.AddRange(new string[] { "Intel", "NVIDIA", "AMD" });
			vendorComboBox.SelectedIndex = 0; // По умолчанию Intel
			vendorComboBox.Left = 10;
			vendorComboBox.Top = 30;
			vendorComboBox.Width = form.ClientSize.Width - 20;
			form.Controls.Add(vendorComboBox);

			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "OK";
			okButton.Width = form.ClientSize.Width - 10;
			okButton.Height = 25;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
			okButton.Top = form.ClientSize.Height - okButton.Height - 5;
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);
			form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

			form.ShowDialog();

			if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				_project.SendInfoToLog("Import cancelled by user", true);
				return;
			}

			string webGLvendor = vendorComboBox.SelectedItem.ToString();
			int targetCount = int.Parse(_project.Variables["rangeEnd"].Value);
			List<string> tempList = new List<string>();

			while (tempList.Count < targetCount)
			{
				_project.SendToLog($"Parsing {webGLvendor} WebGL. {tempList.Count}/{_project.Variables["rangeEnd"].Value} strings parsed", LogType.Info, true, LogColor.Default);
				try { _instance.Launch(ZennoLab.InterfacesLibrary.Enums.Browser.BrowserType.Firefox45, false); } catch { }
				_instance.Launch(ZennoLab.InterfacesLibrary.Enums.Browser.BrowserType.Chromium, false);
				string webglData = _instance.WebGLPreferences.Save();
				_project.SendInfoToLog(webglData);
				if (webglData.Contains(webGLvendor)) tempList.Add(webglData);
			}

			for (int acc0 = 0; acc0 < tempList.Count; acc0++)
			{
				string webGLrenderer = tempList[acc0];
				if (string.IsNullOrEmpty(webGLrenderer)) continue;
				SQL.W3Query(_project, $@"UPDATE {table} SET webgl = '{tempList[acc0].Replace("'", "''")}' WHERE acc0 = {acc0 + 1};");
			}

			_project.SendInfoToLog($"[{tempList.Count}] strings added to [{table}]", true);
			_instance.CanvasRenderMode = ZennoLab.InterfacesLibrary.Enums.Browser.CanvasMode.Block;
			try { _instance.Launch(ZennoLab.InterfacesLibrary.Enums.Browser.BrowserType.WithoutBrowser, false); } catch { }
		}
		private void ImportSettings(string message = "input data please", int width = 600, int height = 400)
		{
			_project.SendInfoToLog($"Opening variables input dialog: {message}", true);

			System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = message;
			form.Width = width;
			form.Height = height;
			form.TopMost = true;
			form.Location = new System.Drawing.Point(108, 108);

			// Массив с именами переменных и плейсхолдерами
			(string varName, string placeholder)[] variableNames = new (string, string)[]
			{
				("settingsApiFirstMail", "API First Mail для доступа к переадресованной почте"),
				("settingsApiPerplexity", "API perplexity для запросов к AI (например прогрева твиттера)"),
				("settingsDsInviteOwn", "Инвайт на свой сервер"),
				("settingsDsOwnServer", "ID канала с инвайтами на вашем сервере"),
				("settingsFmailLogin", "Логин от общего ящика для форвардов на FirstMail"),
				("settingsFmailPass", "Пароль от общего ящика для форвардов на FirstMail"),
				("settingsTgLogGroup", "Id группы для логов в Telegram. Формат {-1002000000009}"),
				("settingsTgLogToken", "Токен Telegram логгера"),
				("settingsTgLogTopic", "Id топика в группе для логов. 0 - если нет топиков"),
				("settingsTgMailGroup", "Id группы с переадресованной почтой в Telegram. Формат {-1002000000009}"),
				("settingsZenFolder", "Путь к папке с профилями и причастным данным. Формат: {F:\\farm\\}"),
				("settingsApiBinance", "Данные для вывода с Binance. Формат: {API_KEY;SECRET_KEY;PROXY}")
			};

			var textBoxes = new Dictionary<string, System.Windows.Forms.TextBox>();

			int currentTop = 5;
			int labelWidth = 150;
			int textBoxWidth = 400;
			int spacing = 5;

			foreach (var (varName, placeholder) in variableNames)
			{
				System.Windows.Forms.Label label = new System.Windows.Forms.Label();
				label.Text = varName + ":";
				label.AutoSize = true;
				label.Left = 5;
				label.Top = currentTop;
				form.Controls.Add(label);

				System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
				textBox.Left = label.Left + labelWidth + spacing;
				textBox.Top = currentTop;
				textBox.Width = textBoxWidth;
				textBox.Text = _project.Variables[varName].Value;

				// Установка плейсхолдера
				if (string.IsNullOrEmpty(textBox.Text)) // Если поле пустое, показываем плейсхолдер
				{
					textBox.Text = placeholder;
					textBox.ForeColor = System.Drawing.Color.Gray;
				}
				textBox.Enter += (s, e) => { if (textBox.Text == placeholder) { textBox.Text = ""; textBox.ForeColor = System.Drawing.Color.Black; } };
				textBox.Leave += (s, e) => { if (string.IsNullOrEmpty(textBox.Text)) { textBox.Text = placeholder; textBox.ForeColor = System.Drawing.Color.Gray; } };

				form.Controls.Add(textBox);

				textBoxes[varName] = textBox;
				currentTop += textBox.Height + spacing;
			}

			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "OK";
			okButton.Width = 50;
			okButton.Height = 25;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
			okButton.Top = currentTop + 10;
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);

			int requiredHeight = okButton.Top + okButton.Height + 40;
			if (form.Height < requiredHeight)
			{
				form.Height = requiredHeight;
			}
			form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

			form.ShowDialog();

			if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				_project.SendInfoToLog("Import cancelled by user", true);
				return;
			}

			string tableName = "settings";
			foreach (var (varName, placeholder) in variableNames)
			{
				string newValue = textBoxes[varName].Text;
				if (newValue == placeholder) newValue = ""; // Если текст — это плейсхолдер, считаем поле пустым
				_project.Variables[varName].Value = newValue;
				_project.SendInfoToLog($"Updated variable {varName}: {newValue}", true);

				if (!string.IsNullOrEmpty(newValue))
				{
					string escapedValue = newValue.Replace("'", "''");
					SQL.W3Query(_project, $"INSERT OR REPLACE INTO {tableName} (var, value) VALUES ('{varName}', '{escapedValue}');");
					_project.SendInfoToLog($"Inserted into {tableName}: {varName} = {newValue}", true);
				}
			}
		}
		public void ShowBalanceTable(string chains = null)
		{
			string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? "accounts." : "";
			string tableName = schemaName + "native";
			var columns = new List<string>();
			
			if (string.IsNullOrEmpty(chains)) 
			{
				if (_project.Variables["DBmode"].Value == "PostgreSQL") chains = SQL.W3Query(_project,$@"SELECT column_name FROM information_schema.columns WHERE table_schema = 'accounts' AND table_name = 'native'");
				else chains = SQL.W3Query(_project,$@"SELECT name FROM pragma_table_info('native')");
				columns = chains.Split('\n').ToList();
			}
			else 
				columns = chains.Split(',').ToList();

			// Пагинация
			int pageSize = 100;
			int offset = 0;
			int totalRows = 0;
			bool isFirstLoad = true; // Флаг для первой загрузки

			// Подсчёт общего количества строк
			string countQuery = $@"SELECT COUNT(*) FROM {tableName} WHERE acc0 <= {_project.Variables["rangeEnd"].Value}";
			string countResult = SQL.W3Query(_project, countQuery, true);
			if (!string.IsNullOrEmpty(countResult) && int.TryParse(countResult, out int count))
			{
				totalRows = count;
			}

			// Создание формы
			var form = new System.Windows.Forms.Form
			{
				Text = "Balance Table",
				Width = 1008,
				Height = 1008,
				StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
				//BackColor = System.Drawing.Color.White
			};
			form.BackColor = System.Drawing.Color.White;
			form.TopMost = true; // Форма поверх всех окон

			// Панель для пагинации
			var panel = new System.Windows.Forms.Panel
			{
				Dock = System.Windows.Forms.DockStyle.Bottom,
				Height = 40
			};
			form.Controls.Add(panel);

			// Кнопки пагинации
			var prevButton = new System.Windows.Forms.Button
			{
				Text = "Previous",
				Width = 100,
				Height = 30,
				Left = 10,
				Top = 5,
				Enabled = false
			};
			var nextButton = new System.Windows.Forms.Button
			{
				Text = "Next",
				Width = 100,
				Height = 30,
				Left = 120,
				Top = 5
			};
			panel.Controls.Add(prevButton);
			panel.Controls.Add(nextButton);

			// DataGridView
			var grid = new System.Windows.Forms.DataGridView
			{
				Dock = System.Windows.Forms.DockStyle.Fill,
				AllowUserToResizeColumns = true,
				AllowUserToResizeRows = true,
				ScrollBars = System.Windows.Forms.ScrollBars.Both,
				AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells,
				ReadOnly = true,
				BackgroundColor = System.Drawing.Color.White
			};
			form.Controls.Add(grid);

			// Убедимся, что панель не перекрывает таблицу
			grid.BringToFront();

			// Метод загрузки данных
			Action loadData = () =>
			{
				string query = $@"SELECT {string.Join(",", columns)} FROM {tableName} 
								WHERE acc0 <= {_project.Variables["rangeEnd"].Value} 
								ORDER BY acc0 LIMIT {pageSize} OFFSET {offset}";
				string result = SQL.W3Query(_project, query, true);

				if (string.IsNullOrEmpty(result))
				{
					_project.SendWarningToLog("No data found in balance table");
					grid.Rows.Clear();
					return;
				}

				var rows = result.Trim().Split('\n');
				_project.SendInfoToLog($"Loaded {rows.Length} rows from {tableName}, offset {offset}", false);

				// Очистка таблицы
				grid.Columns.Clear();
				grid.Rows.Clear();

				// Настройка колонок
				foreach (var col in columns)
				{
					var column = new System.Windows.Forms.DataGridViewColumn
					{
						Name = col.Trim(),
						HeaderText = col.Trim(),
						CellTemplate = new System.Windows.Forms.DataGridViewTextBoxCell(),
						SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable // Отключаем сортировку
					};
					grid.Columns.Add(column);
				}

				// Заполнение данными
				foreach (var row in rows)
				{
					var values = row.Split('|');
					if (values.Length != columns.Count)
					{
						_project.SendWarningToLog($"Invalid row format: {row}. Expected {columns.Count} columns, got {values.Length}", false);
						continue;
					}

					// Форматирование значений (кроме acc0)
					var formattedValues = new string[values.Length];
					formattedValues[0] = values[0]; // acc0 без изменений
					for (int i = 1; i < values.Length; i++)
					{
						if (string.IsNullOrWhiteSpace(values[i]))
						{
							formattedValues[i] = "0.0000000";
							continue;
						}
						try
						{
							string val = values[i].Replace(",", ".");
							if (decimal.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
							{
								formattedValues[i] = balance.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture);
							}
							else
							{
								formattedValues[i] = "0.0000000";
								_project.SendWarningToLog($"Invalid format in {columns[i]}, row {grid.Rows.Count + 1}: '{values[i]}'", false);
							}
						}
						catch (Exception ex)
						{
							formattedValues[i] = "0.0000000";
							_project.SendWarningToLog($"Error formatting in {columns[i]}, row {grid.Rows.Count + 1}: {ex.Message}", false);
						}
					}
					grid.Rows.Add(formattedValues);

				}

				// Обновление кнопок
				prevButton.Enabled = offset > 0;
				nextButton.Enabled = offset + pageSize < totalRows;

				// Установка ширины формы только при первой загрузке
				// if (isFirstLoad)
				// {
				// 	int totalWidth = grid.Columns.Cast<System.Windows.Forms.DataGridViewColumn>().Sum(col => col.Width);
				// 	form.Width = Math.Min(totalWidth + 0, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width);
				// 	_project.SendInfoToLog($"Set form width: {form.Width}, total columns width: {totalWidth}", false);
				// 	isFirstLoad = false;
				// }
				int totalWidth = grid.Columns.Cast<System.Windows.Forms.DataGridViewColumn>().Sum(col => col.Width);
				grid.Width = totalWidth; // Устанавливаем ширину таблицы
				form.Width = Math.Min(totalWidth - 100, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width); // Учитываем скроллбар (~20px)
			};

			// Загрузка первой страницы
			loadData();

			// Обработчики кнопок
			prevButton.Click += (s, e) =>
			{
				if (offset >= pageSize)
				{
					offset -= pageSize;
					loadData();
				}
			};
			nextButton.Click += (s, e) =>
			{
				if (offset + pageSize < totalRows)
				{
					offset += pageSize;
					loadData();
				}
			};

			// Настройка цветов
			grid.CellFormatting += (s, e) =>
			{
				if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
				if (grid.Columns[e.ColumnIndex].Name == "acc0")
					{
						// Здесь задаём стиль для acc0
						e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9, System.Drawing.FontStyle.Bold);
						e.CellStyle.BackColor = System.Drawing.Color.Black; // Пример фона
						e.CellStyle.ForeColor = System.Drawing.Color.White; // Пример цвета текста
						return;
					}

				string value = grid[e.ColumnIndex, e.RowIndex].Value?.ToString();
				if (string.IsNullOrWhiteSpace(value)) return;

				try
				{
					// Парсинг уже отформатированного значения
					if (decimal.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
					{
						e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9);
						if (balance >= 0.1m)
							e.CellStyle.BackColor = System.Drawing.Color.SteelBlue;
						else if (balance >= 0.01m)
							e.CellStyle.BackColor = System.Drawing.Color.Green;
						else if (balance >= 0.001m)
							e.CellStyle.BackColor = System.Drawing.Color.YellowGreen;
						else if (balance >= 0.0001m)
							e.CellStyle.BackColor = System.Drawing.Color.Khaki;							
						else if (balance >= 0.00001m)
							e.CellStyle.BackColor = System.Drawing.Color.LightSalmon;
						else if (balance > 0)
							e.CellStyle.BackColor = System.Drawing.Color.IndianRed;							
						else if (balance == 0)
							{e.CellStyle.BackColor = System.Drawing.Color.White;											
							e.CellStyle.ForeColor = System.Drawing.Color.White;}									
						else
							e.CellStyle.BackColor = System.Drawing.Color.White;
					}
					else
					{
						_project.SendWarningToLog($"Invalid balance format in {grid.Columns[e.ColumnIndex].Name}, row {e.RowIndex + 1}: '{value}'", false);
					}
				}
				catch (Exception ex)
				{
					_project.SendWarningToLog($"Error parsing balance in {grid.Columns[e.ColumnIndex].Name}, row {e.RowIndex + 1}: {ex.Message}", false);
				}
			};

			// Показ формы
			form.ShowDialog();
		}
		private bool ImportStart()
		{
			System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = "!!!Warning";
			form.Width = 400;
			form.Height = 200;
			form.BackColor = System.Drawing.Color.Red;
			form.TopMost = true; // Форма поверх всех окон
			//form.Location = new System.Drawing.Point(108, 108);		
			form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen; // Центрируем окно

			System.Windows.Forms.Label messageLabel = new System.Windows.Forms.Label();
			messageLabel.Text = "Db data will be overwritten!\nContinue?";
			messageLabel.Font = new System.Drawing.Font("Consolas", 15, System.Drawing.FontStyle.Bold); // Жирный шрифт
			messageLabel.ForeColor = System.Drawing.Color.White;
			messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			messageLabel.AutoSize = true;
			messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2; // Центрируем по горизонтали
			messageLabel.Top = 30; // Отступ сверху
			form.Controls.Add(messageLabel);

			messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2;

			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "YES";
			okButton.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold);
			okButton.ForeColor = System.Drawing.Color.White;
			okButton.BackColor = System.Drawing.Color.Red;
			okButton.Width = 100;
			okButton.Height = 30;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 4; // Центрируем кнопку
			okButton.Top = form.ClientSize.Height - okButton.Height - 40; // Отступ снизу
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);
			
			
			System.Windows.Forms.Button cancelButton = new System.Windows.Forms.Button();
			cancelButton.Text = "Cancel";
			cancelButton.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold);
			cancelButton.ForeColor = System.Drawing.Color.White;
			cancelButton.BackColor = System.Drawing.Color.Black;
			cancelButton.Width = 100;
			cancelButton.Height = 30;
			cancelButton.Left = (form.ClientSize.Width - cancelButton.Width) / 4 * 3; // Центрируем кнопку
			cancelButton.Top = form.ClientSize.Height - cancelButton.Height - 40; // Отступ снизу
			cancelButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.Cancel; form.Close(); };
			form.Controls.Add(cancelButton);			
			
			

			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };
			form.ShowDialog();

			if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				_project.SendInfoToLog("Import cancelled by user", true);
				return false;
			}
			return true;
		}
		private bool ImportDone()
		{
						System.Windows.Forms.Form form = new System.Windows.Forms.Form();
			form.Text = "Done";
			form.Width = 500;
			form.Height = 200;
			form.BackColor = System.Drawing.Color.Green;
			form.TopMost = true;
			form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen; // Центрируем окно

			System.Windows.Forms.Label messageLabel = new System.Windows.Forms.Label();
			messageLabel.Text = "Db data imported successful.\nPlease switch off \"CreateDb\" option\n in \"Import\" tab of script settings";
			messageLabel.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold); // Жирный шрифт
			messageLabel.ForeColor = System.Drawing.Color.White;
			messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			messageLabel.AutoSize = true;
			messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2; // Центрируем по горизонтали
			messageLabel.Top = 20; // Отступ сверху
			form.Controls.Add(messageLabel);

			messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2;

			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
			okButton.Text = "Nice";
			okButton.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold);
			okButton.ForeColor = System.Drawing.Color.White;
			okButton.BackColor = System.Drawing.Color.Black;
			okButton.Width = 100;
			okButton.Height = 30;
			okButton.Left = (form.ClientSize.Width - okButton.Width) / 2; // Центрируем кнопку
			okButton.Top = form.ClientSize.Height - okButton.Height - 40; // Отступ снизу
			okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
			form.Controls.Add(okButton);
			

			form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

			form.ShowDialog();
			return true;
		}

	}

}
