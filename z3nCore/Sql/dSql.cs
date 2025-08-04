using Dapper;
using Microsoft.Data.Sqlite;
using Npgsql;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.Linq;

using System.Threading.Tasks;

namespace z3nCore
{
    public enum DatabaseType
    {
        Unknown,
        SQLite,
        PostgreSQL
    }

    public class dSql : IDisposable
    {
        private readonly IDbConnection _connection;
        private readonly string _tableName;
        private readonly Logger _logger;
        private bool _disposed = false;

        public dSql(string dbPath, string dbPass)
        {
            Debug.WriteLine(dbPath);
            _connection = new SqliteConnection($"Data Source={dbPath}");
            _connection.Open();
        }
        public dSql(string hostname, string port, string database, string user, string password)
        {            
            _connection = new NpgsqlConnection($"Host={hostname};Port={port};Database={database};Username={user};Password={password};Pooling=true;Connection Idle Lifetime=100;");
            _connection.Open();
        }
        public dSql(string connectionstring)
        {
            _connection = new NpgsqlConnection(connectionstring);
            _connection.Open();
        }
        public dSql(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }
        


        public DatabaseType ConnectionType
        {
            get
            {
                if (_connection is SqliteConnection)
                    return DatabaseType.SQLite;
                if (_connection is NpgsqlConnection)
                    return DatabaseType.PostgreSQL;
                return DatabaseType.Unknown;
            }
        }

        private void EnsureConnection()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(dSql));

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
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

        public async Task<string> DbReadAsync(string sql, string separator = "|")
        {
            EnsureConnection();
            var result = new List<string>();

            if (_connection is SqliteConnection sqliteConn)
            {
                using (var cmd = new SqliteCommand(sql, sqliteConn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row.Add(reader[i]?.ToString() ?? "");
                        result.Add(string.Join(separator, row));
                    }
                }
            }
            else if (_connection is NpgsqlConnection npgsqlConn)
            {
                using (var cmd = new NpgsqlCommand(sql, npgsqlConn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row.Add(reader[i]?.ToString() ?? "");
                        result.Add(string.Join(separator, row));
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported connection type");
            }

            return string.Join("\r\n", result);
        }  
        public string DbRead(string sql, string separator = "|")
        {
            return DbReadAsync(sql, separator).GetAwaiter().GetResult();
        }

        public async Task<int> DbWriteAsync(string sql, params IDbDataParameter[] parameters)
        {
            EnsureConnection();

            try
            {
                if (_connection is SqliteConnection sqliteConn)
                {
                    using (var cmd = new SqliteCommand(sql, sqliteConn))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                                cmd.Parameters.Add(param);
                        }
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
                else if (_connection is NpgsqlConnection npgsqlConn)
                {
                    using (var cmd = new NpgsqlCommand(sql, npgsqlConn))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                                cmd.Parameters.Add(param);
                        }
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    throw new NotSupportedException("Unsupported connection type");
                }
            }
            catch (Exception ex)
            {
                string formattedQuery = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        var value = param.Value?.ToString() ?? "NULL";
                        formattedQuery = formattedQuery.Replace(param.ParameterName, $"'{value}'");
                    }
                }

                Debug.WriteLine($"Error: {ex.Message}");
                Debug.WriteLine($"Executed query: {formattedQuery}");
                throw new Exception($"SQL Error: {ex.Message} | Query: [{sql}]");
            }
        }
        public int DbWrite(string sql, params IDbDataParameter[] parameters)
        {
            return DbWriteAsync(sql, parameters).GetAwaiter().GetResult();
        }

        public IDbDataParameter CreateParameter(string name, object value)
        {
            if (_connection is SqliteConnection)
            {
                return new SqliteParameter(name, value ?? DBNull.Value);
            }
            else if (_connection is NpgsqlConnection)
            {
                return new NpgsqlParameter(name, value ?? DBNull.Value);
            }
            else
            {
                throw new NotSupportedException("Unsupported connection type");
            }
        }
        public IDbDataParameter[] CreateParameters(params (string name, object value)[] parameters)
        {
            var result = new IDbDataParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                result[i] = CreateParameter(parameters[i].name, parameters[i].value);
            }
            return result;
        }



        public async Task<int> Upd(string toUpd, object id, string tableName = null, string where = null, bool last = false)
        {
            _logger.Send(toUpd);
            var parameters = new DynamicParameters();
            if (tableName == null) tableName = _tableName;
            if (tableName == null) throw new Exception("TableName is null");

            toUpd = QuoteName(toUpd, true);
            tableName = QuoteName(tableName);

            if (last)
            {
                toUpd += ", last = @lastTime";
                parameters.Add("lastTime", DateTime.UtcNow.ToString("MM-ddTHH:mm"));
            }

            string query;
            if (string.IsNullOrEmpty(where))
            {
                query = $"UPDATE {tableName} SET {toUpd} WHERE id = {id}";

            }
            else
            {
                query = $"UPDATE {tableName} SET {toUpd} WHERE {where}";
            }

            try
            {
                return await _connection.ExecuteAsync(query, parameters, commandType: System.Data.CommandType.Text);
            }
            catch (Exception ex)
            {
                string formattedQuery = query;
                foreach (var param in parameters.ParameterNames)
                {
                    var value = parameters.Get<dynamic>(param)?.ToString() ?? "NULL";
                    formattedQuery = formattedQuery.Replace($"@{param}", $"'{value}'");
                }

                Debug.WriteLine($"Error: {ex.Message}");
                Debug.WriteLine($"Executed query: {formattedQuery}");
                throw;
            }
        }
        public async Task Upd(List<string> toWrite, string tableName = null, string where = null, bool last = false)
        {
            int id = 0;
            foreach (var item in toWrite)
            {
                await Upd(item, id, tableName, where, last);
                id++;
            }
        }


        public async Task<string> Get(string toGet, string id, string tableName = null, string where = null)
        {
            var parameters = new DynamicParameters();
            if (tableName == null) tableName = _tableName;
            if (tableName == null) throw new Exception("TableName is null");

            toGet = QuoteName(toGet, true);
            tableName = QuoteName(tableName);



            string query;
            if (string.IsNullOrEmpty(where))
            {
                query = $"SELECT {toGet} FROM {tableName} WHERE id = @id";
                parameters.Add("id", id);
            }
            else
            {
                query = $"SELECT {toGet} FROM {tableName} WHERE {where}";
            }

            try
            {
                return await _connection.ExecuteScalarAsync<string>(query, parameters, commandType: System.Data.CommandType.Text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task AddRange(int range, string tableName = null)
        {
            if (tableName == null) tableName = _tableName;
            if (tableName == null) throw new Exception("TableName is null");

            string query = $@"SELECT COALESCE(MAX(id), 0) FROM {tableName};";

            int current = await _connection.ExecuteScalarAsync<int>(query, commandType: System.Data.CommandType.Text);
            _logger.Send($"{query}-{current}");

            for (int currentId = current + 1; currentId <= range; currentId++)
            {
                _connection.ExecuteAsync($@"INSERT INTO {tableName} (id) VALUES ({currentId}) ON CONFLICT DO NOTHING;");
            }
        }

        private string QuoteName(string name, bool isColumnList = false)
        {
            if (isColumnList)
            {
                name = name.Trim().TrimEnd(',');
                var parts = name.Split(',').Select(p => p.Trim()).ToList();
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
                        result.Add($"\"{part}\"");
                    }
                }
                return string.Join(", ", result);
            }
            return $"\"{name.Replace("\"", "\"\"")}\"";
        }



    }
    
}
