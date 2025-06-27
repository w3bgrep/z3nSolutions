

## X

### Назначение

Класс `X` реализует автоматизацию работы с аккаунтом X (Twitter) в ZennoPoster: загрузка и обновление учётных данных, авторизация через браузер, управление токеном, генерация твитов через внешний API, парсинг профиля и секьюрити-страницы, а также обработку статусов аккаунта.

### Примеры использования

```csharp
// Создать экземпляр с логированием
var x = new X(project, instance, log: true);

// Загрузить и актуализировать аккаунт
string status = x.Xload();

// Пройти авторизацию в браузере (пошагово)
x.XAuth();

// Получить токен и записать в базу
string token = x.XgetToken();

// Сгенерировать твит через внешний API
string tweet = x.GenerateTweet("Тема для твита", "bio пользователя");

// Спарсить профиль и обновить базу
x.ParseProfile();

// Спарсить секьюрити-страницу и обновить базу
x.ParseSecurity();
```


## Описание методов

### Конструктор

```csharp
public X(IZennoPosterProjectModel project, Instance instance, bool log = false)
```

- Загружает креды из базы (`private_twitter`): статус, токен, логин, пароль, 2FA, email, email_pass.
- Заполняет переменные проекта:
`twitterSTATUS`, `twitterTOKEN`, `twitterLOGIN`, `twitterPASSWORD`, `twitterCODE2FA`, `twitterEMAIL`, `twitterEMAIL_PASSWORD`.
- Включает логирование.


### Log

```csharp
protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
```

- Логирует сообщение через внешний Logger.
- Поддерживает автоматическое определение вызывающего метода.


### Xload

```csharp
public string Xload(bool log = false)
```

- Основная точка входа.
- Проверяет статус аккаунта через `XcheckState`.
- Если требуется логин — пытается авторизоваться через токен, затем через форму.
- При статусах `restricted`, `suspended`, `emailCapcha` — обновляет статус в базе и возвращает его.
- При статусе `ok` — обновляет токен и возвращает статус.
- При смешанном или неизвестном статусе — чистит куки, кэш, закрывает вкладки и повторяет попытку.
- При ошибке или таймауте — выбрасывает исключение.


### XAuth

```csharp
public void XAuth()
```

- Пошаговая авторизация через браузер:
    - Клик по кнопке логина, ввод логина, клик "Next".
    - Ввод пароля, клик "Login".
    - Ввод OTP, клик "Next".
    - Обработка ошибок: NotFound, Suspended, WrongPass, SuspiciousLogin.
    - Если требуется — выбор пользователя, подтверждение OAuth.
- При ошибке обновляет статус в базе и выбрасывает исключение.


### XcheckState

```csharp
private string XcheckState(bool log = false)
```

- Навигирует на профиль пользователя.
- Проверяет DOM на наличие признаков:
    - "restricted", "suspended", "login", "emailCapcha", "ok", "mixed".
- При статусе "ok" сверяет логин с ожидаемым.
- При ошибке или таймауте — выбрасывает исключение.


### Xlogin

```csharp
private string Xlogin()
```

- Выполняет авторизацию через форму:
    - Ввод логина, пароль, OTP (2FA).
    - Обработка ошибок: NotFound, WrongPass, Suspended, SuspiciousLogin.
    - Получает токен после успешного входа.
- Возвращает статус (`ok`, `WrongPass`, `NotFound`, `Suspended`, `SuspiciousLogin`, `SomethingWentWrong`).


### XsetToken

```csharp
private void XsetToken()
```

- Устанавливает токен в cookies через JS и перезагружает страницу.


### XgetToken

```csharp
private string XgetToken()
```

- Получает токен из cookies (auth_token), обновляет переменную проекта и базу.
- Возвращает токен.


### GenerateTweet

```csharp
public string GenerateTweet(string content, string bio = "", bool log = false)
```

- Генерирует твит через API Perplexity.
- Формирует JSON-запрос с параметрами (content, bio).
- Парсит ответ, возвращает текст твита (до 220 символов).
- Повторяет попытку при превышении лимита.
- Логирует все этапы и ошибки.


### UpdXCreds

```csharp
public void UpdXCreds(Dictionary data)
```

- Обновляет креды в базе (`twitter`) по данным из словаря.
- Корректно экранирует значения для SQL.


### ParseProfile

```csharp
public void ParseProfile()
```

- Переходит на страницу профиля.
- Получает JSON с данными профиля.
- Парсит и обновляет в базе: дату создания, id, username, description, имя, локацию, аватар, баннер, фолловеры, following, твиты.
- Заполняет список `editProfile` для пустых полей.


### ParseSecurity

```csharp
public void ParseSecurity()
```

- Переходит на страницу секьюрити-данных.
- Вводит пароль для подтверждения.
- Собирает данные: email, телефон, дата создания, страна, язык, пол, дата рождения.
- Обновляет данные в базе (`public_twitter`).
- Заполняет список `editSecurity` для пустых или невалидных полей.


## Вспомогательные детали

- Все сетевые и файловые операции потокобезопасны.
- Для работы с базой используется класс `Sql`.
- Для логирования — внешний Logger (`L0g`).
- Для работы с cookies — вспомогательный класс `Cookies`.
- Для генерации OTP — класс `OTP`.
- Все статусы и ошибки логируются и/или обновляются в базе.


