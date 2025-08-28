# Документация класса dSql

## Обзор
Класс `dSql` в пространстве имен `z3nCore` представляет собой уровень доступа к базам данных, предназначенный для взаимодействия с базами данных SQLite и PostgreSQL с использованием Dapper для ORM-операций и нативного ADO.NET для выполнения необработанных SQL-запросов. Класс предоставляет методы для чтения, записи, обновления и вставки данных с поддержкой параметризованных запросов для предотвращения SQL-инъекций. Класс реализует интерфейс `IDisposable` для правильного управления ресурсами подключения к базе данных.

Эта документация предназначена для разработчиков, интегрирующих или расширяющих класс `dSql` в приложениях на C#, ориентированных на .NET 4.6.2, с использованием синтаксиса C# 6.0.

---

## Зависимости
- **Dapper**: Для доступа к данным в стиле ORM.
- **Microsoft.Data.Sqlite**: Для подключения к базам данных SQLite.
- **Npgsql**: Для подключения к базам данных PostgreSQL.
- **System.Data**: Для интерфейсов ADO.NET.
- **System.Diagnostics**: Для логирования отладочной информации.
- **System.Linq**: Для операций LINQ.
- **System.Threading.Tasks**: Для асинхронных операций.

---

## Определение класса
```csharp
public class dSql : IDisposable
```

### Поля
- `private readonly IDbConnection _connection`: Подключение к базе данных (SQLite или PostgreSQL).
- `private readonly string _tableName`: Имя таблицы по умолчанию для операций (опционально, может быть null).
- `private readonly Logger _logger`: Экземпляр логгера для записи операций (не определен в предоставленном коде; предполагается внешняя зависимость).
- `private bool _disposed`: Отслеживает состояние освобождения ресурсов для предотвращения операций с освобожденным объектом.

---

## Конструкторы
Класс предоставляет несколько конструкторов для инициализации подключения к базе данных в зависимости от типа базы и параметров подключения.

1. **Конструктор для SQLite**
   ```csharp
   public dSql(string dbPath, string dbPass)
   ```
   - **Параметры**:
     - `dbPath` (string): Путь к файлу базы данных SQLite.
     - `dbPass` (string): Пароль для базы данных SQLite (не используется в предоставленной реализации).
   - **Поведение**: Инициализирует `SqliteConnection` и открывает его.
   - **Пример использования**:
     ```csharp
     var db = new dSql("path/to/database.db", "password");
     ```

2. **Конструктор для PostgreSQL (подробный)**
   ```csharp
   public dSql(string hostname, string port, string database, string user, string password)
   ```
   - **Параметры**:
     - `hostname` (string): Имя хоста сервера базы данных.
     - `port` (string): Порт сервера базы данных.
     - `database` (string): Имя базы данных.
     - `user` (string): Пользователь базы данных.
     - `password` (string): Пароль базы данных.
   - **Поведение**: Инициализирует `NpgsqlConnection` с включенным пулом подключений и открывает его.
   - **Пример использования**:
     ```csharp
     var db = new dSql("localhost", "5432", "mydb", "user", "pass");
     ```

3. **Конструктор для PostgreSQL (строка подключения)**
   ```csharp
   public dSql(string connectionstring)
   ```
   - **Параметры**:
     - `connectionstring` (string): Полная строка подключения для PostgreSQL.
   - **Поведение**: Инициализирует `NpgsqlConnection` и открывает его.
   - **Пример использования**:
     ```csharp
     var db = new dSql("Host=localhost;Port=5432;Database=mydb;Username=user;Password=pass");
     ```

4. **Общий конструктор**
   ```csharp
   public dSql(IDbConnection connection)
   ```
   - **Параметры**:
     - `connection` (IDbConnection): Существующее подключение к базе данных (SQLite или PostgreSQL).
   - **Поведение**: Использует предоставленное подключение, обеспечивая его открытие. Выбрасывает `ArgumentNullException`, если `connection` равно `null`.
   - **Пример использования**:
     ```csharp
     var conn = new SqliteConnection("Data Source=mydb.db");
     var db = new dSql(conn);
     ```

---

## Свойства
### ConnectionType
```csharp
public DatabaseType ConnectionType { get; }
```
- **Тип**: `DatabaseType` (enum: `Unknown`, `SQLite`, `PostgreSQL`)
- **Описание**: Возвращает тип подключения к базе данных (`SQLite`, `PostgreSQL` или `Unknown`).
- **Пример использования**:
  ```csharp
  var dbType = db.ConnectionType; // Возвращает DatabaseType.SQLite или DatabaseType.PostgreSQL
  ```

---

## Методы

### EnsureConnection
```csharp
private void EnsureConnection()
```
- **Описание**: Проверяет, что подключение открыто и объект не освобожден. Выбрасывает `ObjectDisposedException`, если объект освобожден, или открывает подключение, если оно закрыто.
- **Доступ**: Приватный
- **Использование**: Внутренний метод, вызываемый публичными методами для проверки состояния подключения.

