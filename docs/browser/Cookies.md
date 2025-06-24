
## Cookies

Класс **Cookies** реализует универсальный интерфейс для получения, установки и сохранения cookies в проектах ZennoPoster. Поддерживает работу как через стандартный CookieContainer, так и через JavaScript, с возможностью интеграции с базой данных и файловой системой.

---

### Пример использования

```csharp
// Получить cookies текущего домена в формате JSON
string cookies = cookiesManager.Get(".");

// Установить cookies из базы данных
cookiesManager.Set("dbMain");

// Сохранить cookies из браузера в файл
cookiesManager.Save(source: "project", target: "file");

// Получить cookies через JS с логированием
string jsCookies = cookiesManager.GetByJs(log: true);

// Установить cookies через JS
cookiesManager.SetByJs(jsCookies);
```


---

## Описание конструктора

```csharp
public Cookies(IZennoPosterProjectModel project, Instance instance, bool log = false)
```

- **instance**: Экземпляр браузера для работы с cookies.
- **log**: Включить расширенное логирование.

---

## Описание методов

### Get

```csharp
public string Get(string domainFilter = "")
```

- **domainFilter** (string, необязательный): Фильтр по домену. Если `"."`, используется домен активной вкладки.
- **Назначение:**
Получает cookies из CookieContainer для всех доменов или только для указанного. Возвращает JSON-массив cookies в формате, совместимом с Chrome и ZennoPoster.

---

### Set

```csharp
public void Set(string cookieSourse = null, string jsonPath = null)
```

- **cookieSourse** (string, необязательный): Источник cookies (`"dbMain"`, `"dbProject"`, `"fromFile"` или JSON-строка).
- **jsonPath** (string, необязательный): Путь к файлу cookies (используется с `"fromFile"`).
- **Назначение:**
Устанавливает cookies в браузер из выбранного источника: базы данных, файла или строки.

---

### Save

```csharp
public void Save(string source = null, string target = null, string jsonPath = null)
```

- **source** (string, необязательный): Источник (`"project"` — из браузера, `"all"` — все cookies).
- **target** (string, необязательный): `"db"` — сохранить в базу, `"file"` — сохранить в файл.
- **jsonPath** (string, необязательный): Путь к файлу для сохранения.
- **Назначение:**
Сохраняет cookies из браузера в базу данных или в файл.
По умолчанию сохраняет cookies текущего аккаунта в базу.

---

### GetByJs

```csharp
public string GetByJs(string domainFilter = "", bool log = false)
```

- **domainFilter** (string, необязательный): Фильтр по домену (не используется в JS-методе, оставлен для совместимости).
- **log** (bool, необязательный): Включить логирование результата.
- **Назначение:**
Получает cookies текущей страницы через JavaScript (`document.cookie`).
Возвращает JSON-массив cookies в формате Chrome.

---

### SetByJs

```csharp
public void SetByJs(string cookiesJson, bool log = false)
```

- **cookiesJson** (string): JSON-массив cookies в формате Chrome.
- **log** (bool, необязательный): Включить логирование процесса.
- **Назначение:**
Устанавливает cookies на текущий домен через JavaScript.
Обрабатывает только уникальные пары домен+имя cookie, выставляет срок действия на год вперёд, логирует результат.

---

## Вспомогательные (внутренние) методы

- `LockObject`: Используется для потокобезопасной работы с файлами.
- `Sql`: Для получения и сохранения cookies в базе данных.
- `Logger`: Для логирования всех операций с cookies.

---

## Особенности

- Поддерживает все основные сценарии работы с cookies: экспорт/импорт между браузером, файлом и базой.
- Формат JSON полностью совместим с Chrome и ZennoPoster.
- Методы через JS позволяют работать с cookies, которые недоступны через CookieContainer (например, HttpOnly или специфичные для JS cookies).
- Все действия логируются для удобства отладки.
- Обеспечена потокобезопасность при работе с файлами.

---
