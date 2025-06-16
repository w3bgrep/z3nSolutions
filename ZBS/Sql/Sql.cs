
using Nethereum.ABI.CompilationMetadata;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class Sql
    {
        private readonly IZennoPosterProjectModel _project;
        protected readonly string _dbMode;
        private readonly bool _logShow;

        private readonly Logger _logger;

        protected bool _pstgr = false;
        protected string _tableName = string.Empty;
        protected string _schemaName = string.Empty;

        public Sql(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _pstgr = _project.Variables["DBmode"].Value == "PostgreSQL" ? true : false;
            _dbMode = _project.Variables["DBmode"].Value;

            if (_pstgr) _logger = new Logger(project, log: log, classEmoji: "🐘");
            else _logger = new Logger(project, log: log, classEmoji: "✒");
            _logShow = log;
        }
        public void Log(string query, string response = null, bool log = false)
        {
            if (!_logShow && !log) return;
            string dbMode = _project.Variables["DBmode"].Value;
            string toLog = null;


            if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                toLog += $"( ▼ ): [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
                if (!string.IsNullOrEmpty(response)) toLog += $"\n          [{response.Replace("\n", "|").Replace("\r", "")}]";
            }
            else
            {
                toLog += $"( ▲ ): [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
            }
            _logger.Send(toLog);
            //_project.L0g(toLog);
        }

        public string TblName(string tableName, bool name = true)
        {
            string schemaName = "projects";
            if (_dbMode == "PostgreSQL")
            {
                if (tableName.Contains("."))
                {
                    schemaName = tableName.Split('.')[0];
                    tableName = tableName.Split('.')[1];
                }
                else if (tableName.Contains("_"))
                {
                    schemaName = tableName.Split('_')[0];
                    tableName = tableName.Split('_')[1];
                }

            }
            else if (_dbMode == "SQLite")
            {
                if (tableName.Contains(".")) tableName = tableName.Replace(".", "_");
            }

            _tableName = tableName;
            _schemaName = schemaName;

            if (name) return tableName;
            else return schemaName;
        }
        private string QuoteColumnNames(string updateString)
        {
            var parts = updateString.Split(',').Select(p => p.Trim()).ToList();
            var result = new List<string>();

            foreach (var part in parts)
            {
                int equalsIndex = part.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string columnName = part.Substring(0, equalsIndex).Trim();
                    string valuePart = part.Substring(equalsIndex).Trim();
                    result.Add($"\"{columnName}\" {valuePart}");
                }
                else
                {
                    result.Add(part);
                }
            }
            return string.Join(", ", result);
        }


        public string DbQ(string query, bool log = false, bool throwOnEx = false)
        {
            string dbMode = _project.Variables["DBmode"].Value;
            string result = null;

            if (dbMode == "SQLite")
            {
                try
                {
                    result = SQLite.lSQL(_project, query, log);
                }
                catch (Exception ex)
                {
                    Log($"!W Err:[{ex.Message}]. debug:\n{query}");
                    if (throwOnEx) throw;
                }
            }
            else if (dbMode == "PostgreSQL")
            {
                try { 
                    result = PostgresDB.DbQueryPostgre(_project, query, log, throwOnEx);
                }
                catch (Exception ex)
                {
                    Log($"!W Err:[{ex.Message}]. debug:\n{query}");
                    if (throwOnEx) throw;
                }
            }
            else throw new Exception($"unknown DBmode: {dbMode}");
            Log(query, result, log: log);
            return result;
        }
        
        public void MkTable(Dictionary<string, string> tableStructure, string tableName = null, bool strictMode = false, bool insertData = false, string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "", string schemaName = "projects", bool log = false)
        {
            string dbMode = _project.Variables["DBmode"].Value;
            if (string.IsNullOrEmpty(tableName))
            {
                if (_project.Variables["makeTable"].Value != "True") return;
                else tableName = $"{_project.Variables["projectName"].Value.ToLower()}";
                if (dbMode != "PostgreSQL") tableName = $"_{tableName}";
            }
            if (dbMode == "SQLite")
            {
                SQLite.lSQLMakeTable(_project, tableStructure, tableName, strictMode);
            }
            else if (dbMode == "PostgreSQL")
            {
                PostgresDB.MkTablePostgre(_project, tableStructure, tableName, strictMode, insertData, host, dbName, dbUser, dbPswd, schemaName, log: log);
            }
            else throw new Exception($"unknown DBmode: {dbMode}");
            return;
        }
 
        
        public void Write(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;

            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            foreach (KeyValuePair<string, string> pair in toWrite)
            {
                string key = pair.Key.Replace("'", "''");
                string value = pair.Value.Replace("'", "''");
                string query = $@"INSERT INTO {_tableName} (key, value) VALUES ('{key}', '{value}')
	                  ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value;";
                DbQ(query, log: log);
            }

        }
        public void UpdTxt(string toUpd, string tableName, string key, bool log = false, bool throwOnEx = false)
        {
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            toUpd = toUpd.Trim().TrimEnd(',');
            DbQ($@"INSERT INTO {_tableName} (key) VALUES ('{key}') ON CONFLICT DO NOTHING;");
            DbQ($@"UPDATE {_tableName} SET {toUpd} WHERE key = '{key}';", log: log, throwOnEx: throwOnEx);

        }

        public void Upd(string toUpd, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, object acc = null)
        {
            
            int acc0 = 0;
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            if (acc != null)
            {
                if (acc is int i) acc0 = i;
                else if (acc is string s) int.TryParse(s, out acc0);
            }
            else int.TryParse(_project.Var("acc0"), out acc0);


            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            string[] keywords = { "blockchain", "browser", "cex_deps", "native", "profile", "settings" };
            if (keywords.Any(keyword => tableName.Contains(keyword))) last = false;
            
            toUpd = toUpd.Trim().TrimEnd(',');
            if (last) toUpd = toUpd + $", last = '{DateTime.UtcNow.ToString("MM-ddTHH:mm")}'";

            toUpd = QuoteColumnNames(toUpd);
            string query = $@"UPDATE {_tableName} SET {toUpd} WHERE acc0 = {acc0};";
            try { _project.Var("lastQuery", query); } catch { }
            DbQ(query, log: log, throwOnEx: throwOnEx);
        }
        public void Upd(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            int dicSize = toWrite.Count;
            AddRange(_tableName, dicSize);

            foreach (KeyValuePair<string, string> pair in toWrite)
            {
                string key = pair.Key;
                string value = pair.Value;
                Upd(value,_tableName,last:last, acc: key);
            }
        }
        public void Upd(List<string> toWrite, string columnName, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            int dicSize = toWrite.Count;
            AddRange(_tableName, dicSize);

            int key = 1;
            foreach (string valToWrite in toWrite)
            {

                Upd($"{columnName} = '{valToWrite.Replace("'", "''")}'", _tableName, last: last, acc: key);
                key++;
            }
        }



        public string Get(string toGet, string tableName = null, bool log = false, bool throwOnEx = false, string key = "acc0", string acc = null)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            if (string.IsNullOrEmpty(acc)) acc = _project.Variables["acc0"].Value;
            if (key == "acc0") return DbQ($@"SELECT {toGet.Trim().TrimEnd(',')} from {_tableName} WHERE acc0 = {acc};", log: log, throwOnEx: throwOnEx);
            else return DbQ($@"SELECT {toGet.Trim().TrimEnd(',')} from {_tableName} WHERE key = '{key}';", log: log, throwOnEx: throwOnEx);
        }


        public string GetRandom(string toGet, string tableName = null, bool log = false, bool acc = false, bool throwOnEx = false, int range = 0, bool single = true, bool invert = false)
        {
            if (range == 0) range = _project.Range();
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            string acc0 = string.Empty;
            if (acc) acc0 = "acc0, ";

            string query = $@"
                SELECT {acc0}{toGet.Trim().TrimEnd(',')} 
                from {_tableName} 
                WHERE TRIM({toGet}) != ''
	            AND acc0 < {range}
                ORDER BY RANDOM()";

            if (single) query += " LIMIT 1;";
            if (invert) query = query.Replace("!=", "=");

            return DbQ(query, log:log, throwOnEx: throwOnEx);
        }


        public string GetColumns(string tableName, string schemaName = "accounts", bool log = false)
        {
            string Q; string name;
            if (tableName.Contains(".")) {
                schemaName = tableName.Split('.')[0];
                name = tableName.Split('.')[1];
            }
            else name = tableName;

            if (_dbMode == "PostgreSQL") Q = $@"SELECT column_name FROM information_schema.columns WHERE table_schema = '{schemaName}' AND table_name = '{name}'";
            else Q = $@"SELECT name FROM pragma_table_info('{name}')";
            return DbQ(Q, log: log).Replace("\n", ", ").Trim(',').Trim();
        }

        public void CreateShemas(string[] schemas)
        {
            if (!_pstgr) return;
            foreach (string name in schemas) DbQ($"CREATE SCHEMA IF NOT EXISTS {name};");
        }


        
        public bool TblExist(string tblName)
        {
            TblName(tblName);
            string resp = null;
            if (_pstgr) resp = DbQ($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}';");
            else resp = DbQ($"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{_tableName}';");
            if (resp == "0" || resp == string.Empty) return false;
            else return true;
        }
        public void TblAdd(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            if (TblExist(tblName)) return;
            if (_pstgr) DbQ($@" CREATE TABLE {_schemaName}.{_tableName} ( {string.Join(", ", tableStructure.Select(kvp => $"\"{kvp.Key}\" {kvp.Value.Replace("AUTOINCREMENT", "SERIAL")}"))} );");
            else DbQ($"CREATE TABLE {_tableName} (" + string.Join(", ", tableStructure.Select(kvp => $"{kvp.Key} {kvp.Value}")) + ");");
        }
        public List<string> TblColumns(string tblName)
        {
            var result = new List<string>();
            TblName(tblName);
            if (_dbMode == "PostgreSQL")
                result = DbQ($@"SELECT column_name FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}';", log: _logShow)
                    .Split('\n')
                    .Select(s => s.Trim())
                    .ToList();
            else
                result = DbQ($"SELECT name FROM pragma_table_info('{_tableName}');", log: _logShow)
                    .Split('\n')
                    .Select(s => s.Trim())
                    .ToList();
            if (_dbMode == "PostgreSQL") _tableName = $"{_schemaName}.{_tableName}";
            return result;
        }
        public  Dictionary<string, string> TblMapForProject( string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''")
        {
            if (string.IsNullOrEmpty(dynamicToDo)) dynamicToDo = _project.Variables["cfgToDo"].Value;


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
            if (!string.IsNullOrEmpty(dynamicToDo)) 
            {
                //dynamicToDo = dynamicToDo.ToLower();
                string[] toDoItems = (dynamicToDo ?? "").Split(',');
                foreach (string taskId in toDoItems)
                {
                    string trimmedTaskId = taskId.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedTaskId) && !tableStructure.ContainsKey(trimmedTaskId))
                    {
                        tableStructure.Add(trimmedTaskId, defaultType);
                    }
                }
            }
            return tableStructure;
        }


        public bool ClmnExist(string tblName, string clmnName)
        {
            TblName(tblName);
            string resp = null;
            if (_pstgr)
                resp = DbQ($@"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}' AND lower(column_name) = lower('{clmnName}');", log: _logShow)?.Trim();
            else
                resp = DbQ($"SELECT COUNT(*) FROM pragma_table_info('{_tableName}') WHERE name='{clmnName}';", log: _logShow);
            if (resp == "0" || resp == string.Empty) return false;
            else return true;

        }
        public void ClmnAdd(string tblName, string clmnName, string defaultValue = "TEXT DEFAULT \"\"")
        {
            TblName(tblName);
            var current = TblColumns(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            if (!current.Contains(clmnName))
            {
                DbQ($@"ALTER TABLE {_tableName} ADD COLUMN {clmnName} {defaultValue};", log: _logShow);
            }

        }
        public void ClmnAdd(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            var current = TblColumns(tblName);
            
            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            Log(string.Join(",", current));
            foreach (var column in tableStructure)
            {
                var keyWd = column.Key.Trim();
                if (!current.Contains(keyWd))
                {
                    Log($"CLMNADD [{keyWd}] not in  [{string.Join(",", current)}] ");
                    DbQ($@"ALTER TABLE {_tableName} ADD COLUMN {keyWd} {column.Value};", log: _logShow);
                }
            }
        }
        public void ClmnDrop(string tblName, string clmnName)
        {
            TblName(tblName);
            var current = TblColumns(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            if (current.Contains(clmnName))
            {
                string cascade = (_pstgr) ? " CASCADE" : null;
                DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {clmnName}{cascade};", log: _logShow);
            }
        }
        public void ClmnDrop(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            var current = TblColumns(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            foreach (var column in tableStructure)
            {
                if (!current.Contains(column.Key))
                {
                    string cascade = _dbMode == "PostgreSQL" ? " CASCADE" : null;
                    DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {column.Key}{cascade};", log: _logShow);
                }
            }
        }
        public void ClmnPrune(string tblName, Dictionary<string, string> tableStructure)
        {

            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            var current = TblColumns(tblName);

            foreach (var column in current)
            {
                if (!tableStructure.ContainsKey(column))
                {
                    Log($"[{column}] not in tableStructure keys, dropping");
                    string cascade = _pstgr ? " CASCADE" : "";
                    DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {column}{cascade};", log:_logShow);
                }
            }
        }           
        public void AddRange(string tblName, int range = 0)
        {
            if (range == 0)
                try
                {
                    range = int.Parse(_project.Variables["rangeEnd"].Value);
                }
                catch
                {
                    Log("var  rangeEnd is empty or 0, fallback to 100");
                    range = 100;
                }

            TblName(tblName);

            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int current = int.Parse(DbQ($@"SELECT COALESCE(MAX(acc0), 0) FROM {_tableName};"));
            Log(current.ToString());
            Log(range.ToString());
            for (int currentAcc0 = current + 1; currentAcc0 <= range; currentAcc0++)
            {
                DbQ($@"INSERT INTO {_tableName} (acc0) VALUES ({currentAcc0}) ON CONFLICT DO NOTHING;");
            }

        }


        public string Proxy()
        {
            TblName("private_profile");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = Get("proxy", _tableName);
            _project.Variables["proxy"].Value = resp;
            return resp;
        }
        public string Bio()
        {
            TblName("public_profile"); if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = DbQ($@"SELECT nickname, bio FROM {_tableName} WHERE acc0 = {_project.Variables["acc0"].Value};");
            string[] respData = resp.Split('|');
            _project.Variables["accNICKNAME"].Value = respData[0].Trim();
            _project.Variables["accBIO"].Value = respData[1].Trim();
            return resp;
        }
        public Dictionary<string,string> Settings(bool set = true)
        {
            var dbConfig = new Dictionary<string,string>();
            
            TblName("private_settings");  
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = DbQ($"SELECT key, value FROM {_tableName}");
            foreach (string varData in resp.Split('\n'))
            {
                string varName = varData.Split('|')[0];
                string varValue = varData.Split('|')[1].Trim();
                dbConfig.Add(varName, varValue);

                if (set) 
                {
                    try { _project.Var(varName, varValue); }
                    catch (Exception e) { Log(e.Message); }
                }
            }
            return dbConfig;

        }
        public string Email(string tableName = "google", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var emailMode = _project.Variables["cfgMail"].Value;
            var resp = DbQ($@"SELECT login, icloud FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value};");

            string[] emailData = resp.Split('|');
            _project.Variables["emailGOOGLE"].Value = emailData[0].Trim();
            _project.Variables["emailICLOUD"].Value = emailData[1].Trim();

            if (emailMode == "Google") resp = emailData[0].Trim();
            if (emailMode == "Icloud") resp = emailData[1].Trim();
            return resp;
        }

        public string Ref(string refCode = null, bool log = false)
        {
            if (string.IsNullOrEmpty(refCode)) refCode = _project.Variables["cfgRefCode"].Value;
            
            TblName(_project.Variables["projectTable"].Value);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            if (string.IsNullOrEmpty(refCode) || refCode == "_") refCode = DbQ($@"SELECT refcode FROM {_tableName}
			WHERE refcode != '_' 
			AND TRIM(refcode) != ''
			ORDER BY RANDOM()
			LIMIT 1;", log);
            return refCode;
        }

        public Dictionary<string, string> GetAddresses(string chains = null)
        {
            var addrss = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(chains)) chains = GetColumns("public_blockchain");
            string[] tikers = chains.Replace(" ","").Split(',');
            string[] addresses = Get(chains, "public_blockchain").Replace(" ", "").Split('|');


            for (int i = 0; i < tikers.Length; i++)
            {
                var tiker = tikers[i].Trim(); 
                var address = addresses[i].Trim();
                addrss.Add(tiker, address);
            }

            return addrss;

        }
        public List<string> MkToDoQueries(string toDo = null, string defaultRange = null, string defaultDoFail = null)
        {
            string tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";


            var nowIso = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            if (string.IsNullOrEmpty(toDo)) toDo = _project.Variables["cfgToDo"].Value;

            string[] toDoItems = (toDo ?? "").Split(',');

            var allQueries = new List<string>();

            foreach (string taskId in toDoItems)
            {
                string trimmedTaskId = taskId.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedTaskId))
                {

                    string range = defaultRange ?? _project.Variables["range"].Value;
                    string doFail = defaultDoFail ?? _project.Variables["doFail"].Value;
                    string failCondition = (doFail != "True" ? "AND status NOT LIKE '%fail%'" : "");
                    string query = $@"SELECT acc0 FROM {_tableName} WHERE acc0 in ({range}) {failCondition} AND status NOT LIKE '%skip%' 
                AND ({trimmedTaskId} < '{nowIso}' OR {trimmedTaskId} = '_')";
                    allQueries.Add(query);
                }
            }

            return allQueries;
        }
        public void FilterAccList(List<string> dbQueries, bool log = false)
        {
            if (!string.IsNullOrEmpty(_project.Variables["acc0Forced"].Value))
            {
                _project.Lists["accs"].Clear();
                _project.Lists["accs"].Add(_project.Variables["acc0Forced"].Value);
                Log($@"manual mode on with {_project.Variables["acc0Forced"].Value}");
                return;
            }

            var allAccounts = new HashSet<string>();
            foreach (var query in dbQueries)
            {
                try
                {
                    var accsByQuery = DbQ(query).Trim();
                    if (!string.IsNullOrWhiteSpace(accsByQuery))
                    {
                        var accounts = accsByQuery.Split('\n').Select(x => x.Trim().TrimStart(','));
                        allAccounts.UnionWith(accounts);
                    }
                }
                catch
                {

                    Log($"{query}");
                }
            }

            if (allAccounts.Count == 0)
            {
                _project.Variables["noAccsToDo"].Value = "True";
                Log($"♻ noAccountsAvailable by queries [{string.Join(" | ", dbQueries)}]");
                return;
            }
            Log($"Initial availableAccounts: [{string.Join(", ", allAccounts)}]");

            if (!string.IsNullOrEmpty(_project.Variables["requiredSocial"].Value))
            {
                string[] demanded = _project.Variables["requiredSocial"].Value.Split(',');
                Log($"Filtering by socials: [{string.Join(", ", demanded)}]");

                foreach (string social in demanded)
                {
                    string tableName = TblName($"private_{social.Trim().ToLower()}");

                    TblName(tableName);
                    if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";


                    var notOK = DbQ($"SELECT acc0 FROM {_tableName} WHERE status NOT LIKE '%ok%'", log)
                        .Split('\n')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x));
                    allAccounts.ExceptWith(notOK);
                    Log($"After {social} filter: [{string.Join("|", allAccounts)}]");
                }
            }
            _project.Lists["accs"].Clear();
            _project.Lists["accs"].AddRange(allAccounts);
            Log($"final list [{string.Join("|", _project.Lists["accs"])}]");
        }

        public string Address(string chainType = "evm")
        {
            chainType = chainType.ToLower().Trim();

            TblName("public_blockchain");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = Get(chainType, _tableName);

            try
            {
                chainType = chainType.ToUpper();
                _project.Variables[$"address{chainType}"].Value = resp;
            }
            catch { }
            return resp;

        }
        public string Key(string chainType = "evm")
        {
            chainType = chainType.ToLower().Trim();
            TblName("private_blockchain");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            switch (chainType)
            {
                case "evm":
                    chainType = "secp256k1";
                    break;
                case "sol":
                    chainType = "base58";
                    break;
                case "seed":
                    chainType = "bip39";
                    break;
                default:
                    throw new Exception("unexpected input. Use (evm|sol|seed|pkFromSeed)");
            }
            
            var resp = Get(chainType, _tableName);
            if (!string.IsNullOrEmpty(_project.Var("cfgPin")))
                return SAFU.Decode(_project, resp);
            else return resp;


        }

    }





}