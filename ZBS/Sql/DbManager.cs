using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public enum schema
    {
        private_blockchain,
        private_twitter,
        private_discord,
        private_google,
        private_github,
        private_api,
        private_settings,
        private_profile,

        public_blockchain,
        public_deposits,
        public_native,
        public_mail,
        public_twitter,
        public_profile,
        public_github,
        public_discord,
        public_google,
        public_browser,
        public_rpc,

        project
    }


    public class DbManager : Sql
    {
        protected bool _logShow = false;
        //protected bool _pstgr = false;
        //protected string _tableName = string.Empty;
        //protected string _schemaName = string.Empty;

        protected readonly int _rangeEnd;

        public DbManager(IZennoPosterProjectModel project, bool log = false)
            : base(project, log: log)
        {
            _logShow = log;
            //_pstgr = _dbMode == "PostgreSQL" ? true : false;
            _rangeEnd = int.TryParse(project.Variables["rangeEnd"].Value, out int rangeEnd) && rangeEnd > 0 ? rangeEnd : 10;

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
        public bool ClmnExist(string tblName, string clmnName)
        {
            TblName(tblName);
            string resp = null;
            if (_pstgr)
                resp = DbQ($@"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}' AND lower(column_name) = lower('{clmnName}');")?.Trim();
            else
                resp = DbQ($"SELECT COUNT(*) FROM pragma_table_info('{_tableName}') WHERE name='{clmnName}';");
            if (resp == "0" || resp == string.Empty) return false;
            else return true;

        }
        public List<string> TblColumns(string tblName)
        {
            TblName(tblName);
            if (_dbMode == "PostgreSQL")
                return DbQ($@"SELECT column_name FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}';", true)
                    .Split('\n')
                    .Select(s => s.Trim())
                    .ToList();
            else
                return DbQ($"SELECT name FROM pragma_table_info('{_tableName}');")
                    .Split('\n')
                    .Select(s => s.Trim())
                    .ToList();
        }

        public void ClmnAdd(string tblName, string clmnName, string defaultValue = "TEXT DEFAULT \"\"")
        {
            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var current = TblColumns(tblName);
            if (!current.Contains(clmnName))
            {
                DbQ($@"ALTER TABLE {_tableName} ADD COLUMN {clmnName} {defaultValue};", true);
            }

        }
        public void ClmnAdd(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            var current = TblColumns(tblName);
            Log(string.Join(",", current));
            foreach (var column in tableStructure)
            {
                var keyWd = column.Key.Trim();
                if (!current.Contains(keyWd))
                {
                    Log($"CLMNADD [{keyWd}] not in  [{string.Join(",", current)}] ");
                    DbQ($@"ALTER TABLE {_tableName} ADD COLUMN {keyWd} {column.Value};", true);
                }
            }
        }
        public void ClmnDrop(string tblName, string clmnName)
        {
            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            var current = TblColumns(tblName);
            if (current.Contains(clmnName))
            {
                string cascade = (_pstgr) ? " CASCADE" : null;
                DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {clmnName}{cascade};", true);
            }
        }
        public void ClmnDrop(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var current = TblColumns(tblName);

            foreach (var column in tableStructure)
            {
                if (!current.Contains(column.Key))
                {
                    string cascade = _dbMode == "PostgreSQL" ? " CASCADE" : null;
                    DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {column.Key}{cascade};", true);
                }
            }
        }
        public void TblAdd(string tblName, Dictionary<string, string> tableStructure)
        {
            Log("TBLADD");
            TblName(tblName);
            if (TblExist(tblName)) return;
            if (_pstgr) DbQ($@" CREATE TABLE {_schemaName}.{_tableName} ( {string.Join(", ", tableStructure.Select(kvp => $"\"{kvp.Key}\" {kvp.Value.Replace("AUTOINCREMENT", "SERIAL")}"))} );");
            else DbQ($"CREATE TABLE {_tableName} (" + string.Join(", ", tableStructure.Select(kvp => $"{kvp.Key} {kvp.Value}")) + ");");
        }
        public void AddRange(string tblName, int range = 108)
        {
            TblName(tblName);

            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int current = int.Parse(DbQ($@"SELECT COALESCE(MAX(acc0), 0) FROM {_tableName};"));
            Log(current.ToString());
            Log(_rangeEnd.ToString());
            for (int currentAcc0 = current + 1; currentAcc0 <= _rangeEnd; currentAcc0++)
            {
                DbQ($@"INSERT INTO {_tableName} (acc0) VALUES ({currentAcc0}) ON CONFLICT DO NOTHING;");
            }

        }


    }
}
