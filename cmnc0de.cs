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


    public class Test
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly bool _logShow;

        public Test(IZennoPosterProjectModel project, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _logShow = log;
        }

        public void Log(string message, Exception ex = null, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;

            // Получаем версию сборки (как в предыдущем ответе)
            string version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";

            // Формируем базовое сообщение
            string logMessage = $"[ 🌍 {callerName}] [v{version}] [{message}]";

            // Если передано исключение, добавляем детали стека
            if (ex != null)
            {
                var stackTrace = new StackTrace(ex, true);
                var frame = stackTrace.GetFrame(0); // Первый кадр — место ошибки
                if (frame != null)
                {
                    string method = frame.GetMethod()?.Name ?? "Unknown";
                    string file = frame.GetFileName() ?? "Unknown";
                    int line = frame.GetFileLineNumber();

                    logMessage += $"\n[Error Details] Method: {method}, File: {file}, Line: {line}";
                }

                // Опционально: полный стек вызовов
                logMessage += $"\n[StackTrace] {ex.StackTrace}";
            }

            _project.L0g(logMessage);
        }


        public Dictionary<string, string> ParseCreds(string format, string data)
        {
            var parsedData = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(data))
                return parsedData;

            string[] formatParts = format.Split(':');
            string[] dataParts = data.Split(':');

            for (int i = 0; i < formatParts.Length && i < dataParts.Length; i++)
            {
                string key = formatParts[i].Trim('{', '}').Trim();
                if (!string.IsNullOrEmpty(key))
                    parsedData[key] = dataParts[i].Trim();
            }
            return parsedData;
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
                recovery2fa = '{fields["RECOVERY_SEED"]}'","twitter");
            }
            catch (Exception ex)
            {
                _project.L0g("!W{ex.Message}");
            }
        }     
    }




   public class Test2
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly W3bRead _w3b;
        private readonly Sql _sql;
        private readonly Starter _starter;
        private readonly IZennoList _accounts;//List<string> _accounts;

        public Test2(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _w3b = new W3bRead(project, log);
            _sql = new Sql(project, log);
            _starter = new Starter(project, log);
            _accounts = project.Lists["accs"];
        }

        public void ProcessAccounts(string mode)
        {
            if (_accounts.Count == 0)
            {
                HandleNoAccounts();
                return;
            }

            switch (mode)
            {
                case "CheckBalance":
                    ProcessCheckBalance();
                    break;
                case "Cooldown":
                case "TopUp":
                    ProcessRandomAccount(mode);
                    break;
                default:
                    throw new ArgumentException($"Неизвестный режим: {mode}");
            }
        }

        private void ProcessCheckBalance()
        {
            while (_accounts.Count > 0)
            {
                string account = _accounts[0];
                _accounts.RemoveAt(0);
                ProcessSingleAccount(account, updateNative: true);
            }

            Log("TimeToChill", thr0w: true);
        }

        private void ProcessRandomAccount(string mode)
        {
            if (mode == "TopUp")
            {
                _accounts.Clear();
                _accounts.AddRange(_project.Variables["range"].Value.Split(','));
            }

            var random = new Random();
            while (_accounts.Count > 0)
            {
                int randomIndex = random.Next(0, _accounts.Count);
                string account = _accounts[randomIndex];
                _accounts.RemoveAt(randomIndex);

                if (!ProcessSingleAccount(account, checkBalance: true, isCooldown: mode == "Cooldown"))
                {
                    continue;
                }

                if (!_project.SetGlobalVar())
                {
                    continue;
                }

                SetProfilePath(account);
                Log($"running: [acc{account}] accs left: [{_accounts.Count}]");
                return; 
            }

            HandleNoAccounts();
        }

        private bool ProcessSingleAccount(string account, bool updateNative = false, bool checkBalance = false, bool isCooldown = false)
        {
            _project.Variables["acc0"].Value = account;
            string adrMove = _sql.Get("move", "blockchain_public");
            if (updateNative)
            {
                string nativeMOVE = _w3b.NativeAPT<string>("https://mainnet.movementnetwork.xyz/v1", adrMove, log: true);
                _sql.Upd($"move = {nativeMOVE}", "native");
                _sql.Upd($"native = '{nativeMOVE}'");
                return true;
            }

            if (checkBalance)
            {
                decimal nativeMOVE = _w3b.NativeAPT<decimal>("https://mainnet.movementnetwork.xyz/v1", adrMove, log: true);
                if (isCooldown)
                {
                    return nativeMOVE != 0; 
                }
                return nativeMOVE <= 0; 
            }

            return true;
        }

        private void SetProfilePath(string account)
        {
            string settingsFolder = _project.Variables["settingsZenFolder"].Value;
            _project.Variables["pathProfileFolder"].Value = $"{settingsFolder}accounts\\profilesFolder\\{account}";
        }

        private void HandleNoAccounts()
        {
            _project.Variables["noAccsToDo"].Value = "True";
            _project.Variables["acc0"].Value = "";
            _project.SendToLog("♻ noAccoutsAvaliable", LogType.Info, true, LogColor.Turquoise);
            throw new Exception("TimeToChill");
        }

        private void Log(string message, bool thr0w = false)
        {
            _project.L0g(message, thr0w: thr0w);
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

        public string GET(string url, string proxyString = "", Dictionary<string, string> headers = null, bool parse = false, [CallerMemberName] string callerName = "")
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
                        string debugHeaders = null;
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            headersString.AppendLine($"{header.Key}: {header.Value}");
                            debugHeaders += $"{header.Key}: {header.Value}";
                        }
                        Log($"Headers\n{debugHeaders.Trim()}");
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
                Log($"!W [GET] RequestErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
                return $"Ошибка: {e.Message}";
            }
            catch (Exception e)
            {
                Log($"!W [GET] UnknownErr: [{e.Message}] url:[{url}] (proxy: {(proxyString != "" ? proxyString : "noProxy")})", callerName);
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



 public class DMail2 : W3bWrite
 {
     private readonly string _key;
     private string _encstring;
     private string _pid;
     private dynamic _allMail;
     private Dictionary<string, string> _headers;
     private readonly NetHttp2 _h;

     private readonly string _getNonce = "https://icp.dmail.ai/api/node/v6/dmail/auth/generate_nonce";
     private readonly string _postVerifySign = "https://icp.dmail.ai/api/node/v6/dmail/auth/evm_verify_signature";
     private readonly string _postRead = "https://icp.dmail.ai/api/node/v6/dmail/inbox_all/read_by_page_with_content";
     private readonly string _postWrite = "https://icp.dmail.ai/api/node/v6/dmail/inbox_all/update_by_bulk";
   

     public DMail2(IZennoPosterProjectModel project, string key = null, bool log = false)
         : base(project, key: key, log: log)
    {
        _key = Key(key);
        _h = new NetHttp2(project, true);
        CheckAuth();
    }

    public void CheckAuth()
    {
        
        checkVars:

        if (!string.IsNullOrEmpty(_pid) && !string.IsNullOrEmpty(_encstring)) goto setHeaders;
        
        try //from project
        {               
            _pid =  _project.Variables["pid"].Value;
            _encstring = _project.Variables["encstring"].Value;
            if (!string.IsNullOrEmpty(_pid) && !string.IsNullOrEmpty(_encstring)) goto setHeaders;
            throw new Exception("noAuthDataFound");
        }
        catch (Exception e)
        {
            Log($"!W {e.Message}");
        }

        try //login
        {
            Auth();
            if (!string.IsNullOrEmpty(_pid) && !string.IsNullOrEmpty(_encstring)) goto setHeaders;
            throw new Exception("noAuthDataAfterLogin WTF?");
        }
        catch (Exception e)
        {
            Log($"!W {e.Message}");
        }

        setHeaders:
        if (string.IsNullOrEmpty(_pid) || string.IsNullOrEmpty(_encstring)) throw new Exception("enpty creds WTF?");
        
        _headers = new Dictionary<string, string>
        {
            { "dm-encstring", _encstring },
            { "dm-pid",_pid }
        };

    }


    private string Key(string key = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            string encryptedkey = _sql.Get("secp256k1", "accounts.blockchain_private");
            key = SAFU.Decode(_project, encryptedkey);
        }

        if (string.IsNullOrEmpty(key))
        {
            Log("!W key is null or empty");
            throw new Exception("emptykey");
        }
        ;
        return key;

    }
    public string Auth()
    {

        var signer = new EthereumMessageSigner();
        string key = _key;
        Log(key);
        string wallet = _key.ToPubEvm();
        string time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

        // 1. GET nonce
        string nonceJson = _h.GET("https://icp.dmail.ai/api/node/v6/dmail/auth/generate_nonce", "");
        dynamic data_nonce = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(nonceJson);
        string nonce = data_nonce.data.nonce;

        string msgToSign = "SIGN THIS MESSAGE TO LOGIN TO THE INTERNET COMPUTER\n\n" +
            "APP NAME: \ndmail\n\n" +
            "ADDRESS: \n" + wallet + "\n\n" +
            "NONCE: \n" + nonce + "\n\n" +
            "CURRENT TIME: \n" + time;

        string sign = signer.EncodeUTF8AndSign(msgToSign, new EthECKey(key));

        // Create JObject for the message
        var message = new JObject
    {
        { "Message", "SIGN THIS MESSAGE TO LOGIN TO THE INTERNET COMPUTER" },
        { "APP NAME", "dmail" },
        { "ADDRESS", wallet },
        { "NONCE", nonce },
        { "CURRENT TIME", time }
    };

        var data = new JObject
    {
        { "message", message },
        { "signature", sign },
        { "wallet_name", "metamask" },
        { "chain_id", 1 }
    };

        string body = JsonConvert.SerializeObject(data);

        // 2. POST to verify signature
        string get_verify_json = _h.POST("https://icp.dmail.ai/api/node/v6/dmail/auth/evm_verify_signature", body);

        dynamic data_dic = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(get_verify_json);
        string encstring = data_dic.data.token;
        string pid = data_dic.data.pid;
        Log($"{encstring} {pid}");
        
        
        try
        {
            _project.Variables["encstring"].Value = encstring;
            _project.Variables["pid"].Value = pid;
        }
        catch { }

        _encstring = encstring;
        _pid = pid;


        _headers = new Dictionary<string, string>
        {
            { "dm-encstring", encstring },
            { "dm-pid",pid }
        };

        return $"{encstring}|{pid}";


    }
    public dynamic GetAll()
    {

        var pageInfo = new JObject {
        { "page", 1 },
        { "pageSize", 20 }
    };
        var data = new JObject {
        { "dm_folder", "inbox" },
        { "store_type", "mail" },
        { "pageInfo", pageInfo }
    };


        string getMsgsBody = JsonConvert.SerializeObject(data);

        string allMailJson = _h.POST("https://icp.dmail.ai/api/node/v6/dmail/inbox_all/read_by_page_with_content", getMsgsBody, headers: _headers, parse: false);

        dynamic mail = JsonConvert.DeserializeObject<ExpandoObject>(allMailJson);
        string count_items = mail.data.list.Count.ToString();
        var allMailObj = mail.data.list;

        _allMail = allMailObj;
        return allMailObj;

    }
    public Dictionary<string, string> ReadMsg(int index = 0, dynamic mail = null, bool markAsRead = true, bool trash = true)
    {
        if (mail == null) mail = _allMail;

        string sender = mail[index].dm_salias.ToString();
        string date = mail[index].dm_date.ToString();

        string dm_scid = mail[index].dm_scid.ToString();
        string dm_smid = mail[index].dm_smid.ToString();

        dynamic content = mail[index].content;
        string subj = content.subject.ToString();
        string html = content.html.ToString();


        var message = new Dictionary<string, string>
    {
        { "sender", sender },
        { "date", date },
        { "subj", subj },
        { "html", html },
        { "dm_scid", dm_scid },
        { "dm_smid", dm_smid },
    };
        if (markAsRead) MarkAsRead(index, dm_scid, dm_smid);
        if (trash) Trash(index, dm_scid, dm_smid);
        return message;
    }

    public void MarkAsRead(int index = 0, string dm_scid = null, string dm_smid = null)
    {
        var status = new JObject {
        {"dm_is_read", 1 }
    };

        if (string.IsNullOrEmpty(dm_scid) || string.IsNullOrEmpty(dm_smid))
        {
            var MessageData = ReadMsg(index);
            MessageData.TryGetValue("dm_scid", out dm_scid);
            MessageData.TryGetValue("dm_smid", out dm_smid);
        }


        var info = new JArray {
        new JObject {
            {"dm_cid", dm_scid},
            {"dm_mid", dm_smid},
            {"dm_foldersource", "inbox"}
        }
    };

        var data = new JObject {
        {"status", status},
        {"mail_info_list", info},
        {"store_type", "mail"}
    };

        string body = JsonConvert.SerializeObject(data);
        string makeRead = _h.POST("https://icp.dmail.ai/api/node/v6/dmail/inbox_all/update_by_bulk", body, headers: _headers, parse: false);
    }
    public string GetUnread(bool parse = false, string key = null)
    {
        CheckAuth();
        string url = $"https://icp.dmail.ai/api2/v2/credits/getUsedMailInfo?pid={_pid}";
        Log(url);
        string GetUnread = _h.GET(url, headers: _headers, parse: true);
        if (!string.IsNullOrEmpty(key))
        {
            dynamic unrd = JsonConvert.DeserializeObject<ExpandoObject>(GetUnread);
            string[] fields = { "mail_unread_count", "message_unread_count", "not_read_count", "used_total_size" };
            if (fields.Contains(key))
            {
                var dataDict = (IDictionary<string, object>)unrd.data;
                return dataDict[key].ToString();
            }
            Log($"!W no object with key [{key}] in json [{GetUnread}]");
            return string.Empty;
        }
       
        return GetUnread;

    }

    public void Trash(int item = 0, string dm_scid = null, string dm_smid = null)
    {

         if (string.IsNullOrEmpty(dm_scid) || string.IsNullOrEmpty(dm_smid))
        {        
            try
            {
                var MessageData = ReadMsg(item);
                MessageData.TryGetValue("dm_scid", out dm_scid);
                MessageData.TryGetValue("dm_smid", out dm_smid);
            }
            catch
            {
                GetAll();
                var MessageData = ReadMsg(item);
                MessageData.TryGetValue("dm_scid", out dm_scid);
                MessageData.TryGetValue("dm_smid", out dm_smid);
            }
        }
 
         var status = new JObject {
            {"dm_folder", "trashs"}
        };

        var info = new JArray {
            new JObject {
                {"dm_cid", dm_scid},
                {"dm_mid", dm_smid},
                {"dm_foldersource", "inbox"}
            }
        };		    

        var data = new JObject {
            {"status", status},
            {"mail_info_list", info},
            {"store_type", "mail"}
        };


        string body = JsonConvert.SerializeObject(data);
        string makeRead = _h.POST("https://icp.dmail.ai/api/node/v6/dmail/inbox_all/update_by_bulk", body, headers: _headers, parse: false);

    }



}


}
