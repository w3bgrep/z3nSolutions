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



using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ZBSolutions;
using static ZBSolutions.InstanceExtensions;
using NBitcoin;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

#endregion

namespace w3tools //by @w3bgrep
{
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

public static class TestStatic
{

        public static void Go(this Instance instance, string url, bool strict = false)
        {
            bool go = false;
            string current = instance.ActiveTab.URL;
            if (strict) if (current != url) go = true;
            if (!strict) if (!current.Contains(url)) go = true;
            if (go) instance.ActiveTab.Navigate(url, "");
        }


}




    public class Url
    {
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

	public class DatabaseTransfer
{
    private readonly Sql _sql;
    private readonly IZennoPosterProjectModel _project;

    public DatabaseTransfer(IZennoPosterProjectModel project)
    {
        _project = project;
        _sql = new Sql(project,true);
    }

    public void TransferTables(string[] tableNames = null)
    {
        _project.Variables["DBmode"].Value = "SQLite";
        
		if (tableNames == null){
			    string tablesQuery = "SELECT name FROM sqlite_master WHERE type='table';";
        		string tablesResult = _sql.DbQ(tablesQuery,true);
				tableNames = tablesResult.Split('\n');
			}
        
        for (int i = 0; i < tableNames.Length; i++)
        {
            string tableName = tableNames[i];
            _project.L0g(tableName);
            if (tableName == "")  continue;

            if (tableName.StartsWith("_"))
            {
                string newTableName = tableName.Substring(1).ToLower();
                string targetSchema = "projects";               
                CopyTable(tableName, targetSchema, newTableName);
            }
			else if (tableName.StartsWith("farm"))
            {
                string newTableName = tableName.Substring(4).ToLower();
                string targetSchema = "projects";            
                CopyTable(tableName, targetSchema, newTableName);
            }
            else if (tableName.StartsWith("acc"))
            {
                string newTableName = tableName.Substring(3).ToLower();
                string targetSchema = "accounts";            
                CopyTable(tableName, targetSchema, newTableName);
            }
        }
    }

    private void CopyTable(string sourceTable, string targetSchema, string targetTable)
    {
        _project.Variables["DBmode"].Value = "SQLite";
        
        string columnsQuery = $"PRAGMA table_info('{sourceTable}');";
        string columnsResult = _sql.DbQ(columnsQuery,true);
		
        
        string[] columnRows = columnsResult.Split('\n');
        string columnNames = "";
        
        for (int i = 0; i < columnRows.Length; i++)
        {
            if (columnRows[i] == "")
            {
                continue;
            }
            
            // Каждая строка содержит данные в формате cid|name|type|notnull|dflt_value|pk
            string[] columnData = columnRows[i].Split('|');
            string columnName = columnData[1];
            
            if (columnNames != "")
            {
                columnNames += ", ";
            }
            columnNames += columnName;
        }

        string selectQuery = $"SELECT {columnNames} FROM {sourceTable};";
		_project.L0g(selectQuery);
        string dataResult = _sql.DbQ(selectQuery);
        
        _project.Variables["DBmode"].Value = "PostgreSQL";
        
		string createTableQuery = $"CREATE TABLE IF NOT EXISTS {targetSchema}.{targetTable} (";
		string[] columns = columnNames.Split(',');
		for (int i = 0; i < columns.Length; i++)
		{
			if (i > 0)
			{
				createTableQuery += ", ";
			}
			string columnName = columns[i].Trim();
			// Если столбец называется "acc0", задаем тип INTEGER, иначе TEXT
			if (columnName == "acc0")
			{
				createTableQuery += columnName + " INTEGER PRIMARY KEY";
			}
			else
			{
				createTableQuery += columnName + " TEXT";
			}
		}
		createTableQuery += ");";
		_sql.DbQ(createTableQuery);

		// Если есть данные, вставляем их
		if (dataResult != "")
		{
			string[] dataRows = dataResult.Split('\n');
			
			// Обрабатываем каждую строку данных
			for (int i = 0; i < dataRows.Length; i++)
			{
				if (dataRows[i] == "")
				{
					continue;
				}
				
				// Формируем запрос на вставку
				string insertQuery = $"INSERT INTO {targetSchema}.{targetTable} ({columnNames}) VALUES (";
				string[] rowValues = dataRows[i].Split('|');
				
				for (int j = 0; j < rowValues.Length; j++)
				{
					if (j > 0)
					{
						insertQuery += ", ";
					}
					string columnName = columns[j].Trim();
					string value = rowValues[j];
					
					// Для столбца acc0 не добавляем кавычки, так как это INTEGER
					if (columnName == "acc0")
					{
						// Если значение пустое, вставляем NULL
						if (value == "")
						{
							insertQuery += "NULL";
						}
						else
						{
							insertQuery += value;
						}
					}
					else
					{
						// Для текстовых столбцов экранируем значения
						value = value.Replace("'", "''");
						insertQuery += $"'{value}'";
					}
				}
				insertQuery += ");";
				
				// Выполняем вставку
				_sql.DbQ(insertQuery);
			}
		}
    }
}

