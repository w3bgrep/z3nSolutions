

##  Sql

### Назначение

Класс **Sql** — универсальный интерфейс для работы с базой данных в проектах ZennoPoster. Поддерживает оба режима работы: PostgreSQL и SQLite (определяется переменной проекта `DBmode`). Позволяет выполнять любые SQL-запросы, создавать и модифицировать таблицы, добавлять и обновлять данные, работать с колонками, фильтровать аккаунты, управлять структурой таблиц и интегрироваться с переменными проекта.

### Примеры использования

```csharp
// Создание экземпляра с логированием
var sql = new Sql(project, log: true);

// Выполнить запрос к базе (автоматически выберет PostgreSQL или SQLite)
string result = sql.DbQ("SELECT * FROM private_google");

// Создать таблицу с нужной структурой
sql.MkTable(new Dictionary<string, string> { { "acc0", "INTEGER PRIMARY KEY" }, { "email", "TEXT" } }, "private_google");

// Добавить или обновить запись по ключу
sql.Write(new Dictionary<string, string> { { "email", "test@mail.com" } }, "private_google");

// Получить значение поля для текущего аккаунта
string email = sql.Get("email", "private_google");

// Получить случайное значение из колонки
string rnd = sql.GetRandom("email", "private_google");

// Добавить колонку, если её нет
sql.ClmnAdd("private_google", "new_field");

// Массово добавить диапазон acc0
sql.AddRange("private_google", 100);

// Фильтровать список аккаунтов по условиям
sql.FilterAccList(sql.MkToDoQueries("todo1,todo2"));
```


## Описание методов

### Log

```csharp
public void Log(string query, string response = null, bool log = false)
```

- Логирует SQL-запросы и ответы через внешний Logger с эмодзи (🐘 для PostgreSQL, ✒ для SQLite).
- Форматирует текст запроса и ответа для удобства чтения.


### DbQ

```csharp
public string DbQ(string query, bool log = false, bool throwOnEx = false)
```

- Выполняет SQL-запрос (автоматически определяет режим работы: SQLite или PostgreSQL).
- Для SELECT возвращает результат как строку, для DML — число затронутых строк.
- Логирует ошибки и может их выбрасывать по флагу `throwOnEx`.


### MkTable

```csharp
public void MkTable(Dictionary<string, string> tableStructure, string tableName = null, bool strictMode = false, bool insertData = false, ...)
```

- Создаёт таблицу с нужной структурой, если её нет.
- Поддерживает строгий режим: удаляет лишние колонки, добавляет недостающие.
- Для PostgreSQL и SQLite вызывает соответствующие методы.


### Write

```csharp
public void Write(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true)
```

- Добавляет или обновляет записи по ключу (INSERT ON CONFLICT/UPDATE).
- Для каждой пары key-value формирует отдельный запрос.


### UpdTxt / Upd

```csharp
public void UpdTxt(string toUpd, string tableName, string key, bool log = false, bool throwOnEx = false)
public void Upd(string toUpd, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, object acc = null)
public void Upd(Dictionary<string, string> toWrite, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, bool byKey = false)
public void Upd(List<string> toWrite, string columnName, string tableName = null, ...)
```

- Обновляет данные в таблице по ключу или списку ключей.
- Автоматически добавляет поле `last` с текущим временем (если не отключено).
- Поддерживает обновление по словарю или списку значений.


### Get

```csharp
public string Get(string toGet, string tableName = null, bool log = false, bool throwOnEx = false, string key = "acc0", string acc = null, string where = "")
```

- Получает значение поля для текущего аккаунта (или по ключу).
- Поддерживает произвольные условия WHERE.


### GetRandom

```csharp
public string GetRandom(string toGet, string tableName = null, bool log = false, bool acc = false, bool throwOnEx = false, int range = 0, bool single = true, bool invert = false)
```

- Получает случайное значение из колонки (или несколько).
- Поддерживает фильтрацию по диапазону acc0, инверсию условий, возврат acc0 вместе с данными.


### GetColumns

```csharp
public string GetColumns(string tableName, string schemaName = "accounts", bool log = false)
```

