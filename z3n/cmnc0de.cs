using Leaf.xNet;
using NBitcoin;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.Enums.Browser;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using z3n;
using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;


namespace w3tools //by @w3bgrep
{

    public  static class TestStatic
    {



    }



    public class ChainOperaAI 
    {

        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        public ChainOperaAI(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "ChainOpera");

        }
        public void GetAuthData()
        {
            var url = "https://chat.chainopera.ai/userCenter/api/v1/";

            _project.Deadline();
            _instance.UseTrafficMonitoring = true;

            _instance.HeClick(("button", "class", "inline-flex\\ items-center\\ justify-center\\ whitespace-nowrap\\ text-sm\\ font-medium\\ transition-colors\\ focus-visible:outline-hidden\\ focus-visible:ring-1\\ focus-visible:ring-primary\\ disabled:pointer-events-none\\ disabled:opacity-50\\ border\\ bg-background\\ hover:border-primary/50\\ shadow-xs\\ hover:bg-primary\\ hover:text-primary-foreground\\ hover:ring-primary/60\\ hover:ring-2\\ active:bg-background\\ active:text-primary\\ active:ring-0\\ px-4\\ py-2\\ h-10\\ truncate\\ rounded-full\\ pr-1\\ w-auto\\ gap-4", "regexp", 0));


        get:
            _project.Deadline(10);
            Thread.Sleep(1000);
            var traffic = _instance.ActiveTab.GetTraffic();
            var data = new Dictionary<string, string>();
            //string param;
            foreach (var t in traffic)
            {
                if (t.Url.Contains(url))
                {

                    var Method = t.Method;
                    var ResultCode = t.ResultCode.ToString();
                    var Url = t.Url;
                    var ResponseContentType = t.ResponseContentType;
                    var RequestHeaders = t.RequestHeaders;
                    var RequestCookies = t.RequestCookies;
                    var RequestBody = t.RequestBody;
                    var ResponseHeaders = t.ResponseHeaders;
                    var ResponseCookies = t.ResponseCookies;
                    var ResponseBody = t.ResponseBody == null ? "" : Encoding.UTF8.GetString(t.ResponseBody, 0, t.ResponseBody.Length);

                    if (Method == "OPTIONS") continue;
                    data.Add("Method", Method);
                    data.Add("ResultCode", ResultCode);
                    data.Add("Url", Url);
                    data.Add("ResponseContentType", ResponseContentType);
                    data.Add("RequestHeaders", RequestHeaders);
                    data.Add("RequestCookies", RequestCookies);
                    data.Add("RequestBody", RequestBody);
                    data.Add("ResponseHeaders", ResponseHeaders);
                    data.Add("ResponseCookies", ResponseCookies);
                    data.Add("ResponseBody", ResponseBody);
                    break;
                }

            }
            if (data.Count == 0) goto get;


            string headersString = data["RequestHeaders"].Trim();//RequestCookies
            _project.L0g(headersString);

            string headerToGet = "authorization";
            var headers = headersString.Split('\n');

            foreach (string header in headers)
            {
                if (header.ToLower().Contains("authorization"))
                {
                    _project.Var("token", header.Split(':')[1]);
                }
                if (header.ToLower().Contains("cookie"))
                {
                    _project.Var("cookie", header.Split(':')[1]);
                }

            }
            _instance.HeClick(("button", "class", "inline-flex\\ items-center\\ justify-center\\ whitespace-nowrap\\ text-sm\\ font-medium\\ transition-colors\\ focus-visible:outline-hidden\\ focus-visible:ring-1\\ focus-visible:ring-primary\\ disabled:pointer-events-none\\ disabled:opacity-50\\ border\\ bg-background\\ hover:border-primary/50\\ shadow-xs\\ hover:bg-primary\\ hover:text-primary-foreground\\ hover:ring-primary/60\\ hover:ring-2\\ active:bg-background\\ active:text-primary\\ active:ring-0\\ px-4\\ py-2\\ h-10\\ truncate\\ rounded-full\\ pr-1\\ w-auto\\ gap-4", "regexp", 0), emu: 1);

        }

