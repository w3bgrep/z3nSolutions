

#region using
using System;
using System.Collections.Generic;
using System.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary;
using ZBSolutions;
using NBitcoin;

#endregion

namespace w3tools //by @w3bgrep
{

    public static class TestStatic
{

    public static void acc0w(this IZennoPosterProjectModel project, object acc0)
    {
        project.Variables["acc0"].Value = acc0?.ToString() ?? string.Empty;
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
    
    public class DbManager2 : Sql
    {
        protected bool _logShow = false;
        //protected bool _pstgr = false;
        //protected string _tableName = string.Empty;
        //protected string _schemaName = string.Empty;

        protected readonly int _rangeEnd;

        public DbManager2(IZennoPosterProjectModel project, bool log = false)
            : base(project, log: log)
        {
            _logShow = log;
            _pstgr = _dbMode == "PostgreSQL" ? true : false;
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






    
    public class DBuilder : DbManager
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly F0rms _f0rm;
        private readonly F0rms2 _f0rm2;

        private string _acc0;

        public DBuilder(IZennoPosterProjectModel project, bool log = false)
            : base(project, log: log)
        {
            _project = project;
            _logShow = log;
            _pstgr = _dbMode == "PostgreSQL" ? true : false;
            _f0rm = new F0rms(_project);
            _f0rm2 = new F0rms2(_project);
            //_acc0 = _project.Variables[acc0];
            //_rangeEnd = int.TryParse(project.Variables["rangeEnd"].Value, out int rangeEnd) && rangeEnd > 0 ? rangeEnd : 10;

        }

        public (Dictionary<int, object> data, List<string> selectedFormat) CollectInputData(string tableName, string formTitle, string[] availableFields, string message = "Select format (one field per box):")
        {
            
            var formFont = new System.Drawing.Font("Iosevka", 10 ); //System.Drawing.FontStyle.Bold
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = formTitle;
            form.Width = 800;
            form.Height = 700;
            form.BackColor = System.Drawing.Color.DarkGray;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            List<string> selectedFormat = new List<string>();
            System.Windows.Forms.TextBox formatDisplay = new System.Windows.Forms.TextBox();
            System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();

            System.Windows.Forms.Label formatLabel = new System.Windows.Forms.Label();
            formatLabel.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold );
            formatLabel.Text = message;
            formatLabel.AutoSize = true;
            formatLabel.Left = 10;
            formatLabel.Top = 10;

            form.Controls.Add(formatLabel);

            System.Windows.Forms.ComboBox[] formatComboBoxes = new System.Windows.Forms.ComboBox[availableFields.Length - 1];
            int spacing = 5;
            int totalSpacing = spacing * (formatComboBoxes.Length - 1);
            int comboWidth = (form.ClientSize.Width - 20 - totalSpacing) / formatComboBoxes.Length;
            for (int i = 0; i < formatComboBoxes.Length; i++)
            {
                formatComboBoxes[i] = new System.Windows.Forms.ComboBox();
                formatComboBoxes[i].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                formatComboBoxes[i].Font = formFont;
                formatComboBoxes[i].Items.AddRange(availableFields);
                formatComboBoxes[i].SelectedIndex = 0;
                formatComboBoxes[i].Left = 10 + i * (comboWidth + spacing);
                formatComboBoxes[i].Top = 30;
                formatComboBoxes[i].Width = comboWidth;
                formatComboBoxes[i].SelectedIndexChanged += (s, e) =>
                {
                    selectedFormat.Clear();
                    foreach (var combo in formatComboBoxes)
                    {
                        if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
                            selectedFormat.Add(combo.SelectedItem.ToString());
                    }
                    formatDisplay.Text = string.Join(":", selectedFormat);
                };
                form.Controls.Add(formatComboBoxes[i]);
            }

            formatDisplay.Left = 10;
            formatDisplay.Top = 60;
            formatDisplay.Font = new System.Drawing.Font("Iosevka", 11, System.Drawing.FontStyle.Bold );
            formatDisplay.BackColor = System.Drawing.Color.Black;
            formatDisplay.ForeColor = System.Drawing.Color.White;  
            formatDisplay.Width = form.ClientSize.Width - 20;
            formatDisplay.ReadOnly = true;
     
            form.Controls.Add(formatDisplay);

            System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
            dataLabel.Text = "Input data (one per line, matching format):";
            dataLabel.Font = formFont;
            dataLabel.AutoSize = true;
            dataLabel.Left = 10;
            dataLabel.Top = 90;
            form.Controls.Add(dataLabel);

            dataInput.Left = 10;
            dataInput.Top = 110;
            dataInput.Width = form.ClientSize.Width - 20;
            dataInput.Multiline = true;
            dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dataInput.BackColor = System.Drawing.Color.Black;
            dataInput.ForeColor = System.Drawing.Color.White;
            form.Controls.Add(dataInput);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold );
            okButton.BackColor = System.Drawing.Color.Black;
            okButton.ForeColor = System.Drawing.Color.LightGreen; 
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            dataInput.Height = okButton.Top - dataInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };
            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();
            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog($"Import to {tableName} cancelled by user", true);
                return (null, null);
            }

            selectedFormat.Clear();
            foreach (var combo in formatComboBoxes)
            {
                if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
                    selectedFormat.Add(combo.SelectedItem.ToString());
            }

