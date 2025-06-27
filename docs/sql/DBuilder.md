

## DBuilder

### Назначение

Класс **DBuilder** — универсальный инструмент для работы с базой данных в проектах ZennoPoster. Позволяет создавать, копировать и модифицировать таблицы различных схем (Google, Twitter, Discord, Blockchain и др.), импортировать данные из текстовых форм и файлов, парсить и записывать ключи, адреса, депозиты, а также выполнять массовое заполнение и структурирование данных через удобные формы.

### Примеры использования

```csharp
// Создать структуру таблицы для схемы private_google
var structure = dbuilder.LoadSchema(schema.private_google);

// Импортировать данные в таблицу по пользовательскому формату
int count = int.Parse(dbuilder.ImportData("private_google", fields, mapping, "Импорт Google", "Выберите формат:"));

// Импортировать приватные ключи
dbuilder.ImportKeys("evm");

// Импортировать адреса в таблицу public_blockchain
dbuilder.ImportAddresses();

// Импортировать адреса депозитов для CEX
dbuilder.ImportDepositAddresses();

// Массовый импорт по выбранным схемам
dbuilder.ImportDB();

// Копировать структуру и данные таблицы
dbuilder.CopyTable("private_google", "private_google_backup");

// Переименовать колонки в таблице
dbuilder.RenameColumns("private_google", new Dictionary<string, string> { { "old", "new" } });

// Импортировать и сопоставить данные для схемы
dbuilder.MapAndImport(schema.private_twitter);
```


## Описание методов

### DefaultColumns

```csharp
public string[] DefaultColumns(schema tableSchem)
```

- Возвращает список стандартных колонок для выбранной схемы таблицы.


### LoadSchema

```csharp
public Dictionary<string, string> LoadSchema(schema tableSchem)
```

- Формирует структуру таблицы (имя поля → тип) для выбранной схемы.
- Поддерживает все основные схемы (private/public, blockchain, profile, api, settings и др.).
- Автоматически определяет primary key ("acc0" или "key").


### ImportData

```csharp
private string ImportData(
    string tableName,
    string[] availableFields,
    Dictionary<string, string> columnMapping,
    string formTitle = "title",
    string message = "Select format (one field per box):",
    int startFrom = 1
)
```

- Открывает форму для выбора формата данных и ввода строк.
- Позволяет сопоставить поля данных с колонками таблицы.
- Импортирует данные построчно, автоматически формирует SQL-запросы.
- Логирует успехи и ошибки, возвращает число успешно добавленных записей.


### ImportKeys

```csharp
public string ImportKeys(string keyType, int startFrom = 1)
```

- Импортирует приватные ключи (seed, evm, sol) в таблицу `private_blockchain`.
- Поддерживает автоматическое кодирование ключей и генерацию адреса из seed/mnemonic.
- Для EVM seed автоматически извлекает приватный ключ и адрес.


### ImportAddresses

```csharp
public string ImportAddresses(int startFrom = 1)
```

- Импортирует адреса в таблицу `public_blockchain`.
- Позволяет задать имя столбца (evm, sol, apt и др.) через форму.
- Создаёт структуру таблицы, если её нет.


### ImportDepositAddresses

```csharp
public string ImportDepositAddresses(int startFrom = 1)
```

- Импортирует адреса депозитов в таблицу `public_deposits`.
- Позволяет задать chain (ETH, BSC и др.) и CEX (binance, okx и др.) через форму.
- Автоматически создаёт нужную колонку.


### MapAndImport

```csharp
public void MapAndImport(schema tableSchem, int startFrom = 1)
```

- Автоматизированный импорт данных для выбранной схемы с помощью форм.
- Позволяет сопоставить пользовательские поля с колонками таблицы (Google, Twitter, Discord, GitHub и др.).
- Для профилей, почты, блокчейна, настроек и API — отдельные сценарии импорта.


### CopyTable

```csharp
public void CopyTable(string sourceTable, string targetTable)
```

- Копирует структуру и данные таблицы из одной в другую (поддержка PostgreSQL и SQLite).
- Проверяет существование исходной таблицы, создаёт новую и копирует все записи.


### RenameColumns

```csharp
public void RenameColumns(string tblName, Dictionary<string, string> renameMap)
```

- Переименовывает колонки таблицы согласно переданной карте переименования.
- Поддержка PostgreSQL и SQLite.


### ImportDB

```csharp
public void ImportDB(schema? schemaValue = null)
```

- Массовый импорт: создаёт схемы, таблицы и запускает MapAndImport для всех выбранных схем (через форму с чекбоксами).
- Если указан конкретный schemaValue, выполняет импорт только для него.


### ParseWebGl

```csharp
public List<string> ParseWebGl(string vendor, int qnt, bool before78 = false)
```

- Собирает отпечатки WebGL для указанного вендора (NVIDIA, AMD, Intel).
- Поддержка эмуляции Canvas и обхода защиты браузера.
- Возвращает список собранных строк.


## Вспомогательные детали

- Все формы реализованы на Windows Forms, поддерживают предпросмотр формата и данных.
- Для работы с ключами используется кодирование через SAFU, генерация адресов — через NBitcoin и Nethereum.
- Поддержка PostgreSQL и SQLite (автоматическое определение и генерация SQL).
- Все действия логируются через методы проекта.
- Встроена поддержка массового импорта и ручного сопоставления полей.