---

### Dispose
```csharp
public void Dispose()
protected virtual void Dispose(bool disposing)
```
- **Описание**: Реализует `IDisposable` для закрытия и освобождения ресурсов подключения к базе данных.
- **Поведение**:
  - Закрывает подключение (игнорируя ошибки).
  - Освобождает подключение и устанавливает `_disposed` в `true`.
  - Подавляет финализацию через `GC.SuppressFinalize`.
- **Пример использования**:
  ```csharp
  using (var db = new dSql("path/to/database.db", ""))
  {
      // Операции с базой данных
  } // Автоматически освобождается
  ```

---

### DbReadAsync
```csharp
public async Task<string> DbReadAsync(string sql, string separator = "|")
```
- **Параметры**:
  - `sql` (string): SQL-запрос SELECT для выполнения.
  - `separator` (string, опционально): Разделитель для значений столбцов в строке (по умолчанию: `"|"`).
- **Возвращает**: `Task<string>`, содержащий строки, объединенные через `\r\n`, с колонками в каждой строке, объединенными разделителем.
- **Описание**: Выполняет SELECT-запрос и возвращает результаты в виде строки. Поддерживает SQLite и PostgreSQL.
- **Исключения**:
  - `NotSupportedException`: Если тип подключения не SQLite и не PostgreSQL.
  - `ObjectDisposedException`: Если объект освобожден.
- **Пример использования**:
  ```csharp
  var result = await db.DbReadAsync("SELECT * FROM users");
  // Возвращает: "1|John|Doe\r\n2|Jane|Smith"
  ```

---

### DbRead
```csharp
public string DbRead(string sql, string separator = "|")
```
- **Параметры**: Те же, что у `DbReadAsync`.
- **Возвращает**: Строка с результатами запроса (синхронная обертка над `DbReadAsync`).
- **Описание**: Синхронная версия `DbReadAsync`.
- **Пример использования**:
  ```csharp
  var result = db.DbRead("SELECT * FROM users");
  ```

---

### DbWriteAsync
```csharp
public async Task<int> DbWriteAsync(string sql, params IDbDataParameter[] parameters)
```
- **Параметры**:
  - `sql` (string): SQL-запрос INSERT, UPDATE или DELETE.
  - `parameters` (IDbDataParameter[]): Опциональные параметры запроса.
- **Возвращает**: `Task<int>`, представляющий количество затронутых строк.
- **Описание**: Выполняет SQL-команду без возврата данных (INSERT, UPDATE, DELETE) с опциональными параметрами.
- **Исключения**:
  - `NotSupportedException`: Если тип подключения не поддерживается.
  - `Exception`: Обертка ошибок базы данных с деталями запроса и параметров.
- **Пример использования**:
  ```csharp
  var param = db.CreateParameter("@name", "John");
  var rows = await db.DbWriteAsync("INSERT INTO users (name) VALUES (@name)", param);
  ```

---

### DbWrite
```csharp
public int DbWrite(string sql, params IDbDataParameter[] parameters)
```
- **Параметры**: Те же, что у `DbWriteAsync`.
- **Возвращает**: Количество затронутых строк (синхронная обертка над `DbWriteAsync`).
- **Описание**: Синхронная версия `DbWriteAsync`.
- **Пример использования**:
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
- **Параметры**:
  - `name` (string): Имя параметра (например, `@param`).
  - `value` (object): Значение параметра (преобразует `null` в `DBNull.Value`).
- **Возвращает**: `IDbDataParameter` (`SqliteParameter` или `NpgsqlParameter`).
- **Описание**: Создает параметр, специфичный для базы данных, для параметризованных запросов.
- **Исключения**:
  - `NotSupportedException`: Если тип подключения не поддерживается.
- **Пример использования**:
  ```csharp
  var param = db.CreateParameter("@name", "John");
  ```

---

### CreateParameters
```csharp
public IDbDataParameter[] CreateParameters(params (string name, object value)[] parameters)
```
- **Параметры**:
  - `parameters` ((string, object)[]): Массив кортежей с именами и значениями параметров.
- **Возвращает**: Массив объектов `IDbDataParameter`.
- **Описание**: Создает несколько параметров, специфичных для базы данных.
- **Пример использования**:
  ```csharp
  var params = db.CreateParameters(("@name", "John"), ("@id", 1));
  ```

---

### Upd (Одиночное обновление)
```csharp
public async Task<int> Upd(string toUpd, object id, string tableName = null, string where = null, bool last = false)
```
- **Параметры**:
  - `toUpd` (string): Список пар столбец-значение, разделенных запятыми (например, `"name = @name, age = @age"`).
  - `id` (object): ID для условия WHERE (используется, если `where` равно `null`).
  - `tableName` (string, опционально): Имя таблицы (по умолчанию `_tableName`).
  - `where` (string, опционально): Пользовательское условие WHERE (переопределяет `id`).
  - `last` (bool, опционально): Если `true`, добавляет `last = @lastTime` с текущим временем UTC.