            if (string.IsNullOrEmpty(dataInput.Text) || selectedFormat.Count == 0)
            {
                _project.SendWarningToLog("Data or format cannot be empty");
                return (null, null);
            }

            string[] lines = dataInput.Text.Trim().Split('\n');
            _project.SendInfoToLog($"Parsing [{lines.Length}] {tableName} data lines", true);

            Dictionary<int, object> data = new Dictionary<int, object>();
            for (int acc0 = 1; acc0 <= lines.Length; acc0++)
            {
                string line = lines[acc0 - 1].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    _project.SendWarningToLog($"Line {acc0} is empty", false);
                    continue;
                }

                if (formTitle.Contains("proxy"))
                {
                    data[acc0] = line;
                }
                else
                {
                    string[] data_parts = line.Split(':');
                    Dictionary<string, string> parsed_data = new Dictionary<string, string>();
                    for (int i = 0; i < selectedFormat.Count && i < data_parts.Length; i++)
                    {
                        parsed_data[selectedFormat[i]] = data_parts[i].Trim();
                    }
                    data[acc0] = parsed_data;
                }
            }

            return (data, selectedFormat);
        }
   

        public string[]  DefaultColumns(schema tableSchem)
        {
 
            switch (tableSchem)
            {

                case schema.public_blockchain:
                case schema.public_native:
                case schema.public_deposits:
                    return new string[] { };
                case schema.private_google:
                    return new string[] {"cookies", "login", "pass", "otpsecret", "otpbackup", "recovery_mail", "recovery_phone" };
                case schema.private_twitter:
                    return new string[] { "cookies", "token", "login", "pass", "otpsecret", "otpbackup", "email", "email_pass" };;
                case schema.private_discord:
                    return new string[]  { "token", "login", "pass", "otpsecret", "otpbackup", "email", "email_pass", "recovery_phone" };
                 case schema.private_github:
                    return new string[]  { "cookies", "token", "login", "pass", "otpsecret", "otpbackup", "email", "email_pass" };
                case schema.private_blockchain:
                    return new string[] { "secp256k1", "base58", "bip39" };

                case schema.private_settings:
                    return new string[] { "value" };
                case schema.private_api:
                    return new string[] { "apikey", "apisecret", "passphrase", "proxy" };
                case schema.private_profile:
                    return new string[] { "proxy", "cookies", "webgl", "zbid" };


                case schema.public_profile:
                    return new string[] { "username", "bio", "brsr_score" };

                case schema.public_rpc:
                   return new string[] { "rpc", "explorer", "explorer_api" };



                case schema.public_google:
                    return new string[] { "status", "last" };
                case schema.public_twitter:
                    return new string[] { "status", "last", "id", "following", "followers", "creation", "givenname", "description", "lang", "birth", "country", "gender", "homelocation", };
                case schema.public_discord:
                    return new string[] { "status", "last", "username", "servers", "roles" };


                default:
                    throw new Exception("no schema");
            }

        }

        public Dictionary<string, string> LoadSchema(schema tableSchem)
        {
            var tableStructure = new Dictionary<string, string>();

            string primary = "acc0";
            string primaryType = "INTEGER PRIMARY KEY";
            var toFill = new List<string>();
            string defaultColumn = "TEXT DEFAULT ''";



            switch (tableSchem)
            {

                case schema.private_twitter:
                case schema.private_google:
                case schema.private_discord:

                case schema.public_google:
                case schema.public_twitter:
                case schema.public_discord:

                case schema.private_blockchain:
                case schema.public_blockchain:
                case schema.public_native:
                case schema.public_deposits:

                case schema.private_profile:
                case schema.public_profile:
                    foreach (string column in DefaultColumns(tableSchem)) toFill.Add(column);
                    break;

                case schema.private_settings:
                case schema.private_api:
                case schema.public_rpc:
                    primary = "key";
                    foreach (string column in DefaultColumns(tableSchem)) toFill.Add(column);
                    break;

                default:
                    throw new Exception("no schema");

            }


            if (primary != "acc0") primaryType = "TEXT PRIMARY KEY";
            tableStructure.Add(primary, primaryType);

            foreach (string name in toFill)
            {
                if (!tableStructure.ContainsKey(name)) tableStructure.Add(name, defaultColumn);
            }
            return tableStructure;
        }     

        private string ImportData(string tableName,  string[] availableFields, Dictionary<string, string> columnMapping, string formTitle = "title", string message = "Select format (one field per box):",int startFrom = 1)
        {
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int lineCount = 0;

            int rangeEnd = _rangeEnd;
            var acc0 = _project.Variables["acc0"];
            acc0.Value = startFrom.ToString();


            var formFont = new System.Drawing.Font("Iosevka", 10 ); //System.Drawing.FontStyle.Bold
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = formTitle;
            form.Width = 800;
            form.Height = 700;
            form.BackColor = System.Drawing.Color.DarkGray;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            List<string> selectedFormat = new List<string>();
            System.Windows.Forms.TextBox formatDisplay = new System.Windows.Forms.TextBox();
            System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();

            System.Windows.Forms.Label formatLabel = new System.Windows.Forms.Label();
            formatLabel.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold );
            formatLabel.Text = message;
            formatLabel.AutoSize = true;
            formatLabel.Left = 10;
            formatLabel.Top = 10;

            form.Controls.Add(formatLabel);

            System.Windows.Forms.ComboBox[] formatComboBoxes = new System.Windows.Forms.ComboBox[availableFields.Length - 1];
            int spacing = 5;
            int totalSpacing = spacing * (formatComboBoxes.Length - 1);
            int comboWidth = (form.ClientSize.Width - 20 - totalSpacing) / formatComboBoxes.Length;
            for (int i = 0; i < formatComboBoxes.Length; i++)
            {
                formatComboBoxes[i] = new System.Windows.Forms.ComboBox();
                formatComboBoxes[i].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                formatComboBoxes[i].Font = formFont;
                formatComboBoxes[i].Items.AddRange(availableFields);
                formatComboBoxes[i].SelectedIndex = 0;
                formatComboBoxes[i].Left = 10 + i * (comboWidth + spacing);
                formatComboBoxes[i].Top = 30;
                formatComboBoxes[i].Width = comboWidth;
                formatComboBoxes[i].SelectedIndexChanged += (s, e) =>
                {
                    selectedFormat.Clear();
                    foreach (var combo in formatComboBoxes)
                    {
                        if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
                            selectedFormat.Add(combo.SelectedItem.ToString());
                    }
                    formatDisplay.Text = string.Join(":", selectedFormat);
                };
                form.Controls.Add(formatComboBoxes[i]);
            }

            formatDisplay.Left = 10;
            formatDisplay.Top = 60;
            formatDisplay.Font = new System.Drawing.Font("Iosevka", 11, System.Drawing.FontStyle.Bold );
            formatDisplay.BackColor = System.Drawing.Color.Black;
            formatDisplay.ForeColor = System.Drawing.Color.White;  
            formatDisplay.Width = form.ClientSize.Width - 20;
            formatDisplay.ReadOnly = true;
     
            form.Controls.Add(formatDisplay);

            System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
            dataLabel.Text = "Input data (one per line, matching format):";
            dataLabel.Font = formFont;
            dataLabel.AutoSize = true;
            dataLabel.Left = 10;
            dataLabel.Top = 90;
            form.Controls.Add(dataLabel);

            dataInput.Left = 10;
            dataInput.Top = 110;
            dataInput.Width = form.ClientSize.Width - 20;
            dataInput.Multiline = true;
            dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dataInput.BackColor = System.Drawing.Color.Black;
            dataInput.ForeColor = System.Drawing.Color.White;
            form.Controls.Add(dataInput);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Font = new System.Drawing.Font("Iosevka", 10, System.Drawing.FontStyle.Bold );
            okButton.BackColor = System.Drawing.Color.Black;
            okButton.ForeColor = System.Drawing.Color.LightGreen; 
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            dataInput.Height = okButton.Top - dataInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };
            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog($"Import to {tableName} cancelled by user", true);
                return "0";
            }

            selectedFormat.Clear();
            foreach (var combo in formatComboBoxes)
            {
                if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
                    selectedFormat.Add(combo.SelectedItem.ToString());
            }

            if (string.IsNullOrEmpty(dataInput.Text) || selectedFormat.Count == 0)
            {
                _project.SendWarningToLog("Data or format cannot be empty");
                return "0";
            }

            string[] lines = dataInput.Text.Trim().Split('\n');
            _project.SendInfoToLog($"Parsing [{lines.Length}] {tableName} data lines", true);

            for (int acc0unt = 1; acc0unt <= lines.Length; acc0unt++)
            {
                string line = lines[acc0unt - 1].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    _project.SendWarningToLog($"Line {acc0unt} is empty", false);
                    continue;
                }
                if (formTitle.Contains("proxy"))
                {
                    try
                    {
                        string dbQuery = $@"UPDATE {_tableName} SET proxy = '{line.Replace("'", "''")}' WHERE acc0 = {acc0unt};";
                        DbQ(dbQuery, true);
                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        _project.SendWarningToLog($"Error processing line {acc0unt}: {ex.Message}", false);
                    }
                }
                else
                {
                    string[] data_parts = line.Split(':');
                    Dictionary<string, string> parsed_data = new Dictionary<string, string>();

                    for (int i = 0; i < selectedFormat.Count && i < data_parts.Length; i++)
                    {
                        parsed_data[selectedFormat[i]] = data_parts[i].Trim();
                    }

                    var queryParts = new List<string>();
                    foreach (var field in columnMapping.Keys)
                    {
                        string value = parsed_data.ContainsKey(field) ? parsed_data[field].Replace("'", "''") : "";
                        if (field == "CODE2FA" && value.Contains('/'))
                            value = value.Split('/').Last();
                        queryParts.Add($"{columnMapping[field]} = '{value}'");
                    }

                    try
                    {
                        string dbQuery = $@"UPDATE {_tableName} SET {string.Join(", ", queryParts)} WHERE acc0 = {acc0};";
                        DbQ(dbQuery, true);
                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        _project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}", false);
                    }
                }
            }

            _project.SendInfoToLog($"[{lineCount}] records added to [{_tableName}]", true);
            return lineCount.ToString();
        }

        public void MapAndImport (schema tableSchem,int startFrom = 1)
        {        
            _project.Variables["acc0"].Value = startFrom.ToString();
            
            var mapping = new Dictionary<string, string>();
            
            Log($"mapping {tableSchem}");
            switch (tableSchem)
            {

                case schema.private_twitter:
                    string[] twitterFields = new string[] { "", "LOGIN", "PASSWORD", "EMAIL", "EMAIL_PASSWORD", "TOKEN", "2FA_SECRET", "2FA_BACKUP" };
                    var twitterMapping = new Dictionary<string, string>
                    {
                        { "LOGIN", "login" },
                        { "PASSWORD", "pass" },
                        { "EMAIL", "email" },
                        { "EMAIL_PASSWORD", "email_pass" },
                        { "TOKEN", "token" },
                        { "2FA_SECRET", "otpsecret" },
                        { "2FA_BACKUP", "otpbackup" }
                    };
                    ImportData(tableSchem.ToString(), twitterFields, twitterMapping, "Import Twitter Data");
                    return;


                case schema.private_discord:
                    string[] discordFields = new string[] { "", "LOGIN", "PASSWORD", "TOKEN", "2FA_SECRET" };
                    var discordMapping = new Dictionary<string, string>
                    {
                        { "LOGIN", "login" },
                        { "PASSWORD", "pass" },
                        { "TOKEN", "token" },
                        { "2FA_SECRET", "otpsecret" }
                    };
                    ImportData(tableSchem.ToString(), discordFields, discordMapping, "Import Discord Data");
                    return;

                case schema.private_google:
                    string[] googleFields = new string[] { "", "LOGIN", "PASSWORD", "RECOVERY_EMAIL", "2FA_SECRET", "2FA_BACKUP" };
                    var googleMapping = new Dictionary<string, string>
                    {
                        { "LOGIN", "login" },
                        { "PASSWORD", "pass" },
                        { "RECOVERY_EMAIL", "recovery_email" },
                        { "2FA_SECRET", "otpsecret" },
                        { "2FA_BACKUP", "otpbackup" }
                    };
                    ImportData(tableSchem.ToString(), googleFields, googleMapping, "Import Google Data");
                    return;                

                case schema.public_mail:
                    string[] fieldsPbMl = new string[] { "ICLOUD", "" };
                    mapping = new Dictionary<string, string>
                    {
                        { "ICLOUD", "icloud" },
                    };
                    ImportData("google", fieldsPbMl, mapping, "Import Icloud");  
                    return;
 
                case schema.public_profile:
                    string[] fieldsPbPf = new string[] { "NICKNAME","BIO", "", };
                    mapping = new Dictionary<string, string>
                    {
                        { "NICKNAME", "nickname" },
                        { "BIO", "bio" },
                    };
                    ImportData("profile", fieldsPbPf, mapping, "Import Bio");
                    return;
                
                case schema.private_profile:
                   var profile = _f0rm2.Get1x1("proxy", "input proxy, don't change key!");
                   Upd (profile,tableSchem.ToString());

                    return;

                case schema.private_blockchain:
                    ImportKeys("seed");
                    ImportKeys("evm");
                    ImportKeys("sol");
                    return;

                case schema.public_deposits:
                    ImportDepositAddresses();
                    return;

                case schema.private_settings:
                    var phK = new List<string> { 
                        "discord_invite",
                        "discord_hub",
                        "tg_logger_token",	
                        "tg_logger_group",
                        "tg_logger_topic",
                        "profiles_folder"
                        };


                    var phV = new List<string> { 
                        "Инвайт на свой сервер",
                        "ID канала с инвайтами на вашем сервере",
                        "Токен Telegram логгера",
                        "Id группы для логов в Telegram. Формат {-1002000000009}",
                        "Id топика в группе для логов. 0 - если нет топиков",
                        "Путь к папке с профилями и причастным данным. Формат: {F:\\farm\\}"
                        };

                    
                    var settings = _f0rm.GetKeyValuePairs(phK.Count(), phK,phV,"input settings",prepareUpd:false);
                    Write(settings,tableSchem.ToString());
                    return;

                case schema.private_api:
                    phK = new List<string> { "api", "apisecret", "proxy", };
                    var toWrite = _f0rm2.GetKeyValueString(phK.Count(), phK,null,"input binance api");                 
                    UpdTxt (toWrite,tableSchem.ToString(),"binance");
                    
                    phK = new List<string> { "api", "apisecret", "passphrase", };
                    toWrite = _f0rm2.GetKeyValueString(phK.Count(), phK,null,"input okx api");                 
                    UpdTxt (toWrite,tableSchem.ToString(),"okx");
 
                    phK = new List<string> { "api", "apisecret", "passphrase", };
                    toWrite = _f0rm2.GetKeyValueString(phK.Count(), phK,null,"input firstmail login as apisecret)");                 
                    UpdTxt (toWrite,tableSchem.ToString(),"firstmail"); 
 
                    phK = new List<string> { "api",  };
                    toWrite = _f0rm2.GetKeyValueString(phK.Count(), phK,null,"input perplexity api");                 
                    UpdTxt (toWrite,tableSchem.ToString(),"perplexity"); 
 
                    return;



                default:
                    throw new Exception($"no schema [{tableSchem}]");

            }

        }

        public string ImportKeys(string keyType, int startFrom = 1)
        {
            
            TblName("private_blockchain");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int rangeEnd = _rangeEnd +1 ;
            var acc0 = _project.Variables["acc0"];
            acc0.Value = startFrom.ToString();

            var blockchain = new Blockchain();

            var formFont = new System.Drawing.Font("Iosevka", 10 ); //System.Drawing.FontStyle.Bold

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = $"Import {keyType} keys";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true; // Форма поверх всех окон
            form.Location = new System.Drawing.Point(108, 108);

            System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
            dataLabel.Text = $"Input {keyType} keys (one per line):";
            dataLabel.AutoSize = true;
            dataLabel.Left = 10;
            dataLabel.Top = 10;
            form.Controls.Add(dataLabel);

            System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();
            dataInput.Left = 10;
            dataInput.Top = 30;
            dataInput.Width = form.ClientSize.Width - 20;
            dataInput.Multiline = true;
            dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dataInput.MaxLength = 1000000;
            form.Controls.Add(dataInput);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            dataInput.Height = okButton.Top - dataInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            if (string.IsNullOrEmpty(dataInput.Text))
            {
                _project.SendWarningToLog("Input could not be empty");
                return "0"; 
            }

            string[] lines = dataInput.Text.Trim().Split('\n');
            _project.SendInfoToLog($"Parsing [{lines.Length}] strings", false);

            for (int i = 0; i < lines.Length && int.Parse(acc0.Value) <= rangeEnd; i++)
            {
                string key = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;

                try
                {
                    switch (keyType)
                    {
                        case "seed":
                            string encodedSeed = SAFU.Encode(_project, key);
                            Upd($"bip39 = '{encodedSeed}'",_tableName);
                            //_sql.DbQ($@"UPDATE {_tableName} SET bip39 = '{encodedSeed}' WHERE acc0 = {acc0.Value};", true);
                            break;

                        case "evm":
                            string privateKey;
                            string address;

                            if (key.Split(' ').Length > 1) 
                            {
                                var mnemonicObj = new Mnemonic(key);
                                var hdRoot = mnemonicObj.DeriveExtKey();
                                var derivationPath = new NBitcoin.KeyPath("m/44'/60'/0'/0/0");
                                privateKey = hdRoot.Derive(derivationPath).PrivateKey.ToHex();
                            }
                            else
                            {
                                privateKey = key;
                            }

                            string encodedEvmKey = SAFU.Encode(_project, privateKey);
                            address = blockchain.GetAddressFromPrivateKey(privateKey);
                            DbQ($@"UPDATE {_tableName} SET secp256k1 = '{encodedEvmKey}' WHERE acc0 = {acc0.Value};", true);
                            break;

                        case "sol":
                            string encodedSolKey = SAFU.Encode(_project, key);
                            DbQ($@"UPDATE {_tableName} SET base58 = '{encodedSolKey}' WHERE acc0 = {acc0.Value};", true);
                            break;

                        default:
                            _project.SendWarningToLog($"Unknown key type: {keyType}");
                            return lines.Length.ToString();
                    }

                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing record {acc0.Value}: {ex.Message}", false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
            }

            return lines.Length.ToString();
        }
        
        public string ImportAddresses(int startFrom = 1)
        {
            string tablename = "public_blockchain";
            TblName(tablename);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int rangeEnd = _rangeEnd;
            var acc0 = _project.Variables["acc0"];
            acc0.Value = startFrom.ToString();


            acc0.Value = "1";

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Import Addresses";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            // Поле для ввода имени столбца
            System.Windows.Forms.Label columnLabel = new System.Windows.Forms.Label();
            columnLabel.Text = "Column name (e.g., evm, sol):";
            columnLabel.AutoSize = true;
            columnLabel.Left = 10;
            columnLabel.Top = 10;
            form.Controls.Add(columnLabel);

            System.Windows.Forms.TextBox columnInput = new System.Windows.Forms.TextBox();
            columnInput.Left = 10;
            columnInput.Top = 30;
            columnInput.Width = form.ClientSize.Width - 20;
            columnInput.Text = "input address label here ex: evm | apt |sol ";//_project.Variables["addressType"].Value; // Предполагаем, что переменная существует
            form.Controls.Add(columnInput);

            // Поле для ввода адресов
            System.Windows.Forms.Label addressLabel = new System.Windows.Forms.Label();
            addressLabel.Text = "Addresses (one per line):";
            addressLabel.AutoSize = true;
            addressLabel.Left = 10;
            addressLabel.Top = 60;
            form.Controls.Add(addressLabel);

            System.Windows.Forms.TextBox addressInput = new System.Windows.Forms.TextBox();
            addressInput.Left = 10;
            addressInput.Top = 80;
            addressInput.Width = form.ClientSize.Width - 20;
            addressInput.Multiline = true;
            addressInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            addressInput.MaxLength = 1000000;
            form.Controls.Add(addressInput);

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 20;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            addressInput.Height = okButton.Top - addressInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            if (string.IsNullOrEmpty(columnInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Column name or addresses cannot be empty");
                return "0";
            }

            string columnName = columnInput.Text.ToLower();

            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {columnName, "TEXT DEFAULT ''"}
            };
            TblAdd(tablename, tableStructure);

            string[] lines = addressInput.Text.Trim().Split('\n');
            int lineCount = 0;

            for (int i = 0; i < lines.Length && int.Parse(acc0.Value) <= rangeEnd; i++)
            {
                string address = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(address))
                {
                    _project.SendWarningToLog($"Line {acc0.Value} is empty");
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                    continue;
                }

                try
                {
                    _project.SendInfoToLog($"Processing acc0 = {acc0.Value}, address = '{address}'", false);
                    Upd($"{columnName} = '{address}'",_tableName, last:false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                    lineCount++;
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing record {acc0.Value} for {columnName}: {ex.Message}", false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
            }

            _project.SendInfoToLog($"[{lineCount}] strings added to [{_tableName}]", true);
            return lineCount.ToString();
        }
        public string ImportDepositAddresses(int startFrom = 1)
        {
            string tablename = "public_deposits";
            TblName(tablename);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int rangeEnd = _rangeEnd;
            var acc0 = _project.Variables["acc0"];
            acc0.Value = startFrom.ToString();


            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Import Deposit Addresses";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true; // Форма поверх всех окон
            //form.Location = new System.Drawing.Point(108, 108);

            // Поле для ввода CHAIN
            System.Windows.Forms.Label chainLabel = new System.Windows.Forms.Label();
            chainLabel.Text = "Chain (e.g., ETH, BSC):";
            chainLabel.AutoSize = true;
            chainLabel.Left = 10;
            chainLabel.Top = 10;
            form.Controls.Add(chainLabel);

            System.Windows.Forms.TextBox chainInput = new System.Windows.Forms.TextBox();
            chainInput.Left = 10;
            chainInput.Top = 30;
            chainInput.Width = form.ClientSize.Width - 20;
            chainInput.Text = "ETH";//_project.Variables["depositChain"].Value; // Текущее значение из переменной
            form.Controls.Add(chainInput);

            // Поле для ввода CEX
            System.Windows.Forms.Label cexLabel = new System.Windows.Forms.Label();
            cexLabel.Text = "CEX (e.g., binance, kucoin):";
            cexLabel.AutoSize = true;
            cexLabel.Left = 10;
            cexLabel.Top = 60;
            form.Controls.Add(cexLabel);

            System.Windows.Forms.TextBox cexInput = new System.Windows.Forms.TextBox();
            cexInput.Left = 10;
            cexInput.Top = 80;
            cexInput.Width = form.ClientSize.Width - 20;
            cexInput.Text = "OKX";//_project.Variables["depositCEX"].Value; // Текущее значение из переменной
            form.Controls.Add(cexInput);

            // Поле для ввода адресов
            System.Windows.Forms.Label addressLabel = new System.Windows.Forms.Label();
            addressLabel.Text = "Deposit addresses (one per line):";
            addressLabel.AutoSize = true;
            addressLabel.Left = 10;
            addressLabel.Top = 110;
            form.Controls.Add(addressLabel);

            System.Windows.Forms.TextBox addressInput = new System.Windows.Forms.TextBox();
            addressInput.Left = 10;
            addressInput.Top = 130;
            addressInput.Width = form.ClientSize.Width - 20;
            addressInput.Multiline = true;
            addressInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            form.Controls.Add(addressInput);

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            addressInput.Height = okButton.Top - addressInput.Top - 5;
            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            if (string.IsNullOrEmpty(chainInput.Text) || string.IsNullOrEmpty(cexInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Chain, CEX, or addresses cannot be empty");
                return "0";
            }

            string CHAIN = chainInput.Text.ToLower();
            string CEX = cexInput.Text.ToLower();
            string columnName = $"{CEX}_{CHAIN}";

            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {columnName, "TEXT DEFAULT ''"}
            };
            ClmnAdd(_tableName, tableStructure);

            string[] lines = addressInput.Text.Trim().Split('\n');
            int lineCount = 0;

            for (int acc0index = startFrom; acc0index <= lines.Length; acc0index++) 
            {
                string line = lines[acc0index - 1].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    _project.SendWarningToLog($"Line {acc0index} is empty");
                    continue;
                }

                try
                {
                    acc0.Value = acc0index.ToString();
                    Upd($"{columnName} = '{line}'",_tableName, last:false);
                    // _sql.DbQ($@"UPDATE {table} SET
                    //         {columnName} = '{line}'
                    //         WHERE acc0 = {acc0};");
                    lineCount++;
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}");
                    continue;
                }
            }

            _project.SendInfoToLog($"[{lineCount}] strings added to [{_tableName}]", true);
            return lineCount.ToString();
        }

        

    }

    public class F0rms2
    {
        private readonly IZennoPosterProjectModel _project;

        public F0rms2(IZennoPosterProjectModel project)
        {
            _project = project;
        }

        public Dictionary<string, bool> GetKeyBoolPairs(
            int quantity,
            List<string> keyPlaceholders = null,
            List<string> valuePlaceholders = null,
            string title = "Input Key-Bool Pairs",
            bool prepareUpd = true)
        {
            var result = new System.Collections.Generic.Dictionary<string, bool>();

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = title;
            form.Width = 600;
            form.Height = 40 + quantity * 35; // Адаптивная высота в зависимости от количества полей
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            // Список для хранения текстовых полей и чекбоксов
            var keyLabels = new System.Windows.Forms.Label[quantity];
            var keyTextBoxes = new System.Windows.Forms.TextBox[quantity];
            var valueCheckBoxes = new System.Windows.Forms.CheckBox[quantity];

            int currentTop = 5;
            int labelWidth = 40;
            int keyBoxWidth = 40;
            int checkBoxWidth = 370; // Ширина для чекбокса (для выравнивания)
            int spacing = 5;

            // Создаём поля для ключей и чекбоксов
            for (int i = 0; i < quantity; i++)
            {
                // Метка для ключа
                System.Windows.Forms.Label keyLabel = new System.Windows.Forms.Label();

                string keyDefault = keyPlaceholders != null && i < keyPlaceholders.Count && !string.IsNullOrEmpty(keyPlaceholders[i]) ? keyPlaceholders[i] : $"key{i + 1}";
                keyLabel.Text = keyDefault; //
                //keyTextBoxes[i] = keyDefault;
                //keyLabel.Text = $"Key:";
                keyLabel.AutoSize = true;
                keyLabel.Left = 5;
                keyLabel.Top = currentTop + 5; // Смещение для центрирования
                form.Controls.Add(keyLabel);
                keyLabels[i] = keyLabel;

                // Поле для ключа
                System.Windows.Forms.TextBox keyTextBox = new System.Windows.Forms.TextBox();
                keyTextBox.Left = keyLabel.Left + labelWidth + spacing;
                keyTextBox.Top = currentTop;
                keyTextBox.Width = keyBoxWidth;


                // Чекбокс для значения
                System.Windows.Forms.CheckBox valueCheckBox = new System.Windows.Forms.CheckBox();
                valueCheckBox.Left = keyTextBox.Left + keyBoxWidth + spacing + 10;
                valueCheckBox.Top = currentTop;
                valueCheckBox.Width = checkBoxWidth;
                string valueDefault = valuePlaceholders != null && i < valuePlaceholders.Count && !string.IsNullOrEmpty(valuePlaceholders[i]) ? valuePlaceholders[i] : $"Option{i + 1}";    
                valueCheckBox.Text = valueDefault; // Текст чекбокса для удобства
                valueCheckBox.Checked = false; // По умолчанию выключен


                // Метка для чекбокса
                System.Windows.Forms.Label valueLabel = new System.Windows.Forms.Label();
                valueLabel.Text = $"";
                valueLabel.AutoSize = true;
                valueLabel.Left = valueLabel.Left + labelWidth + spacing;
                
                valueLabel.Top = currentTop + 5; // Смещение для центрирования
                form.Controls.Add(valueLabel);



                form.Controls.Add(valueCheckBox);
                valueCheckBoxes[i] = valueCheckBox;

                currentTop += valueCheckBox.Height + spacing;
            }

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 20;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = currentTop + 10;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);

            // Адаптируем высоту формы
            int requiredHeight = okButton.Top + okButton.Height + 40;
            if (form.Height < requiredHeight)
            {
                form.Height = requiredHeight;
            }

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };
            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            // Показываем форму
            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Input cancelled by user", true);
                return null;
            }

            // Формируем словарь
            int lineCount = 0;
            for (int i = 0; i < quantity; i++)
            {
                string key = keyLabels[i].Text.ToLower().Trim();
                bool value = valueCheckBoxes[i].Checked;

                if (string.IsNullOrEmpty(key))
                {
                    //_project.SendWarningToLog($"Pair {i + 1} skipped: empty key");
                    continue;
                }

                try
                {
                    string dictKey = prepareUpd ? (i + 1).ToString() : key;
                    result.Add(dictKey, value);
                    //_project.SendInfoToLog($"Added to dictionary: [{dictKey}] = [{value}]", false);
                    lineCount++;
                }
                catch (System.Exception ex)
                {
                    _project.SendWarningToLog($"Error adding pair {i + 1}: {ex.Message}");
                }
            }

            if (lineCount == 0)
            {
                _project.SendWarningToLog("No valid key-value pairs entered");
                return null;
            }

            return result;
        }


        public Dictionary<string, string> Get1x1(string keycolumn = "input Column Name",string title = "Input data line per line")
        {
            var result = new Dictionary<string, string>();
            // Создание формы
            
            keycolumn = keycolumn.Trim().ToLower(); 
            
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = title;
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            System.Windows.Forms.Label columnLabel = new System.Windows.Forms.Label();
            columnLabel.Text = "input string for key here (it will be lowered)";
            columnLabel.AutoSize = true;
            columnLabel.Left = 10;
            columnLabel.Top = 10;
            form.Controls.Add(columnLabel);

            System.Windows.Forms.TextBox columnInput = new System.Windows.Forms.TextBox();
            columnInput.Left = 10;
            columnInput.Top = 30;
            columnInput.Width = 50;//form.ClientSize.Width - 20;
            columnInput.Text = keycolumn;//_project.Variables["addressType"].Value; // Предполагаем, что переменная существует
            form.Controls.Add(columnInput);

            System.Windows.Forms.Label addressLabel = new System.Windows.Forms.Label();
            addressLabel.Text = "Input strings (will be devided by \\n):";
            addressLabel.AutoSize = true;
            addressLabel.Left = 10;
            addressLabel.Top = 60;
            form.Controls.Add(addressLabel);

            System.Windows.Forms.TextBox addressInput = new System.Windows.Forms.TextBox();
            addressInput.Left = 10;
            addressInput.Top = 80;
            addressInput.Width = form.ClientSize.Width - 20;
            addressInput.Multiline = true;
            addressInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            addressInput.MaxLength = 1000000;
            form.Controls.Add(addressInput);

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 20;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            addressInput.Height = okButton.Top - addressInput.Top - 5;

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return null;
            }

            if (string.IsNullOrEmpty(columnInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Column name or addresses cannot be empty");
                return null;
            }

            string columnName = columnInput.Text.ToLower().Trim();

            string[] lines = addressInput.Text.Trim().Split('\n');
            int lineCount = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    _project.SendWarningToLog($"Line {i} is empty");
                    continue;
                }

                try
                {

                    string key = (i + 1).ToString();
                    //string value = $"{keycolumn} = '{line}'";
                    string value = $"{keycolumn} = '{line.Replace("'", "''")}'";

                    _project.SendInfoToLog($"k [{i}], val = [{value}]", false);
                    result.Add(key, value);
                    lineCount++;
                }
                catch (Exception ex)
                {

                }
            }

            return result;
        }

        public string GetKeyValueString(
            int quantity,
            List<string> keyPlaceholders = null,
            List<string> valuePlaceholders = null,
            string title = "Input Key-Value Pairs")
        {
            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = title;
            form.Width = 600;
            form.Height = 40 + quantity * 35; // Адаптивная высота
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            // Список для хранения текстовых полей
            var keyTextBoxes = new System.Windows.Forms.TextBox[quantity];
            var valueTextBoxes = new System.Windows.Forms.TextBox[quantity];

            int currentTop = 5;
            int labelWidth = 40;
            int keyBoxWidth = 100;
            int valueBoxWidth = 370;
            int spacing = 5;

            // Создаём поля для ключей и значений
            for (int i = 0; i < quantity; i++)
            {
                // Метка для ключа
                System.Windows.Forms.Label keyLabel = new System.Windows.Forms.Label();
                keyLabel.Text = $"Key:";
                keyLabel.AutoSize = true;
                keyLabel.Left = 5;
                keyLabel.Top = currentTop + 5;
                form.Controls.Add(keyLabel);

                // Поле для ключа
                System.Windows.Forms.TextBox keyTextBox = new System.Windows.Forms.TextBox();
                keyTextBox.Left = keyLabel.Left + labelWidth + spacing;
                keyTextBox.Top = currentTop;
                keyTextBox.Width = keyBoxWidth;

                string keyDefault = keyPlaceholders != null && i < keyPlaceholders.Count && !string.IsNullOrEmpty(keyPlaceholders[i]) 
                    ? keyPlaceholders[i] 
                    : $"key{i + 1}";
                keyTextBox.Text = keyDefault;
                form.Controls.Add(keyTextBox);
                keyTextBoxes[i] = keyTextBox;

                // Метка для значения
                System.Windows.Forms.Label valueLabel = new System.Windows.Forms.Label();
                valueLabel.Text = $"Value:";
                valueLabel.AutoSize = true;
                valueLabel.Left = keyTextBox.Left + keyBoxWidth + spacing + 10;
                valueLabel.Top = currentTop + 5;
                form.Controls.Add(valueLabel);

                // Поле для значения
                System.Windows.Forms.TextBox valueTextBox = new System.Windows.Forms.TextBox();
                valueTextBox.Left = valueLabel.Left + labelWidth + spacing;
                valueTextBox.Top = currentTop;
                valueTextBox.Width = valueBoxWidth;

                // Установка плейсхолдера
                string placeholder = valuePlaceholders != null && i < valuePlaceholders.Count ? valuePlaceholders[i] : "";
                if (!string.IsNullOrEmpty(placeholder))
                {
                    valueTextBox.Text = placeholder;
                    valueTextBox.ForeColor = System.Drawing.Color.Gray;
                    valueTextBox.Enter += (s, e) => { if (valueTextBox.Text == placeholder) { valueTextBox.Text = ""; valueTextBox.ForeColor = System.Drawing.Color.Black; } };
                    valueTextBox.Leave += (s, e) => { if (string.IsNullOrEmpty(valueTextBox.Text)) { valueTextBox.Text = placeholder; valueTextBox.ForeColor = System.Drawing.Color.Gray; } };
                }

                form.Controls.Add(valueTextBox);
                valueTextBoxes[i] = valueTextBox;

                currentTop += valueTextBox.Height + spacing;
            }

            // Кнопка "OK"
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 20;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = currentTop + 10;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);

            // Адаптируем высоту формы
            int requiredHeight = okButton.Top + okButton.Height + 40;
            if (form.Height < requiredHeight)
            {
                form.Height = requiredHeight;
            }

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };
            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            // Показываем форму
            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Input cancelled by user", true);
                return null;
            }

            // Формируем строку
            var pairs = new List<string>();
            int lineCount = 0;

            for (int i = 0; i < quantity; i++)
            {
                string key = keyTextBoxes[i].Text.ToLower().Trim();
                string value = valueTextBoxes[i].Text.Trim();
                string placeholder = valuePlaceholders != null && i < valuePlaceholders.Count ? valuePlaceholders[i] : "";

                if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(value) || value == placeholder)
                {
                    _project.SendWarningToLog($"Pair {i + 1} skipped: empty key or value");
                    continue;
                }

                try
                {
                    string pair = $"{key}='{value.Replace("'", "''")}'";
                    pairs.Add(pair);
                    _project.SendInfoToLog($"Added pair: {pair}", false);
                    lineCount++;
                }
                catch (System.Exception ex)
                {
                    _project.SendWarningToLog($"Error adding pair {i + 1}: {ex.Message}");
                }
            }

            if (lineCount == 0)
            {
                _project.SendWarningToLog("No valid key-value pairs entered");
                return null;
            }

            // Объединяем пары в строку
            return string.Join(", ", pairs);
        }
        }

}
