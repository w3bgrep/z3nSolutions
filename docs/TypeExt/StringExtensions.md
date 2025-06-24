

## StringExtensions

### Назначение

`StringExtensions` — статический класс с набором extension-методов для типа `string`. Реализует утилиты для работы с криптографическими ключами, адресами, диапазонами, преобразованием чисел и строк, парсингом данных, escape-форматированием для Markdown и др.

---

### Примеры использования

```csharp
// Экранирование спецсимволов Markdown
string safe = text.EscapeMarkdown();

// Получить хэш транзакции из ссылки
string hash = link.GetTxHash();

// Определить тип ключа (evm, sol, seed)
string type = input.KeyType();

// Получить публичный EVM-адрес из seed или приватного ключа
string addr = seedOrKey.ToPubEvm();

// Получить приватный ключ по seed и индексу
string priv = seed.ToSepc256k1(0);

// Преобразовать hex-строку в число ETH
string eth = "0x1bc16d674ec80000".HexToString("eth");

// Преобразовать строку в hex (ETH)
string hex = "1.5".StringToHex("eth");

// Проверить короткую форму адреса
bool ok = "0x12…cd".ChkAddress("0x1234567890abcdef");

// Парсить креды по формату
var creds = "login:pass:mail".ParseCreds("{login}:{pass}:{mail}");

// Получить массив аккаунтов из диапазона
string[] accs = "1-5".Range();
```


---

## Описание методов

### EscapeMarkdown

```csharp
public static string EscapeMarkdown(this string text)
```

- Экранирует спецсимволы Markdown в строке: `_ * [ ] ( ) ~ ` > \# + - = | { } . !`

---

### GetTxHash

```csharp
public static string GetTxHash(this string link)
```

- Возвращает хэш транзакции из ссылки (всё после последнего `/`). Если ссылка заканчивается на `/` — возвращает пустую строку. Если ссылка пуста — выбрасывает исключение.

---

### KeyType

```csharp
public static string KeyType(this string input)
```

- Определяет тип ключа:
    - `"keyEvm"` — 64 hex-символа,
    - `"keySol"` — 87–88 base58-символов,
    - `"seed"` — 12 или 24 слова,
    - `""` — нераспознанный.

---

### ToPubEvm

```csharp
public static string ToPubEvm(this string key)
```

- Для seed-фразы: получает приватный ключ по стандартному пути `m/44'/60'/0'/0/0` и возвращает публичный EVM-адрес.
- Для приватного ключа: возвращает публичный EVM-адрес.

---

### ToSepc256k1

```csharp
public static string ToSepc256k1(this string seed, int path = 0)
```

- Получает приватный ключ по seed-фразе и индексу (путь `m/44'/60'/0'/0/{path}`).

---

### TxToString

```csharp
public static string[] TxToString(this string txJson)
```

- Принимает JSON-транзакцию, возвращает массив:
`[gas, value, sender, data, recipient, gwei]`
(gas и value — hex, gwei — в десятичном виде).

---

### ChkAddress

```csharp
public static bool ChkAddress(this string shortAddress, string fullAddress)
```

- Проверяет, соответствует ли сокращённый адрес (с `…` внутри) полному (по префиксу и суффиксу, без учёта регистра).

---

### ParseCreds

```csharp
public static Dictionary<string, string> ParseCreds(this string data, string format)
```

- Парсит строку данных по формату, например:
`"login:pass:mail".ParseCreds("{login}:{pass}:{mail}")`
→ словарь с ключами login, pass, mail.

---

### StringToHex

```csharp
public static string StringToHex(this string value, string convert = "")
```

- Преобразует строку-число в hex-строку (0x...).
Поддерживает множители:
    - `"gwei"` — *1e9,
    - `"eth"` — *1e18.

---

### HexToString

```csharp
public static string HexToString(this string hexValue, string convert = "")
```

- Преобразует hex-строку (0x...) в строку-число.
Поддерживает делители:
    - `"gwei"` — /1e9,
    - `"eth"` — /1e18.

---

### Range

```csharp
public static string[] Range(this string accRange)
```

- Преобразует строку диапазона в массив:
    - `"1-3"` → `["1","2","3"]`
    - `"1,2,3"` → `["1","2","3"]`
    - `"5"` → `["1","2","3","4","5"]`
- Если строка пуста — выбрасывает исключение.

---

## Вспомогательные (внутренние) методы

- `DetectKeyType(this string input)`: Внутренняя логика для определения типа ключа (seed, key).
- Вспомогательные преобразования с использованием NBitcoin, Newtonsoft.Json, System.Numerics.

---

## Особенности

- Методы расширения доступны для всех строковых объектов.
- Поддержка работы с криптографическими ключами и адресами.
- Удобный синтаксис для парсинга, преобразования и проверки строк в сценариях автоматизации.


