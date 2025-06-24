

## Traffic

Класс **Traffic** предназначен для работы с сетевым трафиком в проектах ZennoPoster. Позволяет получать параметры HTTP-запросов и ответов, фильтровать трафик по URL, извлекать отдельные параметры (метод, код ответа, заголовки, тело запроса/ответа и др.), а также получать значения конкретных HTTP-заголовков.

---

### Пример использования

```csharp
// Создание экземпляра с логированием
var traffic = new Traffic(project, instance, log: true);

// Получить все параметры трафика по части URL
string allParams = traffic.Get("api/v1/resource", null);

// Получить только тело ответа (ResponseBody)
string body = traffic.Get("api/v1/resource", "ResponseBody");

// Получить значение заголовка Authorization
string token = traffic.GetHeader("api/v1/resource", "Authorization");
```


---

## Описание конструктора

```csharp
public Traffic(IZennoPosterProjectModel project, Instance instance, bool log = false)
```

- **instance**: Экземпляр браузера для мониторинга трафика.
- **log**: Включить логирование действий класса.

---

## Описание методов

### Get

```csharp
public string Get(string url, string parametr, bool reload = false)
```

- **url** (string): Часть URL для фильтрации трафика.
- **parametr** (string): Название параметра для возврата. Доступные параметры:
`Method`, `ResultCode`, `Url`, `ResponseContentType`,
`RequestHeaders`, `RequestCookies`, `RequestBody`,
`ResponseHeaders`, `ResponseCookies`, `ResponseBody`.
Если не указан или пустой, возвращаются все параметры в формате "ключ-значение".
- **reload** (bool, необязательный): Перезагрузить страницу перед сбором трафика (по умолчанию false).

**Назначение:**
Возвращает значение выбранного параметра трафика для первого подходящего запроса по части URL, либо строку со всеми параметрами.

---

### Get

```csharp
public Dictionary<string, string> Get(string url, bool reload = false)
```

- **url** (string): Часть URL для фильтрации трафика.
- **reload** (bool, необязательный): Перезагрузить страницу перед сбором трафика.

**Назначение:**
Возвращает словарь всех параметров (ключ-значение) для первого подходящего запроса по части URL.
Если подходящий запрос не найден — повторяет попытку.

---

### GetHeader

```csharp
public string GetHeader(string url, string headerToGet = "Authorization", bool trim = true, bool reload = false)
```

- **url** (string): Часть URL для фильтрации трафика.
- **headerToGet** (string, необязательный): Название HTTP-заголовка для извлечения (по умолчанию "Authorization").
- **trim** (bool, необязательный): Удалять ли префикс "Bearer" и лишние пробелы (по умолчанию true).
- **reload** (bool, необязательный): Перезагрузить страницу перед сбором трафика.

**Назначение:**
Извлекает значение указанного HTTP-заголовка из RequestHeaders первого подходящего запроса по части URL.
Если trim=true, удаляет "Bearer" и пробелы.

---

## Вспомогательные (внутренние) методы

- `Deadline(int seconds = 0)`: Контроль времени выполнения операций (вызывается для предотвращения зависаний).
- `Logger`: Для логирования этапов работы с трафиком.

---

## Особенности

- Использует встроенный мониторинг трафика ZennoPoster (`UseTrafficMonitoring = true`).
- Фильтрация по части URL, а не по точному совпадению.
- Исключает запросы с методом OPTIONS.
- Автоматически повторяет попытку, если подходящий трафик не найден.
- Все параметры запроса и ответа доступны в виде словаря.
- Удобно для работы с авторизацией и API-запросами.

---