        public string ReqGet(string path, bool parse = false , bool log = false)
        {

            string token = _project.Variables["token"].Value;
            string cookie = _project.Variables["cookie"].Value;

            var headers = new Dictionary<string, string>
            {
                { "authority", "chat.chainopera.ai" },
                { "authorization", $"{token}" },
                { "method", "GET" },
                { "path", path },
                { "accept", "application/json, text/plain, */*" },
                { "accept-encoding", "gzip, deflate, br" },
                { "accept-language", "en-US,en;q=0.9" },
                { "content-type", "application/json" },
                { "origin", "https://chat.chainopera.ai" },
                { "priority", "u=1, i" },

                { "sec-ch-ua", "\"Chromium\";v=\"134\", \"Not:A-Brand\";v=\"24\", \"Google Chrome\";v=\"134\"" },
                { "sec-ch-ua-mobile", "?0" },
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "sec-fetch-dest", "empty" },
                { "sec-fetch-mode", "cors" },
                { "sec-fetch-site", "same-site" },

                { "cookie", $"{cookie}" },
            };



            string[] headerArray = headers.Select(header => $"{header.Key}:{header.Value}").ToArray();
            string url = $"https://chat.chainopera.ai{path}";
            string response;

            try
            {
                response = _project.GET(url, "+", headerArray, log: false, parse);
                _logger.Send(response);
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Err HTTPreq: {ex.Message}");
                throw;
            }
            
