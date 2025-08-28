# Техническая документация класса dSql

## Обзор
Класс `dSql` представляет универсальную обертку для работы с базами данных SQLite и PostgreSQL, предоставляющую унифицированный интерфейс для выполнения SQL операций. Реализует паттерн Disposable для корректного управления ресурсами подключения.

**Namespace:** `z3nCore`  
**Поддерживаемые БД:** SQLite, PostgreSQL  
**Зависимости:** Dapper, Microsoft.Data.Sqlite, Npgsql

## Перечисления

### DatabaseType
```csharp
public enum DatabaseType
{
    Unknown,     // Неизвестный тип соединения
    SQLite,      // SQLite база данных
    PostgreSQL   // PostgreSQL база данных
}
```

## Поля класса

| Поле | Тип | Модификатор | Описание |
|------|-----|-------------|----------|
| `_connection` | `IDbConnection` | `private readonly` | Активное подключение к БД |
| `_tableName` | `string` | `private readonly` | Имя таблицы по умолчанию |
| `_logger` | `Logger` | `private readonly` | Экземпляр логгера |
| `_disposed` | `bool` | `private` | Флаг состояния освобождения ресурсов |

## Конструкторы

### 1. SQLite подключение (файловая БД)
```csharp
public dSql(string dbPath, string dbPass)
```
**Параметры:**
- `dbPath` - путь к файлу SQLite базы данных
- `dbPass` - пароль (в текущей реализации не используется)

**Примечание:** Автоматически открывает соединение

### 2. PostgreSQL подключение (развернутые параметры)
```csharp
public dSql(string hostname, string port, string database, string user, string password)
```
**Параметры:**
- `hostname` - хост сервера PostgreSQL
- `port` - порт подключения
- `database` - имя базы данных
- `user` - имя пользователя
- `password` - пароль пользователя

**Дополнительно:** Включает connection pooling

### 3. PostgreSQL подключение (строка соединения)
```csharp
public dSql(string connectionstring)
```
**Параметры:**
- `connectionstring` - полная строка подключения PostgreSQL

### 4. Инъекция существующего соединения
```csharp
public dSql(IDbConnection connection)
```
**Параметры:**
- `connection` - существующее подключение к БД

**Исключения:** `ArgumentNullException` если connection равен null

## Свойства

### ConnectionType
```csharp
public DatabaseType ConnectionType { get; }
```
**Возвращает:** Тип текущего подключения к базе данных

## Низкоуровневые методы управления ресурсами

### EnsureConnection()
```csharp
private void EnsureConnection()
```
**Назначение:** Проверяет состояние соединения и переоткрывает его при необходимости  
**Исключения:** `ObjectDisposedException` если объект уже освобожден

### Dispose()
```csharp
public void Dispose()
```
**Назначение:** Освобождает ресурсы подключения, реализует паттерн IDisposable

### Dispose(bool)
```csharp
protected virtual void Dispose(bool disposing)
```
**Параметры:**
- `disposing` - флаг принудительного освобождения ресурсов

**Примечание:** Игнорирует исключения при закрытии соединения

## Базовые методы работы с параметрами

### CreateParameter()
```csharp
public IDbDataParameter CreateParameter(string name, object value)
```
**Назначение:** Создает параметр для SQL запроса в зависимости от типа подключения  
**Параметры:**
- `name` - имя параметра
- `value` - значение параметра (null преобразуется в DBNull.Value)

**Возвращает:** `IDbDataParameter` соответствующего типа  
**Исключения:** `NotSupportedException` для неподдерживаемых типов соединений

### CreateParameters()
```csharp
public IDbDataParameter[] CreateParameters(params (string name, object value)[] parameters)
```
**Назначение:** Создает массив параметров из кортежей  
**Параметры:**
- `parameters` - массив кортежей (имя, значение)

**Возвращает:** Массив `IDbDataParameter[]`

## Базовые методы выполнения SQL

### DbReadAsync()
```csharp
public async Task<string> DbReadAsync(string sql, string separator = "|")
```
**Назначение:** Асинхронное выполнение SELECT запросов с возвратом результата в виде строки  
**Параметры:**
- `sql` - SQL запрос для выполнения
- `separator` - разделитель колонок в результирующей строке (по умолчанию "|")

**Возвращает:** Строку с результатами, где строки разделены `\r\n`, а колонки указанным разделителем  
**Исключения:** `NotSupportedException` для неподдерживаемых типов соединений

### DbRead()
```csharp
public string DbRead(string sql, string separator = "|")
```
**Назначение:** Синхронная версия DbReadAsync()  
**Примечание:** Использует .GetAwaiter().GetResult() для синхронного выполнения

