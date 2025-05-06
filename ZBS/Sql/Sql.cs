
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
        private readonly L0g _log;
        protected readonly string _dbMode;
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
                if (!string.IsNullOrEmpty(response)) toLog += $"\n          [{response.Replace("\n", "|").Replace("\r", "")}]";
            }
            else
            {
                toLog += $"[ ▲ {dbMode}]: [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
            }
            _project.L0g(toLog);
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
                    SqlLog($"!W Err:[{ex.Message}]. Q:[{query}]");
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
                    SqlLog($"!W Err:[{ex.Message}]. Q:[{query}]");
                    if (throwOnEx) throw;
                }
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
        public string BinanceApiKeys(string tableName = "settings", string schemaName = "accounts")
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"{schemaName}." : "") + tableName;
            return DbQ($"SELECT value FROM {tableName} WHERE var = 'settingsApiBinance';");
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

        public Dictionary<string, string> GetAddresses(string chains = null)
        {
            var addrss = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(chains)) chains = GetColumns("blockchain_public");
            string[] tikers = chains.Replace(" ","").Split(',');
            string[] addresses = Get(chains, "blockchain_public").Replace(" ", "").Split('|');


            for (int i = 0; i < tikers.Length; i++)
            {
                var tiker = tikers[i].Trim(); 
                var address = addresses[i].Trim();
                addrss.Add(tiker, address);
            }

            return addrss;

        }






    }


    

    
}