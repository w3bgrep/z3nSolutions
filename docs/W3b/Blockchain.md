

## Blockchain

### Назначение

Класс **Blockchain** реализует универсальный интерфейс для работы с EVM- и BTC-совместимыми сетями: генерация seed-фраз, получение адресов и приватных ключей, чтение баланса, формирование и отправка транзакций, чтение данных из смарт-контрактов, ABI-энкодинг/декодинг, а также вспомогательные методы для работы с ABI и RPC в проектах ZennoPoster.

### Примеры использования

```csharp
// Создание экземпляра для EVM-сети
var blockchain = new Blockchain(privateKey, 1, "https://mainnet.infura.io/v3/yourkey");

// Получить адрес по приватному ключу
string address = blockchain.GetAddressFromPrivateKey("0x...");

// Получить баланс кошелька (EVM)
string balance = await blockchain.GetBalance();

// Прочитать данные из смарт-контракта
string result = await blockchain.ReadContract(contractAddress, "balanceOf", abi, address);

// Отправить транзакцию (legacy)
string txHash = await blockchain.SendTransaction("0xRecipient", 0.1m, "0x", 21000, 30_000_000_000);

// Отправить транзакцию (EIP-1559)
string txHash = await blockchain.SendTransactionEIP1559("0xRecipient", 0.1m, "0x", 21000, 40_000_000_000, 2_000_000_000);

// Генерация seed-фразы (12 слов, английский)
string seed = Blockchain.GenerateMnemonic();

// Получить пары адрес-приватный ключ для ETH из seed
var ethAccounts = Blockchain.MnemonicToAccountEth(seed, 5);

// Получить пары адрес-приватный ключ для BTC из seed
var btcAccounts = Blockchain.MnemonicToAccountBtc(seed, 5, "Bech32");

// Получить баланс по адресу (ETH, через RPC)
string wei = Blockchain.GetEthAccountBalance("0x...", "https://mainnet.infura.io/v3/...");
```


## Описание методов

### Конструкторы

```csharp
public Blockchain(string walletKey, int chainId, string jsonRpc)
public Blockchain(string jsonRpc)
public Blockchain()
```

- **walletKey**: приватный ключ кошелька (hex, с/без 0x)
- **chainId**: id сети (например, 1 для Ethereum mainnet)
- **jsonRpc**: RPC-эндпоинт EVM-сети


### Получение адреса по приватному ключу

```csharp
public string GetAddressFromPrivateKey(string privateKey)
```

- Возвращает EVM-адрес, соответствующий приватному ключу.


### Получение баланса кошелька

```csharp
public async Task<string> GetBalance()
```

- Возвращает баланс кошелька (в ETH, decimal в строке) по текущему приватному ключу и RPC.


### Вызов функции смарт-контракта (read-only)

```csharp
public async Task<string> ReadContract(string contractAddress, string functionName, string abi, params object[] parameters)
```

- Вызывает функцию контракта по ABI и возвращает результат.
- Поддерживает tuple, BigInteger, bool, string, byte[].


### Отправка транзакции (legacy)

```csharp
public async Task<string> SendTransaction(string addressTo, decimal amount, string data, BigInteger gasLimit, BigInteger gasPrice)
```

- Отправляет транзакцию с заданными параметрами (legacy, gasPrice).


### Отправка транзакции (EIP-1559)

```csharp
public async Task<string> SendTransactionEIP1559(string addressTo, decimal amount, string data, BigInteger gasLimit, BigInteger maxFeePerGas, BigInteger maxPriorityFeePerGas)
```

- Отправляет транзакцию с поддержкой EIP-1559 (maxFeePerGas, maxPriorityFeePerGas).


### Генерация seed-фразы

```csharp
public static string GenerateMnemonic(string wordList = "English", int wordCount = 12)
```

- Возвращает seed-фразу (12/15/18/21/24 слов) на выбранном языке (English, Japanese, Chinese Simplified/Traditional, Spanish, French, Portuguese, Czech).


### Получение адресов и приватных ключей из seed (ETH)

```csharp
public static Dictionary<string, string> MnemonicToAccountEth(string words, int amount)
```

- Возвращает словарь: адрес → приватный ключ для первых `amount` адресов по стандартному пути.


### Получение адресов и приватных ключей из seed (BTC)

```csharp
public static Dictionary<string, string> MnemonicToAccountBtc(string mnemonic, int amount, string walletType = "Bech32")
```

- **walletType**: "Bech32", "P2PKH compress", "P2PKH uncompress", "P2SH"
- Возвращает словарь: адрес → приватный ключ для первых `amount` адресов.


### Получение баланса по адресу (ETH)

```csharp
public static string GetEthAccountBalance(string address, string jsonRpc)
```

- Возвращает баланс адреса в Wei (строка).


### Работа с ABI (encode/decode)

#### Получение типов входных/выходных параметров функции

```csharp
public static string[] GetFuncInputTypes(string abi, string functionName)
public static Dictionary<string, string> GetFuncInputParameters(string abi, string functionName)
public static Dictionary<string, string> GetFuncOutputParameters(string abi, string functionName)
```

- Возвращают массив типов или словарь имя:тип для входных/выходных параметров функции.


#### Получение сигнатуры функции

```csharp
public static string GetFuncAddress(string abi, string functionName)
```

- Возвращает sha3-сигнатуру функции.


#### Декодирование данных из ответа

```csharp
public static Dictionary<string, string> AbiDataDecode(string abi, string functionName, string data)
```

- Декодирует hex-ответ функции по ABI и возвращает словарь имя:значение.


#### Энкодинг данных для вызова функции

```csharp
public static string EncodeTransactionData(string abi, string functionName, string[] types, object[] values)
public static string EncodeParam(string type, object value)
public static string EncodeParams(string[] types, object[] values)
public static string EncodeParams(Dictionary<string, string> parameters)
```

- Формируют строку данных для вызова функции (input data).


#### Вспомогательные методы

```csharp
public static object[] ValuesToArray(params dynamic[] inputValues)
```

- Преобразует массив значений в object[] для передачи в вызов.


## Особенности

- Использует библиотеки Nethereum и NBitcoin для работы с EVM и BTC.
- Поддержка всех популярных типов адресов и путей деривации.
- Корректная обработка ошибок, поддержка асинхронных операций.
- Универсальный интерфейс для генерации seed, получения адресов, работы с контрактами и транзакциями.
- Вспомогательные методы для ABI-энкодинга/декодинга и работы с параметрами функций.

