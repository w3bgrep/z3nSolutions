<img src="https://r2cdn.perplexity.ai/pplx-full-logo-primary-dark%402x.png" class="logo" width="120"/>

## Документация класса ChromeExt

Класс **ChromeExt** предоставляет инструменты для управления расширениями браузера Chromium в ZennoPoster: получение версии расширения, установка, включение/отключение, удаление расширений, а также автоматизация работы с менеджером расширений через интерфейс браузера.

---

### Пример использования

```csharp
// Создание экземпляра с логированием
var chromeExt = new ChromeExt(project, instance, log: true);

// Получение версии расширения по ID
string version = chromeExt.GetVer("pbgjpgbpljobkekbhnnmlikbbfhbhmem");

// Установка расширения из CRX-файла
bool installed = chromeExt.Install("pbgjpgbpljobkekbhnnmlikbbfhbhmem", "One-Click-Extensions-Manager.crx");

// Включение/отключение расширений по имени или ID
chromeExt.Switch("MetaMask, pbgjpgbpljobkekbhnnmlikbbfhbhmem");

// Удаление расширения по ID
chromeExt.Rm("pbgjpgbpljobkekbhnnmlikbbfhbhmem");

// Массовое удаление расширений
chromeExt.Rm(new string[] { "id1", "id2" });
```


---

## Описание конструкторов

```csharp
public ChromeExt(IZennoPosterProjectModel project, bool log = false)
```

- **log** (bool, необязательный): Включает расширенное логирование.

```csharp
public ChromeExt(IZennoPosterProjectModel project, Instance instance, bool log = false)
```

- **instance**: Экземпляр браузера для управления расширениями.
- **log** (bool, необязательный): Включает расширенное логирование.

---

## Описание методов

### GetVer

```csharp
public string GetVer(string extId)
```

- **extId** (string): ID расширения Chrome.
- **Назначение:**
Получает версию установленного расширения по его ID, анализируя файл профиля `Secure Preferences`.
В случае отсутствия секции или расширения выбрасывает исключение.

---

### Install

```csharp
public bool Install(string extId, string fileName, bool log = false)
```

- **extId** (string): ID расширения.
- **fileName** (string): Имя CRX-файла расширения (должен находиться в папке проекта с расширением `.crx`).
- **log** (bool, необязательный): Включить логирование установки.
- **Назначение:**
Устанавливает расширение из CRX-файла, если оно ещё не установлено.
В случае отсутствия файла выбрасывает исключение.
Возвращает true при успешной установке, false если уже установлено.

---

### Switch

```csharp
public void Switch(string toUse = "", bool log = false)
```

- **toUse** (string, необязательный): Список имён или ID расширений, которые должны быть включены (через запятую).
- **log** (bool, необязательный): Включить логирование процесса.
- **Назначение:**
Автоматически включает или отключает расширения, используя менеджер One-Click-Extensions-Manager.
Эмулирует действия пользователя для включения/отключения нужных расширений по имени или ID.

---

### Rm

```csharp
public void Rm(string[] ExtToRemove)
```

- **ExtToRemove** (string[]): Массив ID расширений для удаления.
- **Назначение:**
Массовое удаление расширений по ID.

```csharp
public void Rm(string ExtToRemove)
```

- **ExtToRemove** (string): ID расширения для удаления.
- **Назначение:**
Удаляет расширение по ID. В случае ошибки удаление игнорируется.

---

## Вспомогательные (внутренние) методы

- `Install(string extId, string fileName, bool log = false)`: Проверяет наличие расширения и устанавливает его из CRX-файла.
- `Switch(string toUse = "", bool log = false)`: Автоматизирует включение/отключение расширений через интерфейс менеджера.
- `Rm(string ExtToRemove)`: Удаляет расширение по ID через Instance.
- `Rm(string[] ExtToRemove)`: Массовое удаление расширений.

---

## Особенности

- Получение версии расширения происходит напрямую из профиля пользователя (файл Secure Preferences).
- Для управления расширениями используется эмуляция действий пользователя через менеджер One-Click-Extensions-Manager.
- Все действия логируются через интеграцию с внешним Logger.
- Методы безопасны к ошибкам: при невозможности удалить расширение исключения не прерывают выполнение.

---

**ChromeExt** — вспомогательный класс для автоматизации управления расширениями Chromium в проектах ZennoPoster: установка, удаление, включение/отключение и получение информации о расширениях.

<div style="text-align: center">⁂</div>

[^1]: ChromeExt.cs

