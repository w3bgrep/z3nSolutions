

##  Requests

### Назначение

Класс `Requests` реализует набор статических методов-расширений для `IZennoPosterProjectModel` для выполнения HTTP-запросов (GET, POST), работы с прокси, автоматического логирования, разбора JSON-ответов и централизованной установки прокси для браузерного инстанса в ZennoPoster.

---

### Примеры использования

```csharp
// GET-запрос с логированием и парсингом JSON
string response = project.GET("https://api.example.com/data", log: true, parseJson: true);

// POST-запрос с телом, прокси и заголовками
string result = project.POST("https://api.example.com/post", "{\"key\":\"value\"}", proxy: "+", headers: new[] { "Authorization: Bearer token" });

// Установить прокси для Instance и проверить смену IP
project.SetProxy(instance, proxyString: "+");
```


---

## Описание методов

### GET

```csharp
public static string GET(
    this IZennoPosterProjectModel project,
    string url,
    string proxy = "",
    string[] headers = null,
    bool log = false,
    bool parseJson = false,
    int deadline = 15,
    bool throwOnFail = false
)
```

- Выполняет HTTP GET-запрос по адресу `url`.
- **proxy**: строка прокси (`""` — не использовать, `"+"` — взять из переменной/базы, либо явная строка).
- **headers**: массив дополнительных заголовков.
- **log**: логировать запрос и ответ.
- **parseJson**: разобрать ответ как JSON через `project.Json.FromString`.
- **deadline**: таймаут в секундах.
- **throwOnFail**: выбрасывать исключение при ошибке.
- Возвращает тело ответа (или пустую строку при ошибке).

---

### POST

```csharp
public static string POST(
    this IZennoPosterProjectModel project,
    string url,
    string body,
    string proxy = "",
    string[] headers = null,
    bool log = false,
    bool parseJson = false,
    int deadline = 15,
    bool throwOnFail = false
)
```

- Выполняет HTTP POST-запрос по адресу `url` с телом `body`.
- Параметры аналогичны GET.
- Логирует тело запроса и ответ (если `log = true`).
- Возвращает тело ответа (или пустую строку при ошибке).

---

### SetProxy

```csharp
public static void SetProxy(this IZennoPosterProjectModel project, Instance instance, string proxyString = null)
```

- Устанавливает прокси для браузерного инстанса.
- **proxyString**: строка прокси (`null`/`""` — не использовать, `"+"` — взять из переменной/базы, либо явная строка).
- Проверяет смену IP через сервис `api.ipify.org` (до 60 секунд).
- Обновляет переменные проекта `ip` и `proxy`.
- Логирует результат и выбрасывает исключение при неудаче.

---

## Вспомогательные (внутренние) методы

- `ParseProxy(IZennoPosterProjectModel project, string proxyString, Logger logger = null)`:
Приводит строку прокси к формату `http://user:pass@host:port` или `http://host:port`.
Если `proxyString == "+"`, берёт прокси из переменной проекта или из базы (`private_profile`).
Логирует источник прокси и ошибки парсинга.
- `ParseJson(IZennoPosterProjectModel project, string json, Logger logger = null)`:
Пытается разобрать строку как JSON через `project.Json.FromString`. Логирует ошибки парсинга.

---

## Особенности

- Все сетевые запросы потокобезопасны (используется `LockObject`).
- Логирование реализовано через внешний класс Logger.
- Прокси можно задавать явно, через переменную проекта или из базы (автоматически).
- Проверка смены IP после установки прокси обязательна.
- Поддержка парсинга JSON-ответов встроена (по желанию).
- Исключения при ошибках запросов и прокси могут быть подавлены или проброшены (по параметру).

---

\