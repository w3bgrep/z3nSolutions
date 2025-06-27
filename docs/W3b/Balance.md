

##  Balance

### Назначение

Класс **Balance** реализует универсальный интерфейс для получения балансов и вспомогательных данных по EVM, Solana, Sui, Aptos, Initia и другим сетям через RPC и публичные API. Поддерживает работу с основными стандартами токенов (ERC20, ERC721, ERC1155, SPL, SUI, APT), получение chainId, цены газа, nonce, а также массовый сбор балансов по нескольким сетям. Встроена поддержка прокси, логирование и универсальный парсер адресов.

### Примеры использования

```csharp
// Получить chainId сети
int chainId = balance.ChainId<int>();

// Получить цену газа в Gwei
decimal gas = balance.GasPrice<decimal>();

// Получить nonce для адреса
int nonce = balance.NonceEVM<int>();

// Получить баланс ERC20 токена
decimal usdt = balance.ERC20<decimal>("0xdAC17F958D2ee523a2206206994597C13D831ec7");

// Получить баланс NFT (ERC721) токенов
int nftCount = balance.ERC721<int>("0xNFTcontractAddress");

// Получить баланс SPL токена (Solana)
decimal sol = balance.SPL<decimal>("So11111111111111111111111111111111111111112");

// Получить баланс SUI токена
decimal sui = balance.SUIt<decimal>("0x2::sui::SUI");

// Получить баланс APT токена
decimal apt = balance.APTt<decimal>("0x1::aptos_coin::AptosCoin");

// Получить баланс Initia токена
decimal init = balance.INITt<decimal>();

// Получить список всех токенов ERC721 для адреса
List<BigInteger> tokenIds = balance.ERC721TokenIds("0xNFTcontractAddress", rpc, address);

// Получить балансы USDE по всем сетям
var dict = balance.DicToken(new[] { "ethereum", "arbitrum" });
```


## Описание методов

### ChainId

```csharp
public T ChainId(string rpc = null, string proxy = null, bool log = false)
```

- Получает chainId EVM-сети через RPC.
- Возвращает как int или string.


### GasPrice

```csharp
public T GasPrice(string rpc = null, string proxy = null, bool log = false)
```

- Получает текущую цену газа в Gwei.
- Возвращает decimal или string.


### NonceEVM

```csharp
public T NonceEVM(string rpc = null, string address = null, string proxy = null, bool log = false)
```

- Получает nonce (число исходящих транзакций) для адреса.
- Возвращает int или string.


### ERC20

```csharp
public T ERC20(
    string tokenContract,
    string rpc = null,
    string address = null,
    string tokenDecimal = "18",
    string proxy = null,
    bool log = false
)
```

- Получает баланс ERC20 токена.
- **tokenContract** — адрес токена.
- **tokenDecimal** — количество знаков после запятой (по умолчанию 18).
- Возвращает decimal или string.


### ERC721

```csharp
public T ERC721(
    string tokenContract,
    string rpc = null,
    string address = null,
    string proxy = null,
    bool log = false
)
```

- Получает количество ERC721 токенов (NFT) на адресе.
- Возвращает int, string или BigInteger.


### ERC1155

```csharp
public T ERC1155(
    string tokenContract,
    string tokenId,
    string rpc = null,
    string address = null,
    string proxy = null,
    bool log = false
)
```

- Получает баланс ERC1155 токена по tokenId.
- Возвращает int, string или BigInteger.


### SPL

```csharp
public T SPL(
    string tokenMint,
    string address = null,
    int floor = 0,
    string rpc = null,
    string proxy = null,
    bool log = false
)
```

- Получает баланс SPL токена на Solana.
- **tokenMint** — адрес токена.
- Возвращает decimal или string.


### SUIt

```csharp
public T SUIt(
    string coinType,
    string address = null,
    string rpc = null,
    string proxy = null,
    bool log = false
)
```

- Получает баланс токена в сети Sui.
- **coinType** — тип монеты (например, "0x2::sui::SUI").
- Возвращает decimal или string.


### APTt

```csharp
public T APTt(
    string coinType,
    string address = null,
    string rpc = null,
    string proxy = null,
    bool log = false
)
```

- Получает баланс токена в сети Aptos.
- **coinType** — тип монеты (например, "0x1::aptos_coin::AptosCoin").
- Возвращает decimal или string.


### INITt

```csharp
public T INITt(
    string address = null,
    string chain = "interwoven-1",
    string token = "uinit",
    bool log = false
)
```

- Получает баланс токена Initia через публичный API.
- **token** — тикер токена (по умолчанию "uinit").
- Возвращает decimal, double или string.


### ERC721TokenIds

```csharp
public List<BigInteger> ERC721TokenIds(
    string tokenContract,
    string rpc,
    string address,
    string proxy = null,
    bool log = false
)
```

- Получает список всех ID ERC721 токенов (NFT), принадлежащих адресу.
- Проверяет поддержку интерфейса ERC721Enumerable.
- Возвращает список BigInteger.


### DicToken

```csharp
public Dictionary<string, decimal> DicToken(
    string[] chainsToUse = null,
    bool log = false,
    string tokenEVM = null,
    string tokenSPL = null
)
```

- Получает балансы токена (например, USDE) по всем указанным сетям.
- **chainsToUse** — массив имён сетей.
- **tokenEVM**/**tokenSPL** — адреса токенов для EVM/Solana (по умолчанию USDE).
- Возвращает словарь: сеть → баланс.


## Вспомогательные детали

- **Прокси:** поддержка прокси-строки через параметр, `"+"` — брать из переменной проекта.
- **Логирование:** все методы поддерживают логирование через внешний Logger.
- **Парсинг адреса:** если адрес не указан — берётся из переменной проекта или базы.
- **Форматирование:** все числа приводятся к строке с нужной точностью через FloorDecimal.


## Особенности

- Универсальный интерфейс для всех популярных сетей: EVM, Solana, Sui, Aptos, Initia.
- Поддержка всех стандартных токенов: ERC20, ERC721, ERC1155, SPL, SUI, APT.
- Массовый сбор балансов по нескольким сетям.
- Корректная обработка ошибок и исключений, логирование всех этапов.
- Гибкая работа с прокси и адресами.
- Поддержка работы с большими числами (BigInteger).

