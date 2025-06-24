

##InstanceExtensions

### Назначение

`InstanceExtensions` — набор extension-методов для класса `Instance` в ZennoPoster, обеспечивающих универсальный интерфейс поиска, получения, клика, установки значений HTML-элементов, а также взаимодействия с DOM через JavaScript и работу с Cloudflare Turnstile.

---

### Примеры использования

```csharp
// Получить HtmlElement по id
HtmlElement el = instance.GetHe(("myId", "id"));

// Получить текст элемента по селектору, с таймаутом 5 секунд
string text = instance.HeGet(("myId", "id"), deadline: 5, atr: "innertext");

// Клик по элементу с эмуляцией мыши
instance.HeClick(("myId", "id"), emu: 1);

// Установить значение в input по name
instance.HeSet(("login", "name"), "mylogin");

// Клик через JS
instance.JsClick("document.querySelector('#myBtn')");

// Решить Cloudflare Turnstile
instance.ClFlv2();
```


---

## Описание методов

### GetHe

```csharp
public static HtmlElement GetHe(this Instance instance, object obj, string method = "")
```

- Универсальный поиск элемента по:
    - `HtmlElement` (возвращает как есть, если не void)
    - Кортежу (string, string): (значение, "id"/"name")
    - Кортежу (string, string, string, string, int): (tag, attribute, pattern, mode, position)
- В случае неудачи выбрасывает исключение с деталями поиска.

---

### HeGet

```csharp
public static string HeGet(this Instance instance, object obj, string method = "", int deadline = 10, string atr = "innertext", int delay = 1, string comment = "", bool thr0w = true)
```

- Возвращает значение атрибута (по умолчанию `innertext`) найденного элемента.
- Повторяет попытки до истечения таймаута (`deadline`), с задержкой (`delay`).
- Если `method == "!"`, ожидает отсутствие элемента (возвращает null или кидает исключение).
- В случае ошибки — возвращает null или выбрасывает исключение в зависимости от `thr0w`.

---

### HeClick

```csharp
public static void HeClick(this Instance instance, object obj, string method = "", int deadline = 10, int delay = 1, string comment = "", bool thr0w = true, int emu = 0)
```

- Кликает по найденному элементу.
- Можно включить/выключить эмуляцию мыши (`emu`).
- Поддерживает режим "clickOut" — клик вне элемента.
- Таймаут и задержка настраиваются.

---

### HeSet

```csharp
public static void HeSet(this Instance instance, object obj, string value, string method = "id", int deadline = 10, int delay = 1, string comment = "", bool thr0w = true)
```

- Устанавливает значение в найденный input/textarea.
- Использует задержку и повторяет попытки до таймаута.
- Имитация ручного ввода через `WaitFieldEmulationDelay`.

---

### HeDrop

```csharp
public static void HeDrop(this Instance instance, Func elementSearch)
```

- Удаляет найденный элемент из DOM (через его родителя).

---

### JsClick

```csharp
public static string JsClick(this Instance instance, string selector, int delay = 2)
```

- Выполняет клик по элементу через JS (например, `document.querySelector(...)`).
- Задержка перед выполнением.
- Возвращает результат или сообщение об ошибке.

---

### JsSet

```csharp
public static string JsSet(this Instance instance, string selector, string value, int delay = 2)
```

- Устанавливает значение поля через JS.
- Генерирует событие `input` для корректной работы.
- Возвращает результат или ошибку.

---

### JsPost

```csharp
public static string JsPost(this Instance instance, string script, int delay = 0)
```

- Выполняет произвольный JS-код на странице.
- Возвращает результат выполнения или ошибку.

---

### ClFlv2

```csharp
public static void ClFlv2(this Instance instance)
```

- Автоматизация решения Cloudflare Turnstile:
    - Ищет элемент с id `cf-turnstile`.
    - Получает координаты и кликает по центру.
    - Ждёт появления токена (`cf-turnstile-response`).
    - Возвращает токен или кидает исключение при неудаче.

---

## Вспомогательные (внутренние) методы

- `ClipboardLock`, `ClipboardSemaphore`, `LockObject`: Для потокобезопасной работы с буфером обмена и критическими секциями.
- Вспомогательные try-catch блоки для устойчивости к ошибкам поиска и взаимодействия с DOM.

---

## Особенности

- Универсальный интерфейс поиска и работы с элементами: поддержка разных форматов задания цели.
- Все методы поддерживают таймауты, задержки, подробные сообщения об ошибках.
- Поддержка эмуляции мыши, ручного и JS-взаимодействия.
- Подходит для сложных сценариев автоматизации и парсинга в ZennoPoster.



