

##  Manager (WebWallet)

### Назначение

Класс **Manager** (или `WebWallet`) реализует универсальный интерфейс для автоматизации управления браузерными криптокошельками (MetaMask, Rabby, Backpack, Razor, Zerion, Keplr) в ZennoPoster: запуск и переключение расширений, активация/деактивация кошельков, подтверждение транзакций, выбор источника для Keplr, интеграция с менеджером расширений и ручное управление через профиль Chrome.

### Примеры использования

```csharp
// Запустить нужные кошельки по списку
manager.Launch("MetaMask,Backpack");

// Включить/отключить расширения по списку
manager.Switch("MetaMask,Backpack");

// Подтвердить транзакцию в нужном кошельке
manager.Approve("MetaMask");

// Установить источник для Keplr (seed/pkey)
manager.KeplrSetSource("seed");
```


## Описание методов

### Launch (IEnumerable)

```csharp
public void Launch(IEnumerable requiredWallets, bool log = false)
```

- Перебирает список кошельков и вызывает соответствующий метод запуска для каждого:
    - MetaMask: `MetaMaskWallet.MetaMaskLnch`
    - Rabby: `RabbyWallet.RabbyLnch`
    - Backpack: `BackpackWallet.Launch`
    - Razor: `RazorWallet.RazorLnch`
    - Zerion: `ZerionWallet.Launch`
    - Keplr: `KeplrWallet.KeplrLaunch`
- Логирует действия.
- Если кошелёк не распознан — пишет в лог.


### Launch (string)

```csharp
public void Launch(string requiredWallets, bool log = false)
```

- Принимает строку с перечнем кошельков через запятую.
- Преобразует в список через `ParseWallets` и вызывает основной `Launch`.


### Switch

```csharp
public void Switch(string toUse = "", bool log = false)
```

- Включает/выключает расширения-кошельки через One-Click-Extensions-Manager:

1. Открывает менеджер расширений.
2. Перебирает все расширения, сравнивает их имена/ID с `toUse`.
3. Включает нужные и выключает лишние (через клик по кнопке).
4. Восстанавливает исходные настройки эмуляции мыши.
- Если не удалось — пробует напрямую изменить файл профиля Chrome (`Secure Preferences`), меняет поле `state` у нужных расширений (1 — включено, 0 — выключено), сохраняет изменения.


### Approve (enum)

```csharp
public void Approve(W wallet, bool log = false)
```

- Подтверждает транзакцию/действие в нужном кошельке:
    - MetaMask: `MetaMaskWallet.MetaMaskConfirm`
    - Backpack: `BackpackWallet.Approve`
    - Keplr: `KeplrWallet.KeplrApprove`
    - Zerion: `ZerionWallet.Sign`
- Если кошелёк не поддерживает approve — выбрасывает исключение.


### Approve (string)

```csharp
public void Approve(string wallet, bool log = false)
```

- Принимает имя кошелька как строку, преобразует в enum и вызывает основной `Approve`.
- Если имя невалидно — выбрасывает исключение.


### KeplrSetSource

```csharp
public void KeplrSetSource(string source, bool log = false)
```

- Устанавливает нужный источник (seed/pkey) для Keplr через `KeplrWallet.KeplrSetSource`.


### ParseWallets

```csharp
private List<W> ParseWallets(string requiredWallets, bool log = false)
```

- Преобразует строку с перечнем кошельков в список enum-значений.
- Если имя невалидно — пишет в лог.


### WalLog

```csharp
private void WalLog(string message, bool log = false)
```

- Логирует сообщения с префиксом `[WalletManager]` через `project.SendInfoToLog`.

