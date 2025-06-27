

##  SQLite

### Назначение

Класс `SQLite` реализует вспомогательные статические методы для работы с базой данных SQLite в проектах ZennoPoster: выполнение SQL-запросов на чтение и изменение, создание и синхронизацию структуры таблиц, массовое добавление строк, логирование SQL-операций и обработку ошибок.

### Примеры использования

```csharp
// Выполнить SELECT-запрос с логированием
string result = SQLite.lSQL(project, "SELECT id, name FROM users", log: true);

// Выполнить INSERT или UPDATE с логированием ошибок
SQLite.lSQL(project, "UPDATE users SET name='John' WHERE id=1", log: true);

// Создать или синхронизировать структуру таблицы
SQLite.lSQLMakeTable(project, new Dictionary<string, string> {
    { "id", "INTEGER PRIMARY KEY" },
    { "name", "TEXT" }
}, tableName: "users", strictMode: true);
```


## Описание методов

### lSQL

```csharp
public static string lSQL(
    IZennoPosterProjectModel project,
    string query,
    bool log = false,
    bool ignoreErrors = false
)
```

- Выполняет SQL-запрос к базе SQLite, путь к которой берётся из переменной проекта `DBsqltPath`.
- Автоматически определяет провайдера (`Odbc`) и формирует строку подключения.
- Возвращает результат запроса в виде строки (для SELECT — строки с разделителями, для изменений — число затронутых строк).
- При `log = true`:
    - Для SELECT — логирует запрос и результат.
    - Для других запросов — логирует только запрос.
- При ошибке:
    - Логирует сообщение об ошибке (через внешний Logger).
    - Если `ignoreErrors = false`, выбрасывает исключение.
    - Если `ignoreErrors = true`, возвращает пустую строку.


### lSQLMakeTable

```csharp
public static void lSQLMakeTable(
    IZennoPosterProjectModel project,
    Dictionary<string, string> tableStructure,
    string tableName = "",
    bool strictMode = false
)
```

- Создаёт таблицу с заданной структурой, если её нет.
- Если таблица уже существует:
    - При `strictMode = true` — удаляет все колонки, которых нет в структуре.
    - Добавляет недостающие колонки, если они есть в структуре, но отсутствуют в таблице.
- Если в структуре есть поле `acc0` и определена переменная `rangeEnd`:
    - Добавляет строки с acc0 от 1 до `rangeEnd` (если их ещё нет).
- Все действия логируются через lSQL.


## Вспомогательные детали

- Строка подключения формируется с использованием ODBC:
`Dsn=SQLite3 Datasource; database=ПУТЬ_К_БАЗЕ;`
- Для логирования используется метод `project.SendToLog` и внешний Logger (`L0g`).
- Для парсинга структуры таблицы используется запрос `pragma_table_info`.
- Для массового добавления строк используется `INSERT OR IGNORE`.


