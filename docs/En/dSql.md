# dSql Class Documentation

## Overview
The `dSql` class is a database access layer in the `z3nCore` namespace, designed to facilitate interaction with SQLite and PostgreSQL databases using Dapper for ORM operations and native ADO.NET for raw SQL execution. It provides methods for reading, writing, updating, and inserting data, with support for parameterized queries to prevent SQL injection. The class implements `IDisposable` to manage database connection resources properly.

This documentation is intended for developers integrating or extending the `dSql` class in C# applications targeting .NET 4.6.2, using C# 6.0 syntax.

---

## Dependencies
- **Dapper**: For ORM-style data access.
- **Microsoft.Data.Sqlite**: For SQLite database connectivity.
- **Npgsql**: For PostgreSQL database connectivity.
- **System.Data**: For ADO.NET interfaces.
- **System.Diagnostics**: For debug logging.
- **System.Linq**: For LINQ operations.
- **System.Threading.Tasks**: For asynchronous operations.

---

## Class Definition
```csharp
public class dSql : IDisposable
```

### Fields
- `private readonly IDbConnection _connection`: The database connection (SQLite or PostgreSQL).
- `private readonly string _tableName`: Default table name for operations (optional, can be null).
- `private readonly Logger _logger`: Logger instance for logging operations (not defined in provided code; assumed to be an external dependency).
- `private bool _disposed`: Tracks disposal state to prevent operations on disposed objects.

---

## Constructors
The class provides multiple constructors to initialize the database connection based on the database type and connection details.

1. **SQLite Constructor**
   ```csharp
   public dSql(string dbPath, string dbPass)
   ```
   - **Parameters**:
     - `dbPath` (string): Path to the SQLite database file.
     - `dbPass` (string): Password for the SQLite database (not used in the provided implementation).
   - **Behavior**: Initializes a `SqliteConnection` and opens it.
   - **Usage**:
     ```csharp
     var db = new dSql("path/to/database.db", "password");
     ```

2. **PostgreSQL Constructor (Detailed)**
   ```csharp
   public dSql(string hostname, string port, string database, string user, string password)
   ```
   - **Parameters**:
     - `hostname` (string): Database server hostname.
     - `port` (string): Database server port.
     - `database` (string): Database name.
     - `user` (string): Database user.
     - `password` (string): Database password.
   - **Behavior**: Initializes a `NpgsqlConnection` with pooling enabled and opens it.
   - **Usage**:
     ```csharp
     var db = new dSql("localhost", "5432", "mydb", "user", "pass");
     ```

3. **PostgreSQL Constructor (Connection String)**
   ```csharp
   public dSql(string connectionstring)
   ```
   - **Parameters**:
     - `connectionstring` (string): Full PostgreSQL connection string.
   - **Behavior**: Initializes a `NpgsqlConnection` and opens it.
   - **Usage**:
     ```csharp
     var db = new dSql("Host=localhost;Port=5432;Database=mydb;Username=user;Password=pass");
     ```

4. **Generic Constructor**
   ```csharp
   public dSql(IDbConnection connection)
   ```
   - **Parameters**:
     - `connection` (IDbConnection): Pre-existing database connection (SQLite or PostgreSQL).
   - **Behavior**: Uses the provided connection, ensuring it is open. Throws `ArgumentNullException` if `connection` is null.
   - **Usage**:
     ```csharp
     var conn = new SqliteConnection("Data Source=mydb.db");
     var db = new dSql(conn);
     ```

---

## Properties
### ConnectionType
```csharp
public DatabaseType ConnectionType { get; }
```
- **Type**: `DatabaseType` (enum: `Unknown`, `SQLite`, `PostgreSQL`)
- **Description**: Returns the type of database connection (`SQLite`, `PostgreSQL`, or `Unknown`).
- **Usage**:
  ```csharp
  var dbType = db.ConnectionType; // Returns DatabaseType.SQLite or DatabaseType.PostgreSQL
  ```

---

## Methods

### EnsureConnection
```csharp
private void EnsureConnection()
```
- **Description**: Verifies that the connection is open and the object is not disposed. Throws `ObjectDisposedException` if disposed, or opens the connection if closed.
- **Access**: Private
- **Usage**: Internal method called by public methods to ensure a valid connection state.

---

### Dispose
```csharp
public void Dispose()
protected virtual void Dispose(bool disposing)
```
- **Description**: Implements `IDisposable` to close and dispose of the database connection.
- **Behavior**:
  - Closes the connection (ignoring errors).
  - Disposes of the connection and sets `_disposed` to `true`.
  - Suppresses finalization via `GC.SuppressFinalize`.
- **Usage**:
  ```csharp
  using (var db = new dSql("path/to/database.db", ""))
  {
      // Database operations
  } // Automatically disposed
  ```

---

