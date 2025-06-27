

##   Native

### Назначение

Класс **Native** реализует универсальный интерфейс для получения балансов нативных токенов в сетях EVM, Solana, Sui, Aptos, Initia и других через RPC и публичные API. Поддерживает работу с прокси, автоматическое определение адреса и RPC, логирование, а также массовый сбор балансов по нескольким сетям.

### Примеры использования

```csharp
// Получить баланс EVM-кошелька (ETH, BNB и др.)
decimal eth = native.EVM<decimal>();

// Получить баланс Solana-кошелька
decimal sol = native.SOL<decimal>();

// Получить баланс Aptos-кошелька
decimal apt = native.APT<decimal>();

// Получить баланс Sui-кошелька
decimal sui = native.SUI<decimal>();

// Получить баланс Initia-кошелька
double init = native.Init<double>();

// Получить балансы по всем сетям
var balances = native.DicNative(new[] { "ethereum", "solana", "aptos" });
```


## Описание методов

### EVM

```csharp
public string EVM(string rpc = null, string address = null, string proxy = null, bool log = false)
public T EVM<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
```

- Получает баланс нативного токена EVM (ETH, BNB, MATIC и др.) через RPC.
- **rpc**: адрес RPC (если не задан — берётся из переменной проекта `blockchainRPC`).
- **address**: адрес кошелька (если не задан — берётся из переменной/ключа или из базы).
- **proxy**: строка прокси (`"+"` — из переменной проекта, иначе строка формата `host:port:user:pass`).
- **log**: логировать результат.
- Возвращает баланс в ETH (или другой валюте) как строку (18 знаков после запятой) или тип T (`decimal`, `string`).


### SOL

```csharp
public string SOL(string rpc = null, string address = null, string proxy = null, bool log = false)
public T SOL<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
```

- Получает баланс SOL через RPC Solana.
- **rpc**: адрес RPC (по умолчанию `https://api.mainnet-beta.solana.com`).
- **address**: адрес кошелька (если не задан — берётся из базы).
- **proxy**: строка прокси.
- Возвращает баланс в SOL (9 знаков после запятой) как строку или тип T (`decimal`, `string`).


### APT

```csharp
public string APT(string rpc = null, string address = null, string proxy = null, bool log = false)
public T APT<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
```

- Получает баланс Aptos через RPC.
- **rpc**: адрес RPC (по умолчанию `https://fullnode.mainnet.aptoslabs.com/v1`).
- **address**: адрес кошелька (если не задан — берётся из базы).
- **proxy**: строка прокси.
- Возвращает баланс в APT (8 знаков после запятой) как строку или тип T (`decimal`, `string`).


### SUI

```csharp
public string SUI(string rpc = null, string address = null, string proxy = null, bool log = false)
public T SUI<T>(string rpc = null, string address = null, string proxy = null, bool log = false)
```

- Получает баланс SUI через RPC.
- **rpc**: адрес RPC (по умолчанию `https://fullnode.mainnet.sui.io`).
- **address**: адрес кошелька (если не задан — берётся из базы).
- **proxy**: строка прокси.
- Возвращает баланс в SUI (9 знаков после запятой) как строку или тип T (`decimal`, `string`).


### Init

```csharp
public T Init<T>(string address = null, string chain = "interwoven-1", string token = "uinit", bool log = false)
```

- Получает баланс токена Initia через публичный API.
- **address**: адрес кошелька (если не задан — берётся из переменной проекта).
- **chain**: название цепочки (по умолчанию `interwoven-1`).
- **token**: тикер токена (по умолчанию `uinit`).
- Возвращает баланс токена как тип T (`double`, `string`).


### DicNative

```csharp
public Dictionary<string, decimal> DicNative(string[] chainsToUse = null, bool log = false)
```

- Получает балансы нативных токенов по всем указанным сетям.
- **chainsToUse**: массив имён сетей (например, `ethereum`, `solana`, `aptos`). Если не задан — берётся из переменной проекта `cfgChains`.
- Возвращает словарь: имя сети → баланс (`decimal`).


## Вспомогательные детали

- Все методы автоматически используют адрес и RPC из переменных проекта или базы, если параметры не заданы явно.
- Прокси поддерживаются в формате `host:port:user:pass` или через переменную проекта.
- Для EVM-адресов используется метод `ToPubEvm` для получения публичного адреса из ключа.
- Все балансы приводятся к строке с фиксированной точностью (18 для EVM, 9 для Solana, 8 для Aptos, 9 для SUI).
- Логирование реализовано через встроенный Logger.


## Особенности

- Универсальный интерфейс для получения балансов по основным сетям: EVM, Solana, Sui, Aptos, Initia.
- Поддержка прокси, автоматического определения адреса и RPC.
- Корректная обработка ошибок и исключений, логирование всех этапов.
- Поддержка массового сбора балансов по нескольким сетям.
- Методы возвращают значения как в строковом, так и в числовом формате (через generic T).

