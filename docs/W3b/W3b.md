
##  W3b

### Назначение

Класс **W3b** реализует универсальный интерфейс для работы с EVM-совместимыми сетями в ZennoPoster: автоматизация отправки транзакций (legacy/EIP-1559), управление RPC и адресами, чтение и форматирование балансов, логирование, подстановку ключей, а также вспомогательные методы для работы с числами и строками.

### Примеры использования

```csharp
// Создать экземпляр с логированием и своим приватным ключом
var w3b = new W3b(project, log: true, key: "0x...");

// Получить RPC для сети
string rpc = w3b.Rpc("arbitrum");

// Получить список RPC для нескольких сетей
var rpcs = w3b.Rpc(new[] { "ethereum", "bsc", "polygon" });

// Отправить legacy-транзакцию
string txHash = w3b.SendLegacy(rpc, "0xContract", "0xData", 0.1m, "0xPrivateKey");

// Отправить EIP-1559 транзакцию
string txHash = w3b.Send1559(rpc, "0xContract", "0xData", 0.1m, "0xPrivateKey");

// Перевести hex в decimal (ETH)
string eth = w3b.HexToDecimalString("0x1bc16d674ec80000", "eth");

// Ограничить число до 8 знаков после запятой
string floored = w3b.FloorDecimal<string>(123.456789123456m, 8);
```


## Описание методов

### Конструктор

```csharp
public W3b(IZennoPosterProjectModel project, bool log = false, string key = null)
```

- Инициализирует проект, логгер, объект SQL, подгружает RPC, определяет основной аккаунт и ключ.
- Устанавливает культуру потока в InvariantCulture.
- Если ключ не передан, автоматически подставляет из базы.


### Rpc

```csharp
public string Rpc(string chain)
public List<string> Rpc(string[] chains)
```

- Возвращает RPC-эндпоинт для заданной сети (или списка сетей).
- Если не найден — выбрасывает исключение и пишет в лог.
- Встроен fallback-список для всех популярных EVM, Solana, Aptos и др.


### SendLegacy

```csharp
public string SendLegacy(string chainRpc, string contractAddress, string encodedData, decimal value, string walletKey, int speedup = 1)
```

- Формирует и отправляет legacy-транзакцию через RPC:
    - Получает chainId и адрес отправителя.
    - Получает цену газа, увеличивает её на коэффициент `speedup`.
    - Оценивает лимит газа с запасом (+50%).
    - Формирует и подписывает транзакцию через Blockchain.SendTransaction.
    - Возвращает хэш транзакции.
- Все ошибки логируются и пробрасываются.


### Send1559

```csharp
public string Send1559(string chainRpc, string contractAddress, string encodedData, decimal value, string walletKey, int speedup = 1)
```

- Формирует и отправляет EIP-1559 транзакцию:
    - Получает chainId и адрес отправителя.
    - Получает цену газа, рассчитывает maxFeePerGas и priorityFee с учётом `speedup`.
    - Оценивает лимит газа с запасом (+50%).
    - Формирует и подписывает транзакцию через Blockchain.SendTransactionEIP1559.
    - Возвращает хэш транзакции.
- Все ошибки логируются и пробрасываются.


### HexToDecimalString

```csharp
protected string HexToDecimalString(string hexValue, string convert = "")
```

- Переводит hex-строку (например, "0x1bc16d674ec80000") в decimal.
- **convert**:
    - `"gwei"` — делит на 1e9,
    - `"eth"` — делит на 1e18,
    - по умолчанию — возвращает число как строку.
- Возвращает "0" при ошибке.


### FloorDecimal

```csharp
protected T FloorDecimal<T>(decimal value, int? decimalPlaces = null)
```

- Ограничивает число до нужного количества знаков после запятой (по умолчанию 18).
- Поддерживает типы возврата: string, int, double, decimal.
- Корректно обрабатывает переполнение и ошибки.


### ApplyKey

```csharp
protected string ApplyKey(string key = null)
```

- Если ключ не передан — берёт из базы через Sql.Key("evm").
- Если ключ пустой — выбрасывает исключение.
- Возвращает приватный ключ.


### Acc0

```csharp
private string Acc0()
```

- Получает значение переменной проекта "acc0" (если это число).
- Если переменная не задана — возвращает пустую строку и пишет предупреждение в лог.


### Log

```csharp
protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
protected void Log(string address, string balance, string rpc, string contract = null, [CallerMemberName] string callerName = "", bool log = false)
```

- Логирует действия через Logger и/или project.L0g.
- Автоматически определяет имя вызывающего метода.


## Вспомогательные детали

- Все методы потокобезопасны.
- Культура потока всегда InvariantCulture (гарантия корректных числовых преобразований).
- Для работы с RPC используется fallback-словарь популярных сетей.
- Все ошибки и исключения логируются через Logger.
- Для работы с транзакциями используется Nethereum.


## Особенности

- Универсальный интерфейс для отправки транзакций (legacy/EIP-1559) и работы с RPC.
- Поддержка всех популярных EVM-сетей (Ethereum, BSC, Polygon, Arbitrum, Solana и др.).
- Гибкая работа с приватными ключами и адресами.
- Встроенные методы для форматирования чисел и преобразования hex/decimal.
- Поддержка логирования на всех этапах.
