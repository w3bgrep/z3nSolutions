using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{


    public class DataImporter
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly string _schema;
        private readonly Sql _sql;

        public DataImporter(IZennoPosterProjectModel project, Instance instance, string schema = "accounts")
        {
            _project = project;
            _instance = instance;
            _schema = schema;
            _sql = new Sql(_project);
        }

        public bool ImportAll()
        {
            //OnStart.InitVariables(_project); 

            if (_project.Variables["cfgConfirmRebuildDB"].Value != "True")
            {
                _project.SendInfoToLog("Database rebuild not confirmed, skipping import", true);
                return false;
            }
            var import = _project.Variables["toImport"].Value;
            if (!ImportStart()) return false;

            CreateSchema(true);
            if (import.Contains("settings")) ImportSettings();
            if (import.Contains("proxy")) ImportProxy();

            if (import.Contains("evm")) ImportKeys("evm");
            if (import.Contains("sol")) ImportKeys("sol");
            if (import.Contains("seed")) ImportKeys("seed");
            if (import.Contains("deps")) ImportDepositAddresses();

            if (import.Contains("google")) ImportGoogle();
            if (import.Contains("icloud")) ImportIcloud();
            if (import.Contains("twitter")) ImportTwitter();
            if (import.Contains("discord")) ImportDiscord();
            
            if (import.Contains("nickname")) ImportNickname();
            if (import.Contains("bio")) ImportBio();

            if (import.Contains("webgl")) ImportWebGL(_instance);
            ImportDone();
            return true;
        }

        public void CreateSchema(bool log = false)
        {
            string defaultColumn = "TEXT DEFAULT ''";
            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";

            if (!string.IsNullOrEmpty(schemaName))
            {
                _sql.DbQ("CREATE SCHEMA IF NOT EXISTS accounts;",log);
            }

            // Profile
            string tableName = schemaName + "profile";
            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {"nickname", defaultColumn},
                {"bio", defaultColumn},
                {"cookies", defaultColumn},
                {"webgl", defaultColumn},
                {"timezone", defaultColumn},
                {"proxy", defaultColumn}
            };
            _sql.MkTable(tableStructure, tableName, false, schemaName: "");
            if (log) _project.SendInfoToLog($"Table {tableName} created", true);

            // Twitter
            tableName = schemaName + "twitter";
            tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {"cooldown", "INTEGER DEFAULT 0"},
                {"status", defaultColumn},
                {"last", defaultColumn},
                {"token", defaultColumn},
                {"login", defaultColumn},
                {"password", defaultColumn},
                {"code2fa", defaultColumn},
                {"email", defaultColumn},
                {"email_pass", defaultColumn},
                {"recovery2fa", defaultColumn}
            };
            _sql.MkTable(tableStructure, tableName, false, schemaName: "");
            if (log) _project.SendInfoToLog($"Table {tableName} created", true);

            // Discord
            tableName = schemaName + "discord";
            tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {"cooldown", "INTEGER DEFAULT 0"},
                {"status", defaultColumn},
                {"last", defaultColumn},
                {"servers", defaultColumn},
                {"roles", defaultColumn},
                {"token", defaultColumn},
                {"login", defaultColumn},
                {"password", defaultColumn},
                {"code2fa", defaultColumn}
            };
            _sql.MkTable(tableStructure, tableName, false, schemaName: "");
            if (log) _project.SendInfoToLog($"Table {tableName} created", true);

            // Google
            tableName = schemaName + "google";
            tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {"cooldown", "INTEGER DEFAULT 0"},
                {"status", defaultColumn},
                {"last", defaultColumn},
                {"login", defaultColumn},
                {"password", defaultColumn},
                {"recovery_email", defaultColumn},
                {"code2fa", defaultColumn},
                {"recovery2fa", defaultColumn},
                {"icloud", defaultColumn}
            };
            _sql.MkTable(tableStructure, tableName, false, schemaName: "");
            if (log) _project.SendInfoToLog($"Table {tableName} created", true);

            // Blockchain Private
            tableName = schemaName + "blockchain_private";
            tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {"secp256k1", defaultColumn},
                {"base58", defaultColumn},
                {"bip39", defaultColumn}
            };
            _sql.MkTable(tableStructure, tableName, false, schemaName: "");
            if (log) _project.SendInfoToLog($"Table {tableName} created", true);

            // Blockchain Public
            tableName = schemaName + "blockchain_public";
            tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {"evm", defaultColumn},
                {"sol", defaultColumn},
                {"apt", defaultColumn},
                {"sui", defaultColumn},
                {"osmo", defaultColumn},
                {"xion", defaultColumn},
                {"ton", defaultColumn},
                {"taproot", defaultColumn}
            };
            _sql.MkTable(tableStructure, tableName, false, schemaName: "accounts");
            if (log) _project.SendInfoToLog($"Table {tableName} created", true);

            // Settings (без schemaName)
            tableName = schemaName + "settings";
            tableStructure = new Dictionary<string, string>
            {
                {"var", "TEXT PRIMARY KEY"},
                {"value", "TEXT DEFAULT ''"}
            };
            _sql.MkTable(tableStructure, tableName, false, schemaName: "");
            if (log) _project.SendInfoToLog($"Table {tableName} created", true);
        }
        private string ImportData(string tableName, string formTitle, string[] availableFields, Dictionary<string, string> columnMapping, string message = "Select format (one field per box):")
        {
            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
            string table = schemaName + tableName;
            int lineCount = 0;

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = formTitle;
            form.Width = 800;
            form.Height = 700;
            form.TopMost = true; // Форма поверх всех окон
            form.Location = new System.Drawing.Point(108, 108);

            List<string> selectedFormat = new List<string>();
            System.Windows.Forms.TextBox formatDisplay = new System.Windows.Forms.TextBox();
            System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();

            // Метка для выбора формата
            System.Windows.Forms.Label formatLabel = new System.Windows.Forms.Label();
            formatLabel.Text = message;
            formatLabel.AutoSize = true;
            formatLabel.Left = 10;
            formatLabel.Top = 10;
            form.Controls.Add(formatLabel);

            // Создаём ComboBox в строку
            System.Windows.Forms.ComboBox[] formatComboBoxes = new System.Windows.Forms.ComboBox[availableFields.Length - 1]; // -1 из-за пустой строки
            int spacing = 5;
            int totalSpacing = spacing * (formatComboBoxes.Length - 1);
            int comboWidth = (form.ClientSize.Width - 20 - totalSpacing) / formatComboBoxes.Length;
            for (int i = 0; i < formatComboBoxes.Length; i++)
            {
                formatComboBoxes[i] = new System.Windows.Forms.ComboBox();
                formatComboBoxes[i].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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

            // Поле для отображения текущего формата
            formatDisplay.Left = 10;
            formatDisplay.Top = 60;
            formatDisplay.Width = form.ClientSize.Width - 20;
            formatDisplay.ReadOnly = true;
            form.Controls.Add(formatDisplay);

            // Метка и поле для ввода данных
            System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
            dataLabel.Text = "Input data (one per line, matching format):";
            dataLabel.AutoSize = true;
            dataLabel.Left = 10;
            dataLabel.Top = 90;
            form.Controls.Add(dataLabel);

            dataInput.Left = 10;
            dataInput.Top = 110;
            dataInput.Width = form.ClientSize.Width - 20;
            dataInput.Multiline = true;
            dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            form.Controls.Add(dataInput);

            // Кнопка "OK"
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
            // Показываем форму
            form.ShowDialog();
            // Если форма закрыта крестиком, прерываем выполнение
            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog($"Import to {tableName} cancelled by user", true);
                return "0";
            }

            // Собираем формат из ComboBox
            selectedFormat.Clear();
            foreach (var combo in formatComboBoxes)
            {
                if (!string.IsNullOrEmpty(combo.SelectedItem?.ToString()))
                    selectedFormat.Add(combo.SelectedItem.ToString());
            }

            // Проверка введённых данных
            if (string.IsNullOrEmpty(dataInput.Text) || selectedFormat.Count == 0)
            {
                _project.SendWarningToLog("Data or format cannot be empty");
                return "0";
            }

            string[] lines = dataInput.Text.Trim().Split('\n');
            _project.SendInfoToLog($"Parsing [{lines.Length}] {tableName} data lines", true);

            // Обработка строк
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
                    try
                    {
                        string dbQuery = $@"UPDATE {table} SET proxy = '{line.Replace("'", "''")}' WHERE acc0 = {acc0};";
                        _sql.DbQ(dbQuery, true);
                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        _project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}", false);
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

                    // Формируем SQL-запрос на основе маппинга
                    var queryParts = new List<string>();
                    foreach (var field in columnMapping.Keys)
                    {
                        string value = parsed_data.ContainsKey(field) ? parsed_data[field].Replace("'", "''") : "";
                        if (field == "CODE2FA" && value.Contains('/'))
                            value = value.Split('/').Last();
                        queryParts.Add($"{columnMapping[field]} = '{value}'");
                    }
                    //queryParts.Add("cooldown = 0");

                    try
                    {
                        string dbQuery = $@"UPDATE {table} SET {string.Join(", ", queryParts)} WHERE acc0 = {acc0};";
                        _sql.DbQ(dbQuery, true);
                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        _project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}", false);
                    }
                }
            }

            _project.SendInfoToLog($"[{lineCount}] records added to [{table}]", true);
            return lineCount.ToString();
        }
        private string ImportTwitter()
        {
            string[] twitterFields = new string[] { "", "LOGIN", "PASSWORD", "EMAIL", "EMAIL_PASSWORD", "TOKEN", "CODE2FA", "RECOVERY_SEED" };
            var twitterMapping = new Dictionary<string, string>
            {
                { "LOGIN", "login" },
                { "PASSWORD", "password" },
                { "EMAIL", "email" },
                { "EMAIL_PASSWORD", "email_pass" },
                { "TOKEN", "token" },
                { "CODE2FA", "code2fa" },
                { "RECOVERY_SEED", "recovery2fa" }
            };
            return ImportData("twitter", "Import Twitter Data", twitterFields, twitterMapping);
        }
        private string ImportDiscord()
        {
            string[] discordFields = new string[] { "", "LOGIN", "PASSWORD", "TOKEN", "CODE2FA" };
            var discordMapping = new Dictionary<string, string>
            {
                { "LOGIN", "login" },
                { "PASSWORD", "password" },
                { "TOKEN", "token" },
                { "CODE2FA", "code2fa" }
            };
            return ImportData("discord", "Import Discord Data", discordFields, discordMapping);
        }
        private string ImportGoogle()
        {
            string[] googleFields = new string[] { "", "LOGIN", "PASSWORD", "RECOVERY_EMAIL", "CODE2FA", "RECOVERY_SEED" };
            var googleMapping = new Dictionary<string, string>
            {
                { "LOGIN", "login" },
                { "PASSWORD", "password" },
                { "RECOVERY_EMAIL", "recovery_email" },
                { "CODE2FA", "code2fa" },
                { "RECOVERY_SEED", "recovery2fa" }
            };
            return ImportData("google", "Import Google Data", googleFields, googleMapping);
        }
        private string ImportIcloud()
        {
            string[] fields = new string[] { "ICLOUD", "" };
            var mapping = new Dictionary<string, string>
            {
                { "ICLOUD", "icloud" },
            };
            return ImportData("google", "Import Icloud", fields, mapping);
        }
        private string ImportNickname()
        {
            string[] fields = new string[] { "NICKNAME", "" };
            var mapping = new Dictionary<string, string>
            {
                { "NICKNAME", "nickname" },
            };
            return ImportData("profile", "Import nickname", fields, mapping);
        }
        private string ImportBio()
        {
            string[] fields = new string[] { "BIO", "" };
            var mapping = new Dictionary<string, string>
            {
                { "BIO", "bio" },
            };
            return ImportData("profile", "Import Bio", fields, mapping);
        }
        private string ImportProxy()
        {
            string[] fields = new string[] { "PROXY", "" };
            var mapping = new Dictionary<string, string>
            {
                { "PROXY", "proxy" },
            };
            return ImportData("profile", "Import proxy ", fields, mapping, message: "Proxy format: http://login1:pass1@111.111.111.111:1111");
        }
        private string ImportKeys(string keyType)
        {
            var acc0 = _project.Variables["acc0"];
            int rangeEnd = int.Parse(_project.Variables["rangeEnd"].Value);
            var blockchain = new Blockchain();

            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
            string privateTable = schemaName + "blockchain_private";
            string publicTable = schemaName + "blockchain_public";
            acc0.Value = "1";

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
                return "0"; // Возвращаем "0", так как обработка не началась
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
                            _sql.DbQ($@"UPDATE {privateTable} SET bip39 = '{encodedSeed}' WHERE acc0 = {acc0.Value};", true);
                            break;

                        case "evm":
                            string privateKey;
                            string address;

                            if (key.Split(' ').Length > 1) // Простая проверка на мнемонику
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
                            _sql.DbQ($@"UPDATE {privateTable} SET secp256k1 = '{encodedEvmKey}' WHERE acc0 = {acc0.Value};", true);
                            _sql.DbQ($@"UPDATE {publicTable} SET evm = '{address}' WHERE acc0 = {acc0.Value};", true);
                            break;

                        case "sol":
                            string encodedSolKey = SAFU.Encode(_project, key);
                            _sql.DbQ($@"UPDATE {privateTable} SET base58 = '{encodedSolKey}' WHERE acc0 = {acc0.Value};", true);
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
        public string ImportAddressesOld()
        {
            var acc0 = _project.Variables["acc0"];
            int rangeEnd = int.Parse(_project.Variables["rangeEnd"].Value);

            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
            string tableName = schemaName + "blockchain_public";

            acc0.Value = "1";

            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "chose type and input keys devided by new line, than ckick OK. Close window to continue";

            form.Width = 1008;
            form.Height = 1008;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            System.Windows.Forms.Label typeLabel = new System.Windows.Forms.Label();
            typeLabel.Text = "Select address type:";
            typeLabel.AutoSize = true;
            typeLabel.Left = 10;
            typeLabel.Top = 10;
            form.Controls.Add(typeLabel);

            System.Windows.Forms.ComboBox typeComboBox = new System.Windows.Forms.ComboBox();
            typeComboBox.Left = 10;
            typeComboBox.Top = 30;
            typeComboBox.Width = form.ClientSize.Width - 20;
            typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            var addressTypes = new[] { "evm", "sol", "apt", "sui" };
            typeComboBox.Items.AddRange(addressTypes);
            typeComboBox.SelectedIndex = 0;
            form.Controls.Add(typeComboBox);

            System.Windows.Forms.Label dataLabel = new System.Windows.Forms.Label();
            dataLabel.Text = "Input addresses (one per line):";
            dataLabel.AutoSize = true;
            dataLabel.Left = 10;
            dataLabel.Top = 60;
            form.Controls.Add(dataLabel);

            System.Windows.Forms.TextBox dataInput = new System.Windows.Forms.TextBox();
            dataInput.Left = 10;
            dataInput.Top = 80;
            dataInput.Width = form.ClientSize.Width - 20;
            dataInput.Multiline = true;
            dataInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dataInput.MaxLength = 1000000;
            form.Controls.Add(dataInput);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 20;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            dataInput.Height = okButton.Top - dataInput.Top - 5;

            string placeholder = "Chose type and input keys devided by new line, than ckick OK. Close window to continue\r\nВыбери тип адреса и вставь адсеса в форму каждый с новой строки. Нажми ОК. Повтори для нужных типов адресов и закрой окно";
            // Установка плейсхолдера
            if (string.IsNullOrEmpty(dataInput.Text)) // Если поле пустое, показываем плейсхолдер
            {
                dataInput.Text = placeholder;
                dataInput.ForeColor = System.Drawing.Color.Gray;
                //dataInput.Font = new System.Drawing.Font("Lucida Console", 10, System.Drawing.FontStyle.Bold);
            }
            dataInput.Enter += (s, e) => { if (dataInput.Text == placeholder) { dataInput.Text = ""; dataInput.ForeColor = System.Drawing.Color.Black; } };
            dataInput.Leave += (s, e) => { if (string.IsNullOrEmpty(dataInput.Text)) { dataInput.Text = placeholder; dataInput.ForeColor = System.Drawing.Color.Gray; } };

            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

        show:
            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return "0";
            }

            if (string.IsNullOrEmpty(dataInput.Text))
            {
                _project.SendWarningToLog("Input could not be empty");
                goto show;
            }

            string selectedType = typeComboBox.SelectedItem.ToString();
            string[] lines = dataInput.Text.Trim().Split('\n');
            _project.SendInfoToLog($"Parsing [{lines.Length}] addresses for {selectedType}", false);

            for (int i = 0; i < lines.Length && int.Parse(acc0.Value) <= rangeEnd; i++)
            {
                string address = lines[i].Trim();
                // Пустые строки записываются как '' без пропуска
                try
                {
                    _project.SendInfoToLog($"Processing acc0 = {acc0.Value}, address = '{address}'", false);
                    _sql.DbQ($@"UPDATE {tableName} SET {selectedType} = '{address}' WHERE acc0 = {acc0.Value};", true);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing record {acc0.Value} for {selectedType}: {ex.Message}", false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
            }
            _project.SendInfoToLog($"Imported {lines.Length} strings", true);
            goto show;
        }

        public string ImportAddresses()
        {
            var acc0 = _project.Variables["acc0"];
            int rangeEnd = int.Parse(_project.Variables["rangeEnd"].Value);
            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
            string tableName = schemaName + "blockchain_public";

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

            // Проверка ввода
            if (string.IsNullOrEmpty(columnInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Column name or addresses cannot be empty");
                return "0";
            }

            // Формирование имени столбца
            string columnName = columnInput.Text.ToLower();

            // Создание таблицы (если нужно)
            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {columnName, "TEXT DEFAULT ''"}
            };
            _sql.MkTable(tableStructure, tableName);
            //SQL.W3MakeTable(_project, tableStructure, tableName);

            // Обработка адресов
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
                    _sql.DbQ($@"UPDATE {tableName} SET {columnName} = '{address}' WHERE acc0 = {acc0.Value};", true);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                    lineCount++;
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing record {acc0.Value} for {columnName}: {ex.Message}", false);
                    acc0.Value = (int.Parse(acc0.Value) + 1).ToString();
                }
            }

            _project.SendInfoToLog($"[{lineCount}] strings added to [{tableName}]", true);
            return lineCount.ToString();
        }
        public string ImportDepositAddresses()
        {
            string table = (_project.Variables["DBmode"].Value == "PostgreSQL" ? $"accounts." : null) + "cex_deps";
            // Создание формы
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Import Deposit Addresses";
            form.Width = 420;
            form.Height = 700;
            form.TopMost = true; // Форма поверх всех окон
            form.Location = new System.Drawing.Point(108, 108);

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
            chainInput.Text = _project.Variables["depositChain"].Value; // Текущее значение из переменной
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
            cexInput.Text = _project.Variables["depositCEX"].Value; // Текущее значение из переменной
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

            // Проверка ввода
            if (string.IsNullOrEmpty(chainInput.Text) || string.IsNullOrEmpty(cexInput.Text) || string.IsNullOrEmpty(addressInput.Text))
            {
                _project.SendWarningToLog("Chain, CEX, or addresses cannot be empty");
                return "0";
            }

            // Формирование имени столбца
            string CHAIN = chainInput.Text.ToLower();
            string CEX = cexInput.Text.ToLower();
            string columnName = $"{CEX}_{CHAIN}";

            // Создание таблицы
            var tableStructure = new Dictionary<string, string>
            {
                {"acc0", "INTEGER PRIMARY KEY"},
                {columnName, "TEXT DEFAULT ''"}
            };
            _sql.MkTable(tableStructure, table, log:true);
            //SQL.W3MakeTable(_project, tableStructure, table); // Используем полное имя с схемой

            string[] lines = addressInput.Text.Trim().Split('\n');
            int lineCount = 0;

            for (int acc0 = 1; acc0 <= lines.Length; acc0++) // Начинаем с 1, как в других методах
            {
                string line = lines[acc0 - 1].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    _project.SendWarningToLog($"Line {acc0} is empty");
                    continue;
                }

                try
                {
                    _sql.DbQ($@"UPDATE {table} SET
						{columnName} = '{line}'
						WHERE acc0 = {acc0};");
                    lineCount++;
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error processing line {acc0}: {ex.Message}");
                    continue;
                }
            }

            _project.SendInfoToLog($"[{lineCount}] strings added to [{table}]", true);
            return lineCount.ToString();
        }
        private void ImportWebGL(Instance instance, string tableName = "profile")
        {

            _project.SendWarningToLog("This function works only on ZennoPoster versions below 7.8", true);
            _instance.CanvasRenderMode = ZennoLab.InterfacesLibrary.Enums.Browser.CanvasMode.Allow;

            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? $"{_schema}." : "";
            string table = schemaName + tableName;

            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Import WebGL Data";
            form.Width = 420;
            form.Height = 200; // Компактная форма, так как только выбор вендора
            form.TopMost = true; // Форма поверх всех окон
            form.Location = new System.Drawing.Point(108, 108);

            System.Windows.Forms.Label vendorLabel = new System.Windows.Forms.Label();
            vendorLabel.Text = "Select WebGL Vendor:";
            vendorLabel.AutoSize = true;
            vendorLabel.Left = 10;
            vendorLabel.Top = 10;
            form.Controls.Add(vendorLabel);

            System.Windows.Forms.ComboBox vendorComboBox = new System.Windows.Forms.ComboBox();
            vendorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            vendorComboBox.Items.AddRange(new string[] { "Intel", "NVIDIA", "AMD" });
            vendorComboBox.SelectedIndex = 0; // По умолчанию Intel
            vendorComboBox.Left = 10;
            vendorComboBox.Top = 30;
            vendorComboBox.Width = form.ClientSize.Width - 20;
            form.Controls.Add(vendorComboBox);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = form.ClientSize.Width - 10;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = form.ClientSize.Height - okButton.Height - 5;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);
            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); }; // Фиксируем позицию перед показом

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return;
            }

            string webGLvendor = vendorComboBox.SelectedItem.ToString();
            int targetCount = int.Parse(_project.Variables["rangeEnd"].Value);
            List<string> tempList = new List<string>();

            while (tempList.Count < targetCount)
            {
                _project.SendToLog($"Parsing {webGLvendor} WebGL. {tempList.Count}/{_project.Variables["rangeEnd"].Value} strings parsed", LogType.Info, true, LogColor.Default);
                try { _instance.Launch(ZennoLab.InterfacesLibrary.Enums.Browser.BrowserType.Firefox45, false); } catch { }
                _instance.Launch(ZennoLab.InterfacesLibrary.Enums.Browser.BrowserType.Chromium, false);
                string webglData = _instance.WebGLPreferences.Save();
                _project.SendInfoToLog(webglData);
                if (webglData.Contains(webGLvendor)) tempList.Add(webglData);
            }

            for (int acc0 = 0; acc0 < tempList.Count; acc0++)
            {
                string webGLrenderer = tempList[acc0];
                if (string.IsNullOrEmpty(webGLrenderer)) continue;
                _sql.DbQ($@"UPDATE {table} SET webgl = '{tempList[acc0].Replace("'", "''")}' WHERE acc0 = {acc0 + 1};");
            }

            _project.SendInfoToLog($"[{tempList.Count}] strings added to [{table}]", true);
            _instance.CanvasRenderMode = ZennoLab.InterfacesLibrary.Enums.Browser.CanvasMode.Block;
            try { _instance.Launch(ZennoLab.InterfacesLibrary.Enums.Browser.BrowserType.WithoutBrowser, false); } catch { }
        }
        private void ImportSettings(string message = "input data please", int width = 600, int height = 400)
        {
            _project.SendInfoToLog($"Opening variables input dialog: {message}", true);

            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = message;
            form.Width = width;
            form.Height = height;
            form.TopMost = true;
            form.Location = new System.Drawing.Point(108, 108);

            // Массив с именами переменных и плейсхолдерами
            (string varName, string placeholder)[] variableNames = new (string, string)[]
            {
                ("settingsApiFirstMail", "API First Mail для доступа к переадресованной почте"),
                ("settingsApiPerplexity", "API perplexity для запросов к AI (например прогрева твиттера)"),
                ("settingsDsInviteOwn", "Инвайт на свой сервер"),
                ("settingsDsOwnServer", "ID канала с инвайтами на вашем сервере"),
                ("settingsFmailLogin", "Логин от общего ящика для форвардов на FirstMail"),
                ("settingsFmailPass", "Пароль от общего ящика для форвардов на FirstMail"),
                ("settingsTgLogGroup", "Id группы для логов в Telegram. Формат {-1002000000009}"),
                ("settingsTgLogToken", "Токен Telegram логгера"),
                ("settingsTgLogTopic", "Id топика в группе для логов. 0 - если нет топиков"),
                ("settingsTgMailGroup", "Id группы с переадресованной почтой в Telegram. Формат {-1002000000009}"),
                ("settingsZenFolder", "Путь к папке с профилями и причастным данным. Формат: {F:\\farm\\}"),
                ("settingsApiBinance", "Данные для вывода с Binance. Формат: {API_KEY;SECRET_KEY;PROXY}")
            };

            var textBoxes = new Dictionary<string, System.Windows.Forms.TextBox>();

            int currentTop = 5;
            int labelWidth = 150;
            int textBoxWidth = 400;
            int spacing = 5;

            foreach (var (varName, placeholder) in variableNames)
            {
                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                label.Text = varName + ":";
                label.AutoSize = true;
                label.Left = 5;
                label.Top = currentTop;
                form.Controls.Add(label);

                System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
                textBox.Left = label.Left + labelWidth + spacing;
                textBox.Top = currentTop;
                textBox.Width = textBoxWidth;
                textBox.Text = _project.Variables[varName].Value;

                // Установка плейсхолдера
                if (string.IsNullOrEmpty(textBox.Text)) // Если поле пустое, показываем плейсхолдер
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = System.Drawing.Color.Gray;
                }
                textBox.Enter += (s, e) => { if (textBox.Text == placeholder) { textBox.Text = ""; textBox.ForeColor = System.Drawing.Color.Black; } };
                textBox.Leave += (s, e) => { if (string.IsNullOrEmpty(textBox.Text)) { textBox.Text = placeholder; textBox.ForeColor = System.Drawing.Color.Gray; } };

                form.Controls.Add(textBox);

                textBoxes[varName] = textBox;
                currentTop += textBox.Height + spacing;
            }

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "OK";
            okButton.Width = 50;
            okButton.Height = 25;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2;
            okButton.Top = currentTop + 10;
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);

            int requiredHeight = okButton.Top + okButton.Height + 40;
            if (form.Height < requiredHeight)
            {
                form.Height = requiredHeight;
            }
            form.Load += (s, e) => { form.Location = new System.Drawing.Point(108, 108); };

            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return;
            }

            string tableName = "settings";
            foreach (var (varName, placeholder) in variableNames)
            {
                string newValue = textBoxes[varName].Text;
                if (newValue == placeholder) newValue = ""; // Если текст — это плейсхолдер, считаем поле пустым
                _project.Variables[varName].Value = newValue;
                _project.SendInfoToLog($"Updated variable {varName}: {newValue}", true);

                if (!string.IsNullOrEmpty(newValue))
                {
                    string escapedValue = newValue.Replace("'", "''");
                    _sql.DbQ($"INSERT OR REPLACE INTO {tableName} (var, value) VALUES ('{varName}', '{escapedValue}');");
                    //_sql.DbQ($"INSERT OR REPLACE INTO {tableName} (var, value) VALUES ('{varName}', '{escapedValue}');");
                    _project.SendInfoToLog($"Inserted into {tableName}: {varName} = {newValue}", true);
                }
            }
        }
        public void ShowBalanceTable(string chains = null)
        {
            string schemaName = _project.Variables["DBmode"].Value == "PostgreSQL" ? "accounts." : "";
            string tableName = schemaName + "native";
            var columns = new List<string>();

            if (string.IsNullOrEmpty(chains))
            {
                if (_project.Variables["DBmode"].Value == "PostgreSQL") chains = _sql.DbQ($@"SELECT column_name FROM information_schema.columns WHERE table_schema = 'accounts' AND table_name = 'native'");
                else chains = _sql.DbQ($@"SELECT name FROM pragma_table_info('native')");
                columns = chains.Split('\n').ToList();
            }
            else
                columns = chains.Split(',').ToList();

            int pageSize = 100;
            int offset = 0;
            int totalRows = 0;

            string countQuery = $@"SELECT COUNT(*) FROM {tableName} WHERE acc0 <= {_project.Variables["rangeEnd"].Value}";
            string countResult = _sql.DbQ(countQuery, true);
            if (!string.IsNullOrEmpty(countResult) && int.TryParse(countResult, out int count))
            {
                totalRows = count;
            }

            var form = new System.Windows.Forms.Form
            {
                Text = "Balance Table",
                Width = 1008,
                Height = 1008,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
            };
            form.BackColor = System.Drawing.Color.White;
            form.TopMost = true; 

            // Панель для пагинации
            var panel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 40
            };
            form.Controls.Add(panel);

            var prevButton = new System.Windows.Forms.Button
            {
                Text = "Previous",
                Width = 100,
                Height = 30,
                Left = 10,
                Top = 5,
                Enabled = false
            };
            var nextButton = new System.Windows.Forms.Button
            {
                Text = "Next",
                Width = 100,
                Height = 30,
                Left = 120,
                Top = 5
            };
            panel.Controls.Add(prevButton);
            panel.Controls.Add(nextButton);

            var grid = new System.Windows.Forms.DataGridView
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                AllowUserToResizeColumns = true,
                AllowUserToResizeRows = true,
                ScrollBars = System.Windows.Forms.ScrollBars.Both,
                AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells,
                ReadOnly = true,
                BackgroundColor = System.Drawing.Color.White
            };
            form.Controls.Add(grid);

            grid.BringToFront();

            Action loadData = () =>
            {
                string query = $@"SELECT {string.Join(",", columns)} FROM {tableName} 
								WHERE acc0 <= {_project.Variables["rangeEnd"].Value} 
								ORDER BY acc0 LIMIT {pageSize} OFFSET {offset}";
                string result = _sql.DbQ(query, true);

                if (string.IsNullOrEmpty(result))
                {
                    _project.SendWarningToLog("No data found in balance table");
                    grid.Rows.Clear();
                    return;
                }

                var rows = result.Trim().Split('\n');
                _project.SendInfoToLog($"Loaded {rows.Length} rows from {tableName}, offset {offset}", false);

                grid.Columns.Clear();
                grid.Rows.Clear();

                foreach (var col in columns)
                {
                    var column = new System.Windows.Forms.DataGridViewColumn
                    {
                        Name = col.Trim(),
                        HeaderText = col.Trim(),
                        CellTemplate = new System.Windows.Forms.DataGridViewTextBoxCell(),
                        SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable // Отключаем сортировку
                    };
                    grid.Columns.Add(column);
                }

                foreach (var row in rows)
                {
                    var values = row.Split('|');
                    if (values.Length != columns.Count)
                    {
                        _project.SendWarningToLog($"Invalid row format: {row}. Expected {columns.Count} columns, got {values.Length}", false);
                        continue;
                    }

                    var formattedValues = new string[values.Length];
                    formattedValues[0] = values[0]; // acc0 без изменений
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(values[i]))
                        {
                            formattedValues[i] = "0.0000000";
                            continue;
                        }
                        try
                        {
                            string val = values[i].Replace(",", ".");
                            if (decimal.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
                            {
                                formattedValues[i] = balance.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                formattedValues[i] = "0.0000000";
                                _project.SendWarningToLog($"Invalid format in {columns[i]}, row {grid.Rows.Count + 1}: '{values[i]}'", false);
                            }
                        }
                        catch (Exception ex)
                        {
                            formattedValues[i] = "0.0000000";
                            _project.SendWarningToLog($"Error formatting in {columns[i]}, row {grid.Rows.Count + 1}: {ex.Message}", false);
                        }
                    }
                    grid.Rows.Add(formattedValues);

                }

                prevButton.Enabled = offset > 0;
                nextButton.Enabled = offset + pageSize < totalRows;


                int totalWidth = grid.Columns.Cast<System.Windows.Forms.DataGridViewColumn>().Sum(col => col.Width);
                grid.Width = totalWidth; // Устанавливаем ширину таблицы
                form.Width = Math.Min(totalWidth - 100, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width); // Учитываем скроллбар (~20px)
            };

            loadData();

            prevButton.Click += (s, e) =>
            {
                if (offset >= pageSize)
                {
                    offset -= pageSize;
                    loadData();
                }
            };
            nextButton.Click += (s, e) =>
            {
                if (offset + pageSize < totalRows)
                {
                    offset += pageSize;
                    loadData();
                }
            };

            // Настройка цветов
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (grid.Columns[e.ColumnIndex].Name == "acc0")
                {
                    // Здесь задаём стиль для acc0
                    e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9, System.Drawing.FontStyle.Bold);
                    e.CellStyle.BackColor = System.Drawing.Color.Black; // Пример фона
                    e.CellStyle.ForeColor = System.Drawing.Color.White; // Пример цвета текста
                    return;
                }

                string value = grid[e.ColumnIndex, e.RowIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(value)) return;

                try
                {
                    // Парсинг уже отформатированного значения
                    if (decimal.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
                    {
                        e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9);
                        if (balance >= 0.1m)
                            e.CellStyle.BackColor = System.Drawing.Color.SteelBlue;
                        else if (balance >= 0.01m)
                            e.CellStyle.BackColor = System.Drawing.Color.Green;
                        else if (balance >= 0.001m)
                            e.CellStyle.BackColor = System.Drawing.Color.YellowGreen;
                        else if (balance >= 0.0001m)
                            e.CellStyle.BackColor = System.Drawing.Color.Khaki;
                        else if (balance >= 0.00001m)
                            e.CellStyle.BackColor = System.Drawing.Color.LightSalmon;
                        else if (balance > 0)
                            e.CellStyle.BackColor = System.Drawing.Color.IndianRed;
                        else if (balance == 0)
                        {
                            e.CellStyle.BackColor = System.Drawing.Color.White;
                            e.CellStyle.ForeColor = System.Drawing.Color.White;
                        }
                        else
                            e.CellStyle.BackColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        _project.SendWarningToLog($"Invalid balance format in {grid.Columns[e.ColumnIndex].Name}, row {e.RowIndex + 1}: '{value}'", false);
                    }
                }
                catch (Exception ex)
                {
                    _project.SendWarningToLog($"Error parsing balance in {grid.Columns[e.ColumnIndex].Name}, row {e.RowIndex + 1}: {ex.Message}", false);
                }
            };

            // Показ формы
            form.ShowDialog();
        }
        private bool ImportStart()
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "!!!Warning";
            form.Width = 400;
            form.Height = 200;
            form.BackColor = System.Drawing.Color.Red;
            form.TopMost = true; 
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen; 

            System.Windows.Forms.Label messageLabel = new System.Windows.Forms.Label();
            messageLabel.Text = "Db data will be overwritten!\nContinue?";
            messageLabel.Font = new System.Drawing.Font("Consolas", 15, System.Drawing.FontStyle.Underline); 
            messageLabel.ForeColor = System.Drawing.Color.White;
            messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            messageLabel.AutoSize = true;
            messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2; 
            messageLabel.Top = 30; // Отступ сверху
            form.Controls.Add(messageLabel);

            messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2;

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "YES";
            okButton.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold);
            okButton.ForeColor = System.Drawing.Color.White;
            okButton.BackColor = System.Drawing.Color.Red;
            okButton.Width = 100;
            okButton.Height = 30;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 4; // Центрируем кнопку
            okButton.Top = form.ClientSize.Height - okButton.Height - 40; // Отступ снизу
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);


            System.Windows.Forms.Button cancelButton = new System.Windows.Forms.Button();
            cancelButton.Text = "Cancel";
            cancelButton.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold);
            cancelButton.ForeColor = System.Drawing.Color.White;
            cancelButton.BackColor = System.Drawing.Color.Black;
            cancelButton.Width = 100;
            cancelButton.Height = 30;
            cancelButton.Left = (form.ClientSize.Width - cancelButton.Width) / 4 * 3; // Центрируем кнопку
            cancelButton.Top = form.ClientSize.Height - cancelButton.Height - 40; // Отступ снизу
            cancelButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.Cancel; form.Close(); };
            form.Controls.Add(cancelButton);



            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };
            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                _project.SendInfoToLog("Import cancelled by user", true);
                return false;
            }
            return true;
        }
        private bool ImportDone()
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Done";
            form.Width = 500;
            form.Height = 200;
            form.BackColor = System.Drawing.Color.Green;
            form.TopMost = true;
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen; // Центрируем окно

            System.Windows.Forms.Label messageLabel = new System.Windows.Forms.Label();
            messageLabel.Text = "Db data imported successful.\nPlease switch off \"CreateDb\" option\n in \"Import\" tab of script settings";
            messageLabel.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold); // Жирный шрифт
            messageLabel.ForeColor = System.Drawing.Color.White;
            messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            messageLabel.AutoSize = true;
            messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2; // Центрируем по горизонтали
            messageLabel.Top = 20; // Отступ сверху
            form.Controls.Add(messageLabel);

            messageLabel.Left = (form.ClientSize.Width - messageLabel.Width) / 2;

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "Nice";
            okButton.Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Bold);
            okButton.ForeColor = System.Drawing.Color.White;
            okButton.BackColor = System.Drawing.Color.Black;
            okButton.Width = 100;
            okButton.Height = 30;
            okButton.Left = (form.ClientSize.Width - okButton.Width) / 2; // Центрируем кнопку
            okButton.Top = form.ClientSize.Height - okButton.Height - 40; // Отступ снизу
            okButton.Click += (s, e) => { form.DialogResult = System.Windows.Forms.DialogResult.OK; form.Close(); };
            form.Controls.Add(okButton);


            form.FormClosing += (s, e) => { if (form.DialogResult != System.Windows.Forms.DialogResult.OK) form.DialogResult = System.Windows.Forms.DialogResult.Cancel; };

            form.ShowDialog();
            return true;
        }

    }
}
