
##OKXApi

Класс **OKXApi** реализует работу с биржей OKX через HTTP API: получение балансов субаккаунтов, перевод средств между аккаунтами, вывод средств, создание субаккаунтов, массовый сбор остатков и получение рыночных цен. Поддерживает автоматическую подстановку ключей из базы данных, работу с прокси и детальное логирование.

---

### Пример использования

```csharp
// Создание экземпляра с логированием
var okx = new OKXApi(project, log: true);

// Получение списка субаккаунтов
var subs = okx.OKXGetSubAccs();

// Получение максимального вывода по субаккаунту
var maxList = okx.OKXGetSubMax("sub1");

// Получение балансов субаккаунта (trading)
var tradingBalances = okx.OKXGetSubTrading("sub1");

// Получение балансов субаккаунта (funding)
var fundingBalances = okx.OKXGetSubFunding("sub1");

// Перевод средств с субаккаунта на основной
okx.OKXSubToMain("sub1", "USDT", 10.5m);

// Вывод средств
okx.OKXWithdraw("0x123...", "USDT", "ERC20", 100, 1);

// Получение текущей цены пары
decimal price = okx.OKXPrice<decimal>("BTC-USDT");
```


---

## Описание конструктора

```csharp
public OKXApi(IZennoPosterProjectModel project, bool log = false)
```

- **log** (bool, необязательный): Включает детальное логирование всех операций OKXApi.

---

## Описание методов

### OKXGetSubAccs

```csharp
public List<string> OKXGetSubAccs(string proxy = null, bool log = false)
```

- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Получает список субаккаунтов пользователя.

---

### OKXGetSubMax

```csharp
public List<string> OKXGetSubMax(string accName, string proxy = null, bool log = false)
```

- **accName** (string): Имя субаккаунта.
- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Получает максимальные суммы для вывода по всем валютам на выбранном субаккаунте.

---

### OKXGetSubTrading

```csharp
public List<string> OKXGetSubTrading(string accName, string proxy = null, bool log = false)
```

- **accName** (string): Имя субаккаунта.
- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Получает торговые балансы субаккаунта.

---

### OKXGetSubFunding

```csharp
public List<string> OKXGetSubFunding(string accName, string proxy = null, bool log = false)
```

- **accName** (string): Имя субаккаунта.
- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Получает балансы субаккаунта на funding-счёте.

---

### OKXGetSubsBal

```csharp
public List<string> OKXGetSubsBal(string proxy = null, bool log = false)
```

- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Получает балансы всех субаккаунтов с детализацией по валютам и суммам.

---

### OKXWithdraw

```csharp
public void OKXWithdraw(string toAddress, string currency, string chain, decimal amount, decimal fee, string proxy = null, bool log = false)
```

- **toAddress** (string): Адрес для вывода.
- **currency** (string): Валюта (например, "USDT").
- **chain** (string): Сеть (например, "ERC20").
- **amount** (decimal): Сумма для вывода.
- **fee** (decimal): Комиссия.
- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Выполняет вывод средств на указанный адрес через выбранную сеть.

---

### OKXSubToMain

```csharp
private void OKXSubToMain(string fromSubName, string currency, decimal amount, string accountType = "6", string proxy = null, bool log = false)
```

- **fromSubName** (string): Имя субаккаунта-отправителя.
- **currency** (string): Валюта.
- **amount** (decimal): Сумма перевода.
- **accountType** (string, необязательный): Тип счёта-источника ("6" — funding, "18" — trading).
- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Переводит средства с субаккаунта на основной аккаунт.

---

### OKXCreateSub

```csharp
public void OKXCreateSub(string subName, string accountType = "1", string proxy = null, bool log = false)
```

- **subName** (string): Имя нового субаккаунта.
- **accountType** (string, необязательный): Тип субаккаунта.
- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Создаёт новый субаккаунт.

---

### OKXDrainSubs

```csharp
public void OKXDrainSubs()
```

- **Назначение:** Переводит все остатки со всех субаккаунтов на основной аккаунт (funding и trading).

---

### OKXAddMaxSubs

```csharp
public void OKXAddMaxSubs()
```

- **Назначение:** Массовое создание субаккаунтов с уникальными именами.

---

### OKXPrice

```csharp
public T OKXPrice<T>(string pair, string proxy = null, bool log = false)
```

- **pair** (string): Торговая пара (например, "BTC-USDT").
- **proxy** (string, необязательный): Прокси для запроса.
- **log** (bool, необязательный): Принудительное логирование.
- **Назначение:** Получает последнюю цену по торговой паре. Тип возвращаемого значения определяется шаблоном (decimal или string).

---

### CexLog

```csharp
public void CexLog(string toSend = "", string callerName = "", bool log = false)
```

- **toSend** (string, необязательный): Сообщение для логирования.
- **callerName** (string, необязательный): Имя вызывающего метода (автоматически подставляется).
- **log** (bool, необязательный): Принудительно логировать даже если логирование отключено.
- **Назначение:** Логирует действия и ответы, связанные с работой OKXApi.

---

## Вспомогательные (внутренние) методы

- `OkxKeys()`: Получает API-ключ, секрет и passphrase из базы данных.
- `MapNetwork(string chain, bool log)`: Преобразует название сети (chain) в формат, используемый OKX.
- `CalculateHmacSha256ToBaseSignature(string message, string key)`: Генерирует подпись HMAC SHA256 и кодирует в Base64 для запросов.
- `OKXPost(string url, object body, string proxy = null, bool log = false)`: Выполняет POST-запрос к API OKX с нужными заголовками и логированием.
- `OKXGet(string url, string proxy = null, bool log = false)`: Выполняет GET-запрос к API OKX с нужными заголовками и логированием.

---

## Особенности

- Ключи API автоматически берутся из базы данных проекта.
- Все запросы подписываются с помощью HMAC SHA256 и снабжаются временной меткой.
- Поддерживается логирование всех запросов и ответов.
- Сети автоматически приводятся к нужному для OKX виду.
- Методы возвращают готовые к обработке данные (списки, значения, сообщения об ошибках).

---

**OKXApi** — инструмент для интеграции с биржей OKX: автоматизация управления субаккаунтами, переводов, выводов средств и получения рыночных данных.

