using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public interface ISql
    {
        void Log(string query, string response = null, bool log = false);
        string TblName(string tableName, bool name = true);
        string DbQ(string query, bool log = false, bool throwOnEx = false);
        void MkTable(Dictionary<string, string> tableStructure, string tableName = null, bool strictMode = false, bool insertData = false, string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "", string schemaName = "projects", bool log = false);
        void Write(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true);
        void UpdTxt(string toUpd, string tableName, string key, bool log = false, bool throwOnEx = false);
        void Upd(string toUpd, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, object acc = null);
        void Upd(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false);
        void Upd(List<string> toWrite, string columnName, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false);
        string Get(string toGet, string tableName = null, bool log = false, bool throwOnEx = false, string key = "acc0", string acc = null, string where = "");
        string GetRandom(string toGet, string tableName = null, bool log = false, bool acc = false, bool throwOnEx = false, int range = 0, bool single = true, bool invert = false);
        string GetColumns(string tableName, bool log = false);
        List<string> GetColumnList(string tableName, bool log = false);
        void CreateShemas(string[] schemas);
        bool TblExist(string tblName);
        void TblAdd(string tblName, Dictionary<string, string> tableStructure);
        List<string> TblColumns(string tblName);
        Dictionary<string, string> TblMapForProject(string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''");
        bool ClmnExist(string tblName, string clmnName);
        void ClmnAdd(string tblName, string clmnName, string defaultValue = "TEXT DEFAULT ''");
        void ClmnAdd(string tblName, Dictionary<string, string> tableStructure);
        void ClmnDrop(string tblName, string clmnName);
        void ClmnDrop(string tblName, Dictionary<string, string> tableStructure);
        void ClmnPrune(string tblName, Dictionary<string, string> tableStructure);
        void AddRange(string tblName, int range = 0);
        string Proxy();
        string Bio();
        Dictionary<string, string> Settings(bool set = true);
        string Email(string tableName = "google", string schemaName = "accounts");
        string Ref(string refCode = null, bool log = false);
        Dictionary<string, string> GetAddresses(string chains = null);
        List<string> MkToDoQueries(string toDo = null, string defaultRange = null, string defaultDoFail = null);
        void FilterAccList(List<string> dbQueries, bool log = false);
        string Address(string chainType = "evm");
        string Key(string chainType = "evm");
    }

    public class Db : ISql
    {
        private readonly SqlUtils _utils;
        private readonly SqlStructure _structure;
        private readonly SqlRead _read;
        private readonly SqlWrite _write;
        private readonly SqlTaskManager _taskManager;
        public readonly string[] columnsDefault = { "status", "last" };
        public readonly string[] columnsSocial = { "status", "last", "cookie", "login", "pass", "otpsecret", "email", "recovery" };

        public Db(IZennoPosterProjectModel project, bool log = false)
        {
            _utils = new SqlUtils(project, log);
            _structure = new SqlStructure(project, log, _utils);
            _read = new SqlRead(project, log, _utils);
            _write = new SqlWrite(project, log, _utils);
            _taskManager = new SqlTaskManager(project, log, _utils);
        }

        public void Log(string query, string response = null, bool log = false) => _utils.Log(query, response, log);
        public string TblName(string tableName, bool name = true) => _utils.TblName(tableName, name);
        public string DbQ(string query, bool log = false, bool throwOnEx = false) => _structure.DbQ(query, log, throwOnEx);
        public void MkTable(Dictionary<string, string> tableStructure, string tableName = null, bool strictMode = false, bool insertData = false, string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "", string schemaName = "projects", bool log = false)
            => _structure.MkTable(tableStructure, tableName, strictMode, insertData, host, dbName, dbUser, dbPswd, schemaName, log);
        public void Write(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true)
            => _write.Write(toWrite, tableName, log, throwOnEx, last);
        public void UpdTxt(string toUpd, string tableName, string key, bool log = false, bool throwOnEx = false)
            => _write.UpdTxt(toUpd, tableName, key, log, throwOnEx);
        public void Upd(string toUpd, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, object acc = null)
            => _write.Upd(toUpd, tableName, log, throwOnEx, last, acc);
        public void Upd(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false)
            => _write.Upd(toWrite, tableName, log, throwOnEx, last, byKey);
        public void Upd(List<string> toWrite, string columnName, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false)
            => _write.Upd(toWrite, columnName, tableName, log, throwOnEx, last, byKey);
        public string Get(string toGet, string tableName = null, bool log = false, bool throwOnEx = false, string key = "acc0", string acc = null, string where = "")
            => _read.Get(toGet, tableName, log, throwOnEx, key, acc, where);
        public string GetRandom(string toGet, string tableName = null, bool log = false, bool acc = false, bool throwOnEx = false, int range = 0, bool single = true, bool invert = false)
            => _read.GetRandom(toGet, tableName, log, acc, throwOnEx, range, single, invert);
        public string GetColumns(string tableName, bool log = false) => _structure.GetColumns(tableName, log);
        public List<string> GetColumnList(string tableName, bool log = false) => _structure.GetColumnList(tableName, log);
        public void CreateShemas(string[] schemas) => _structure.CreateShemas(schemas);
        public bool TblExist(string tblName) => _structure.TblExist(tblName);
        public void TblAdd(string tblName, Dictionary<string, string> tableStructure) => _structure.TblAdd(tblName, tableStructure);
        public List<string> TblColumns(string tblName) => _structure.TblColumns(tblName);
        public Dictionary<string, string> TblMapForProject(string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''")
            => _structure.TblMapForProject(staticColumns, dynamicToDo, defaultType);
        public bool ClmnExist(string tblName, string clmnName) => _structure.ClmnExist(tblName, clmnName);
        public void ClmnAdd(string tblName, string clmnName, string defaultValue = "TEXT DEFAULT ''") => _structure.ClmnAdd(tblName, clmnName, defaultValue);
        public void ClmnAdd(string tblName, Dictionary<string, string> tableStructure) => _structure.ClmnAdd(tblName, tableStructure);
        public void ClmnDrop(string tblName, string clmnName) => _structure.ClmnDrop(tblName, clmnName);
        public void ClmnDrop(string tblName, Dictionary<string, string> tableStructure) => _structure.ClmnDrop(tblName, tableStructure);
        public void ClmnPrune(string tblName, Dictionary<string, string> tableStructure) => _structure.ClmnPrune(tblName, tableStructure);
        public void AddRange(string tblName, int range = 0) => _write.AddRange(tblName, range);
        public string Proxy() => _read.Proxy();
        public string Bio() => _read.Bio();
        public Dictionary<string, string> Settings(bool set = true) => _read.Settings(set);
        public string Email(string tableName = "google", string schemaName = "accounts") => _read.Email(tableName, schemaName);
        public string Ref(string refCode = null, bool log = false) => _read.Ref(refCode, log);
        public Dictionary<string, string> GetAddresses(string chains = null) => _read.GetAddresses(chains);
        public List<string> MkToDoQueries(string toDo = null, string defaultRange = null, string defaultDoFail = null)
            => _taskManager.MkToDoQueries(toDo, defaultRange, defaultDoFail);
        public void FilterAccList(List<string> dbQueries, bool log = false) => _taskManager.FilterAccList(dbQueries, log);
        public string Address(string chainType = "evm") => _read.Address(chainType);
        public string Key(string chainType = "evm") => _read.Key(chainType);
    }

}
