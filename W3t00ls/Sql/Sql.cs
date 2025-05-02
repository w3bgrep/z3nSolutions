using Nethereum.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace W3t00ls
{
    public class Sql
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly L0g _log;
        private readonly string _dbMode;
        private readonly bool _logShow;

        public Sql(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _log = new L0g(_project);
            _dbMode = _project.Variables["DBmode"].Value;
            _logShow = log;
        }
        public void SqlLog(string query, string response = null, bool log = false)
        {
            if (!_logShow && !log) return;
            string dbMode = _project.Variables["DBmode"].Value;
            string toLog = null;

            if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                toLog += $"[ ▼ {dbMode}]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
                if (!string.IsNullOrEmpty(response)) toLog += $"\n          [{response.Replace('\n', '|')}]";
            }
            else
            {
                toLog += $"[ ▲ {dbMode}]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
            }
            _project.L0g(toLog);
            //_log.Send(toLog);
        }
        public string DbQ(string query, bool log = false, bool throwOnEx = false)
        {
            string dbMode = _project.Variables["DBmode"].Value;
            string result = null;

            if (dbMode == "SQLite")
            {
                result = SQLite.lSQL(_project, query, log);
            }
            else if (dbMode == "PostgreSQL")
            {
                result = PostgresDB.DbQueryPostgre(_project, query, log, throwOnEx);
            }
            else throw new Exception($"unknown DBmode: {dbMode}");
            SqlLog(query, result, log: log);
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
        public void Upd(string toUpd, string tableName = null, bool log = false, bool throwOnEx = false)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            if (_dbMode == "PostgreSQL" && !tableName.Contains(".")) tableName = "accounts." + tableName;

            var Q = $@"UPDATE {tableName} SET {toUpd.Trim().TrimEnd(',')}, last = '{DateTime.UtcNow.ToString("MM-ddTHH:mm")}' WHERE acc0 = {_project.Variables["acc0"].Value};";
            DbQ(Q, log: log, throwOnEx: throwOnEx);
        }
        public string Get(string toGet, string tableName = null, bool log = false, bool throwOnEx = false)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            if (_dbMode == "PostgreSQL" && !tableName.Contains(".")) tableName = "accounts." + tableName;

            var Q = $@"SELECT {toGet.Trim().TrimEnd(',')} from {tableName} WHERE acc0 = {_project.Variables["acc0"].Value};";
            return DbQ(Q, log: log, throwOnEx: throwOnEx);
        }
        public string KeyEVM(string tableName = "blockchain_private", string schemaName = "accounts")
        {
            string table = (_dbMode == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($"SELECT secp256k1 FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            return SAFU.Decode(_project, resp);
        }
        public string KeySOL(string tableName = "blockchain_private", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($"SELECT base58 FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            return SAFU.Decode(_project, resp);
        }
        public string Seed(string tableName = "blockchain_private", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($"SELECT bip39 FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            return SAFU.Decode(_project, resp);
        }
        public string AdrEvm(string tableName = "blockchain_public", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($"SELECT evm FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            _project.Variables["addressEvm"].Value = resp; return resp;
        }
        public string AdrSol(string tableName = "blockchain_public", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($"SELECT sol FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            _project.Variables["addressSol"].Value = resp; return resp;
        }
        public string Proxy(string tableName = "profile", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($"SELECT proxy FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value}");
            _project.Variables["proxy"].Value = resp;
            try { _project.Variables["proxyLeaf"].Value = resp.Replace("//", "").Replace("@", ":"); } catch { }
            return resp;
        }
        public string Bio(string tableName = "profile", string schemaName = "accounts")
        {

            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($@"SELECT nickname, bio FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value};");
            string[] respData = resp.Split('|');
            _project.Variables["accNICKNAME"].Value = respData[0].Trim();
            _project.Variables["accBIO"].Value = respData[1].Trim();
            return resp;
        }
        public string Settings(string tableName = "settings", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            return DbQ($"SELECT var, value FROM {table}");
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
        public string Twitter(string tableName = "twitter", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($@"SELECT status, token, login, password, code2fa, emailLogin, emailPass FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value};");

            string[] twitterData = resp.Split('|');
            _project.Variables["twitterSTATUS"].Value = twitterData[0].Trim();
            _project.Variables["twitterTOKEN"].Value = twitterData[1].Trim();
            _project.Variables["twitterLOGIN"].Value = twitterData[2].Trim();
            _project.Variables["twitterPASSWORD"].Value = twitterData[3].Trim();
            _project.Variables["twitterCODE2FA"].Value = twitterData[4].Trim();
            _project.Variables["twitterEMAIL"].Value = twitterData[5].Trim();
            _project.Variables["twitterEMAIL_PASSWORD"].Value = twitterData[6].Trim();
            return _project.Variables["twitterSTATUS"].Value;

        }
        public string Discord(string tableName = "discord", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($@"SELECT status, token, login, password, code2FA, username, servers FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value};");
            string[] discordData = resp.Split('|');
            _project.Variables["discordSTATUS"].Value = discordData[0].Trim();
            _project.Variables["discordTOKEN"].Value = discordData[1].Trim();
            _project.Variables["discordLOGIN"].Value = discordData[2].Trim();
            _project.Variables["discordPASSWORD"].Value = discordData[3].Trim();
            _project.Variables["discord2FACODE"].Value = discordData[4].Trim();
            _project.Variables["discordUSERNAME"].Value = discordData[5].Trim();
            _project.Variables["discordSERVERS"].Value = discordData[6].Trim();
            return _project.Variables["discordSTATUS"].Value;
        }
        public string Google(string tableName = "google", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;

            var resp = DbQ($@"SELECT status, login, password, code2FA, recoveryEmail, recovery2FA FROM {table} WHERE acc0 = {_project.Variables["acc0"].Value};");

            string[] googleData = resp.Split('|');
            _project.Variables["googleSTATUS"].Value = googleData[0].Trim();
            _project.Variables["googleLOGIN"].Value = googleData[1].Trim();
            _project.Variables["googlePASSWORD"].Value = googleData[2].Trim();
            _project.Variables["google2FACODE"].Value = googleData[3].Trim();
            _project.Variables["googleSECURITY_MAIL"].Value = googleData[4].Trim();
            _project.Variables["googleBACKUP_CODES"].Value = googleData[5].Trim();
            return _project.Variables["googleSTATUS"].Value;
        }
        public void TwitterTokenUpdate(string tableName = "twitter", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var resp = DbQ($"UPDATE {table} SET token = '{_project.Variables["twitterTOKEN"].Value}' WHERE acc0 = {_project.Variables["acc0"].Value};");

        }
        public void UpdAddressSol(string address = "", string tableName = "blockchain_public", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            if (address == "") address = _project.Variables["addressSol"].Value;
            DbQ($"UPDATE {table} SET sol = '{address}' WHERE acc0 = {_project.Variables["acc0"].Value};");

        }
        public string BinanceApiKeys(string tableName = "settings", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            return DbQ($"SELECT value FROM {tableName} WHERE var = 'settingsApiBinance';");
        }
        public string[] okxKeys(string tableName = "settings", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            var key = DbQ($"SELECT value FROM {table} WHERE var = 'okx_apikey';", true);
            var secret = DbQ($"SELECT value FROM {table} WHERE var = 'okx_secret';");
            var passphrase = DbQ($"SELECT value FROM {table} WHERE var = 'okx_passphrase';");
            string[] result = new string[] { key, secret, passphrase };
            return result;
        }

        public string Ref(string refCode = null, bool log = false)
        {
            if (string.IsNullOrEmpty(refCode)) refCode = _project.Variables["cfgRefCode"].Value;

            if (string.IsNullOrEmpty(refCode) || refCode == "_") refCode = DbQ($@"SELECT refcode FROM {_project.Variables["projectTable"].Value}
			WHERE refcode != '_' 
			AND TRIM(refcode) != ''
			ORDER BY RANDOM()
			LIMIT 1;", log);
            return refCode;
        }

    }


    public class TableMngr : Sql
    {
        private readonly IZennoPosterProjectModel _project;

        protected readonly string _tableName;
        protected readonly string _schemaName;
        private readonly bool _logShow;


        public TableMngr(IZennoPosterProjectModel project, string tablename = null, string schemaName = null, bool log = false)
            : base(project, log)
        {
            _project = project;
            _logShow = log;
            _tableName = tablename;
            _schemaName = schemaName;
            if (string.IsNullOrEmpty(_tableName)) _tableName =  _project.ExecuteMacro(_project.Name).Split('.')[0].ToLower();
            if (string.IsNullOrEmpty(_schemaName)) _schemaName = "projects";
            _logShow = log;

        }
        private void Log(string query, string response = null, bool log = false)
        {
            if (!_logShow && !log) return;
            string dbMode = _project.Variables["DBmode"].Value;
            string toLog = null;

            if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                toLog += $"[ ▼ {dbMode}]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
                if (!string.IsNullOrEmpty(response)) toLog += $"\n          [{response.Replace('\n', '|')}]";
            }
            else
            {
                toLog += $"[ ▲ {dbMode}]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
            }
            _project.L0g(toLog);
        }
        private bool CreateIfNotExist(Dictionary<string, string> tableStructure)
        {
            string tableName = _tableName;
            string schemaName = _schemaName;

            if (_tableName.Contains("."))
            {
                tableName = _tableName.Split('.')[0];
                schemaName = _tableName.Split('.')[1];
            }
            string query = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_name = '{tableName}';";
            var resp = DbQ($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_name = '{tableName}';");
            string tableExists = resp.Trim() ?? "0";

            if (tableExists == "0")
            {
                DbQ($@"
					CREATE TABLE {schemaName}.{tableName} (
						{string.Join(", ", tableStructure.Select(kvp => $"\"{kvp.Key}\" {kvp.Value.Replace("AUTOINCREMENT", "SERIAL")}"))}
					);", _logShow);
                return true;
            }
            return false;
        }
        private void ManageColumns(Dictionary<string, string> tableStructure, bool prune)
        {
            string ChkQ = DbQ($@"SELECT column_name FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}';", true);

            var currentColumns = ChkQ.Split('\n').ToList();

            if (prune)
            {
                var current = string.Join("|", currentColumns);
                var keys = string.Join("|", tableStructure.Keys);

                //var columnsToRemove = currentColumns.Where(col => !tableStructure.ContainsKey(col)).ToList();
                var columnsToRemove = new List<string>();
                foreach (var col in currentColumns)
                {
                    string column = col.Trim();
                    if (!tableStructure.ContainsKey(column))
                    {
                        SqlLog($"[{col}] ! in [{keys}] ");
                        columnsToRemove.Add(column);
                    }
                }

                var ToRemove = string.Join("|", columnsToRemove);
                SqlLog(ToRemove);

                foreach (var column in columnsToRemove)
                {
                    DbQ($@"ALTER TABLE {_schemaName}.{_tableName} DROP COLUMN {column} CASCADE;", true);
                }
            }
            foreach (var column in tableStructure)
            {
                string columnExists = DbQ($@"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}' AND lower(column_name) = lower('{column.Key}');")?.Trim() ?? "0";
                if (columnExists == "0")
                {
                    DbQ($@"ALTER TABLE {_schemaName}.{_tableName} ADD COLUMN {column.Key} {column.Value};", true);
                }
            }
        }
        private void FillInitial(string cfgRangeEnd,bool log = false)
        {
            if (!int.TryParse(cfgRangeEnd, out int rangeEnd) || rangeEnd <= 0)
                throw new ArgumentException("cfgRangeEnd must be a positive integer");

            string maxAcc0Query = $@"SELECT COALESCE(MAX(acc0), 0) FROM {_schemaName}.{_tableName};";

            string maxAcc0Str = DbQ(maxAcc0Query)?.Trim() ?? "0";
            if (!int.TryParse(maxAcc0Str, out int maxAcc0))
            {
                maxAcc0 = 1;
            }

            if (maxAcc0 < rangeEnd)
            {
                for (int currentAcc0 = maxAcc0 + 1; currentAcc0 <= rangeEnd; currentAcc0++)
                {
                    string insertQuery = $@"INSERT INTO {_schemaName}.{_tableName} (acc0) VALUES ({currentAcc0}) ON CONFLICT DO NOTHING;";

                    DbQ(insertQuery, _logShow);
                }
            }
            else
            {
                if (log)
                {
                    Log($"No new data to insert. Max acc0 ({maxAcc0}) is >= rangeEnd ({rangeEnd})");
                }
            }
        }
        public void ManageTable(Dictionary<string, string> tableStructure, bool prune)
        {
            var created = CreateIfNotExist (tableStructure);
            ManageColumns(tableStructure, prune:prune);
            FillInitial(_project.Variables["rangeEnd"].Value);
        }

    }
}