- Возвращает список всех колонок таблицы (через запятую).


### TblName

```csharp
public string TblName(string tableName, bool name = true)
```

- Определяет имя таблицы и схемы для текущего режима работы.
- Возвращает имя таблицы или схемы.


### TblExist

```csharp
public bool TblExist(string tblName)
```

- Проверяет существование таблицы.


### TblAdd

```csharp
public void TblAdd(string tblName, Dictionary<string, string> tableStructure)
```

- Создаёт таблицу, если её нет.


### TblColumns

```csharp
public List<string> TblColumns(string tblName)
```

- Возвращает список колонок таблицы.


### TblMapForProject

```csharp
public Dictionary<string, string> TblMapForProject(string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''")
```

- Формирует структуру таблицы для проекта: добавляет статические и динамические колонки.


### ClmnExist

```csharp
public bool ClmnExist(string tblName, string clmnName)
```

- Проверяет существование колонки.


### ClmnAdd

```csharp
public void ClmnAdd(string tblName, string clmnName, string defaultValue = "TEXT DEFAULT \"\"")
public void ClmnAdd(string tblName, Dictionary<string, string> tableStructure)
```

- Добавляет колонку (или несколько) в таблицу, если их нет.


### ClmnDrop

```csharp
public void ClmnDrop(string tblName, string clmnName)
public void ClmnDrop(string tblName, Dictionary<string, string> tableStructure)
```

- Удаляет колонку (или несколько) из таблицы.


### ClmnPrune

```csharp
public void ClmnPrune(string tblName, Dictionary<string, string> tableStructure)
```

- Удаляет все колонки, которых нет в структуре.


### AddRange

```csharp
public void AddRange(string tblName, int range = 0)
```

- Массово добавляет строки с acc0 от текущего максимального до нужного значения.


### Proxy

```csharp
public string Proxy()
```

- Получает прокси из таблицы `private_profile` и сохраняет его в переменную проекта.


### Bio

```csharp
public string Bio()
```

- Получает nickname и bio из таблицы `public_profile` и сохраняет их в переменные проекта.


### Settings

```csharp
public Dictionary<string, string> Settings(bool set = true)
```

- Получает все настройки из таблицы `private_settings` и (опционально) сохраняет их в переменные проекта.


### Email

```csharp
public string Email(string tableName = "google", string schemaName = "accounts")
```

- Получает email-логин и icloud по текущему аккаунту, сохраняет их в переменные.


### Ref

```csharp
public string Ref(string refCode = null, bool log = false)
```

- Получает реферальный код из таблицы проекта (случайный, если не задан).


### GetAddresses

```csharp
public Dictionary<string, string> GetAddresses(string chains = null)
```

- Возвращает словарь адресов по тикерам из таблицы `public_blockchain`.


### MkToDoQueries

```csharp
public List<string> MkToDoQueries(string toDo = null, string defaultRange = null, string defaultDoFail = null)
```

- Формирует список SQL-запросов для отбора аккаунтов по задачам (todo).


### FilterAccList

```csharp
public void FilterAccList(List<string> dbQueries, bool log = false)
```

- Формирует список доступных аккаунтов по результатам SQL-запросов.
- Поддерживает ручной режим через переменную `acc0Forced`.
- Фильтрует аккаунты по статусу в социальных сетях, если задано.


### Address

```csharp
public string Address(string chainType = "evm")
```

- Получает адрес по типу сети (evm, sol и др.) из таблицы `public_blockchain`, сохраняет в переменную.


### Key

```csharp
public string Key(string chainType = "evm")
```

- Получает приватный ключ по типу (evm, sol, seed) из таблицы `private_blockchain`, при необходимости декодирует через SAFU.


## Вспомогательные детали

- Все методы автоматически выбирают режим работы (PostgreSQL или SQLite) по переменной проекта.
- Логирование реализовано через внешний Logger.
- Поддержка массовых операций, автоматического создания и изменения структуры таблиц.
- Интеграция с переменными и списками проекта для автоматизации отбора и фильтрации аккаунтов.
- Корректная обработка ошибок и исключений.

