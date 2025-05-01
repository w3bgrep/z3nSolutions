using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
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
            _log.Send(toLog);
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
                result = PostgresDB.pSQL(_project, query, log, throwOnEx);
            }
            else throw new Exception($"unknown DBmode: {dbMode}");
            SqlLog(query, result, log: log);
            return result;
        }
        public void MkTable(Dictionary<string, string> tableStructure, string tableName = "", bool strictMode = false, bool insertData = false, string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "", string schemaName = "projects", bool log = false)
        {
            string dbMode = _project.Variables["DBmode"].Value;

            if (dbMode == "SQLite")
            {
                SQLite.lSQLMakeTable(_project, tableStructure, tableName, strictMode);
            }
            else if (dbMode == "PostgreSQL")
            {
                PostgresDB.pSQLMakeTable(_project, tableStructure, tableName, strictMode, insertData, host, dbName, dbUser, dbPswd, schemaName, log: log);
            }
            else throw new Exception($"unknown DBmode: {dbMode}");
            return ;
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
}
