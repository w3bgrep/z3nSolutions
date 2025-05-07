using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZXing.QrCode.Internal;

namespace ZBSolutions
{
    public class TableSvc : Sql
    {

        private readonly IZennoPosterProjectModel _project;

        protected readonly string _tableName;
        protected readonly string _schemaName;
        private readonly bool _logShow;


        public TableSvc(IZennoPosterProjectModel project, string tablename = null, string schemaName = null, bool log = false)
            : base(project, log)
        {
            _project = project;
            _logShow = log;
            _tableName = tablename;
            _schemaName = schemaName;
            if (string.IsNullOrEmpty(_tableName)) _tableName = _project.ExecuteMacro(_project.Name).Split('.')[0].ToLower();
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
        private void FillInitial(string cfgRangeEnd, bool log = false)
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
        public void MkTable(Dictionary<string, string> tableStructure, bool prune = false)
        {
            if (_dbMode == "PostgreSQL")
            {
                CreateIfNotExist(tableStructure);
                ManageColumns(tableStructure, prune: prune);
                FillInitial(_project.Variables["rangeEnd"].Value);
            }
            else
            {
                string tableExists = DbQ($"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{_tableName}';");
                if (tableExists.Trim() == "0")
                {
                    string createTableQuery = $"CREATE TABLE {_tableName} (";
                    createTableQuery += string.Join(", ", tableStructure.Select(kvp => $"{kvp.Key} {kvp.Value}"));
                    createTableQuery += ");";
                    DbQ(createTableQuery);
                }
                else
                {

                    string[] currentColumns = DbQ($"SELECT name FROM pragma_table_info('{_tableName}');").Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (prune)
                    {
                        var columnsToRemove = currentColumns.Where(col => !tableStructure.ContainsKey(col)).ToList();
                        foreach (var column in columnsToRemove) DbQ($"ALTER TABLE {_tableName} DROP COLUMN {column};");
                    }

                    foreach (var column in tableStructure)
                    {
                        string resp = DbQ($"SELECT COUNT(*) FROM pragma_table_info('{_tableName}') WHERE name='{column.Key}';");
                        if (resp.Trim() == "0") DbQ($"ALTER TABLE {_tableName} ADD COLUMN {column.Key} {column.Value};");
                    }


                    if (tableStructure.ContainsKey("acc0") && !string.IsNullOrEmpty(_project.Variables["rangeEnd"].Value))
                    {
                        for (int currentAcc0 = 1; currentAcc0 <= int.Parse(_project.Variables["rangeEnd"].Value); currentAcc0++)
                        { DbQ($"INSERT OR IGNORE INTO {_tableName} (acc0) VALUES ('{currentAcc0}');"); }
                    }
                }
            }

        }
        public Dictionary<string, string> CreateTableStructure(string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''")
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
            string[] toDoItems = (dynamicToDo ?? "").Split(',');
            foreach (string taskId in toDoItems)
            {
                string trimmedTaskId = taskId.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedTaskId) && !tableStructure.ContainsKey(trimmedTaskId))
                {
                    tableStructure.Add(trimmedTaskId, defaultType);
                }
            }
            return tableStructure;
        }


        
    }
}