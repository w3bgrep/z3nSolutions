

##  ISAFU

### Назначение

**SAFU** — универсальный статический класс для симметричного шифрования и дешифрования строк в проектах ZennoPoster с использованием пользовательского PIN-кода (`cfgPin`). Поддерживает аппаратно-зависимый пароль (HWPass) и расширяем через интерфейс **ISAFU** для реализации альтернативных методов защиты.

### Примеры использования

```csharp
// Инициализация SAFU (вызывается один раз в начале работы)
SAFU.Initialize(project);

// Зашифровать строку с использованием cfgPin
string encrypted = SAFU.Encode(project, "секретные_данные");

// Расшифровать строку с использованием cfgPin
string decrypted = SAFU.Decode(project, encrypted);

// Получить аппаратно-зависимый пароль
string hwPass = SAFU.HWPass(project);
```


## Описание интерфейса ISAFU

```csharp
public interface ISAFU
{
    string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log);
    string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log);
    string HWPass(IZennoPosterProjectModel project, bool log);
}
```

- **Encode**: шифрует строку с использованием PIN-кода из переменной проекта `cfgPin`.
- **Decode**: дешифрует строку с использованием PIN-кода.
- **HWPass**: возвращает аппаратно-зависимый пароль на основе серийного номера материнской платы и текущего аккаунта.


## Описание класса SimpleSAFU (реализация ISAFU)

### Encode

```csharp
public string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log)
```

- Если `cfgPin` не задан — возвращает исходную строку.
- Если строка пустая — возвращает пустую строку.
- Иначе шифрует строку с помощью AES и PIN-кода из `cfgPin`.


### Decode

```csharp
public string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log)
```

- Если `cfgPin` не задан — возвращает исходную строку.
- Если строка пустая — возвращает пустую строку.
- Иначе пытается расшифровать строку с помощью AES и PIN-кода из `cfgPin`.
- При ошибке логирует исключение и пробрасывает его дальше.


### HWPass

```csharp
public string HWPass(IZennoPosterProjectModel project, bool log)
```

- Получает серийный номер материнской платы через WMI (`Win32_BaseBoard.SerialNumber`).
- Склеивает его с текущим аккаунтом (`project.Var("acc0")`).
- Возвращает результат как аппаратно-зависимый пароль.


## Описание класса SAFU

### Initialize

```csharp
public static void Initialize(IZennoPosterProjectModel project)
```

- Регистрирует реализации функций Encode, Decode и HWPass в FunctionStorage, если они ещё не добавлены.
- По умолчанию используется SimpleSAFU.


### Encode

```csharp
public static string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log = false)
```

- Если `cfgPin` не задан — возвращает исходную строку.
- Иначе вызывает зарегистрированную функцию Encode.


### Decode

```csharp
public static string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log = false)
```

- Если `cfgPin` не задан — возвращает исходную строку.
- Иначе вызывает зарегистрированную функцию Decode.


### HWPass

```csharp
public static string HWPass(IZennoPosterProjectModel project, bool log = false)
```

- Вызывает зарегистрированную функцию HWPass.


## Вспомогательные детали

- Для шифрования/дешифрования используется AES (реализован в отдельном классе AES).
- Все функции потокобезопасны (используется ConcurrentDictionary).
- Для логирования ошибок и событий используются стандартные методы проекта.
- Можно расширять через собственные реализации ISAFU и регистрацию в FunctionStorage.


