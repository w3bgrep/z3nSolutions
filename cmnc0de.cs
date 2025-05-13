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

        public void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;
            _project.L0g($"[ 🌍 {callerName}] [{message}]");
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

public class DMail : W3bWrite
{
    private readonly string _key;
    private string _encstring;
    private string _pid;
    private dynamic _allMail;
    private Dictionary<string, string> _headers;

    private readonly NetHttp _h;
    public DMail(IZennoPosterProjectModel project, string key = null, bool log = false)
    : base(project, key:key, log:log)
    {          
        _key = Key(key);
        _h = new NetHttp(project, true);
    }

    public string Key(string key = null) 
    {
        if (string.IsNullOrEmpty(key))
        {
            string encryptedkey = _sql.Get("secp256k1", "accounts.blockchain_private");
            key =  SAFU.Decode(_project, encryptedkey);
        }

        if (string.IsNullOrEmpty(key)) 
        {
            Log("!W key is null or empty");
            throw new Exception("emptykey");
        };
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

        try
        {
            _project.Variables["encstring"].Value = encstring;
            _project.Variables["pid"].Value = pid;
        }
        catch { }

        Log($"{encstring} {pid}");

        _encstring = encstring; _pid = pid;


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

    public Dictionary<string, string> ReadMsg(int index = 0, dynamic mail = null, bool markAsRead = true)
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
            MessageData.TryGetValue("dm_scid",out dm_scid);
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



}
    





}
