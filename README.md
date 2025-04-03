ИИ шаб ридми (не воспринимать за правду (пока эот сообщение тут))

# w3tools

Библиотека `w3tools` — это набор утилит и классов для автоматизации задач в ZennoPoster, включая работу с блокчейнами, базами данных, шифрованием, логированием и взаимодействием с веб-интерфейсами. 
Иными словами это мультитул для Web3 автоматизации.



## Установка

1. Скопируйте dll из ExternalAssemblies в дирректорию  "c:\Program Files\ZennoLab\RU\ZennoPoster Pro V7\7.7.21.0\Progs\ExternalAssemblies\"  - версия и диск могут отличаться в зависимости от вашей системы.
2. Скопируйте w3tools.sc (или w3t00ls.sc) себе на компьютер

## Зависимости

- ZennoLab.CommandCenter
- Nethereum.Web3
- NBitcoin
- Newtonsoft.Json
- Leaf.xNet
- Npgsql
- ZXing

## Основные модули

### 1. Migrate
Устаревшие методы для обратной совместимости. Рекомендуется заменить на актуальные вызовы из других классов.

### 2. SAFU
Шифрование и дешифрование данных с использованием AES.
- `Encode` — шифрование строки.
- `Decode` — дешифрование строки.
- `HWPass` — получение ключа шифрования.

### 3. Loggers
Логирование с форматированием и поддержкой уровней (Info, Warning, Error).
- `W3Log` — основное логирование.
- `Report` — отправка отчета в Telegram.
- `l0g` — гибкое логирование с настройкой типа и цвета.

### 4. OnStart
Инициализация проекта.
- `InitVariables` — настройка начальных переменных.
- `SetRange` — установка диапазона аккаунтов.
- `SetProfile` — настройка профиля браузера.

### 5. SQL
Работа с базами данных SQLite и PostgreSQL.
- `W3Query` — выполнение SQL-запросов.
- `W3MakeTable` — создание таблиц.

### 6. Http
HTTP-запросы.
- `W3Get` — простой GET-запрос.
- `W3Post` — POST-запрос с настройками.

### 7. InstanceExtensions
Расширения для работы с экземплярами ZennoPoster.
- `SetProxy` — установка прокси.
- `SetCookiesFromDB` — загрузка cookies из базы данных.
- `BrowserScanCheck` — проверка уникальности браузера через browserscan.net.

### 8. Blockchain
Работа с блокчейнами (Ethereum, Bitcoin).
- `GetBalance` — получение баланса.
- `SendTransaction` — отправка транзакций.
- `MnemonicToAccountEth` — генерация адресов из мнемонической фразы.

### 9. HanaGarden
Интеграция с GraphQL API HanaGarden.
- `GetGardenInfo` — получение информации о саде.
- `ExecuteGrowAll` — выполнение действий роста.
- `CollectRewards` — сбор наград.

## Примеры использования

### Инициализация проекта
```csharp
w3tools.OnStart.IntitProjectEvironment(project);