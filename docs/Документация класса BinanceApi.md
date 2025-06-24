<img src="https://r2cdn.perplexity.ai/pplx-full-logo-primary-dark%402x.png" class="logo" width="120"/>

## Документация класса BinanceApi

Класс **BinanceApi** предназначен для работы с биржей Binance через HTTP API: получение баланса активов, вывод средств, просмотр истории выводов, а также автоматическую подстановку ключей и прокси из базы данных проекта. Поддерживает детальное логирование и работу с различными сетями (chain).

---

### Пример использования

```csharp
// Создание экземпляра с логированием
var binance = new BinanceApi(project, log: true);

// Получение баланса всех монет
string balance = binance.GetUserAsset();

// Получение баланса конкретной монеты
string ethBalance = binance.GetUserAsset("ETH");

// Вывод средств
string withdrawResult = binance.Withdraw("0.1", "ERC20", "ETH", "0x123...");

// Получение истории выводов
string history = binance.GetWithdrawHistory(project);

// Получение истории по конкретному ID
string withdrawInfo = binance.GetWithdrawHistory(project, "123456");
```


---

## Описание конструктора

```csharp
public BinanceApi(IZennoPosterProjectModel project, bool log = false)
```

- **log** (bool, необязательный): Включает детальное логирование всех операций BinanceApi.

---

## Описание методов

### GetUserAsset

```csharp
public string GetUserAsset(string coin = "")
```

- **coin** (string, необязательный): Символ монеты (например, "ETH"). Если не указан — возвращает список всех активов и их балансы.
- **Назначение:** Получает баланс пользователя по всем монетам или только по указанной. Если монета не найдена — возвращает сообщение об отсутствии.

---

### Withdraw

```csharp
public string Withdraw(string amount, string network, string coin = "ETH", string address = "")
```

- **amount** (string): Сумма для вывода.
- **network** (string): Сеть вывода (например, "ERC20", "BSC").
- **coin** (string, необязательный): Символ монеты (по умолчанию "ETH").
- **address** (string): Адрес для вывода.
- **Назначение:** Выполняет вывод средств на указанный адрес в заданной сети.

---

### GetWithdrawHistory

```csharp
public string GetWithdrawHistory(IZennoPosterProjectModel project, string searchId = "")
```

- **searchId** (string, необязательный): Идентификатор заявки на вывод. Если не указан — возвращает список всех выводов. Если указан — возвращает детали по конкретному выводу.
- **Назначение:** Получает историю выводов пользователя. Если передан идентификатор — возвращает только информацию по нему.

---

### CexLog

```csharp
public void CexLog(string toSend = "", string callerName = "", bool log = false)
```

- **toSend** (string, необязательный): Сообщение для логирования.
- **callerName** (string, необязательный): Имя вызывающего метода (автоматически подставляется).
- **log** (bool, необязательный): Принудительно логировать даже если логирование отключено.
- **Назначение:** Логирует действия и ответы, связанные с работой BinanceApi.

---

## Вспомогательные (внутренние) методы

- `BinanceKeys()`: Получает API-ключ, секрет и прокси из базы данных проекта.
- `MapNetwork(string chain, bool log)`: Преобразует название сети (chain) в формат, используемый Binance. Логирует выбор сети.
- `MkSign(string parameters)`: Генерирует подпись HMAC SHA256 для параметров запроса.
- `TimeStamp()`: Возвращает текущий UTC timestamp в формате ISO.
- `BinancePOST(string method, string body, bool log = false)`: Выполняет POST-запрос к API Binance с нужными заголовками и логированием.
- `BinanceGET(string method, string parameters, bool log = false)`: Выполняет GET-запрос к API Binance с нужными заголовками и логированием.

---

## Особенности

- Ключи API и прокси автоматически берутся из базы данных проекта.
- Все запросы подписываются с помощью HMAC SHA256.
- Поддерживается логирование всех запросов и ответов.
- Сети переводятся в нужный для Binance вид автоматически.
- Методы возвращают текстовые ответы, готовые к дальнейшей обработке или выводу.

---

**BinanceApi** — инструмент для интеграции с биржей Binance: автоматизация получения баланса, вывода средств, истории транзакций и работы с несколькими сетями[^1].

<div style="text-align: center">⁂</div>

[^1]: BinanceApi.cs

[^2]: https://www.binance.com/en/binance-api

[^3]: https://codepal.ai/code-generator/query/f3AMZ6Qh/csharp-binance-api

[^4]: https://docs.binance.us

[^5]: https://developers.binance.com/docs/binance-spot-api-docs/rest-api

[^6]: https://python-binance.readthedocs.io/en/latest/binance.html

[^7]: https://pub.dev/documentation/binance_pay/latest/binance_pay/BinancePay-class.html

[^8]: https://blog.csdn.net/weixin_33660045/article/details/148358781

[^9]: https://hexdocs.pm/binance_api/BinanceApi.html

[^10]: https://github.com/kleninmaxim/binance-api

[^11]: https://algotrading101.com/learn/binance-python-api-guide/

