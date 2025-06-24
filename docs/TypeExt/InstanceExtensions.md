

## InstanceExtensions

### Назначение

Расширяет возможности объекта `Instance` в ZennoPoster: универсальный поиск и взаимодействие с HTML-элементами, управление вводом, кликами, удалением, а также выполнение и автоматизация действий через JavaScript и работу с Cloudflare Turnstile.

---

### Примеры использования

```csharp
// Поиск элемента по ID
var el = instance.GetHe(("myId", "id"));

// Получить текст элемента
string text = instance.HeGet(("myId", "id"), atr: "innertext");

// Клик по элементу с эмуляцией мыши
instance.HeClick(("submit", "name"), emu: 1);

// Закликивание элемента до исчезновения
instance.HeClick(("popup-close", "class"), method: "clickOut");

// Установить значение в input
instance.HeSet(("login", "name"), "mylogin");

// Удалить элемент из DOM
instance.HeDrop(() => instance.GetHe(("ad-banner", "id")));

// Клик по элементу через JS
instance.JsClick("document.querySelector('.btn')");

// Установить значение через JS
instance.JsSet("document.querySelector('#input')", "value");

// Выполнить произвольный JS
instance.JsPost("document.body.style.background = 'red'");

// Решить Cloudflare Turnstile
instance.ClFlv2();
```


---

## Описание методов

### GetHe

```csharp
public static HtmlElement GetHe(this Instance instance, object obj, string method = "")
```

- Универсальный поиск элемента:
    - Если передан `HtmlElement` — возвращает его (ошибка, если IsVoid).
    - Кортеж (значение, "id"/"name") — поиск по id или name.
    - Кортеж (tag, attribute, pattern, mode, pos) — поиск по атрибуту и позиции.
    - Для поиска последнего совпадения по атрибуту используйте `method == "last"`.
- Исключение при неудаче поиска.

---

### HeGet

```csharp
public static string HeGet(this Instance instance, object obj, string method = "", int deadline = 10, string atr = "innertext", int delay = 1, string comment = "", bool thr0w = true)
```

- Получение значения атрибута (`atr`, по умолчанию "innertext") найденного элемента.
- Повторяет попытки поиска до истечения `deadline` (секунды), с задержкой между попытками (`delay`).
- Если `method == "!"` — ожидает отсутствие элемента (возвращает null при успехе).
- При ошибке или таймауте — возвращает null или выбрасывает исключение (по `thr0w`).

---

### HeClick

```csharp
public static void HeClick(this Instance instance, object obj, string method = "", int deadline = 10, int delay = 1, string comment = "", bool thr0w = true, int emu = 0)
```

- Клик по найденному элементу.
- `emu > 0` — включает эмуляцию мыши, `< 0` — выключает, `0` — не меняет.
- Если `method == "clickOut"` — кликает по элементу в цикле, пока он не исчезнет или не истечёт `deadline`.
- При ошибке или таймауте — выбрасывает исключение или завершает работу (по `thr0w`).

---

### HeSet

```csharp
public static void HeSet(this Instance instance, object obj, string value, string method = "id", int deadline = 10, int delay = 1, string comment = "", bool thr0w = true)
```

- Устанавливает значение в найденный input/textarea.
- Имитация ручного ввода через задержку (`WaitFieldEmulationDelay`).
- Таймаут и задержка между попытками.

---

### HeDrop

```csharp
public static void HeDrop(this Instance instance, Func elementSearch)
```

- Удаляет найденный элемент из DOM через его родителя.

---

### JsClick

```csharp
public static string JsClick(this Instance instance, string selector, int delay = 2)
```

- Выполняет клик по элементу через JavaScript.
- Возвращает строку результата или сообщение об ошибке.

---

### JsSet

```csharp
public static string JsSet(this Instance instance, string selector, string value, int delay = 2)
```

- Устанавливает значение поля через JavaScript и генерирует событие `input`.
- Возвращает строку результата или сообщение об ошибке.

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

- Решение Cloudflare Turnstile:
    - Поиск элемента с id `cf-turnstile`.
    - Клик по центру элемента.
    - Ожидание появления токена ответа (`cf-turnstile-response`).
    - Возвращает токен или выбрасывает исключение при таймауте.

---

## Вспомогательные (внутренние) детали

- `ClipboardLock`, `ClipboardSemaphore`, `LockObject`: для потокобезопасности.
- Все методы используют try-catch с возвратом понятных ошибок.
- Таймауты и задержки настраиваются.
- Эмуляция мыши восстанавливается после операций.

---

## Особенности

- Все методы реализованы как extension для `Instance`.
- Поддержка сложных сценариев поиска и взаимодействия с элементами.
- Универсальный интерфейс для работы с DOM и JS.
- Корректная обработка ошибок и таймаутов.
- Поддержка автоматизации решения Cloudflare Turnstile.

