using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    internal class SqLoad : Sql
    {

        private readonly IZennoPosterProjectModel _project;

        protected readonly string _tableName;
        protected readonly string _schemaName;
        private readonly bool _logShow;
        private readonly string _dbMode;

        public SqLoad(IZennoPosterProjectModel project, string tablename = null, string schemaName = null, bool log = false)
            : base(project, log)
        {
            _project = project;
            _logShow = log;
            _tableName = tablename;
            _schemaName = schemaName;
            _dbMode = _project.Variables["DBmode"].Value;
            if (string.IsNullOrEmpty(_tableName)) _tableName = _project.ExecuteMacro(_project.Name).Split('.')[0].ToLower();
            if (string.IsNullOrEmpty(_schemaName)) _schemaName = "projects";
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


    }
}
