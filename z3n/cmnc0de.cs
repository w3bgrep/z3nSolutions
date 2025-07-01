using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using ZennoLab.InterfacesLibrary.Enums.Browser;
using System.Globalization;
using System.Runtime.CompilerServices;
using Leaf.xNet;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Security.Policy;
using ZennoLab.InterfacesLibrary;
using z3n;
using NBitcoin;

using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Numerics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;
using Newtonsoft.Json.Linq;
using System.Dynamic;

using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;

using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI;
using Nethereum.Util;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;


namespace w3tools //by @w3bgrep
{

    public  static class TestStatic
    {

        public static string UnixToHuman(this IZennoPosterProjectModel project, string decodedResultExpire = null)
        {
            var _log = new Logger(project, classEmoji: "☻");
            if (string.IsNullOrEmpty(decodedResultExpire)) decodedResultExpire = project.Var("varSessionId");
            if (!string.IsNullOrEmpty(decodedResultExpire))
            {
                int intEpoch = int.Parse(decodedResultExpire);
                string converted = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(intEpoch).ToShortDateString();
                _log.Send(converted);
                return converted;

                
            }
            return string.Empty;
        }
        public static decimal Math(this IZennoPosterProjectModel project, string varA, string operation, string varB, string varRslt = "a_")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            decimal a = decimal.Parse(project.Var(varA));
            decimal b = decimal.Parse(project.Var(varB));
            decimal result;
            switch (operation)
            {
                case "+":

                    result = a + b;
                    break;
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    break;
                default:
                    throw new Exception($"unsuppoted operation {operation}");
            }
            try { project.Var(varRslt, $"{result}"); } catch { }
            return result;
        }
        public static string CookiesToJson(string cookies)
        {
            try
            {
                if (string.IsNullOrEmpty(cookies))
                {
                    return "[]";
                }

                var result = new List<Dictionary<string, string>>();
                var cookiePairs = cookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in cookiePairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                        continue;

                    var keyValue = trimmedPair.Split(new[] { '=' }, 2);
                    if (keyValue.Length != 2)
                    {
                        continue;
                    }

                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        result.Add(new Dictionary<string, string>
                    {
                        { "name", key },
                        { "value", value }
                    });
                    }
                }

                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                return json;
            }
            catch (Exception ex)
            {
                return "[]";
            }
        }

        public static void Sleep(this IZennoPosterProjectModel project,int min, int max)
        {
            Thread.Sleep(new Random().Next(min, min) * 1000);

        }


    }

    public class Rnd2
    {


        public string Seed()
        {
            return Blockchain.GenerateMnemonic("English", 12);
        }
        public string RandomHex(int length)
        {
            const string chars = "0123456789abcdef";
            var random = new Random();
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return "0x" + new string(result);
        }
        public string RandomHash(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public string Nickname()
        {
            string[] adjectives = {
                "Sunny", "Mystic", "Wild", "Cosmic", "Shadow", "Lunar", "Blaze", "Dream", "Star", "Vivid",
                "Frost", "Neon", "Gloomy", "Swift", "Silent", "Fierce", "Radiant", "Dusk", "Nova", "Spark",
                "Crimson", "Azure", "Golden", "Midnight", "Velvet", "Stormy", "Echo", "Vortex", "Phantom", "Bright",
                "Chill", "Rogue", "Daring", "Lush", "Savage", "Twilight", "Crystal", "Zesty", "Bold", "Hazy",
                "Vibrant", "Gleam", "Frosty", "Wicked", "Serene", "Bliss", "Rusty", "Hollow", "Sleek", "Pale"
            };

            string[] nouns = {
                "Wolf", "Viper", "Falcon", "Spark", "Catcher", "Rider", "Echo", "Flame", "Voyage", "Knight",
                "Raven", "Hawk", "Storm", "Tide", "Drift", "Shade", "Quest", "Blaze", "Wraith", "Comet",
                "Lion", "Phantom", "Star", "Cobra", "Dawn", "Arrow", "Ghost", "Sky", "Vortex", "Wave",
                "Tiger", "Ninja", "Dreamer", "Seeker", "Glider", "Rebel", "Spirit", "Hunter", "Flash", "Beacon",
                "Jaguar", "Drake", "Scout", "Path", "Glow", "Riser", "Shadow", "Bolt", "Zephyr", "Forge"
            };

            string[] suffixes = { "", "", "", "", "", "X", "Z", "Vibe", "Glow", "Rush", "Peak", "Core", "Wave", "Zap" };

            Random random = new Random(Guid.NewGuid().GetHashCode());

            string adjective = adjectives[random.Next(adjectives.Length)];
            string noun = nouns[random.Next(nouns.Length)];
            string suffix = suffixes[random.Next(suffixes.Length)];

            string nickname = $"{adjective}{noun}{suffix}";

            if (nickname.Length > 15)
            {
                nickname = nickname.Substring(0, 15);
            }

            return nickname;
        }
        public int Int(IZennoPosterProjectModel project, string Var)
        {
            string value = string.Empty;
            try
            {
                value = project.Variables[Var].Value;
            }
            catch (Exception e)
            {
                project.SendInfoToLog(e.Message);
            }
            if (value == string.Empty) project.L0g($"no Value from [{Var}] `w");

            if (value.Contains("-"))
            {
                var min = int.Parse(value.Split('-')[0].Trim());
                var max = int.Parse(value.Split('-')[1].Trim());
                return new Random().Next(min, max);
            }
            return int.Parse(value.Trim());
        }

        public double RndPercent(double input, double percent, double maxPercent)
        {
            if (percent < 0 || maxPercent < 0 || percent > 100 || maxPercent > 100)
                throw new ArgumentException("Percent and MaxPercent must be between 0 and 100");

            if (!double.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                throw new ArgumentException("Input cannot be converted to double");

            double percentageValue = number * (percent / 100.0);

            Random random = new Random();
            double randomReductionPercent = random.NextDouble() * maxPercent;
            double reduction = percentageValue * (randomReductionPercent / 100.0);

            double result = percentageValue - reduction;

            if (result <= 0)
            {
                result = Math.Max(percentageValue * 0.01, 0.0001);
            }

            return result;
        }
        public double RndPercent(decimal input, double percent, double maxPercent)
        {
            if (percent < 0 || maxPercent < 0 || percent > 100 || maxPercent > 100)
                throw new ArgumentException("Percent and MaxPercent must be between 0 and 100");

            if (!double.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                throw new ArgumentException("Input cannot be converted to double");

            double percentageValue = number * (percent / 100.0);

            Random random = new Random();
            double randomReductionPercent = random.NextDouble() * maxPercent;
            double reduction = percentageValue * (randomReductionPercent / 100.0);

            double result = percentageValue - reduction;

            if (result <= 0)
            {
                result = Math.Max(percentageValue * 0.01, 0.0001);
            }

            return result;
        }

        public decimal Decimal(IZennoPosterProjectModel project, string Var)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string value = string.Empty;
            try
            {
                value = project.Variables[Var].Value;
            }
            catch (Exception e)
            {
                project.SendInfoToLog(e.Message);
            }
            if (value == string.Empty) project.L0g($"no Value from [{Var}] `w");

            if (value.Contains("-"))
            {
                var min = decimal.Parse(value.Split('-')[0].Trim());
                var max = decimal.Parse(value.Split('-')[1].Trim());
                Random rand = new Random();
                return min + (decimal)(rand.NextDouble() * (double)(max - min));
            }
            return decimal.Parse(value.Trim());
        }

    }

    public class CommonTx2 : W3b
    {

        private readonly W3bRead _read;
        public CommonTx2(IZennoPosterProjectModel project, string key = null, bool log = false)
        : base(project, log)
        {
            _key = ApplyKey(key);
            _adrEvm = _key.ToPubEvm();//_sql.Address("evm");
            _read = new W3bRead(project);
        }



        public string Approve(string contract, string spender, string amount, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;
            string key = _sql.Key("EVM");

            string abi = @"[{""inputs"":[{""name"":""spender"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""}]";

            string txHash = null;

            string[] types = { "address", "uint256" };
            BigInteger amountValue;


            if (amount.ToLower() == "max")
            {
                amountValue = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935"); // max uint256
            }
            else if (amount.ToLower() == "cancel")
            {
                amountValue = BigInteger.Zero;
            }
            else
            {
                try
                {
                    amountValue = BigInteger.Parse(amount);
                    if (amountValue < 0)
                        throw new ArgumentException("Amount cannot be negative");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to parse amount '{amount}': {ex.Message}");
                }
            }

            object[] values = { spender, amountValue };

            try
            {
                txHash = SendLegacy(
                    rpc,
                    contract,
                    z3n.Encoder.EncodeTransactionData(abi, "approve", types, values),
                    0,
                    key,
                    3
                );
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    Log($"!W:{ex.Message}");
                }

            }
            catch (Exception ex)
            {
                Log($"!W:{ex.Message}");
                throw;
            }

            Log($"[APPROVE] {contract} for spender {spender} with amount {amount}...");
            return txHash;
        }
        public string Wrap(string contract, decimal value, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;
            string key = _sql.Key("EVM");

            string abi = @"[{""inputs"":[],""name"":""deposit"",""outputs"":[],""stateMutability"":""payable"",""type"":""function""}]";

            string txHash = null;

            string[] types = { };
            object[] values = { };

            try
            {
                txHash = SendLegacy(
                    rpc,
                    contract,
                     z3n.Encoder.EncodeTransactionData(abi, "deposit", types, values),
                    value,
                    key,
                    3
                );
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    Log($"!W:{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Log($"!W:{ex.Message}");
                throw;
            }

            Log($"[WRAP] {value} native to {contract}...");
            return txHash;
        }
        public string Send(string to, decimal amount, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;

            string txHash = null;

            try
            {
                txHash = SendLegacy(
                    rpc,
                    to,
                    "",
                    amount,
                    _key,
                    3
                );
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    Log($"!W:{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Log($"!W:{ex.Message}");
                throw;
            }

            Log($"[SEND_NATIVE] {amount} to {to}...");
            return txHash;
        }

        public string SendERC20(string contract, string to, decimal amount, string rpc = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (string.IsNullOrEmpty(rpc)) rpc = _read._defRpc;

            string txHash = null;

            try
            {

                string abi = @"[{""inputs"":[{""name"":""to"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":"""",""type"":""bool""}],""stateMutability"":""nonpayable"",""type"":""function""}]";
                string[] types = { "address", "uint256" };
                decimal scaledAmount = amount * 1000000000000000000m;
                BigInteger amountValue = (BigInteger)Math.Floor(scaledAmount);
                object[] values = { to, amountValue };
                string encoded = z3n.Encoder.EncodeTransactionData(abi, "transfer", types, values);
                txHash = SendLegacy(
                    rpc,
                    contract,
                     encoded,
                    0,
                    _key,
                    3
                );
                try
                {
                    _project.Variables["blockchainHash"].Value = txHash;
                }
                catch (Exception ex)
                {
                    _logger.Send($"!W:{ex.Message}", show: true);
                }
            }
            catch (Exception ex)
            {
                _logger.Send($"!W:{ex.Message}", show: true);
                throw;
            }

            _logger.Send($"sent [{amount}] of [{contract}]  to {to} by {rpc}\n{txHash}");
            return txHash;
        }

    }



    public class Accountant
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Sql _sql;
        private readonly Logger _logger;
        private int _offset; 

        public Accountant(IZennoPosterProjectModel project,  bool log = false)
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


    public class BinanceApi2
    {


        private readonly IZennoPosterProjectModel _project;
        private readonly Logger _logger;

        private readonly bool _logShow;
        private readonly Sql _sql;


        private  string _apiKey;
        private  string _secretKey;
        private  string _proxy;

        public BinanceApi2(IZennoPosterProjectModel project, bool log = false)
        {
            _project = project;
            _logger = new Logger(project, log: log, classEmoji: "💸");
            LoadKeys();
        }
        
       
        public string Withdraw(string coin, string network, string address, string amount)
        {

            network = MapNetwork(network);
            string timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            string message = $"coin={coin}&network={network}&address={address}&amount={amount}&timestamp={timestamp}";
            string signature = CalculateHmacSha256Signature(message);
            string payload = $"coin={coin}&network={network}&address={address}&amount={amount}&timestamp={timestamp}&signature={signature}";
            string url = "https://api.binance.com/sapi/v1/capital/withdraw/apply";

            var result = Post(url, payload);
            return result;

        }

        public Dictionary<string,string> GetUserAsset()
        {
            string url = "https://api.binance.com/sapi/v3/asset/getUserAsset";
            string message = $"timestamp={DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}";
            string signature = CalculateHmacSha256Signature(message);
            string payload = $@"{message}&signature={signature}";
            var result = Post(url, payload);

            _project.Json.FromString(result);

            var balances = new Dictionary<string, string>();
            foreach (var item in _project.Json)
            {
                string asset = item.asset;
                string free = item.free;
                balances.Add(asset,free);
            }
            return balances;
        }
        public string GetUserAsset(string coin)
        {
            return GetUserAsset()[coin];
        }

        public List<string> GetWithdrawHistory()
        {

            string url =  "https://api.binance.com/sapi/v1/capital/withdraw/history";
            string message = $"timestamp={DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}";
            string signature = CalculateHmacSha256Signature(message);
            string payload = $"{message}&signature={signature}";
            url = url + payload;


            string response = Get(url);


            _project.Json.FromString(response);

            var historyList = new List<string>();
            foreach (var item in _project.Json)
            {
                string id = item.id;
                string amount = item.amount;
                string coin = item.coin;
                string status = item.status.ToString();
                historyList.Add($"{id}:{amount}:{coin}:{status}");
            }
            return historyList;
        }
        public string GetWithdrawHistory(string searchId = "")
        {
            var historyList = GetWithdrawHistory();
          
            foreach (string withdrawal in historyList)
            {
                if (withdrawal.Contains(searchId))
                    return withdrawal;
            }
            return $"NoIdFound: {searchId}";
        }

        private string MapNetwork(string chain)
        {
            chain = chain.ToLower();
            switch (chain)
            {
                case "arbitrum": return "ARBITRUM";
                case "ethereum": return "ETH";
                case "base": return "BASE";
                case "bsc": return "BSC";
                case "avalanche": return "AVAXC";
                case "polygon": return "MATIC";
                case "optimism": return "OPTIMISM";
                case "trc20": return "TRC20";
                case "zksync": return "ZkSync";
                case "aptos": return "APT";
                default:
                    throw new ArgumentException("Unsupported network: " + chain);
            }
        }
        private string CalculateHmacSha256Signature(string message)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            using (var hmacSha256 = new HMACSHA256(keyBytes))
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hashBytes = hmacSha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        private string Post(string url, string payload)
        {
            var result = ZennoPoster.HTTP.Request(
                ZennoLab.InterfacesLibrary.Enums.Http.HttpMethod.POST,
                url, // url
                payload,
                "application/x-www-form-urlencoded; charset=utf-8",
                _proxy,
                "UTF-8",
                ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.BodyOnly,
                10000,
                "",
                _project.Profile.UserAgent,
                true,
                5,
                new string[] {
                    "X-MBX-APIKEY: "+ _apiKey,
                    "Content-Type: application/x-www-form-urlencoded; charset=utf-8"
                },
                "",
                false,
                false,
                _project.Profile.CookieContainer
                );
                return result;
        }
        private string Get(string url)
        {

            string result = ZennoPoster.HttpGet(
                            url,
                            _proxy,
                            "UTF-8",
                            ResponceType.BodyOnly,
                            30000,
                            "",
                            "Mozilla/4.0",
                            true,
                            5,
                            new string[] {
                            "X-MBX-APIKEY: "+_apiKey,
                            "Content-Type: application/x-www-form-urlencoded; charset=utf-8"
                            },
                            "",
                            true
                        );
            _logger.Send($"json received: [{result}]");
            _project.Json.FromString(result);

            return result;
        }
        private void LoadKeys()
        {
            var creds = new Sql(_project).Get("apikey, apisecret, proxy", "private_api", where: "key = 'binance'").Split('|');


            _apiKey = creds[0];
            _secretKey = creds[1];
            _proxy = creds[2];


        }

    }






}


