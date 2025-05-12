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


    





}