 public class X2
 {
     protected readonly IZennoPosterProjectModel _project;
     protected readonly Instance _instance;
     protected readonly bool _logShow;
     protected readonly Sql _sql;

     protected string _status;
     protected string _token;
     protected string _login;
     protected string _pass;
     protected string _2fa;
     protected string _email;
     protected string _email_pass;

     public X2(IZennoPosterProjectModel project, Instance instance, bool log = false)
     {
                   
         _project = project;
         _instance = instance;
         _sql = new Sql(_project);
         _logShow = log;
         
         LoadCreds();

     }

     protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
     {
         if (!_logShow && !log) return;
         var stackFrame = new System.Diagnostics.StackFrame(1);
         var callingMethod = stackFrame.GetMethod();
         if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
         _project.L0g($"[ 💠  {callerName}] [{tolog}] ");
     }

     private string LoadCreds()
     {
         string[] xCreds = _sql.Get(" status, token, login, password, code2fa, emailLogin, emailPass", "twitter").Split('|');
         _status = xCreds[0];
         _token  = xCreds[1];
         _login = xCreds[2];
         _pass = xCreds[3];
         _2fa =  xCreds[4];
         _email = xCreds[5];
         _email_pass = xCreds[6];
         try {
             _project.Variables["twitterSTATUS"].Value = _status;
             _project.Variables["twitterTOKEN"].Value = _token;
             _project.Variables["twitterLOGIN"].Value = _login;
             _project.Variables["twitterPASSWORD"].Value = _pass;
             _project.Variables["twitterCODE2FA"].Value = _2fa;
             _project.Variables["twitterEMAIL"].Value = _email;
             _project.Variables["twitterEMAIL_PASSWORD"].Value = _email_pass;
         }
         catch (Exception ex)
         {
             _project.SendInfoToLog(ex.Message);
         }

         return _status;

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
             _project.L0g($"{DateTime.Now - start}s check... URLNow:[{_instance.ActiveTab.URL}]");
             if (DateTime.Now > deadline) throw new Exception("timeout");

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
                 else
                 {
                     status = "mixed";
                     _project.L0g($"!W {status}. Detected  [{check}] instead [UserAvatar-Container-{login}] {DateTime.Now - start}");
                 }
             }
             else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Something\\ went\\ wrong.\\ Try\\ reloading.", "regexp", 0).IsVoid)
             {
                 _instance.ActiveTab.MainDocument.EvaluateScript("location.reload(true)");
                 Thread.Sleep(3000);
                 continue;
             }
         }
         _project.L0g($"{status} {DateTime.Now - start}");
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
         var cookJson = _instance.GetCookies(_project,".");
         JArray toParse = JArray.Parse(cookJson);
         int i = 0; var token = "";
         while (token == "")
         {
             if (toParse[i]["name"].ToString() == "auth_token") token = toParse[i]["value"].ToString();
             i++;
         }
         _project.Variables["twitterTOKEN"].Value = token;
         _sql.Upd($"token = '{token}'", "twitter");
         return token;
     }
     private string Xlogin()
     {
         DateTime deadline = DateTime.Now.AddSeconds(60);
         var login = _project.Variables["twitterLOGIN"].Value;

         _instance.ActiveTab.Navigate("https://x.com/", ""); Thread.Sleep(2000);
         _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 1, thr0w: false);
         _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);
         _instance.HeClick(("a", "data-testid", "login", "regexp", 0));
         _instance.HeSet(("input:text", "autocomplete", "username", "text", 0), login, deadline: 30);
         _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");

         if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Sorry, we could not find your account')]", 0).IsVoid) return "NotFound";

         _instance.HeSet(("password", "name"), _project.Variables["twitterPASSWORD"].Value);


         _instance.HeClick(("button", "data-testid", "LoginForm_Login_Button", "regexp", 0), "clickOut");

         if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Wrong password!')]", 0).IsVoid) return "WrongPass";

         var codeOTP = OTP.Offline(_project.Variables["twitterCODE2FA"].Value);
         _instance.HeSet(("text", "name"), codeOTP);


         _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");

         if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Your account is suspended')]", 0).IsVoid) return "Suspended";
         if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Oops,\\ something\\ went\\ wrong.\\ Please\\ try\\ again\\ later.", "regexp", 0).IsVoid) return "SomethingWentWrong";
         if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", "Suspicious\\ login\\ prevented", "regexp", 0).IsVoid) return "SuspiciousLogin";

         _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 1, thr0w: false);
         _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);
         XgetToken();
         return "ok";
     }
     public string Xload(bool log = false)
     {


         bool tokenUsed = false;
         DateTime deadline = DateTime.Now.AddSeconds(60);
     check:

         if (DateTime.Now > deadline) throw new Exception("timeout");

         var status = XcheckState(log: true);

         if (status == "login" && !tokenUsed)
         {
             XsetToken();
             tokenUsed = true;
             Thread.Sleep(3000);
         }
         else if (status == "login" && tokenUsed)
         {
             var login = Xlogin();
             _project.L0g($"{login}");
             Thread.Sleep(3000);
         }
         else if (status == "mixed")
         {
            _instance.CloseAllTabs();
            _instance.ClearCookie("x.com");
            _instance.ClearCache("x.com");
            _instance.ClearCookie("twitter.com");
            _instance.ClearCache("twitter.com");	
            goto check;
            
         }
         if (status == "restricted" || status == "suspended" || status == "emailCapcha")
         {
             _sql.Upd($"status = '{status}'", "twitter");
             return status;
         }
         else if (status == "ok")
         {
             _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 0, thr0w: false);
             _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);

             XgetToken();
             return status;
         }
         else
             _project.L0g($"unknown {status}");
         goto check;
     }

     public void XAuth()
     {
         DateTime deadline = DateTime.Now.AddSeconds(60);
     check:
         if (DateTime.Now > deadline) throw new Exception("timeout");
         _instance.HeClick(("button", "innertext", "Accept\\ all\\ cookies", "regexp", 0), deadline: 0, thr0w: false);
         _instance.HeClick(("button", "data-testid", "xMigrationBottomBar", "regexp", 0), deadline: 0, thr0w: false);

         string state = null;


         if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Sorry, we could not find your account')]", 0).IsVoid) state = "NotFound";
         else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Your account is suspended')]", 0).IsVoid) state = "Suspended";
         else if (!_instance.ActiveTab.FindElementByXPath("//*[contains(text(), 'Wrong password!')]", 0).IsVoid) state = "WrongPass";
         else if (!_instance.ActiveTab.FindElementByAttribute("span", "innertext", "Oops,\\ something\\ went\\ wrong.\\ Please\\ try\\ again\\ later.", "regexp", 0).IsVoid) state = "SomethingWentWrong";
         else if (!_instance.ActiveTab.FindElementByAttribute("*", "innertext", "Suspicious\\ login\\ prevented", "regexp", 0).IsVoid) state = "SuspiciousLogin";



         else if (!_instance.ActiveTab.FindElementByAttribute("input:text", "autocomplete", "username", "text", 0).IsVoid) state = "InputLogin";
         else if (!_instance.ActiveTab.FindElementByAttribute("input:password", "autocomplete", "current-password", "text", 0).IsVoid) state = "InputPass";
         else if (!_instance.ActiveTab.FindElementByAttribute("input:text", "data-testid", "ocfEnterTextTextInput", "text", 0).IsVoid) state = "InputOTP";


         else if (!_instance.ActiveTab.FindElementByAttribute("a", "data-testid", "login", "regexp", 0).IsVoid) state = "ClickLogin";
         else if (!_instance.ActiveTab.FindElementByAttribute("li", "data-testid", "UserCell", "regexp", 0).IsVoid) state = "CheckUser";


         _project.L0g(state);

         switch (state)
         {
             case "NotFound":
             case "Suspended":
             case "SuspiciousLogin":
             case "WrongPass":
                 _sql.Upd($"status = '{state}'", "twitter");
                 throw new Exception($"{state}");
             case "ClickLogin":
                 _instance.HeClick(("a", "data-testid", "login", "regexp", 0));
                 goto check;
             case "InputLogin":
                 _instance.HeSet(("input:text", "autocomplete", "username", "text", 0), _login, deadline: 30);
                 _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");
                 goto check;
             case "InputPass":
                 _instance.HeSet(("input:password", "autocomplete", "current-password", "text", 0), _pass);
                 _instance.HeClick(("button", "data-testid", "LoginForm_Login_Button", "regexp", 0), "clickOut");
                 goto check;
             case "InputOTP":
                 _instance.HeSet(("input:text", "data-testid", "ocfEnterTextTextInput", "text", 0), OTP.Offline(_2fa));
                 _instance.HeClick(("span", "innertext", "Next", "regexp", 1), "clickOut");
                 goto check;
             case "CheckUser":
                 string userdata = _instance.HeGet(("li", "data-testid", "UserCell", "regexp", 0));
                 if (userdata.Contains(_login))
                 {
                     _instance.HeClick(("button", "data-testid", "OAuth_Consent_Button", "regexp", 0));
                     goto check;
                 }
                 else
                 {
                     throw new Exception("wrong account");
                 }
             default:
                 _project.L0g($"unknown state [{state}]");
                 break;

         }

         if (!_instance.ActiveTab.URL.Contains("x.com") && !_instance.ActiveTab.URL.Contains("twitter.com"))
             _project.L0g("auth done");
         else goto check;
     }

     public void UpdXCreds(Dictionary<string, string> data)
     {
         var fields = new Dictionary<string, string>
         {
             { "LOGIN", data.ContainsKey("LOGIN") ? data["LOGIN"].Replace("'", "''") : "" },
             { "PASSWORD", data.ContainsKey("PASSWORD") ? data["PASSWORD"].Replace("'", "''") : "" },
             { "EMAIL", data.ContainsKey("EMAIL") ? data["EMAIL"].Replace("'", "''") : "" },
             { "EMAIL_PASSWORD", data.ContainsKey("EMAIL_PASSWORD") ? data["EMAIL_PASSWORD"].Replace("'", "''") : "" },
             //{ "TOKEN", data.ContainsKey("TOKEN") ? data["TOKEN"].Replace("'", "''") : "" },

             { "TOKEN", data.ContainsKey("TOKEN") ? (data["TOKEN"].Contains('=') ? data["TOKEN"].Split('=').Last().Replace("'", "''") : data["TOKEN"].Replace("'", "''")) : "" },

             { "CODE2FA", data.ContainsKey("CODE2FA") ? (data["CODE2FA"].Contains('/') ? data["CODE2FA"].Split('/').Last().Replace("'", "''") : data["CODE2FA"].Replace("'", "''")) : "" },
             { "RECOVERY_SEED", data.ContainsKey("RECOVERY_SEED") ? data["RECOVERY_SEED"].Replace("'", "''") : "" }
         };

         var _sql = new Sql(_project);
         try
         {
             _sql.Upd($@"token = '{fields["TOKEN"]}', 
             login = '{fields["LOGIN"]}', 
             password = '{fields["PASSWORD"]}', 
             code2fa = '{fields["CODE2FA"]}', 
             emaillogin = '{fields["EMAIL"]}', 
             emailpass = '{fields["EMAIL_PASSWORD"]}', 
             recovery2fa = '{fields["RECOVERY_SEED"]}'", "twitter");
         }
         catch (Exception ex)
         {
             _project.L0g("!W{ex.Message}");
         }
     }

     

     public void ParseProfile()
     {
         _instance.HeClick(("*", "data-testid", "AppTabBar_Profile_Link", "regexp", 0));


         string json = _instance.HeGet(("*", "data-testid", "UserProfileSchema-test", "regexp", 0));

         var jo = JObject.Parse(json);
         var main = jo["mainEntity"];

         string dateCreated = jo["dateCreated"].ToString();
         string id = main["identifier"].ToString();

         string username = main["additionalName"].ToString();
         string description = main["description"].ToString();
         string givenName = main["givenName"].ToString();
         string homeLocation = main["homeLocation"]["name"].ToString();

         string ava = main["image"]["contentUrl"].ToString();
         string banner = main["image"]["thumbnailUrl"].ToString();

         var interactionStatistic = main["interactionStatistic"];

         string Followers = interactionStatistic[0]["userInteractionCount"].ToString();
         string Following = interactionStatistic[1]["userInteractionCount"].ToString();
         string Tweets = interactionStatistic[2]["userInteractionCount"].ToString();

         _sql.Upd($@"dateCreated = '{dateCreated}',
                     id = '{id}',
                     username = '{username}',
                     description = '{description}',
                     givenName = '{givenName}',
                     homeLocation = '{homeLocation}',
                     ava = '{ava}',
                     banner = '{banner}',
                     Followers = '{Followers}',
                     Following = '{Following}',
                     Tweets = '{Tweets}',
                     ");


         try{
             var toFill = _project.Lists["editProfile"];
             toFill.Clear();

             if (description == "") toFill.Add("description");
             if (homeLocation == "") toFill.Add("homeLocation");
             if (ava == "https://abs.twimg.com/sticky/default_profile_images/default_profile_400x400.png") toFill.Add("ava");
             if (banner == "https://abs.twimg.com/sticky/default_profile_images/default_profile_normal.png") toFill.Add("banner");

         }
         catch { }

     }
     public void ParseSecurity()
     {

         _instance.ActiveTab.Navigate("https://x.com/settings/your_twitter_data/account", "");

     scan:
         try
         {
             _instance.HeSet(("current_password", "name"), _pass, deadline: 1);
             _instance.HeClick(("button", "innertext", "Confirm", "regexp", 0));
         }
         catch { }
         var tIdList = _instance.ActiveTab.FindElementsByAttribute("*", "data-testid", ".", "regexp").ToList();

         if (tIdList.Count < 50)
         {
             Thread.Sleep(3000);
             goto scan;
         }

         string email = null;
         string phone = null;
         string creation = null;
         string country = null;
         string lang = null;
         string gender = null;
         string birth = null;


         foreach (HtmlElement he in tIdList)
         {
             string pName = null;
             string pValue = null;
             string testid = he.GetAttribute("data-testid");
             string href = he.GetAttribute("href");
             string text = he.InnerText;

             switch (testid)
             {
                 case "account-creation":
                     pName = text.Split('\n')[0];
                     pValue = text.Replace(pName, "").Replace("\n", " ").Trim();
                     creation = pValue;
                     continue;
                 case "pivot":
                     pName = text.Split('\n')[0];
                     pValue = text.Replace(pName, "").Replace("\n", " ").Trim();
                     switch (pName)
                     {
                         case "Phone":
                             phone = pValue;
                             break;
                         case "Email":
                             email = pValue;
                             break;
                         case "Country":
                             country = pValue;
                             break;
                         case "Languages":
                             lang = pValue;
                             break;
                         case "Gender":
                             gender = pValue;
                             break;
                         case "Birth date":
                             birth = pValue;
                             break;
                     }
                     continue;
                 default:
                     continue;
             }
         }
         _sql.Upd($@"creation = '{creation}',
                     email = '{email}',
                     phone = '{phone}',
                     country = '{country}',
                     lang = '{lang}',
                     gender = '{gender}',
                     birth = '{birth}',
                     ");


         try
         {
             var emails = _sql.Get("gmail, icloud, firstmail", "mail_public");
             var address = _sql.Get("evm", "blockchain_public");
             var toFill = _project.Lists["editSecurity"];
             toFill.Clear();
             
             if (!emails.Contains(email) || !email.Contains(address)) toFill.Add("email");

         }
         catch { }
     }
 }





    public class NetHttp2
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly bool _logShow;

        public NetHttp2(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _logShow = log;
        }

        protected void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;
            _project.L0g($"[ 🌍 {callerName}] [{message}]");
        }
        protected void ParseJson(string json)
        {
            try {
                _project.Json.FromString(json);
            }
            catch (Exception ex) {
                Log($"[!W {ex.Message}] [{json}]");
            }
        }
        public WebProxy ParseProxy(string proxyString, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(proxyString))
            {
                return null;
            }
            if (proxyString == "+") proxyString = _project.Variables["proxy"].Value;
            try
            {
                WebProxy proxy = new WebProxy();

                if (proxyString.Contains("//")) proxyString = proxyString.Split('/')[2];

                if (proxyString.Contains("@")) // Прокси с авторизацией (login:pass@proxy:port)
                {
                    string[] parts = proxyString.Split('@');
                    string credentials = parts[0];
                    string proxyHost = parts[1];

                    proxy.Address = new Uri("http://" + proxyHost);
                    string[] creds = credentials.Split(':');
                    proxy.Credentials = new NetworkCredential(creds[0], creds[1]);

                    Log($"proxy set:{proxyHost}", callerName);
                }
                else // Прокси без авторизации (proxy:port)
                {
                    proxy.Address = new Uri("http://" + proxyString);
                    Log($"proxy set: ip:{proxyString}", callerName);
                }

                return proxy;
            }
            catch (Exception e)
            {
                Log($"Ошибка настройки прокси: {e.Message}", callerName, true);
                return null;
            }
        }


        public string GET(string url, string proxyString = "", Dictionary<string, string> headers = null, bool parse = false, [CallerMemberName] string callerName = "")
        {
            string debugHeaders = null;
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(15);

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent; // Same as in POST
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                    }

                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        Log("Set-Cookie found: " + cookies, callerName);
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (parse) ParseJson(result);
                    Log($"{result}", callerName);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                Log($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: {proxyString}), Headers\n{debugHeaders.Trim()}", callerName);
                return $"Ошибка: {e.Message}";
            }
            catch (Exception e)
            {
                Log($"!W [GET] UnknownErr: [{e.Message}] url:[{url}] (proxy: {proxyString}) Headers\n{debugHeaders.Trim()}", callerName);
                return $"Ошибка: {e.Message}";
            }
        }
        public string POST(string url, string body, string proxyString = "", Dictionary<string, string> headers = null, bool parse = false, [CallerMemberName] string callerName = "", bool throwOnFail = false)
        {
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var content = new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;//"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                        }
                    }

                    headersString.AppendLine($"Content-Type: application/json; charset=UTF-8");

                    Log(body);

                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        Log("Set-Cookie found: " + cookies, callerName);
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    Log(result);
                    if (parse) ParseJson(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                Log($"!W RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
                if (throwOnFail) throw;
                return "";
            }
            catch (Exception e)
            {
                Log($"!W UnknownErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
                if (throwOnFail) throw;
                return "";
            }
        }
  
        public string PUT(string url, string body = "", string proxyString = "", Dictionary<string, string> headers = null, bool parse = false, [CallerMemberName] string callerName = "")
        {
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var content = string.IsNullOrEmpty(body) ? null : new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                        }
                    }

                    if (content != null)
                    {
                        headersString.AppendLine($"Content-Type: application/json; charset=UTF-8");
                        Log(body, callerName);
                    }

                    HttpResponseMessage response = client.PutAsync(url, content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        Log("Set-Cookie found: " + cookies, callerName);
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Log(result, callerName);
                    if (parse) ParseJson(result);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                Log($"!W RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
                return $"Ошибка: {e.Message}";
            }
            catch (Exception e)
            {
                Log($"!W UnknownErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
                return $"Ошибка: {e.Message}";
            }
        }        
 
        public string DELETE(string url, string proxyString = "", Dictionary<string, string> headers = null, [CallerMemberName] string callerName = "")
        {
            
            string debugHeaders = null;
            try
            {
                WebProxy proxy = ParseProxy(proxyString);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    StringBuilder headersString = new StringBuilder();
                    headersString.AppendLine("[debugRequestHeaders]:");

                    string defaultUserAgent = _project.Profile.UserAgent;
                    if (headers == null || !headers.ContainsKey("User-Agent"))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", defaultUserAgent);
                        headersString.AppendLine($"User-Agent: {defaultUserAgent} (default)");
                    }

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                    }

                    HttpResponseMessage response = client.DeleteAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    StringBuilder responseHeadersString = new StringBuilder();
                    responseHeadersString.AppendLine("[debugResponseHeaders]:");
                    foreach (var header in response.Headers)
                    {
                        var value = string.Join(", ", header.Value);
                        responseHeadersString.AppendLine($"{header.Key}: {value}");
                    }

                    string cookies = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                    {
                        cookies = cookieValues.Aggregate((a, b) => a + "; " + b);
                        Log("Set-Cookie found: " + cookies, callerName);
                    }

                    try
                    {
                        _project.Variables["debugCookies"].Value = cookies;
                    }
                    catch { }

                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Log(result, callerName);
                    return result.Trim();
                }
            }
            catch (HttpRequestException e)
            {
                Log($"!W [DELETE] RequestErr: [{e.Message}] url:[{url}] (proxy: {proxyString}), Headers\n{debugHeaders.Trim()}", callerName);
                return $"Ошибка: {e.Message}";
            }
            catch (Exception e)
            {
                Log($"!W [DELETE] UnknownErr: [{e.Message}] url:[{url}] (proxy: {proxyString}) Headers\n{debugHeaders.Trim()}", callerName);
                return $"Ошибка: {e.Message}";
            }
        }
 
        public bool ManageRole(string botToken, string guildId, string roleName, string userId, bool assignRole, [CallerMemberName] string callerName = "")
{
    try
    {
        // Заголовки для авторизации
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bot {botToken}" },
            { "User-Agent", "DiscordBot/1.0" } // Явно задаём User-Agent, чтобы Discord не блочил
        };
        Log($"Заголовки для запроса: {string.Join(", ", headers.Select(h => $"{h.Key}: {h.Value}"))}", callerName);

        // 1. Получаем список ролей сервера, чтобы найти ID роли по имени
        string rolesUrl = $"https://discord.com/api/v10/guilds/{guildId}/roles";
        Log($"Отправляем GET: {rolesUrl}", callerName);
        string rolesResponse = GET(rolesUrl, headers: headers, callerName: callerName);
        
        Log($"Ответ от GET: {rolesResponse}", callerName);
        if (rolesResponse.StartsWith("Ошибка"))
        {
            Log($"!W Не удалось получить роли сервера: {rolesResponse}", callerName, true);
            return false;
        }

        // Парсим JSON для поиска роли
        JArray roles = JArray.Parse(rolesResponse);
        var role = roles.FirstOrDefault(r => r["name"].ToString().Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (role == null)
        {
            Log($"!W Роль с именем '{roleName}' не найдена на сервере", callerName, true);
            return false;
        }
        string roleId = role["id"].ToString();
        Log($"Найдена роль: {roleName} (ID: {roleId})", callerName);

        // 2. Формируем URL для выдачи или удаления роли
        string url = $"https://discord.com/api/v10/guilds/{guildId}/members/{userId}/roles/{roleId}";
        
        // 3. Выполняем запрос в зависимости от assignRole
        string result;
        if (assignRole)
        {
            Log($"Отправляем PUT: {url}", callerName);
            result = PUT(url, "", proxyString: null, headers: headers, callerName: callerName);
        }
        else
        {
            Log($"Отправляем DELETE: {url}", callerName);
            result = DELETE(url, proxyString: null, headers: headers, callerName: callerName);
        }

        Log($"Ответ от {(assignRole ? "PUT" : "DELETE")}: {result}", callerName);
        if (result.StartsWith("Ошибка"))
        {
            Log($"!W Не удалось {(assignRole ? "выдать" : "удалить")} роль: {result}", callerName, true);
            return false;
        }

        Log($"{(assignRole ? "Роль успешно выдана" : "Роль успешно удалена")}: {roleName} для пользователя {userId}", callerName);
        return true;
    }
    catch (Exception e)
    {
        Log($"!W Ошибка при управлении ролью: [{e.Message}]", callerName, true);
        return false;
    }
}
 
 
 
        public void CheckProxy(string url = "http://api.ipify.org/", string proxyString = null)
        {
            if (string.IsNullOrEmpty(proxyString)) proxyString = _project.Variables["proxy"].Value;
            WebProxy proxy = ParseProxy(proxyString);

            string ipWithoutProxy = GET(url, null);

            string ipWithProxy = "notSet";
            if (proxy != null)
            {
                ipWithProxy = GET(url, proxyString);
            }
            else
            {
                ipWithProxy = "noProxy";
            }

            Log($"local: {ipWithoutProxy}, proxified: {ipWithProxy}");

            if (ipWithProxy != ipWithoutProxy && !ipWithProxy.StartsWith("Ошибка") && ipWithProxy != "Прокси не настроен")
            {
                Log($"Succsessfuly proxified: {ipWithProxy}");
            }
            else if (ipWithProxy.StartsWith("Ошибка") || ipWithProxy == "Прокси не настроен")
            {
                Log($"!W proxy error: {ipWithProxy}");
            }
            else
            {
                Log($"!W ip still same. Proxy was not applyed");
            }
        }


    }







}
