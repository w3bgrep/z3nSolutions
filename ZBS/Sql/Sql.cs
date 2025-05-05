
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

    }


    

    
}