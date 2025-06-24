
## ProjectExtensions

### Назначение

`ProjectExtensions` — набор extension-методов для `IZennoPosterProjectModel`, расширяющий стандартные возможности работы с переменными, логированием, глобальными переменными, выбором аккаунтов, обработкой ошибок, работой с файлами и таблицами в проектах ZennoPoster.

---

### Примеры использования

```csharp
// Логирование с автоматическим определением вызывающего метода
project.L0g("Сообщение для лога");

// Получение информации об ошибке последнего действия
string[] err = project.GetErr(instance);

// Установка значения переменной acc0
project.acc0w("12345");

// Получение диапазона аккаунтов из строки "1-10"
int max = project.Range("1-10");

// Инкрементировать счетчик переменной
int newVal = project.VarCounter("myVar", 1);

// Получить значение переменной
string val = project.Var("myVar");

// Установить значение переменной
project.Var("myVar", "newValue");

// Получить случайное значение из диапазона переменной
string rnd = project.VarRnd("myVar");

// Математическая операция над переменными
decimal res = project.VarsMath("varA", '+', "varB", "resultVar");

// Работа с глобальными переменными
bool ok = project.GlobalSet();
project.GlobalGet();
project.GlobalNull();

// Выбрать случайный аккаунт из списка accs
bool selected = project.ChooseSingleAcc();

// Получить свежие креды из файла
string creds = project.GetNewCreds("mail");

// Построить структуру таблицы для базы
var tbl = project.TblMap(new string[] { "email", "pass" }, "todo1,todo2");
```


---

## Описание методов

### Логирование и обработка ошибок

```csharp
public static void L0g(this IZennoPosterProjectModel project, string toLog, [CallerMemberName] string callerName = "", bool show = true, bool thr0w = false, bool toZp = true)
```

- Логирует сообщение через внешний Logger с поддержкой кастомизации и выбрасыванием исключения.

```csharp
public static string[] GetErr(this IZennoPosterProjectModel project, Instance instance)
```

- Возвращает массив строк с деталями последней ошибки (ActionId, тип, сообщение, stacktrace, innerException).
- Формирует failReport и сохраняет скриншот при ошибке.

---

### Работа с переменными

```csharp
public static void acc0w(this IZennoPosterProjectModel project, object acc0)
```

- Устанавливает значение переменной `acc0`.

```csharp
public static int Range(this IZennoPosterProjectModel project, string accRange = null, string output = null, bool log = false)
```

- Парсит диапазон аккаунтов из строки вида "1-10" или "1,2,3".
- Заполняет переменные rangeStart, rangeEnd, range.

```csharp
public static int VarCounter(this IZennoPosterProjectModel project, string varName, int input)
```

- Инкрементирует (или декрементирует) числовую переменную.

```csharp
public static string Var(this IZennoPosterProjectModel project, string Var)
public static string Var(this IZennoPosterProjectModel project, string var, string value)
```

- Получает или устанавливает значение переменной по имени.

```csharp
public static string VarRnd(this IZennoPosterProjectModel project, string Var)
```

- Если значение переменной содержит диапазон (например, "10-20"), возвращает случайное число из диапазона.

```csharp
public static decimal VarsMath(this IZennoPosterProjectModel project, string varA, char operation, string varB, string varRslt = "a_")
```

- Выполняет математическую операцию (+, -, *, /) над двумя переменными и сохраняет результат.

---

### Работа с глобальными переменными

```csharp
public static bool GlobalSet(this IZennoPosterProjectModel project, bool log = false)
```

- Привязывает текущий поток (accN) к проекту через глобальные переменные, очищает занятые, логирует занятость.

```csharp
public static void GlobalGet(this IZennoPosterProjectModel project)
```

- Получает список занятых аккаунтов, очищает если задано.

```csharp
public static void GlobalNull(this IZennoPosterProjectModel project)
```

- Сбрасывает глобальную переменную для текущего аккаунта.

---

### Выбор аккаунта

```csharp
public static bool ChooseSingleAcc(this IZennoPosterProjectModel project)
```

- Случайным образом выбирает аккаунт из списка `accs`, удаляет его из списка, проверяет занятость через глобальные переменные, логирует результат.

---

### Работа с файлами аккаунтов

```csharp
public static string GetNewCreds(this IZennoPosterProjectModel project, string dataType)
```

- Получает первую строку из файла fresh, переносит её в файл used, возвращает строку.

---

### Построение структуры таблицы

```csharp
public static Dictionary<string, string> TblMap(this IZennoPosterProjectModel project, string[] staticColumns, string dynamicToDo = null, string defaultType = "TEXT DEFAULT ''")
```

- Формирует структуру таблицы для базы данных: добавляет статические и динамические колонки (например, из cfgToDo).

---

## Вспомогательные (внутренние) методы

- `LockObject`, `FileLock`: Для потокобезопасности при работе с глобальными переменными и файлами.
- Логирование и обработка ошибок интегрированы с внешним Logger.

---

## Особенности

- Все методы реализованы как extension для `IZennoPosterProjectModel`.
- Логирование и обработка ошибок централизованы.
- Поддержка потокобезопасности при работе с глобальными переменными и файлами.
- Удобные методы для парсинга диапазонов, работы с переменными, управления аккаунтами и построения структуры таблиц.