- **Возвращает**: `Task<int>`, представляющий количество затронутых строк.
- **Описание**: Обновляет записи в указанной таблице с использованием Dapper. Логирует строку `toUpd` через `_logger`.
- **Исключения**:
  - `Exception`: Если `tableName` равно `null`.
  - Обертка ошибок базы данных с деталями запроса.
- **Пример использования**:
  ```csharp
  var rows = await db.Upd("name = @name", 1, "users", null, true);
  ```

---

### Upd (Пакетное обновление)
```csharp
public async Task Upd(List<string> toWrite, string tableName = null, string where = null, bool last = false)
```
- **Параметры**:
  - `toWrite` (List<string>): Список пар столбец-значение для обновления.
  - `tableName` (string, опционально): Имя таблицы (по умолчанию `_tableName`).
  - `where` (string, опционально): Пользовательское условие WHERE.
  - `last` (bool, опционально): Если `true`, добавляет обновление столбца `last`.
- **Описание**: Выполняет `Upd` для каждого элемента в `toWrite`, увеличивая `id` с 0.
- **Пример использования**:
  ```csharp
  var updates = new List<string> { "name = @name1", "name = @name2" };
  await db.Upd(updates, "users");
  ```

---

### Get
```csharp
public async Task<string> Get(string toGet, string id, string tableName = null, string where = null)
```
- **Параметры**:
  - `toGet` (string): Список столбцов для выбора, разделенных запятыми.
  - `id` (string): ID для условия WHERE (используется, если `where` равно `null`).
  - `tableName` (string, опционально): Имя таблицы (по умолчанию `_tableName`).
  - `where` (string, опционально): Пользовательское условие WHERE.
- **Возвращает**: `Task<string>`, содержащий значение первого столбца первой строки.
- **Описание**: Извлекает данные из указанной таблицы с использованием Dapper.
- **Исключения**:
  - `Exception`: Если `tableName` равно `null`.
  - Обертка ошибок базы данных.
- **Пример использования**:
  ```csharp
  var name = await db.Get("name", "1", "users");
  ```

---

### AddRange
```csharp
public async Task AddRange(int range, string tableName = null)
```
- **Параметры**:
  - `range` (int): Верхняя граница для ID, которые нужно вставить.
  - `tableName` (string, опционально): Имя таблицы (по умолчанию `_tableName`).
- **Описание**: Вставляет записи с последовательными ID от `MAX(id) + 1` до `range` в таблицу, используя `ON CONFLICT DO NOTHING` для пропуска дубликатов.
- **Исключения**:
  - `Exception`: Если `tableName` равно `null`.
- **Пример использования**:
  ```csharp
  await db.AddRange(10, "users"); // Вставляет ID до 10, если они еще не существуют
  ```

---

### QuoteName
```csharp
private string QuoteName(string name, bool isColumnList = false)
```
- **Параметры**:
  - `name` (string): Имя для экранирования (столбец или таблица).
  - `isColumnList` (bool, опционально): Если `true`, обрабатывает список столбцов, разделенных запятыми.
- **Возвращает**: Экранированную строку (например, `"name"` или `"col1, col2 = @val"`).
- **Описание**: Экранирует имена таблиц или столбцов, заключая их в двойные кавычки, обрабатывает списки, разделенные запятыми, для обновлений.
- **Доступ**: Приватный
- **Использование**: Внутренний метод для построения SQL-запросов.

---

## Замечания по использованию
- **Управление подключением**: Всегда используйте `dSql` в блоке `using` для корректного освобождения ресурсов подключения.
- **Параметризованные запросы**: Используйте `CreateParameter` или `CreateParameters` для безопасной передачи значений в `DbWriteAsync` или `DbWrite`.
- **Обработка ошибок**: Методы выбрасывают подробные исключения с информацией о запросе и параметрах для отладки.
- **Имя таблицы**: Если `_tableName` не задано и `tableName` не предоставлено, методы `Upd`, `Get` и `AddRange` выбросят исключение.
- **Логирование**: Зависимость `_logger` не определена в предоставленном коде. Убедитесь, что она реализована, или удалите вызовы логирования, если они не нужны.
- **SQL-инъекции**: Метод `QuoteName` предотвращает SQL-инъекции для имен таблиц/столбцов, но убедитесь, что параметры `toUpd`, `toGet` и `where` безопасны или параметризованы.

---

## Пример использования
```csharp
using (var db = new dSql("path/to/database.db", ""))
{
    // Чтение данных
    var result = await db.DbReadAsync("SELECT id, name FROM users");
    Console.WriteLine(result);

    // Запись данных
    var param = db.CreateParameter("@name", "John");
    await db.DbWriteAsync("INSERT INTO users (name) VALUES (@name)", param);

    // Обновление данных
    await db.Upd("name = @name", 1, "users");

    // Получение одного значения
    var name = await db.Get("name", "1", "users");
    Console.WriteLine(name);

    // Добавление диапазона ID
    await db.AddRange(5, "users");
}
```

---


