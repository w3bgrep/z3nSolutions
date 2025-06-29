

## Google

### Назначение

Класс `Google` реализует автоматизацию авторизации и работы с аккаунтом Google в ZennoPoster: получение и установка учётных данных из базы, прохождение многошаговой авторизации (логин, пароль, 2FA, капча, восстановление), проверку состояния аккаунта, управление cookies и обработку типичных сценариев входа.

### Примеры использования

```csharp
// Создание экземпляра с логированием
var google = new Google(project, instance, log: true);

// Авторизация в Google-аккаунте (все этапы, cookies сохраняются)
string state = google.Load();

// Получить статус авторизации (без действий)
string status = google.GetState();

// Принудительный выбор пользователя в Google-аккаунте
string authResult = google.GAuth();

// Сохранить cookies аккаунта в базу
google.SaveCookies();
```


## Описание методов

### Конструктор

```csharp
public Google(IZennoPosterProjectModel project, Instance instance, bool log = false)
```

- Загружает креды из базы (`private_google`): статус, логин, пароль, OTP, резервный email, коды восстановления, cookies.
- Заполняет переменные проекта:
`googleSTATUS`, `googleLOGIN`, `googlePASSWORD`, `google2FACODE`, `googleSECURITY_MAIL`, `googleBACKUP_CODES`, `googleCOOKIES`.
- Включает логирование через внешний Logger.


### Load

```csharp
public string Load(bool log = false, bool cookieBackup = true)
```

- Открывает страницу Google-аккаунта.
- Проходит все этапы авторизации:
    - Ввод логина, пароля, OTP-кода (2FA), обработка капчи, отмена добавления телефона.
    - При ошибке логина/пароля — чистит куки и повторяет попытку.
    - При появлении капчи — вызывает CapGuru и повторяет попытку.
    - При ошибках браузера или блокировках — обновляет статус в базе и выбрасывает исключение.
- После успешного входа сохраняет cookies (если `cookieBackup = true`).
- Возвращает итоговый статус (`ok`, `wrong`, `inputLogin`, `inputPassword`, `inputOtp`, `CAPCHA`, `phoneVerify`, `badBrowser` и др.).


### GetState

```csharp
public string GetState(bool log = false)
```

- Анализирует DOM текущей вкладки и определяет состояние авторизации:
    - `signedIn` — пользователь вошёл
    - `CAPCHA` — требуется капча
    - `PhoneVerify` — требуется подтверждение по телефону
    - `BadBrowser` — Google требует другой браузер
    - `inputLogin` — требуется ввод логина
    - `inputPassword` — требуется ввод пароля
    - `inputOtp` — требуется ввод OTP-кода
    - `addRecoveryPhone` — Google предлагает добавить телефон
    - `undefined` — состояние не определено
- Для `signedIn` проверяет, совпадает ли текущий аккаунт с ожидаемым логином:
    - Если совпадает, возвращает `ok`
    - Если не совпадает, чистит куки и возвращает `wrong`
- Для `undefined` пытается кликнуть по кнопке входа и повторяет проверку.
- Логирует все статусы.


### GAuth

```csharp
public string GAuth(bool log = false)
```

- Принудительно выбирает нужного пользователя в Google-аккаунте:
    - Ищет контейнер пользователя с совпадающим логином.
    - Кликает по нему, ожидает появления нужного состояния.
    - Если требуется — кликает по кнопке "Continue".
- Если выбран неверный аккаунт — чистит куки и закрывает вкладки.
- Возвращает результат (`SUCCESS with continue`, `SUCCESS. without confirmation`, `FAIL. Wrong account`, `FAIL. No loggined Users Found`).


### SaveCookies

```csharp
public void SaveCookies()
```

- Переходит на YouTube и страницу аккаунта Google.
- Ждёт загрузки.
- Получает cookies текущего аккаунта через вспомогательный класс `Cookies`.
- Сохраняет cookies в базу (`private_google`).


## Вспомогательные (внутренние) методы

- `DbCreds()`: Загружает креды из базы и заполняет переменные проекта.
- Логирование всех этапов через внешний Logger.
- Использование класса `Sql` для работы с базой.


