using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
{
    public interface IPostgresConnection : IDisposable
    {
        void Open();
        string Read(string sql, string separator = "|");
        int Write(string sql, params NpgsqlParameter[] parameters);
        bool IsConnected { get; }
    }

    public class Postgres : IPostgresConnection
    {
        private NpgsqlConnection _connection;
        private readonly string _connectionString;
        private bool _disposed = false;

        public bool IsConnected => _connection?.State == ConnectionState.Open;

        public Postgres(string host, string database, string user, string password)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host cannot be null or empty", nameof(host));
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentException("Database cannot be null or empty", nameof(database));
            if (string.IsNullOrWhiteSpace(user))
                throw new ArgumentException("User cannot be null or empty", nameof(user));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var hostPort = ParseHostPort(host, 5432);
            var hostname = hostPort.Item1;
            var port = hostPort.Item2;
            _connectionString = $"Host={hostname};Port={port};Database={database};Username={user};Password={password};Pooling=true;Connection Lifetime=0;";
        }

        private static Tuple<string, int> ParseHostPort(string input, int defaultPort)
        {
            var parts = input.Split(':');
            if (parts.Length == 2)
            {
                int port;
                if (int.TryParse(parts[1], out port))
                {
                    return Tuple.Create(parts[0], port);
                }
            }
            return Tuple.Create(input, defaultPort);
        }

        public void Open()
        {
            try
            {
                _connection?.Dispose();
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }
            catch (Exception ex)
            {
                _connection?.Dispose();
                _connection = null;
                throw new InvalidOperationException($"Database connection failed: {ex.Message}", ex);
            }
        }

        private void EnsureConnection()
        {
            if (_connection?.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection is not open. Call Open() first.");
        }

        public string Read(string sql, string separator = "|")
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL query cannot be null or empty", nameof(sql));

            EnsureConnection();
            var result = new List<string>();

            using (var cmd = new NpgsqlCommand(sql, _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader[i]?.ToString() ?? string.Empty;
                    }
                    result.Add(string.Join(separator, row));
                }
            }

            return string.Join(Environment.NewLine, result);
        }

        public int Write(string sql, params NpgsqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL query cannot be null or empty", nameof(sql));

            EnsureConnection();

            using (var cmd = new NpgsqlCommand(sql, _connection))
            {
                if (parameters?.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                try
                {
                    return cmd.ExecuteNonQuery();
                }
                catch (NpgsqlException ex)
                {
                    throw new InvalidOperationException($"SQL Error: {ex.Message} | Query: [{sql}]", ex);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _connection?.Close();
                }
                catch
                {
                    // Игнорируем ошибки при закрытии
                }
                finally
                {
                    _connection?.Dispose();
                    _disposed = true;
                }
            }
        }
    }

    public static class PostgresHelpers
    {
        public static string ExecuteQuery(
            string query,
            string host = "localhost:5432",
            string dbName = "postgres",
            string dbUser = "postgres",
            string dbPassword = "",
            bool throwOnError = false)
        {
            if (string.IsNullOrWhiteSpace(dbPassword))
                throw new ArgumentException("PostgreSQL password cannot be null or empty");

            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty");

            using (var db = new Postgres(host, dbName, dbUser, dbPassword))
            {
                try
                {
                    db.Open();

                    if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        return db.Read(query);
                    }
                    else
                    {
                        var result = db.Write(query);
                        return result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    if (throwOnError) throw;
                    return $"{ex.Message}\n[{query}]";
                }
            }
        }

        public static string Raw(string query, bool throwOnEx = false, string host = "localhost:5432",
            string dbName = "postgres", string dbUser = "postgres", string dbPswd = "")
        {
            return ExecuteQuery(query, host, dbName, dbUser, dbPswd, throwOnEx);
        }
    }

    public class PostgresTableManager
    {
        private readonly IPostgresConnection _connection;

        public PostgresTableManager(IPostgresConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public bool TableExists(string schemaName, string tableName)
        {
            var query = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schema AND table_name = @table";
            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@schema", schemaName),
                new NpgsqlParameter("@table", tableName)
            };

            var result = _connection.Read(query);
            int count;
            return int.TryParse(result?.Trim(), out count) && count > 0;
        }

        public void CreateTable(string schemaName, string tableName, Dictionary<string, string> columns)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentException("Schema name cannot be null or empty");
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty");
            if (columns == null || !columns.Any())
                throw new ArgumentException("Columns cannot be null or empty");

            var columnsDefinition = string.Join(", ",
                columns.Select(kvp => $"\"{kvp.Key}\" {kvp.Value.Replace("AUTOINCREMENT", "SERIAL")}"));

            // Используем параметризованный запрос для безопасности
            var query = $"CREATE TABLE IF NOT EXISTS \"{schemaName}\".\"{tableName}\" ({columnsDefinition})";

            _connection.Write(query);
        }

        public List<string> GetTableColumns(string schemaName, string tableName)
        {
            var query = "SELECT column_name FROM information_schema.columns WHERE table_schema = @schema AND table_name = @table";
            var cmd = $"SELECT column_name FROM information_schema.columns WHERE table_schema = '{schemaName}' AND table_name = '{tableName}'";

            var result = _connection.Read(cmd);
            if (string.IsNullOrEmpty(result))
                return new List<string>();

            return result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(line => line.Split('|')[0])
                         .ToList();
        }

        public void AddColumn(string schemaName, string tableName, string columnName, string columnType)
        {
            // Проверяем существование колонки перед добавлением
            var checkQuery = $"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = '{schemaName}' AND table_name = '{tableName}' AND column_name = '{columnName}'";
            var exists = _connection.Read(checkQuery);

            int count;
            if (!int.TryParse(exists?.Trim(), out count) || count == 0)
            {
                var query = $"ALTER TABLE \"{schemaName}\".\"{tableName}\" ADD COLUMN \"{columnName}\" {columnType}";
                _connection.Write(query);
            }
        }

        public void DropColumn(string schemaName, string tableName, string columnName)
        {
            var query = $"ALTER TABLE \"{schemaName}\".\"{tableName}\" DROP COLUMN IF EXISTS \"{columnName}\" CASCADE";
            _connection.Write(query);
        }
    }

    public static class ImprovedPostgresUtils
    {
        public static string DbQueryPostgre(IZennoPosterProjectModel project, string query, bool throwOnEx = false,
            string host = "localhost:5432", string dbName = "postgres", string dbUser = "postgres", string dbPswd = "")
        {
            // Получаем параметры из проекта если они не переданы
            if (string.IsNullOrEmpty(dbName))
                dbName = project.Variables["DBpstgrName"]?.Value ?? "postgres";
            if (string.IsNullOrEmpty(dbUser))
                dbUser = project.Variables["DBpstgrUser"]?.Value ?? "postgres";

            if (string.IsNullOrEmpty(dbPswd))
            {
                dbPswd = project.Variables["DBpstgrPass"]?.Value;
                if (string.IsNullOrEmpty(dbPswd))
                    throw new Exception("PostgreSQL password isNull");
            }

            using (var db = new Postgres(host, dbName, dbUser, dbPswd))
            {
                try
                {
                    db.Open();

                    if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        return db.Read(query);
                    }
                    else
                    {
                        return db.Write(query).ToString();
                    }
                }
                catch (Exception ex)
                {
                    if (throwOnEx) throw;

                    project.SendWarningToLog(ex.Message);
                    return string.Empty;
                }
            }
        }

        public static void MkTablePostgre(IZennoPosterProjectModel project, Dictionary<string, string> tableStructure,
            string tableName = "", bool strictMode = false, bool insertData = true, string host = null,
            string dbName = "postgres", string dbUser = "postgres", string dbPswd = "",
            string schemaName = "projects", bool log = false)
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = project.Variables["projectTable"]?.Value ?? throw new ArgumentException("Table name is required");

            if (string.IsNullOrEmpty(dbPswd))
            {
                dbPswd = project.Variables["DBpstgrPass"]?.Value;
                if (string.IsNullOrEmpty(dbPswd))
                    throw new Exception("PostgreSQL password is null");
            }

            using (var db = new Postgres(host ?? "localhost:5432", dbName, dbUser, dbPswd))
            {
                try
                {
                    db.Open();
                    var tableManager = new PostgresTableManager(db);

                    // Парсим схему и таблицу если переданы через точку
                    var schema = schemaName;
                    var table = tableName;
                    if (tableName.Contains("."))
                    {
                        var parts = tableName.Split('.');
                        schema = parts[0];
                        table = parts[1];
                    }

                    // Создаем таблицу если не существует
                    if (!tableManager.TableExists(schema, table))
                    {
                        tableManager.CreateTable(schema, table, tableStructure);
                        if (log) project.SendInfoToLog($"Table {schema}.{table} created");
                    }

                    // Управляем колонками
                    ManageTableColumns(tableManager, schema, table, tableStructure, strictMode, project, log);

                    // Вставляем начальные данные если нужно
                    if (tableStructure.ContainsKey("acc0") && !string.IsNullOrEmpty(project.Variables["rangeEnd"]?.Value))
                    {
                        InsertInitialData(db, schema, table, project.Variables["rangeEnd"].Value, project, log);
                    }
                }
                catch (Exception ex)
                {
                    project.SendToLog($"Ошибка выполнения: {ex.Message}", LogType.Warning);
                    throw;
                }
            }
        }

        private static void ManageTableColumns(PostgresTableManager tableManager, string schemaName, string tableName,
            Dictionary<string, string> tableStructure, bool strictMode, IZennoPosterProjectModel project, bool log)
        {
            var currentColumns = tableManager.GetTableColumns(schemaName, tableName);

            if (strictMode)
            {
                var columnsToRemove = currentColumns.Where(col => !tableStructure.ContainsKey(col)).ToList();
                foreach (var column in columnsToRemove)
                {
                    tableManager.DropColumn(schemaName, tableName, column);
                    if (log) project.SendInfoToLog($"Column {column} dropped from {schemaName}.{tableName}");
                }
            }

            foreach (var column in tableStructure)
            {
                if (!currentColumns.Contains(column.Key, StringComparer.OrdinalIgnoreCase))
                {
                    tableManager.AddColumn(schemaName, tableName, column.Key, column.Value);
                    if (log) project.SendInfoToLog($"Column {column.Key} added to {schemaName}.{tableName}");
                }
            }
        }

        private static void InsertInitialData(Postgres db, string schemaName, string tableName, string cfgRangeEnd,
            IZennoPosterProjectModel project, bool log)
        {
            int rangeEnd;
            if (!int.TryParse(cfgRangeEnd, out rangeEnd) || rangeEnd <= 0)
                throw new ArgumentException("cfgRangeEnd must be a positive integer");

            var maxAcc0Query = $"SELECT COALESCE(MAX(acc0), 0) FROM \"{schemaName}\".\"{tableName}\"";
            var maxAcc0Str = db.Read(maxAcc0Query)?.Trim() ?? "0";

            int maxAcc0;
            if (!int.TryParse(maxAcc0Str, out maxAcc0))
            {
                maxAcc0 = 0;
                if (log) project.SendWarningToLog("Failed to parse max acc0, defaulting to 0");
            }

            if (maxAcc0 < rangeEnd)
            {
                for (int currentAcc0 = maxAcc0 + 1; currentAcc0 <= rangeEnd; currentAcc0++)
                {
                    var insertQuery = $"INSERT INTO \"{schemaName}\".\"{tableName}\" (acc0) VALUES ({currentAcc0}) ON CONFLICT DO NOTHING";
                    db.Write(insertQuery);
                }

                if (log) project.SendInfoToLog($"Inserted {rangeEnd - maxAcc0} new records");
            }
            else if (log)
            {
                project.SendInfoToLog($"No new data to insert. Max acc0 ({maxAcc0}) is >= rangeEnd ({rangeEnd})");
            }
        }
    }
}