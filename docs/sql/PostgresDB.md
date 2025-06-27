

##  PostgresDB

### Назначение

Класс **PostgresDB** реализует работу с PostgreSQL из C\# через Npgsql: подключение к базе, выполнение SQL-запросов на чтение и запись, создание и модификация таблиц, массовое добавление строк, а также вспомогательные статические методы для интеграции с проектами ZennoPoster.

### Примеры использования

```csharp
// Подключение и чтение данных
using (var db = new PostgresDB("localhost:5432", "mydb", "user", "pass")) {
    db.open();
    string result = db.DbRead("SELECT id, name FROM users");
    db.close();
}

// Выполнение записи с параметрами
using (var db = new PostgresDB("localhost:5432", "mydb", "user", "pass")) {
    db.open();
    int affected = db.DbWrite("UPDATE users SET name=@name WHERE id=@id",
        new NpgsqlParameter("@name", "NewName"),
        new NpgsqlParameter("@id", 1)
    );
    db.close();
}

// Выполнение запроса из ZennoPoster-проекта
string outStr = PostgresDB.DbQueryPostgre(project, "SELECT * FROM mytable");
```


## Описание методов

### Конструктор

```csharp
public PostgresDB(string host, string database, string user, string password)
```

- **host**: адрес сервера и порт (например, "localhost:5432")
- **database**: имя базы данных
- **user**: имя пользователя
- **password**: пароль

Автоматически парсит порт из строки, если указан через двоеточие.

### open

```csharp
public void open()
```

- Открывает соединение с базой.
- При ошибке выбрасывает исключение с текстом ошибки.


### close

```csharp
public void close()
```

- Закрывает соединение, если оно открыто.


### Dispose

```csharp
public void Dispose()
```

- Корректно закрывает и освобождает соединение (для использования с using).


### DbRead

```csharp
public string DbRead(string sql, string separator = "|")
```

- Выполняет SQL-запрос (обычно SELECT).
- Возвращает результат в виде строк, где колонки разделены `separator`, строки — через `\r\n`.
- Пример:

```
id|name
1|Alice
2|Bob
```


### DbWrite

```csharp
public int DbWrite(string sql, params NpgsqlParameter[] parameters)
```

- Выполняет SQL-запрос на изменение данных (INSERT, UPDATE, DELETE).
- Поддерживает параметры для защиты от SQL-инъекций.
- Возвращает количество затронутых строк.
- При ошибке выбрасывает исключение с текстом запроса.


### Статические методы для ZennoPoster

#### DbQueryPostgre

```csharp
public static string DbQueryPostgre(
    IZennoPosterProjectModel project,
    string query,
    bool log = false,
    bool throwOnEx = false,
    string host = "localhost:5432",
    string dbName = "postgres",
    string dbUser = "postgres",
    string dbPswd = "",
    string callerName = ""
)
```

- Выполняет запрос к PostgreSQL из проекта ZennoPoster.
- Автоматически подхватывает параметры подключения из переменных проекта, если не заданы явно.
- Для SELECT возвращает результат как строки, для других запросов — число затронутых строк.
- Логирует ошибки и может их выбрасывать по флагу `throwOnEx`.


#### MkTablePostgre

```csharp
public static void MkTablePostgre(
    IZennoPosterProjectModel project,
    Dictionary<string, string> tableStructure,
    string tableName = "",
    bool strictMode = false,
    bool insertData = true,
    string host = null,
    string dbName = "postgres",
    string dbUser = "postgres",
    string dbPswd = "",
    string schemaName = "projects",
    bool log = false
)
```

- Создаёт таблицу с нужной структурой, если её нет.
- Сравнивает структуру таблицы с требуемой, добавляет недостающие колонки, при `strictMode` удаляет лишние.
- Если в структуре есть поле `acc0` и задан диапазон, автоматически добавляет строки с acc0 от текущего максимального до нужного значения.
- Все действия логируются.


## Вспомогательные (внутренние) методы

- `ParseHostPort(string input, int defaultPort)`: парсит строку хоста и возвращает кортеж (host, port).
- `EnsureConnection()`: выбрасывает исключение, если соединение не открыто.
- `CheckAndCreateTable(...)`: проверяет наличие таблицы и создаёт её при необходимости.
- `ManageColumns(...)`: добавляет/удаляет колонки для приведения структуры к требуемой.
- `InsertInitialData(...)`: массово добавляет строки с acc0.