            return response;






        }


    }
    public class Accountant2
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Sql _sql;
        private readonly Logger _logger;
        private int _offset;

        public Accountant2(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;

            _logger = new Logger(project, log: log, classEmoji: "$");
            _sql = new Sql(project, log: log);
        }

        public void ShowBalanceTable(string chains = null)
        {
            var tableName = "public_native";
            var columns = new List<string>();

            if (string.IsNullOrEmpty(chains))
                columns = new Sql(_project).GetColumnList("public_native");

            else
                columns = chains.Split(',').ToList();


            int pageSize = 100;
            _offset = 0; // Инициализируем offset
            int totalRows = int.Parse(_project.Variables["rangeEnd"].Value);

            var form = CreateForm();
            var panel = CreatePaginationPanel(form);
            var prevButton = CreatePreviousButton(panel);
            var nextButton = CreateNextButton(panel);
            var grid = CreateDataGridView(form);

            Action loadData = () => LoadData(grid, columns, tableName, pageSize, ref _offset, totalRows, prevButton, nextButton);
            loadData();

            ConfigurePaginationEvents(prevButton, nextButton, pageSize, totalRows, loadData);
            ConfigureCellFormatting(grid, columns);

            form.ShowDialog();
        }

        private System.Windows.Forms.Form CreateForm()
        {
            var form = new System.Windows.Forms.Form
            {
                Text = "Balance Table",
                Width = 1008,
                Height = 800,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
            };
            form.BackColor = System.Drawing.Color.White;
            form.TopMost = true;
            return form;
        }

        private System.Windows.Forms.Panel CreatePaginationPanel(System.Windows.Forms.Form form)
        {
            var panel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 40
            };
            form.Controls.Add(panel);
            return panel;
        }

        private System.Windows.Forms.Button CreatePreviousButton(System.Windows.Forms.Panel panel)
        {
            var prevButton = new System.Windows.Forms.Button
            {
                Text = "Previous",
                Width = 100,
                Height = 30,
                Left = 10,
                Top = 5,
                Enabled = false
            };
            panel.Controls.Add(prevButton);
            return prevButton;
        }

        private System.Windows.Forms.Button CreateNextButton(System.Windows.Forms.Panel panel)
        {
            var nextButton = new System.Windows.Forms.Button
            {
                Text = "Next",
                Width = 100,
                Height = 30,
                Left = 120,
                Top = 5
            };
            panel.Controls.Add(nextButton);
            return nextButton;
        }

        private System.Windows.Forms.DataGridView CreateDataGridView(System.Windows.Forms.Form form)
        {
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
            return grid;
        }

        private void LoadData(System.Windows.Forms.DataGridView grid, List<string> columns, string tableName, int pageSize, ref int offset, int totalRows, System.Windows.Forms.Button prevButton, System.Windows.Forms.Button nextButton)
        {
            string result = new Sql(_project).Get($"{string.Join(",", columns)}", tableName, where: $"acc0 <= '{_project.Var("rangeEnd")}' ORDER BY acc0");

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
                    SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
                };
                grid.Columns.Add(column);
            }

            // Добавляем столбец для суммы
            var sumColumn = new System.Windows.Forms.DataGridViewColumn
            {
                Name = "RowSum",
                HeaderText = "RowSum",
                CellTemplate = new System.Windows.Forms.DataGridViewTextBoxCell(),
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
            };
            grid.Columns.Add(sumColumn);

            foreach (var row in rows)
            {
                var values = row.Split('|');
                if (values.Length != columns.Count)
                {
                    _project.SendWarningToLog($"Invalid row format: {row}. Expected {columns.Count} columns, got {values.Length}", false);
                    continue;
                }

                var formattedValues = new string[values.Length + 1];
                formattedValues[0] = values[0]; // acc0 без изменений
                decimal rowSum = 0;
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
                            rowSum += balance;
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
                formattedValues[values.Length] = rowSum.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture);
                grid.Rows.Add(formattedValues);
            }

            prevButton.Enabled = offset > 0;
            nextButton.Enabled = offset + pageSize < totalRows;

            int totalWidth = grid.Columns.Cast<System.Windows.Forms.DataGridViewColumn>().Sum(col => col.Width);
            if (totalWidth < 1000) totalWidth = 1000;
            grid.Width = totalWidth;
            grid.FindForm().Width = Math.Min(totalWidth - 100, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width);

            var sumRow = CalculateColumnSums(grid, columns);
            grid.Rows.Add(sumRow);

            // Визуальное выделение строки суммы
            var lastRow = grid.Rows[grid.Rows.Count - 1];
            lastRow.DefaultCellStyle.Font = new System.Drawing.Font(grid.Font, System.Drawing.FontStyle.Bold);
            lastRow.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
        }
  

        private void ConfigurePaginationEvents(System.Windows.Forms.Button prevButton, System.Windows.Forms.Button nextButton, int pageSize, int totalRows, Action loadData)
        {
            prevButton.Click += (s, e) =>
            {
                if (_offset >= pageSize) // Используем поле _offset
                {
                    _offset -= pageSize;
                    loadData();
                }
            };
            nextButton.Click += (s, e) =>
            {
                if (_offset + pageSize < totalRows)
                {
                    _offset += pageSize;
                    loadData();
                }
            };
        }

        private void ConfigureCellFormatting(System.Windows.Forms.DataGridView grid, List<string> columns)
        {
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (grid.Columns[e.ColumnIndex].Name == "acc0")
                {
                    e.CellStyle.Font = new System.Drawing.Font("Lucida Console", 9, System.Drawing.FontStyle.Bold);
                    e.CellStyle.BackColor = System.Drawing.Color.Black;
                    e.CellStyle.ForeColor = System.Drawing.Color.White;
                    return;
                }

                string value = grid[e.ColumnIndex, e.RowIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(value)) return;

                try
                {
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
        }

        private string[] CalculateColumnSums(System.Windows.Forms.DataGridView grid, List<string> columns)
        {
            var sums = new string[columns.Count];
            sums[0] = "ИТОГО"; // Первый столбец — подпись

            for (int col = 1; col < columns.Count; col++)
            {
                decimal sum = 0;
                foreach (System.Windows.Forms.DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    var val = row.Cells[col].Value?.ToString();
                    if (decimal.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal d))
                        sum += d;
                }
                sums[col] = sum.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture);
            }
            return sums;
        }

        public void ShowBalanceTableFromList(List<string> data)
        {
            var columns = new List<string> { "Счет", "Баланс" };

            // Создаём форму и грид
            var form = CreateForm();
            var grid = CreateDataGridView(form);

            // Очищаем и настраиваем грид
            grid.Columns.Clear();
            grid.Rows.Clear();
            grid.AllowUserToAddRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            // Добавляем столбцы
            grid.Columns.Add("acc", "Счет");
            grid.Columns.Add("balance", "Баланс");

            // Заполняем строки из списка
            decimal sum = 0;
            foreach (var line in data)
            {
                var parts = line.Split(':');
                if (parts.Length != 2)
                    continue;
                string acc = parts[0].Trim();
                string balanceStr = parts[1].Replace(",", ".").Trim();
                if (!decimal.TryParse(balanceStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
                    balance = 0;
                sum += balance;
                grid.Rows.Add(acc, balance.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture));
            }

            // Добавляем итоговую строку
            int sumRowIdx = grid.Rows.Add("TOTAL", sum.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture));
            var sumRow = grid.Rows[sumRowIdx];
            sumRow.DefaultCellStyle.Font = new System.Drawing.Font(grid.Font, System.Drawing.FontStyle.Bold);
            sumRow.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

            // Жирная линия под итоговой строкой
            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex == sumRowIdx && e.RowIndex >= 0)
                {
                    e.Paint(e.CellBounds, System.Windows.Forms.DataGridViewPaintParts.All);
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black, 3))
                    {
                        e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }
                    e.Handled = true;
                }
            };

            // Форматирование балансов (цвета)
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
                string value = grid[e.ColumnIndex, e.RowIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(value)) return;
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
            };

            form.ShowDialog();
        }



    }


    public class GitHub2
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;

        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _login;
        protected string _pass;
        protected string _2fa;

        public GitHub2(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project);
            _logger = new Logger(project, log: log, classEmoji: "🐱");
            LoadCreds();

        }
        public void LoadCreds()
        {
            string[] creds = _sql.Get("status, login, password, otpsecret, cookies", "private_github").Split('|');
            try { _status = creds[0]; _project.Variables["googleSTATUS"].Value = _status; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _login = creds[1]; _project.Variables["github_login"].Value = _login; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _pass = creds[2]; _project.Variables["github_pass"].Value = _pass; } catch (Exception ex) { _logger.Send(ex.Message); }
            try { _2fa = creds[3]; _project.Variables["github_code"].Value = _2fa; } catch (Exception ex) { _logger.Send(ex.Message); }

        }

        private void InputCreds()
        {



        }



    }



    public class Sql2
    {
        private readonly IZennoPosterProjectModel _project;
        protected readonly string _dbMode;
        private readonly bool _logShow;

        private readonly Logger _logger;

        protected bool _pstgr = false;
        protected string _tableName = string.Empty;
        protected string _schemaName = string.Empty;

        public Sql2(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _pstgr = _project.Variables["DBmode"].Value == "PostgreSQL" ? true : false;
            _dbMode = _project.Variables["DBmode"].Value;

            if (_pstgr) _logger = new Logger(project, log: log, classEmoji: "🐘");
            else _logger = new Logger(project, log: log, classEmoji: "✒");
            _logShow = log;
        }
        public void Log(string query, string response = null, bool log = false)
        {
            if (!_logShow && !log) return;
            string dbMode = _project.Variables["DBmode"].Value;
            string toLog = null;


            if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                toLog += $"( ▼ ): [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
                if (!string.IsNullOrEmpty(response)) toLog += $"\n          [{response.Replace("\n", "|").Replace("\r", "")}]";
            }
            else
            {
                toLog += $"( ▲ ): [{Regex.Replace(query.Trim(), @"\s+", " ")}]";
            }
            _logger.Send(toLog);
            //_project.L0g(toLog);
        }

        public string TblName(string tableName, bool name = true)
        {
            string schemaName = "projects";
            if (_dbMode == "PostgreSQL")
            {
                if (tableName.Contains("."))
                {
                    schemaName = tableName.Split('.')[0];
                    tableName = tableName.Split('.')[1];
                }
                else if (tableName.Contains("_"))
                {
                    schemaName = tableName.Split('_')[0];
                    tableName = tableName.Split('_')[1];
                }

            }
            else if (_dbMode == "SQLite")
            {
                if (tableName.Contains(".")) tableName = tableName.Replace(".", "_");
            }

            _tableName = tableName;
            _schemaName = schemaName;

            if (name) return tableName;
            else return schemaName;
        }
        private string QuoteColumnNames(string updateString)
        {
            var parts = updateString.Split(',').Select(p => p.Trim()).ToList();
            var result = new List<string>();

            foreach (var part in parts)
            {
                int equalsIndex = part.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string columnName = part.Substring(0, equalsIndex).Trim();
                    string valuePart = part.Substring(equalsIndex).Trim();
                    result.Add($"\"{columnName}\" {valuePart}");
                }
                else
                {
                    result.Add(part);
                }
            }
            return string.Join(", ", result);
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
                    Log($"!W Err:[{ex.Message}]. debug:\n{query}");
                    if (throwOnEx) throw;
                }
            }
            else if (dbMode == "PostgreSQL")
            {
                try
                {
                    result = PostgresDB.DbQueryPostgre(_project, query, log, throwOnEx);
                }
                catch (Exception ex)
                {
                    Log($"!W Err:[{ex.Message}]. debug:\n{query}");
                    if (throwOnEx) throw;
                }
            }
            else throw new Exception($"unknown DBmode: {dbMode}");
            Log(query, result, log: log);
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


        public void Write(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;

            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            foreach (KeyValuePair<string, string> pair in toWrite)
            {
                string key = pair.Key.Replace("'", "''");
                string value = pair.Value.Replace("'", "''");
                string query = $@"INSERT INTO {_tableName} (key, value) VALUES ('{key}', '{value}')
	                  ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value;";
                DbQ(query, log: log);
            }

        }
        public void UpdTxt(string toUpd, string tableName, string key, bool log = false, bool throwOnEx = false)
        {
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            toUpd = toUpd.Trim().TrimEnd(',');
            DbQ($@"INSERT INTO {_tableName} (key) VALUES ('{key}') ON CONFLICT DO NOTHING;");
            DbQ($@"UPDATE {_tableName} SET {toUpd} WHERE key = '{key}';", log: log, throwOnEx: throwOnEx);

        }

        public void Upd(string toUpd, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, object acc = null)
        {

            int acc0 = 0;
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            if (acc != null)
            {
                if (acc is int i) acc0 = i;
                else if (acc is string s) int.TryParse(s, out acc0);
            }
            else int.TryParse(_project.Var("acc0"), out acc0);


            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            string[] keywords = { "blockchain", "browser", "cex_deps", "native", "profile", "settings" };
            if (keywords.Any(keyword => tableName.Contains(keyword))) last = false;

            toUpd = toUpd.Trim().TrimEnd(',');
            if (last) toUpd = toUpd + $", last = '{DateTime.UtcNow.ToString("MM-ddTHH:mm")}'";

            toUpd = QuoteColumnNames(toUpd);
            string query = $@"UPDATE {_tableName} SET {toUpd} WHERE acc0 = {acc0};";
            try { _project.Var("lastQuery", query); } catch { }
            DbQ(query, log: log, throwOnEx: throwOnEx);
        }
        public void Upd(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            int dicSize = toWrite.Count;
            AddRange(_tableName, dicSize);

            foreach (KeyValuePair<string, string> pair in toWrite)
            {
                string key = pair.Key;
                string value = pair.Value;
                Upd(value, _tableName, last: last, acc: key);
            }
        }
        public void Upd(List<string> toWrite, string columnName, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            int dicSize = toWrite.Count;
            AddRange(_tableName, dicSize);

            int key = 1;
            foreach (string valToWrite in toWrite)
            {

                Upd($"{columnName} = '{valToWrite.Replace("'", "''")}'", _tableName, last: last, acc: key);
                key++;
            }
        }



        public string Get(string toGet, string tableName = null, bool log = false, bool throwOnEx = false, string key = "acc0", string acc = null, string where = "")
        {
            toGet = QuoteColumnNames(toGet.Trim().TrimEnd(','));
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            if (string.IsNullOrEmpty(where))
            {
                if (string.IsNullOrEmpty(acc))
                    acc = _project.Variables["acc0"].Value;
                if (key == "acc0")
                    return DbQ($@"SELECT {toGet} from {_tableName} WHERE acc0 = {acc};", log: log, throwOnEx: throwOnEx);
                else
                    return DbQ($@"SELECT {toGet} from {_tableName} WHERE key = '{key}';", log: log, throwOnEx: throwOnEx);
            }
            else
                return DbQ($@"SELECT {toGet} from {_tableName} WHERE {where};", log: log, throwOnEx: throwOnEx);
        }

        public string GetRandom(string toGet, string tableName = null, bool log = false, bool acc = false, bool throwOnEx = false, int range = 0, bool single = true, bool invert = false)
        {
            if (range == 0) range = _project.Range();
            if (string.IsNullOrEmpty(tableName)) tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            string acc0 = string.Empty;
            if (acc) acc0 = "acc0, ";

            string query = $@"
                SELECT {acc0}{toGet.Trim().TrimEnd(',')} 
                from {_tableName} 
                WHERE TRIM({toGet}) != ''
	            AND acc0 < {range}
                ORDER BY RANDOM()";

            if (single) query += " LIMIT 1;";
            if (invert) query = query.Replace("!=", "=");

            return DbQ(query, log: log, throwOnEx: throwOnEx);
        }


        public string GetColumns(string tableName, bool log = false)
        {
            TblName(tableName);
            string Q;
            if (_pstgr) Q = $@"SELECT column_name FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}'";
            else Q = $@"SELECT name FROM pragma_table_info('{_tableName}')";
            return DbQ(Q, log: log).Replace("\n", ", ").Trim(',').Trim();
        }

        public List<string> GetColumnList(string tableName, bool log = false)
        {
            TblName(tableName);
            string Q;
            if (_pstgr) Q = $@"SELECT column_name FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}'";
            else Q = $@"SELECT name FROM pragma_table_info('{_tableName}')";

            return DbQ(Q, log: log).Split('\n').ToList();
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
        public void TblAdd(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            if (TblExist(tblName)) return;
            if (_pstgr) DbQ($@" CREATE TABLE {_schemaName}.{_tableName} ( {string.Join(", ", tableStructure.Select(kvp => $"\"{kvp.Key}\" {kvp.Value.Replace("AUTOINCREMENT", "SERIAL")}"))} );");
            else DbQ($"CREATE TABLE {_tableName} (" + string.Join(", ", tableStructure.Select(kvp => $"{kvp.Key} {kvp.Value}")) + ");");
        }
        public List<string> TblColumns(string tblName)
        {
            var result = new List<string>();
            TblName(tblName);
            if (_dbMode == "PostgreSQL")
                result = DbQ($@"SELECT column_name FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}';", log: _logShow)
                    .Split('\n')
                    .Select(s => s.Trim())
                    .ToList();
            else
                result = DbQ($"SELECT name FROM pragma_table_info('{_tableName}');", log: _logShow)
                    .Split('\n')
                    .Select(s => s.Trim())
                    .ToList();
            if (_dbMode == "PostgreSQL") _tableName = $"{_schemaName}.{_tableName}";
            return result;
        }
        public Dictionary<string, string> TblMapForProject(string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''")
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
            if (!string.IsNullOrEmpty(dynamicToDo))
            {
                //dynamicToDo = dynamicToDo.ToLower();
                string[] toDoItems = (dynamicToDo ?? "").Split(',');
                foreach (string taskId in toDoItems)
                {
                    string trimmedTaskId = taskId.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedTaskId) && !tableStructure.ContainsKey(trimmedTaskId))
                    {
                        tableStructure.Add(trimmedTaskId, defaultType);
                    }
                }
            }
            return tableStructure;
        }


        public bool ClmnExist(string tblName, string clmnName)
        {
            TblName(tblName);
            string resp = null;
            if (_pstgr)
                resp = DbQ($@"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = '{_schemaName}' AND table_name = '{_tableName}' AND lower(column_name) = lower('{clmnName}');", log: _logShow)?.Trim();
            else
                resp = DbQ($"SELECT COUNT(*) FROM pragma_table_info('{_tableName}') WHERE name='{clmnName}';", log: _logShow);
            if (resp == "0" || resp == string.Empty) return false;
            else return true;

        }
        public void ClmnAdd(string tblName, string clmnName, string defaultValue = "TEXT DEFAULT ''")
        {
            TblName(tblName);
            var current = TblColumns(tblName);
            if (!current.Contains(clmnName))
            {
                clmnName = QuoteColumnNames(clmnName);
                DbQ($@"ALTER TABLE {_tableName} ADD COLUMN {clmnName} {defaultValue};", log: _logShow);
            }

        }
        public void ClmnAdd(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            var current = TblColumns(tblName);

            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            Log(string.Join(",", current));
            foreach (var column in tableStructure)
            {
                var keyWd = column.Key.Trim();
                if (!current.Contains(keyWd))
                {
                    Log($"CLMNADD [{keyWd}] not in  [{string.Join(",", current)}] ");
                    DbQ($@"ALTER TABLE {_tableName} ADD COLUMN {keyWd} {column.Value};", log: _logShow);
                }
            }
        }
        public void ClmnDrop(string tblName, string clmnName)
        {
            TblName(tblName);
            var current = TblColumns(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            if (current.Contains(clmnName))
            {
                string cascade = (_pstgr) ? " CASCADE" : null;
                DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {clmnName}{cascade};", log: _logShow);
            }
        }
        public void ClmnDrop(string tblName, Dictionary<string, string> tableStructure)
        {
            TblName(tblName);
            var current = TblColumns(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            foreach (var column in tableStructure)
            {
                if (!current.Contains(column.Key))
                {
                    string cascade = _dbMode == "PostgreSQL" ? " CASCADE" : null;
                    DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {column.Key}{cascade};", log: _logShow);
                }
            }
        }
        public void ClmnPrune(string tblName, Dictionary<string, string> tableStructure)
        {

            TblName(tblName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            var current = TblColumns(tblName);

            foreach (var column in current)
            {
                if (!tableStructure.ContainsKey(column))
                {
                    Log($"[{column}] not in tableStructure keys, dropping");
                    string cascade = _pstgr ? " CASCADE" : "";
                    DbQ($@"ALTER TABLE {_tableName} DROP COLUMN {column}{cascade};", log: _logShow);
                }
            }
        }
        public void AddRange(string tblName, int range = 0)
        {
            if (range == 0)
                try
                {
                    range = int.Parse(_project.Variables["rangeEnd"].Value);
                }
                catch
                {
                    Log("var  rangeEnd is empty or 0, fallback to 100");
                    range = 100;
                }

            TblName(tblName);

            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            int current = int.Parse(DbQ($@"SELECT COALESCE(MAX(acc0), 0) FROM {_tableName};"));
            Log(current.ToString());
            Log(range.ToString());
            for (int currentAcc0 = current + 1; currentAcc0 <= range; currentAcc0++)
            {
                DbQ($@"INSERT INTO {_tableName} (acc0) VALUES ({currentAcc0}) ON CONFLICT DO NOTHING;");
            }

        }


        public string Proxy()
        {
            TblName("private_profile");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = Get("proxy", _tableName);
            _project.Variables["proxy"].Value = resp;
            return resp;
        }
        public string Bio()
        {
            TblName("public_profile"); if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = DbQ($@"SELECT nickname, bio FROM {_tableName} WHERE acc0 = {_project.Variables["acc0"].Value};");
            string[] respData = resp.Split('|');
            _project.Variables["accNICKNAME"].Value = respData[0].Trim();
            _project.Variables["accBIO"].Value = respData[1].Trim();
            return resp;
        }
        public Dictionary<string, string> Settings(bool set = true)
        {
            var dbConfig = new Dictionary<string, string>();

            TblName("private_settings");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = DbQ($"SELECT key, value FROM {_tableName}");
            foreach (string varData in resp.Split('\n'))
            {
                string varName = varData.Split('|')[0];
                string varValue = varData.Split('|')[1].Trim();
                dbConfig.Add(varName, varValue);

                if (set)
                {
                    try { _project.Var(varName, varValue); }
                    catch (Exception e) { Log(e.Message); }
                }
            }
            return dbConfig;

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

        public string Ref(string refCode = null, bool log = false)
        {
            if (string.IsNullOrEmpty(refCode)) refCode = _project.Variables["cfgRefCode"].Value;

            TblName(_project.Variables["projectTable"].Value);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            if (string.IsNullOrEmpty(refCode) || refCode == "_") refCode = DbQ($@"SELECT refcode FROM {_tableName}
			WHERE refcode != '_' 
			AND TRIM(refcode) != ''
			ORDER BY RANDOM()
			LIMIT 1;", log);
            return refCode;
        }

        public Dictionary<string, string> GetAddresses(string chains = null)
        {
            var addrss = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(chains)) chains = GetColumns("public_blockchain");
            string[] tikers = chains.Replace(" ", "").Split(',');
            string[] addresses = Get(chains, "public_blockchain").Replace(" ", "").Split('|');


            for (int i = 0; i < tikers.Length; i++)
            {
                var tiker = tikers[i].Trim();
                var address = addresses[i].Trim();
                addrss.Add(tiker, address);
            }

            return addrss;

        }
        public List<string> MkToDoQueries(string toDo = null, string defaultRange = null, string defaultDoFail = null)
        {
            string tableName = _project.Variables["projectTable"].Value;
            TblName(tableName);
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";


            var nowIso = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            if (string.IsNullOrEmpty(toDo)) toDo = _project.Variables["cfgToDo"].Value;

            string[] toDoItems = (toDo ?? "").Split(',');

            var allQueries = new List<string>();

            foreach (string taskId in toDoItems)
            {
                string trimmedTaskId = taskId.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedTaskId))
                {

                    string range = defaultRange ?? _project.Variables["range"].Value;
                    string doFail = defaultDoFail ?? _project.Variables["doFail"].Value;
                    string failCondition = (doFail != "True" ? "AND status NOT LIKE '%fail%'" : "");
                    string query = $@"SELECT acc0 FROM {_tableName} WHERE acc0 in ({range}) {failCondition} AND status NOT LIKE '%skip%' 
                AND ({trimmedTaskId} < '{nowIso}' OR {trimmedTaskId} = '_')";
                    allQueries.Add(query);
                }
            }

            return allQueries;
        }
        public void FilterAccList(List<string> dbQueries, bool log = false)
        {
            if (!string.IsNullOrEmpty(_project.Variables["acc0Forced"].Value))
            {
                _project.Lists["accs"].Clear();
                _project.Lists["accs"].Add(_project.Variables["acc0Forced"].Value);
                Log($@"manual mode on with {_project.Variables["acc0Forced"].Value}");
                return;
            }

            var allAccounts = new HashSet<string>();
            foreach (var query in dbQueries)
            {
                try
                {
                    var accsByQuery = DbQ(query).Trim();
                    if (!string.IsNullOrWhiteSpace(accsByQuery))
                    {
                        var accounts = accsByQuery.Split('\n').Select(x => x.Trim().TrimStart(','));
                        allAccounts.UnionWith(accounts);
                    }
                }
                catch
                {

                    Log($"{query}");
                }
            }

            if (allAccounts.Count == 0)
            {
                _project.Variables["noAccsToDo"].Value = "True";
                Log($"♻ noAccountsAvailable by queries [{string.Join(" | ", dbQueries)}]");
                return;
            }
            Log($"Initial availableAccounts: [{string.Join(", ", allAccounts)}]");

            if (!string.IsNullOrEmpty(_project.Variables["requiredSocial"].Value))
            {
                string[] demanded = _project.Variables["requiredSocial"].Value.Split(',');
                Log($"Filtering by socials: [{string.Join(", ", demanded)}]");

                foreach (string social in demanded)
                {
                    string tableName = TblName($"private_{social.Trim().ToLower()}");

                    //TblName(tableName);
                    //if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
                    var notOK = Get($"acc0", _tableName, where: "status NOT LIKE '%ok%'", log: log)

                    //var notOK = DbQ($"SELECT acc0 FROM {_tableName} WHERE status NOT LIKE '%ok%'", log)
                        .Split('\n')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x));
                    allAccounts.ExceptWith(notOK);
                    Log($"After {social} filter: [{string.Join("|", allAccounts)}]");
                }
            }
            _project.Lists["accs"].Clear();
            _project.Lists["accs"].AddRange(allAccounts);
            Log($"final list [{string.Join("|", _project.Lists["accs"])}]");
        }

        public string Address(string chainType = "evm")
        {
            chainType = chainType.ToLower().Trim();

            TblName("public_blockchain");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";
            var resp = Get(chainType, _tableName);

            try
            {
                chainType = chainType.ToUpper();
                _project.Variables[$"address{chainType}"].Value = resp;
            }
            catch { }
            return resp;

        }
        public string Key(string chainType = "evm")
        {
            chainType = chainType.ToLower().Trim();
            TblName("private_blockchain");
            if (_pstgr) _tableName = $"{_schemaName}.{_tableName}";

            switch (chainType)
            {
                case "evm":
                    chainType = "secp256k1";
                    break;
                case "sol":
                    chainType = "base58";
                    break;
                case "seed":
                    chainType = "bip39";
                    break;
                default:
                    throw new Exception("unexpected input. Use (evm|sol|seed|pkFromSeed)");
            }

            var resp = Get(chainType, _tableName);
            if (!string.IsNullOrEmpty(_project.Var("cfgPin")))
                return SAFU.Decode(_project, resp);
            else return resp;


        }

    }


}