### DbReadAsync
```csharp
public async Task<string> DbReadAsync(string sql, string separator = "|")
```
- **Parameters**:
  - `sql` (string): SQL SELECT query to execute.
  - `separator` (string, optional): Separator for column values in each row (default: `"|"`).
- **Returns**: `Task<string>` containing rows joined by `\r\n`, with columns within each row joined by the separator.
- **Description**: Executes a SELECT query and returns results as a string. Supports both SQLite and PostgreSQL.
- **Exceptions**:
  - `NotSupportedException`: If the connection type is neither SQLite nor PostgreSQL.
  - `ObjectDisposedException`: If the object is disposed.
- **Usage**:
  ```csharp
  var result = await db.DbReadAsync("SELECT * FROM users");
  // Returns: "1|John|Doe\r\n2|Jane|Smith"
  ```

---

### DbRead
```csharp
public string DbRead(string sql, string separator = "|")
```
- **Parameters**: Same as `DbReadAsync`.
- **Returns**: String containing query results (synchronous wrapper around `DbReadAsync`).
- **Description**: Synchronous version of `DbReadAsync`.
- **Usage**:
  ```csharp
  var result = db.DbRead("SELECT * FROM users");
  ```

---

### DbWriteAsync
```csharp
public async Task<int> DbWriteAsync(string sql, params IDbDataParameter[] parameters)
```
- **Parameters**:
  - `sql` (string): SQL INSERT, UPDATE, or DELETE query.
  - `parameters` (IDbDataParameter[]): Optional query parameters.
- **Returns**: `Task<int>` representing the number of affected rows.
- **Description**: Executes a non-query SQL command (INSERT, UPDATE, DELETE) with optional parameters.
- **Exceptions**:
  - `NotSupportedException`: If the connection type is unsupported.
  - `Exception`: Wraps database errors with the query and parameter details.
- **Usage**:
  ```csharp
  var param = db.CreateParameter("@name", "John");
  var rows = await db.DbWriteAsync("INSERT INTO users (name) VALUES (@name)", param);
  ```

---

### DbWrite
```csharp
public int DbWrite(string sql, params IDbDataParameter[] parameters)
```
- **Parameters**: Same as `DbWriteAsync`.
- **Returns**: Number of affected rows (synchronous wrapper around `DbWriteAsync`).
- **Description**: Synchronous version of `DbWriteAsync`.
- **Usage**:
  ```csharp
  var rows = db.DbWrite("UPDATE users SET name = @name WHERE id = @id", 
      db.CreateParameter("@name", "John"), 
      db.CreateParameter("@id", 1));
  ```

---

### CreateParameter
```csharp
public IDbDataParameter CreateParameter(string name, object value)
```
- **Parameters**:
  - `name` (string): Parameter name (e.g., `@param`).
  - `value` (object): Parameter value (converts `null` to `DBNull.Value`).
- **Returns**: `IDbDataParameter` (either `SqliteParameter` or `NpgsqlParameter`).
- **Description**: Creates a database-specific parameter for parameterized queries.
- **Exceptions**:
  - `NotSupportedException`: If the connection type is unsupported.
- **Usage**:
  ```csharp
  var param = db.CreateParameter("@name", "John");
  ```

---

### CreateParameters
```csharp
public IDbDataParameter[] CreateParameters(params (string name, object value)[] parameters)
```
- **Parameters**:
  - `parameters` ((string, object)[]): Array of tuples containing parameter names and values.
- **Returns**: Array of `IDbDataParameter` objects.
- **Description**: Creates multiple database-specific parameters.
- **Usage**:
  ```csharp
  var params = db.CreateParameters(("@name", "John"), ("@id", 1));
  ```

---

### Upd (Single)
```csharp
public async Task<int> Upd(string toUpd, object id, string tableName = null, string where = null, bool last = false)
```
- **Parameters**:
  - `toUpd` (string): Comma-separated list of column-value pairs (e.g., `"name = @name, age = @age"`).
  - `id` (object): ID for the WHERE clause (used if `where` is null).
  - `tableName` (string, optional): Table name (defaults to `_tableName`).
  - `where` (string, optional): Custom WHERE clause (overrides `id`).
  - `last` (bool, optional): If `true`, adds `last = @lastTime` to the update with current UTC time.
- **Returns**: `Task<int>` representing the number of affected rows.
- **Description**: Updates records in the specified table using Dapper. Logs the `toUpd` string via `_logger`.
- **Exceptions**:
  - `Exception`: If `tableName` is null.
  - Wraps database errors with query details.
- **Usage**:
  ```csharp
  var rows = await db.Upd("name = @name", 1, "users", null, true);
  ```

---

### Upd (Batch)
```csharp
public async Task Upd(List<string> toWrite, string tableName = null, string where = null, bool last = false)
```
- **Parameters**:
  - `toWrite` (List<string>): List of column-value pairs to update.
  - `tableName` (string, optional): Table name (defaults to `_tableName`).
  - `where` (string, optional): Custom WHERE clause.
  - `last` (bool, optional): If `true`, adds `last` column update.