### DbWriteAsync()
```csharp
public async Task<int> DbWriteAsync(string sql, params IDbDataParameter[] parameters)
```
**Назначение:** Асинхронное выполнение INSERT/UPDATE/DELETE запросов  
**Параметры:**
- `sql` - SQL запрос для выполнения
- `parameters` - массив параметров запроса

**Возвращает:** Количество затронутых строк  
**Обработка ошибок:** Логирует запрос с подставленными значениями параметров в Debug  
**Исключения:** `Exception` с детализацией SQL ошибки и запроса

### DbWrite()
```csharp
public int DbWrite(string sql, params IDbDataParameter[] parameters)
```
**Назначение:** Синхронная версия DbWriteAsync()

## Вспомогательные методы

### QuoteName()
```csharp
private string QuoteName(string name, bool isColumnList = false)
```
**Назначение:** Экранирует имена таблиц и колонок кавычками для безопасности  
**Параметры:**
- `name` - имя для экранирования
- `isColumnList` - флаг обработки списка колонок с поддержкой выражений типа "column = value"

**Логика:**
- Для обычных имен: оборачивает в двойные кавычки
- Для списков колонок: разбирает по запятым и обрабатывает каждый элемент отдельно
- Поддерживает выражения с знаком равенства

## Высокоуровневые методы (слой абстракции)

### Get()
```csharp
public async Task<string> Get(string toGet, string id, string tableName = null, string where = null)
```
**Назначение:** Получение значения из указанной таблицы  
**Параметры:**
- `toGet` - имя колонки для получения
- `id` - идентификатор записи
- `tableName` - имя таблицы (если null, используется _tableName)
- `where` - дополнительное условие WHERE (переопределяет условие по id)

**Возвращает:** Значение из указанной колонки в виде строки  
**Исключения:** `Exception` если tableName не задан

### Upd() - одиночное обновление
```csharp
public async Task<int> Upd(string toUpd, object id, string tableName = null, string where = null, bool last = false)
```
**Назначение:** Обновление записи в таблице  
**Параметры:**
- `toUpd` - строка SET части запроса (например: "column1 = @value1, column2 = @value2")
- `id` - идентификатор записи для обновления
- `tableName` - имя таблицы (если null, используется _tableName)
- `where` - пользовательское условие WHERE (переопределяет условие по id)
- `last` - флаг автоматического добавления поля last с текущим временем

**Возвращает:** Количество затронутых строк  
**Логирование:** Отправляет toUpd в логгер  
**Исключения:** `Exception` если tableName не задан

### Upd() - массовое обновление
```csharp
public async Task Upd(List<string> toWrite, string tableName = null, string where = null, bool last = false)
```
**Назначение:** Обновление множества записей  
**Параметры:**
- `toWrite` - список строк обновления
- Остальные параметры аналогичны одиночному методу

**Логика:** Выполняет последовательные обновления с инкрементальными id (0, 1, 2...)

### AddRange()
```csharp
public async Task AddRange(int range, string tableName = null)
```
**Назначение:** Добавление записей с последовательными id до указанного числа  
**Параметры:**
- `range` - максимальный id для добавления
- `tableName` - имя таблицы

**Логика:**
1. Определяет максимальный существующий id
2. Добавляет записи с id от (max_id + 1) до range
3. Использует ON CONFLICT DO NOTHING для предотвращения дублирования

**Логирование:** Отправляет информацию о текущем максимальном id в логгер

## Особенности реализации

### Безопасность
- Автоматическое экранирование имен таблиц и колонок
- Использование параметризованных запросов
- Защита от SQL-инъекций через QuoteName()

### Обработка ошибок
- Подробное логирование SQL ошибок с подстановкой параметров
- Вывод отформатированных запросов в Debug консоль
- Graceful обработка ошибок при закрытии соединений

### Производительность
- Асинхронные методы для неблокирующих операций
- Connection pooling для PostgreSQL
- Синхронные обертки для обратной совместимости

### Ограничения
- Поле `_tableName` объявлено но не инициализируется в конструкторах
- Поле `_logger` используется но не инициализируется
- Параметр `dbPass` в SQLite конструкторе игнорируется
- Отсутствует поддержка транзакций

## Рекомендации по использованию

1. **Обязательно используйте using statement** для автоматического освобождения ресурсов
2. **Предпочитайте асинхронные методы** для лучшей производительности
3. **Инициализируйте _tableName** если планируете использовать методы без явного указания таблицы
4. **Тестируйте подключения** перед использованием в продакшене
5. **Логируйте SQL операции** для отладки и мониторинга