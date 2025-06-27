

##  KeplrWallet

### Назначение

Класс **KeplrWallet** автоматизирует работу с расширением-кошельком Keplr (Cosmos/EVM) в ZennoPoster: установка и импорт кошелька, разблокировка, выбор источника (seed/private key), одобрение транзакций, управление импортированными кошельками, а также интеграцию с базой и аппаратно-зависимым паролем.

### Примеры использования

```csharp
// Импортировать кошелёк и выбрать источник (seed/pkey)
var wallet = new KeplrWallet(project, instance, log: true, key: "privkey", seed: "seed words");
string result = wallet.KeplrMain(source: "pkey");

// Запустить установку/разблокировку Keplr
wallet.KeplrLaunch(source: "seed");

// Одобрить транзакцию в Keplr
wallet.KeplrApprove();

// Удалить все кошельки кроме нужных
wallet.KeplrPrune();
```


## Описание основных методов

### KeplrMain

```csharp
public string KeplrMain(string source = "pkey", string fileName = null, bool log = false)
```

- Полный цикл подготовки Keplr: установка, импорт, разблокировка, выбор источника.
- Автоматически определяет состояние расширения:
    - **install**: устанавливает расширение.
    - **import**: импортирует seed-фразу.
    - **inputPass**: разблокирует кошелёк.
    - **setSourse**: выбирает нужный источник (seed/pkey).
- Возвращает строку с результатом (`Keplr set from {source}`).


### KeplrLaunch

```csharp
public void KeplrLaunch(string source = "seed", string fileName = null, bool log = false)
```

- Устанавливает расширение и импортирует seed/private key.
- Если расширение уже установлено — разблокирует и выбирает нужный источник.
- Закрывает лишние вкладки после завершения.


### KeplrApprove

```csharp
public string KeplrApprove(bool log = false)
```

- Ожидает появления вкладки Keplr, кликает по кнопке Approve.
- Ждёт закрытия вкладки, повторяет клик при необходимости.
- Возвращает `"done"` при успехе, выбрасывает исключение при ошибке или таймауте.


### KeplrCheck

```csharp
private string KeplrCheck(bool log = false)
```

- Определяет текущее состояние расширения Keplr:
    - Не установлено → `"install"`
    - Требуется импорт → `"import"`
    - Требуется разблокировка → `"inputPass"`
    - Готово к выбору источника → `"setSourse"`
- Навигирует на popup.html, анализирует DOM.
- Возвращает строку-состояние.


### KeplrImportSeed

```csharp
public void KeplrImportSeed(bool log = false)
```

- Импортирует кошелёк по seed-фразе:
    - Пошагово вводит слова seed в поля.
    - Вводит имя кошелька, пароль и подтверждение.
    - Кликает Import/Next/Save.
    - Ждёт завершения и закрывает лишние вкладки.


### KeplrImportPkey

```csharp
public void KeplrImportPkey(bool temp = false, bool log = false)
```

- Импортирует кошелёк по приватному ключу:
    - Вводит ключ, имя кошелька, пароль.
    - Кликает Import/Next/Save.
    - Ждёт завершения и закрывает лишние вкладки.


### KeplrSetSource

```csharp
public void KeplrSetSource(string source, bool log = false)
```

- Открывает меню выбора кошелька.
- Если импортированы оба типа (seed и pkey) — выбирает нужный.
- Если не все импортированы — добавляет недостающий (через ImportPkey).
- Использует метод KeplrPrune для анализа и очистки.


### KeplrUnlock

```csharp
public void KeplrUnlock(bool log = false)
```

- Разблокирует кошелёк по паролю.
- Вводит пароль, кликает Unlock.
- При ошибке (неверный пароль) — удаляет расширение и выбрасывает исключение.
- Повторяет попытку при сбое интерфейса.


### KeplrPrune

```csharp
public string KeplrPrune(bool keepTemp = false, bool log = false)
```

- Удаляет все кошельки кроме нужных (seed/pkey/temp).
- Находит все плитки кошельков, кликает по меню, выбирает Delete, подтверждает паролем.
- Возвращает строку с типами оставшихся кошельков.


### KeplrClick

```csharp
public void KeplrClick(HtmlElement he)
```

- Кликает по элементу Keplr с учётом смещения координат для корректной эмуляции.


## Вспомогательные детали

- **KeyCheck/SeedCheck**: автоматически подгружают ключ/seed из базы, если не переданы явно.
- **Log**: логирование всех этапов через внешний Logger.
- **Потокобезопасность**: все действия снабжены таймаутами и контролем состояния.
- **Работа с DOM**: все действия реализованы через методы поиска и клика по элементам (`HeClick`, `HeSet`, `HeGet`).
- **Пароль**: используется аппаратно-зависимый HWPass.


