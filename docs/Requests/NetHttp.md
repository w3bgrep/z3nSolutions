

##  NetHttp

### Назначение

Класс `NetHttp` предоставляет расширенный интерфейс для HTTP-запросов (GET, POST, PUT, DELETE) с поддержкой прокси, кастомных заголовков, логирования, работы с cookies, автоматическим разбором JSON-ответов и централизованной установкой прокси для браузерного инстанса в ZennoPoster.

---

### Примеры использования

```csharp
// Создание экземпляра с логированием
var http = new NetHttp(project, log: true);

// GET-запрос с прокси и парсингом JSON
string resp = http.GET("https://api.example.com/data", proxyString: "+", parse: true);

// POST-запрос с телом и заголовками
string result = http.POST("https://api.example.com/post", "{\"key\":\"value\"}", headers: new Dictionary<string, string> { { "Authorization", "Bearer token" } });

// Установить прокси для Instance и проверить смену IP
bool ok = http.ProxySet(instance, proxyString: "+");
```


---

## Описание методов

### GET

```csharp
public string GET(
    string url,
    string proxyString = "",
    Dictionary<string, string> headers = null,
    bool parse = false,
    int deadline = 15,
    string callerName = "",
    bool throwOnFail = false
)
```

- Выполняет HTTP GET-запрос по адресу `url`.
- **proxyString**: строка прокси (`""` — не использовать, `"+"` — взять из базы, либо явная строка).
- **headers**: словарь дополнительных заголовков (объединяются с дефолтными).
- **parse**: разобрать ответ как JSON через `project.Json.FromString`.
- **deadline**: таймаут в секундах.
- **throwOnFail**: выбрасывать исключение при ошибке.
- Возвращает тело ответа (или сообщение об ошибке).

---

### POST

```csharp
public string POST(
    string url,
    string body,
    string proxyString = "",
    Dictionary<string, string> headers = null,
    bool parse = false,
    int deadline = 15,
    string callerName = "",
    bool throwOnFail = false
)
```

- Выполняет HTTP POST-запрос по адресу `url` с телом `body` (отправляется как JSON).
- Параметры аналогичны GET.
- Логирует тело запроса и ответ.
- Возвращает тело ответа (или сообщение об ошибке).

---

### PUT

```csharp
public string PUT(
    string url,
    string body = "",
    string proxyString = "",
    Dictionary<string, string> headers = null,
    bool parse = false,
    string callerName = ""
)
```

- Выполняет HTTP PUT-запрос по адресу `url` с телом `body` (отправляется как JSON).
- Параметры аналогичны POST.
- Возвращает тело ответа (или сообщение об ошибке).

---

### DELETE

```csharp
public string DELETE(
    string url,
    string proxyString = "",
    Dictionary<string, string> headers = null,
    string callerName = ""
)
```

- Выполняет HTTP DELETE-запрос по адресу `url`.
- Параметры аналогичны GET.
- Возвращает тело ответа (или сообщение об ошибке).

---

### ParseProxy

```csharp
public WebProxy ParseProxy(string proxyString, string callerName = "")
```

- Преобразует строку прокси к формату `http://user:pass@host:port` или `http://host:port`.
- Если `proxyString == "+"`, берёт прокси из базы (`private_profile`).
- Поддерживает прокси с авторизацией и без.
- Логирует источник прокси и ошибки парсинга.

---

### CheckProxy

```csharp
public bool CheckProxy(string proxyString = null)
```

- Проверяет работоспособность прокси (или берёт из базы).
- Сравнивает внешний IP без прокси и с прокси через сервис `api.ipify.org`.
- Логирует результат и обновляет переменную `proxy` в проекте.

---

### ProxySet

```csharp
public bool ProxySet(Instance instance, string proxyString = null)
```

- Устанавливает прокси для браузерного инстанса.
- Проверяет смену IP через сервис `api.ipify.org`.
- Обновляет переменные проекта.
- Логирует результат.

---

### BuildHeaders

```csharp
private Dictionary<string, string> BuildHeaders(Dictionary<string, string> inputHeaders = null)
```

- Объединяет дефолтные заголовки (User-Agent и др.) с пользовательскими.
- Входящие заголовки имеют приоритет.

---

### ParseJson

```csharp
protected void ParseJson(string json)
```

- Пытается разобрать строку как JSON через `project.Json.FromString`.
- Логирует ошибки парсинга.

---

### Log

```csharp
protected void Log(string message, string callerName = "", bool forceLog = false)
```

- Логирует сообщение через внешний Logger с поддержкой отключения логирования.

---

## Особенности

- Все сетевые запросы потокобезопасны.
- Логирование реализовано через внешний Logger.
- Прокси можно задавать явно, через базу или переменную проекта.
- Проверка смены IP после установки прокси обязательна.
- Поддержка парсинга JSON-ответов встроена (по желанию).
- Исключения при ошибках запросов и прокси могут быть подавлены или проброшены.
- Cookies из Set-Cookie автоматически сохраняются в переменную проекта `debugCookies`.

---

