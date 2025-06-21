

## F0rms

Класс **F0rms** предоставляет набор вспомогательных методов для интерактивного ввода данных через формы Windows Forms. Позволяет получать строки, пары ключ-значение, булевы значения и выбирать элементы из списка с помощью модальных окон.

---

### Пример использования

```csharp
// Получить строку от пользователя
string input = F0rms.InputBox("Введите комментарий", 400, 200);

// Получить словарь по строкам
var dict = forms.GetLinesByKey("email", "Введите email-адреса построчно");

// Получить несколько пар ключ-значение
var pairs = forms.GetKeyValuePairs(3, new List<string> { "user", "pass", "domain" });

// Получить несколько пар ключ-логическое значение
var boolPairs = forms.GetKeyBoolPairs(2, new List<string> { "isAdmin", "isActive" });

// Получить строку из нескольких пар для SQL
string sqlPairs = forms.GetKeyValueString(2);

// Выбрать элемент из списка
string selected = forms.GetSelectedItem(new List<string> { "Вариант 1", "Вариант 2" });
```


---

## Описание методов

### InputBox

```csharp
public static string InputBox(string message = "input data please", int width = 600, int height = 600)
```

- **message** (string, необязательный): Текст заголовка окна и подсказки для пользователя.
- **width** (int, необязательный): Ширина окна формы.
- **height** (int, необязательный): Высота окна формы.

**Назначение:**
Открывает простую форму с многострочным полем для ввода текста и кнопкой "OK". Возвращает введённую строку после закрытия окна.

---

### GetLinesByKey

```csharp
public Dictionary<string, string> GetLinesByKey(string keycolumn = "input Column Name", string title = "Input data line per line")
```

- **keycolumn** (string, необязательный): Имя столбца (ключа), которое будет использовано для каждой строки.
- **title** (string, необязательный): Заголовок формы.

**Назначение:**
Открывает форму для ввода ключа (названия столбца) и списка строк (значений). Каждая строка преобразуется в пару ключ=значение и добавляется в словарь. Возвращает словарь или null, если ввод отменён или некорректен.

---

### GetKeyValuePairs

```csharp
public Dictionary<string, string> GetKeyValuePairs(
    int quantity,
    List<string> keyPlaceholders = null,
    List<string> valuePlaceholders = null,
    string title = "Input Key-Value Pairs",
    bool prepareUpd = true)
```

- **quantity** (int): Количество пар ключ-значение для ввода.
- **keyPlaceholders** (List<string>, необязательный): Список плейсхолдеров для полей ключей.
- **valuePlaceholders** (List<string>, необязательный): Список плейсхолдеров для полей значений.
- **title** (string, необязательный): Заголовок формы.
- **prepareUpd** (bool, необязательный): Если true, ключи в результирующем словаре будут номерами строк, а значения — строками вида `ключ='значение'`. Если false, ключи и значения — как введены.

**Назначение:**
Открывает форму для ввода нескольких пар ключ-значение. Позволяет задать подсказки для каждого поля. Возвращает словарь с парами или null при отмене/ошибке.

---

### GetKeyBoolPairs

```csharp
public Dictionary<string, bool> GetKeyBoolPairs(
    int quantity,
    List<string> keyPlaceholders = null,
    List<string> valuePlaceholders = null,
    string title = "Input Key-Bool Pairs",
    bool prepareUpd = true)
```

- **quantity** (int): Количество пар ключ-логическое значение.
- **keyPlaceholders** (List<string>, необязательный): Плейсхолдеры для ключей.
- **valuePlaceholders** (List<string>, необязательный): Плейсхолдеры для чекбоксов (текст подписи).
- **title** (string, необязательный): Заголовок формы.
- **prepareUpd** (bool, необязательный): Если true, ключи в словаре будут номерами строк, иначе — как введены.

**Назначение:**
Открывает форму для ввода нескольких ключей и соответствующих им булевых значений (чекбоксы). Возвращает словарь или null при отмене/ошибке.

---

### GetKeyValueString

```csharp
public string GetKeyValueString(
    int quantity,
    List<string> keyPlaceholders = null,
    List<string> valuePlaceholders = null,
    string title = "Input Key-Value Pairs")
```

- **quantity** (int): Количество пар ключ-значение.
- **keyPlaceholders** (List<string>, необязательный): Плейсхолдеры для ключей.
- **valuePlaceholders** (List<string>, необязательный): Плейсхолдеры для значений.
- **title** (string, необязательный): Заголовок формы.

**Назначение:**
Открывает форму для ввода нескольких пар ключ-значение и возвращает строку, объединяющую пары в формате `ключ='значение', ...`, пригодную для SQL или конфигов. Возвращает null при отмене/ошибке.

---

### GetSelectedItem

```csharp
public string GetSelectedItem(
    List<string> items,
    string title = "Select an Item",
    string labelText = "Select:")
```

- **items** (List<string>): Список вариантов для выбора.
- **title** (string, необязательный): Заголовок формы.
- **labelText** (string, необязательный): Подпись к выпадающему списку.

**Назначение:**
Открывает форму с выпадающим списком для выбора одного элемента из списка. Возвращает выбранный элемент или null при отмене/ошибке.

---

## Вспомогательные (внутренние) методы

- `SendInfoToLog(string message, bool important = false)`: Логирует информационные сообщения.
- `SendWarningToLog(string message, bool important = false)`: Логирует предупреждения.

---

## Особенности

- Все формы являются модальными и блокируют выполнение до закрытия окна.
- Поддерживается ввод больших объёмов текста (например, списки email-адресов).
- Ввод можно отменить — в этом случае методы возвращают null.
- Для всех методов реализована базовая валидация и логирование ошибок/отмены ввода.

---

**F0rms** — универсальный вспомогательный класс для интерактивного сбора данных через формы Windows Forms в автоматизированных проектах.



