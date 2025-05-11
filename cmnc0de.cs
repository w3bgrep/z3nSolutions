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

    private static string DetectKeyType(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (Regex.IsMatch(input, @"^[0-9a-fA-F]{64}$"))
            return "key";

        var words = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 12)
            return "seed";
        if (words.Length == 24)
            return "seed";

        return null;
    }
    public static string ToPubEvm(this string key)
    {
        string keyType = key.DetectKeyType();
        var blockchain = new Blockchain();

        if (keyType == "seed")
        {
            var mnemonicObj = new Mnemonic(key);
            var hdRoot = mnemonicObj.DeriveExtKey();
            var derivationPath = new NBitcoin.KeyPath("m/44'/60'/0'/0/0");
            key = hdRoot.Derive(derivationPath).PrivateKey.ToHex();

        }
        return blockchain.GetAddressFromPrivateKey(key);
    }


    public static bool ChkAddress(this string shortAddress, string fullAddress)
    {
        if (string.IsNullOrEmpty(shortAddress) || string.IsNullOrEmpty(fullAddress))
            return false;

        if (!shortAddress.Contains("…") || shortAddress.Count(c => c == '…') != 1)
            return false;

        var parts = shortAddress.Split('…');
        if (parts.Length != 2)
            return false;

        string prefix = parts[0]; 
        string suffix = parts[1]; 

        if (prefix.Length < 4 || suffix.Length < 2)
            return false;

        if (fullAddress.Length < prefix.Length + suffix.Length)
            return false;

        bool prefixMatch = fullAddress.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        bool suffixMatch = fullAddress.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);

        bool result = prefixMatch && suffixMatch;
        
        //project.L0g($"[{shortAddress}]?[{fullAddress}] {result} [{prefixMatch}]?[{suffixMatch}]");
        return result;
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

        protected void Log(string message, [CallerMemberName] string callerName = "", bool forceLog = false)
        {
            if (!_logShow && !forceLog) return;
            _project.L0g($"[ 🌍 {callerName}] [{message}]");
        }       

        public decimal UsdToToken(string tiker, decimal usdAmount, bool log = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var restApi = new OKXApi(_project);

            tiker = tiker.ToUpper();
            decimal price = restApi.OKXPrice<decimal>($"{tiker}-USDT");
            decimal tokenAmount = usdAmount / price;
            return tokenAmount;
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

            // if (!_project.SetGlobalVar())
            // {
            //     return false;
            // }

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



public class ZerionWallet2 : Wlt
{
    protected readonly string _extId = "klghhnkeealcohjjanjjdaeeggmfmlpl";
    protected readonly string _popupUrl = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#";
    protected readonly string _sidepanelUrl = "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#";
    protected readonly string _importPage = "/get-started/import";
    protected readonly string _selectPage = "/wallet-select";
    protected readonly string _historyPage = "/overview/history";
    protected readonly string _fileName;
    protected readonly string _publicFromKey;
    protected readonly string _publicFromSeed;

    public ZerionWallet2(IZennoPosterProjectModel project, Instance instance, bool log = false, string key = null, string seed = null)
        : base(project, instance, log)
    {
        _fileName = "Zerion1.21.3.crx";
        _key = KeyCheck(key);
        _seed = SeedCheck(seed);
        _publicFromKey = _key.ToPubEvm();
        _publicFromSeed = _seed.ToPubEvm();
    }

    private string KeyCheck(string key)
    {
        if (string.IsNullOrEmpty(key))
            key = Decrypt(KeyT.secp256k1);
        if (string.IsNullOrEmpty(key))
            throw new Exception("emptykey");
        return key;
    }
    private string SeedCheck(string seed)
    {
        if (string.IsNullOrEmpty(seed))
            seed = Decrypt(KeyT.bip39);
        if (string.IsNullOrEmpty(seed))
            throw new Exception("emptykey");
        return seed;
    }


    public void Go(string page = null, string mode = "sidepanel")
    {
        string sourseLink;
        string method;
        if ( mode == "sidepanel") sourseLink = _sidepanelUrl;
        else sourseLink = _popupUrl;

        switch (page)
        {
            case "import":
                method = _importPage;
                break;
            case "select":
                method = _selectPage;
                break;
            case "history":
                method = _historyPage;
                break; 
            default:
                method = null; 
                break;  
        }
        
        _instance.ActiveTab.Navigate(sourseLink + method, "");
    }

    public void Add(string source = "seed", bool log = false)
    {

        if (!_instance.ActiveTab.URL.Contains(_importPage)) Go("import");
            
        if (source == "pkey") source = _key;
        else if (source == "seed") source = _seed;

        _instance.HeSet(("seedOrPrivateKey","name"),source);
        _instance.HeClick(("button", "innertext", "Import", "regexp", 0));
        _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
        _instance.HeClick(("button", "class", "_primary", "regexp", 0));
        try{
            _instance.HeClick(("button", "class", "_option", "regexp", 0));
            _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            _instance.HeClick(("button", "class", "_primary", "regexp", 0));
        }
        catch{}

    }

    public void Select(string addressToUse = "key")
    {
        if (addressToUse == "key" ) addressToUse  = _publicFromKey; 
        else if (addressToUse == "seed" ) addressToUse  = _publicFromSeed; 
        go:
        Go("select");
        Thread.Sleep(1000);
        var wallets = _instance.ActiveTab.FindElementsByAttribute("button", "class", "_wallet", "regexp").ToList();

        foreach (HtmlElement wallet in wallets)
        {
            string masked = "";
            string balance = "";
            string ens = "";

            if (wallet.InnerHtml.Contains("M18 21a2.9 2.9 0 0 1-2.125-.875A2.9 2.9 0 0 1 15 18q0-1.25.875-2.125A2.9 2.9 0 0 1 18 15a3.1 3.1 0 0 1 .896.127 1.5 1.5 0 1 0 1.977 1.977Q21 17.525 21 18q0 1.25-.875 2.125A2.9 2.9 0 0 1 18 21")) continue;
            if (wallet.InnerText.Contains("·") )
            {
                ens = wallet.InnerText.Split('\n')[0].Split('·')[0];
                masked = wallet.InnerText.Split('\n')[0].Split('·')[1];
                balance = wallet.InnerText.Split('\n')[1].Trim();
                
            }
            else
            {
                masked = wallet.InnerText.Split('\n')[0];
                balance = wallet.InnerText.Split('\n')[1];
            }
            masked = masked.Trim();

            Log($"[{masked}]{masked.ChkAddress(addressToUse)}[{addressToUse}]");
            
            if(masked.ChkAddress(addressToUse)) 
            {
               _instance.HeClick(wallet);
               return; 
            }              
        }
        Log("address not found");
        Add("seed");
        goto go;


    }






    public void ZerionLnch(string fileName = null, bool log = false)
    {
        if (string.IsNullOrEmpty(fileName)) fileName = _fileName;

        var em = _instance.UseFullMouseEmulation;
        _instance.UseFullMouseEmulation = false;

        if (Install(_extId, fileName)) ZerionImport(log: log);
        else
        {
            ZerionUnlock(log: false);
            ZerionCheck(log: log);
        }
        _instance.CloseExtraTabs();
        _instance.UseFullMouseEmulation = em;
    }

    public bool ZerionImport(string source = "pkey", string refCode = null, bool log = false)
    {
        
        if (string.IsNullOrWhiteSpace(refCode))
        {
            refCode = _sql.DbQ(@"SELECT referralCode
            FROM projects.zerion
            WHERE referralCode != '_' 
            AND TRIM(referralCode) != ''
            ORDER BY RANDOM()
            LIMIT 1;");
        }

        var inputRef = true;
        _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import", "regexp", 0));
        if (source == "pkey")
        {
            _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/private-key", "regexp", 0));
            string key = _key;
            _instance.ActiveTab.FindElementByName("key").SetValue(key, "Full", false);
        }
        else if (source == "seed")
        {
            _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/mnemonic", "regexp", 0));
            string seedPhrase = _seed;
            int index = 0;
            foreach (string word in seedPhrase.Split(' '))
            {
                _instance.ActiveTab.FindElementById($"word-{index}").SetValue(word, "Full", false);
                index++;
            }
        }
        _instance.HeClick(("button", "innertext", "Import\\ wallet", "regexp", 0));
        _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
        _instance.HeClick(("button", "class", "_primary", "regexp", 0));
        _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
        _instance.HeClick(("button", "class", "_primary", "regexp", 0));
        if (inputRef)
        {
            _instance.HeClick(("button", "innertext", "Enter\\ Referral\\ Code", "regexp", 0));
            _instance.HeSet((("referralCode", "name")), refCode);
            _instance.HeClick(("button", "class", "_regular", "regexp", 0));
        }
        return true;
    }

    public void ZerionUnlock(bool log = false)
    {
        _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview", "");

        string active = null;
        try
        {
            active = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0),deadline:2);
        }
        catch
        {
            _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass);
            _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            active = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html\\#/wallet-select", "regexp", 0));
        }
        Log(active, log: log);
    }

    public string ZerionCheck(bool log = false)
    {
        if (_instance.ActiveTab.URL != "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview")
            _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview", "");

        var active = _instance.HeGet(("div", "class", "_uitext_", "regexp", 0));
        var balance = _instance.HeGet(("div", "class", "_uitext_", "regexp", 1));
        var pnl = _instance.HeGet(("div", "class", "_uitext_", "regexp", 2));

        Log($"{active} {balance} {pnl}", log: log);
        return active;
    }

    public bool ZerionApprove(bool log = false)
    {

        try
        {
            var button = _instance.HeGet(("button", "class", "_primary", "regexp", 0));
            Log(button, log: log);
            _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            return true;
        }
        catch (Exception ex)
        {
            Log($"!W {ex.Message}", log: log);
            throw;
        }
    }

    public void ZerionConnect(bool log = false)
    {

        string action = null;
        getState:

        try
        {
            action = _instance.HeGet(("button", "class", "_primary", "regexp", 0));
        }
        catch (Exception ex)
        {
            _project.L0g($"No Wallet tab found. 0");
            return;
        }

        _project.L0g(action);

        switch (action)
        {
            case "Add":
                _project.L0g($"adding {_instance.HeGet(("input:url", "fulltagname", "input:url", "text", 0), atr: "value")}");
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                goto getState;
            case "Close":
                _project.L0g($"added {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                goto getState;
            case "Connect":
                _project.L0g($"connecting {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                goto getState;
            case "Sign":
                _project.L0g($"sign {_instance.HeGet(("div", "class", "_uitext_", "regexp", 0))}");
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
                goto getState;

            default:
                goto getState;

        }


    }

    public bool ZerionWaitTx(int deadline = 60, bool log = false)
    {
        DateTime functionStart = DateTime.Now;
    check:
        bool result;
        if ((DateTime.Now - functionStart).TotalSeconds > deadline) throw new Exception($"!W Deadline [{deadline}]s exeeded");


        if (!_instance.ActiveTab.URL.Contains("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview/history"))
        {
            Tab tab = _instance.NewTab("zw");
            if (tab.IsBusy) tab.WaitDownloading();
            _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/sidepanel.21ca0c41.html#/overview/history", "");

        }
        Thread.Sleep(2000);

        var status = _instance.HeGet(("div", "style", "padding: 0px 16px;", "regexp", 0));



        if (status.Contains("Pending")) goto check;
        else if (status.Contains("Failed")) result = false;
        else if (status.Contains("Execute")) result = true;
        else
        {
            Log($"unknown status {status}");
            goto check;
        }
        _instance.CloseExtraTabs();
        return result;

    }
}







}
