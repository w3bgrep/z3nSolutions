

##   Unlock

### Назначение

Класс **Unlock** реализует работу с NFT-контрактом (ERC-721) по ABI Unlock Protocol: позволяет получать владельца токена (`ownerOf`), дату окончания действия ключа (`keyExpirationTimestampFor`), а также собирать словарь всех владельцев с датами окончания действия их ключей. Используется для анализа доступа и автоматизации работы с контрактами через RPC в ZennoPoster.

### Пример использования

```csharp
// Создать экземпляр с логированием
var unlock = new Unlock(project, log: true);

// Получить дату окончания действия ключа по tokenId
string expiration = unlock.keyExpirationTimestampFor("0xContractAddress", 123);

// Получить владельца токена по tokenId
string owner = unlock.ownerOf("0xContractAddress", 123);

// Получить всех владельцев и даты окончания действия их ключей
var holders = unlock.Holders("0xContractAddress");
```


## Описание методов

### Конструктор

```csharp
public Unlock(IZennoPosterProjectModel project, bool log = false)
```

- **project**: объект ZennoPoster.
- **log**: включает логирование.
- Инициализирует объекты Sql, W3b, Blockchain, автоматически выбирает RPC для сети Optimism.


### keyExpirationTimestampFor

```csharp
public string keyExpirationTimestampFor(string addressTo, int tokenId, bool decode = true)
```

- Вызывает функцию контракта `keyExpirationTimestampFor(uint256 tokenId)` по ABI через RPC.
- **addressTo**: адрес контракта.
- **tokenId**: идентификатор токена.
- **decode**: декодировать ли результат (по умолчанию true).
- Возвращает дату окончания действия ключа (timestamp или декодированное значение).
- В случае ошибки логирует исключение и пробрасывает его.


### ownerOf

```csharp
public string ownerOf(string addressTo, int tokenId, bool decode = true)
```

- Вызывает функцию контракта `ownerOf(uint256 tokenId)` по ABI через RPC.
- **addressTo**: адрес контракта.
- **tokenId**: идентификатор токена.
- **decode**: декодировать ли результат (по умолчанию true).
- Возвращает адрес владельца токена.
- В случае ошибки логирует исключение и пробрасывает его.


### Decode

```csharp
public string Decode(string toDecode, string function)
```

- Декодирует hex-строку результата вызова функции контракта по ABI.
- **toDecode**: hex-строка результата (может начинаться с "0x").
- **function**: имя функции ("ownerOf" или "keyExpirationTimestampFor").
- Возвращает строку с декодированным значением (или список пар ключ-значение).
- Если результат пустой — логирует предупреждение.


### ProcessExpirationResult

```csharp
private string ProcessExpirationResult(string resultExpire)
```

- Декодирует результат функции `keyExpirationTimestampFor`.
- Проверяет, что строка не пуста, приводит к hex-формату, дополняет до 64 символов.
- Возвращает декодированное значение (timestamp).


### Holders

```csharp
public Dictionary<string, string> Holders(string contract)
```

- Собирает словарь всех владельцев токенов и дат окончания действия их ключей.
- Перебирает tokenId начиная с 1, вызывает `ownerOf` и `keyExpirationTimestampFor` для каждого.
- Если владелец — `0x0000000000000000000000000000000000000000` (burn/null), завершает перебор.
- Возвращает словарь: адрес владельца → дата окончания действия (оба в нижнем регистре).


## Вспомогательные детали

- Все вызовы контрактов выполняются через объект Blockchain и функцию ReadContract с нужным ABI.
- Для декодирования используется внешний декодер ABI (`z3n.Decoder.AbiDataDecode`).
- Все ошибки и исключения логируются через project.L0g или SendToLog.
- RPC для работы выбирается автоматически для сети Optimism через W3b.


## Особенности

- Универсальный интерфейс для получения владельцев и дат окончания NFT-ключей по контракту Unlock Protocol.
- Поддержка автоматического перебора всех токенов и сбора информации по ним.
- Корректная обработка ошибок и информативное логирование.
- Легко интегрируется в проекты ZennoPoster для анализа доступа и автоматизации управления NFT-ключами.