- **Description**: Executes `Upd` for each item in `toWrite`, incrementing `id` from 0.
- **Usage**:
  ```csharp
  var updates = new List<string> { "name = @name1", "name = @name2" };
  await db.Upd(updates, "users");
  ```

---

### Get
```csharp
public async Task<string> Get(string toGet, string id, string tableName = null, string where = null)
```
- **Parameters**:
  - `toGet` (string): Comma-separated list of columns to select.
  - `id` (string): ID for the WHERE clause (used if `where` is null).
  - `tableName` (string, optional): Table name (defaults to `_tableName`).
  - `where` (string, optional): Custom WHERE clause.
- **Returns**: `Task<string>` containing the first column of the first row.
- **Description**: Retrieves data from the specified table using Dapper.
- **Exceptions**:
  - `Exception`: If `tableName` is null.
  - Wraps database errors.
- **Usage**:
  ```csharp
  var name = await db.Get("name", "1", "users");
  ```

---

### AddRange
```csharp
public async Task AddRange(int range, string tableName = null)
```
- **Parameters**:
  - `range` (int): Upper bound for IDs to insert.
  - `tableName` (string, optional): Table name (defaults to `_tableName`).
- **Description**: Inserts records with sequential IDs from `MAX(id) + 1` to `range` into the table, using `ON CONFLICT DO NOTHING` to skip duplicates.
- **Exceptions**:
  - `Exception`: If `tableName` is null.
- **Usage**:
  ```csharp
  await db.AddRange(10, "users"); // Inserts IDs up to 10 if not already present
  ```

---

### QuoteName
```csharp
private string QuoteName(string name, bool isColumnList = false)
```
- **Parameters**:
  - `name` (string): Name to quote (column or table).
  - `isColumnList` (bool, optional): If `true`, processes a comma-separated list of columns.
- **Returns**: Quoted string (e.g., `"name"` or `"col1, col2 = @val"`).
- **Description**: Escapes table or column names by wrapping them in double quotes, handling comma-separated lists for updates.
- **Access**: Private
- **Usage**: Internal method for SQL query construction.

---

## Usage Notes
- **Connection Management**: Always use `dSql` within a `using` block to ensure proper disposal of database connections.
- **Parameterized Queries**: Use `CreateParameter` or `CreateParameters` to safely pass values to `DbWriteAsync` or `DbWrite`.
- **Error Handling**: Methods throw detailed exceptions with query and parameter information for debugging.
- **Table Name**: If `_tableName` is not set and `tableName` is not provided, methods like `Upd`, `Get`, and `AddRange` will throw an exception.
- **Logging**: The `_logger` dependency is not defined in the provided code. Ensure it is implemented or remove logging calls if not needed.
- **SQL Injection**: The `QuoteName` method mitigates SQL injection for table/column names, but ensure `toUpd`, `toGet`, and `where` parameters are safe or parameterized.

---

## Example Usage
```csharp
using (var db = new dSql("path/to/database.db", ""))
{
    // Read data
    var result = await db.DbReadAsync("SELECT id, name FROM users");
    Console.WriteLine(result);

    // Write data
    var param = db.CreateParameter("@name", "John");
    await db.DbWriteAsync("INSERT INTO users (name) VALUES (@name)", param);

    // Update data
    await db.Upd("name = @name", 1, "users");

    // Get single value
    var name = await db.Get("name", "1", "users");
    Console.WriteLine(name);

    // Add range of IDs
    await db.AddRange(5, "users");
}
```

---

## Limitations
- **Logger Dependency**: The `_logger` field is not defined in the provided code, which may cause compilation errors unless implemented.
- **Parameter Handling in Upd/Get**: The `Upd` and `Get` methods use `DynamicParameters`, but parameter values for `toUpd` must be handled externally, which can lead to SQL injection if not sanitized.
- **Connection Type**: Only SQLite and PostgreSQL are supported. Other database types throw `NotSupportedException`.
- **Table Name Requirement**: Methods requiring a table name will fail if neither `_tableName` nor `tableName` is provided.
- **Synchronous Methods**: `DbRead` and `DbWrite` block the calling thread, which may impact performance in async contexts.

---

## Recommendations
- **Add Logger Implementation**: Define the `Logger` class or remove `_logger` references if logging is not required.
- **Enhance Parameter Safety**: For `Upd` and `Get`, consider parsing `toUpd` to extract parameters and validate them to prevent SQL injection.
- **Connection Pooling**: For PostgreSQL, pooling is enabled in the constructor, but consider exposing pooling options for SQLite or other databases.
- **Error Handling**: Add more specific exception types for different error scenarios (e.g., connection failures vs. query errors).
- **Documentation**: Add XML comments to the class and methods for better IDE support.