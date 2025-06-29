

##  Rnd

### Назначение

Класс **Rnd** реализует генерацию случайных данных для проектов ZennoPoster: seed-фраз, hex-строк, хэшей, никнеймов, случайных чисел и процентных значений. Используется для автоматизации тестирования, генерации уникальных данных, имитации пользовательских сценариев и работы с криптографическими задачами.

### Примеры использования

```csharp
// Сгенерировать seed-фразу (12 слов)
string seed = rnd.Seed();

// Случайная hex-строка длиной 32 символа
string hex = rnd.RandomHex(32);

// Случайная строка-хэш из букв и цифр длиной 16
string hash = rnd.RandomHash(16);

// Случайный никнейм (до 15 символов)
string nick = rnd.Nickname();

// Получить случайное число из переменной проекта (диапазон "1-10" или конкретное значение)
int value = rnd.Int(project, "myVar");

// Получить случайное значение процента с разбросом
double v = rnd.RndPercent(100, 30, 10);
```


## Описание методов

### Seed

```csharp
public string Seed()
```

- Генерирует seed-фразу (12 английских слов) через внешний генератор Blockchain.GenerateMnemonic.
- Используется для создания криптокошельков и тестовых данных.


### RandomHex

```csharp
public string RandomHex(int length)
```

- Генерирует случайную hex-строку длиной `length`.
- Использует символы 0-9, a-f.
- Возвращает строку с префиксом `0x`.


### RandomHash

```csharp
public string RandomHash(int length)
```

- Генерирует случайную строку длиной `length` из латинских букв (верхний и нижний регистр) и цифр.
- Используется для имитации идентификаторов, токенов и др.


### Nickname

```csharp
public string Nickname()
```

- Генерирует никнейм из случайных прилагательного, существительного и (опционально) суффикса.
- Списки слов фиксированы (50 прилагательных, 50 существительных, 7 суффиксов + пустые).
- Никнейм ограничен 15 символами.
- Используется для генерации уникальных имён пользователей, тестовых данных.


### Int

```csharp
public int Int(IZennoPosterProjectModel project, string Var)
```

- Получает значение переменной проекта `Var`.
- Если значение содержит "-", парсит диапазон и возвращает случайное число из диапазона (включительно).
- Если значение — конкретное число, возвращает его как int.
- При ошибке логирует исключение и возвращает 0.


### RndPercent (перегрузка для double)

```csharp
public double RndPercent(double input, double percent, double maxPercent)
```

- Возвращает случайное значение, уменьшенное на случайный процент от `percent` до `maxPercent` от исходного `input`.
- Пример: для (100, 30, 10) — сначала вычисляется 30% от 100, затем случайно уменьшается до 10% от этого значения.
- Гарантирует, что результат всегда положительный.


### RndPercent (перегрузка для decimal)

```csharp
public double RndPercent(decimal input, double percent, double maxPercent)
```

- Аналогична версии для double, но принимает decimal в качестве исходного значения.


## Вспомогательные детали

- Для генерации случайных значений используется `System.Random` с уникальным seed (для никнеймов — от Guid).
- Все методы потокобезопасны.
- Для генерации seed-фразы требуется внешний компонент Blockchain.GenerateMnemonic.


## Особенности

- Универсальные методы для генерации любых случайных данных: строк, чисел, seed, никнеймов.
- Поддержка диапазонов и разброса процентов.
- Удобно использовать для тестирования, генерации уникальных идентификаторов, криптографии и имитации пользовательских данных.
- Все ошибки логируются через проект.

