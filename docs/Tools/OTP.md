
##  OTP

### Назначение

Класс **OTP** реализует вспомогательные методы для работы с одноразовыми кодами (One Time Password, TOTP) и получения OTP-кодов из почтового ящика через API firstmail.ltd. Используется для автоматизации двухфакторной аутентификации (2FA) и подтверждения действий в проектах ZennoPoster.

### Примеры использования

```csharp
// Получить актуальный TOTP-код по секрету (base32)
string code = OTP.Offline("JBSWY3DPEHPK3PXP");

// Получить OTP-код из письма через FirstMail API
string otp = OTP.FirstMail(project, email: "user@gmail.com", proxy: "socks5://...");
```


## Описание методов

### Offline

```csharp
public static string Offline(string keyString, int waitIfTimeLess = 5)
```

- **keyString**: секретный ключ в формате base32 (например, для Google Authenticator).
- **waitIfTimeLess**: если до смены кода осталось меньше указанного количества секунд, метод дождётся генерации нового кода (по умолчанию 5 секунд).
- **Логика:**

1. Декодирует ключ из base32.
2. Генерирует TOTP-код через библиотеку OtpNet.
3. Если до смены кода осталось ≤ waitIfTimeLess секунд — ждёт, пока появится новый код, и возвращает его.
4. Возвращает актуальный 6-значный код.


### FirstMail

```csharp
public static string FirstMail(IZennoPosterProjectModel project, string email = "", string proxy = "")
```

- **project**: объект проекта ZennoPoster.
- **email**: адрес, на который должен прийти OTP (по умолчанию — из переменной `googleLOGIN`).
- **proxy**: строка прокси (опционально).
- **Логика:**

1. Получает логин и пароль для firstmail из переменных проекта (`settingsFmailLogin`, `settingsFmailPass`).
2. Формирует запрос к API firstmail.ltd для получения последнего письма.
3. Передаёт заголовки, включая API-ключ (`settingsApiFirstMail`), User-Agent, язык и др.
4. Выполняет HTTP GET-запрос через ZennoPoster.HttpGet.
5. Парсит JSON-ответ: извлекает адрес доставки (`deliveredTo`), текст и html письма.
6. Проверяет, что письмо пришло на нужный email.
7. Ищет 6-значный OTP-код в тексте письма (сначала в `text`, затем в `html`).
8. Если код найден — возвращает его.
9. Если код не найден — выбрасывает исключение ("OTP not found in message with correct email").
10. Если письмо не на нужный email — выбрасывает исключение с указанием email.


## Особенности

- Для генерации TOTP используется библиотека OtpNet (совместимо с Google Authenticator).
- Метод Offline гарантирует получение свежего кода, даже если до смены осталось мало времени.
- Для работы с почтой через FirstMail требуется заранее настроенный API-ключ и учётные данные в переменных проекта.
- Все ошибки (отсутствие кода, неправильный email) выбрасываются как исключения с подробным сообщением.
- Поддерживается работа с прокси при запросе к API.

