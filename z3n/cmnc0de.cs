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

    public class FirstMail
    {

        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;

        private string _key;
        private string _login;
        private string _pass;

        public FirstMail(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logger = new Logger(project, log: log, classEmoji: "FirstMail");
            LoadKeys();
        }

        private void LoadKeys()
        {
            var creds = new Sql(_project).Get("apikey, apisecret, passphrase", "private_api", where: "key = 'firstmail'").Split('|');

            _key = creds[0];
            _login = creds[1];
            _pass = creds[2];

        }



        public string GetMail(string email)
        {
            string encodedLogin = Uri.EscapeDataString(_login);
            string encodedPass = Uri.EscapeDataString(_pass);

            string url = $"https://api.firstmail.ltd/v1/mail/one?username={encodedLogin}&password={encodedPass}";

            string[] headers = new string[]
            {
                $"accept: application/json",
                "accept-encoding: gzip, deflate, br",
                $"accept-language: {_project.Profile.AcceptLanguage}",
                "sec-ch-ua-mobile: ?0",
                "sec-ch-ua-platform: \"Windows\"",
                "sec-fetch-dest: document",
                "sec-fetch-mode: navigate",
                "sec-fetch-site: none",
                "sec-fetch-user: ?1",
                "upgrade-insecure-requests: 1",
                $"X-API-KEY: {_key}"
            };

            string result = ZennoPoster.HttpGet(
                url,
                "",
                "UTF-8",
                ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                5000,
                "",
                _project.Profile.UserAgent,
                true,
                5,
                headers,
                "",
                false);

            _project.Json.FromString(result);
            return result;

        }

        public string GetOTP(string email)
        {
            
            string json = GetMail(email);
            _project.Json.FromString(json);
            string deliveredTo = _project.Json.to[0];
            string text = _project.Json.text;
            string html = _project.Json.html;


            if (!deliveredTo.Contains(email)) throw new Exception($"Fmail: Email {email} not found in last message");
            else
            {
                Match match = Regex.Match(text, @"\b\d{6}\b");
                if (match.Success) return match.Value;
                match = Regex.Match(html, @"\b\d{6}\b");
                if (match.Success) return match.Value;
                else throw new Exception("Fmail: OTP not found in message with correct email");
            }

        }

    }





}


