

##  Cryptography

### Назначение

Класс **Cryptography** предоставляет вспомогательные методы для симметричного шифрования/дешифрования по алгоритму AES (ECB, PKCS7), а также для работы с адресами в формате Bech32 (конвертация bech32 ↔ hex). Вспомогательные методы реализуют преобразование строк в байтовые массивы и обратно, а также вычисление MD5-хэша.

### Примеры использования

```csharp
// AES-шифрование строки
string encrypted = AES.EncryptAES("секрет", "пароль");

// AES-дешифрование строки
string decrypted = AES.DecryptAES(encrypted, "пароль");

// Перевести bech32-адрес в hex
string hex = Bech32.Bech32ToHex("init1q2w3e4r5t6y7u8i9o0p...");

// Перевести hex-адрес в bech32
string bech = Bech32.HexToBech32("0x00112233445566778899aabbccddeeff00112233");
```


## Описание методов

### AES

#### EncryptAES

```csharp
public static string EncryptAES(string phrase, string key, bool hashKey = true)
```

- **phrase**: строка для шифрования.
- **key**: строка-ключ (если hashKey=true, сначала преобразуется в MD5).
- **hashKey**: если true (по умолчанию), ключ хэшируется через MD5.
- **Возвращает**: hex-строку зашифрованных данных.
- **Логика**:

1. Ключ преобразуется в байты (MD5 или hex).
2. Исходная строка кодируется в UTF-8.
3. Используется AES (ECB, PKCS7), ключ — keyArray.
4. Возвращается hex-строка результата.


#### DecryptAES

```csharp
public static string DecryptAES(string hash, string key, bool hashKey = true)
```

- **hash**: hex-строка зашифрованных данных.
- **key**: строка-ключ (аналогично EncryptAES).
- **hashKey**: если true, ключ хэшируется через MD5.
- **Возвращает**: расшифрованную строку в UTF-8.
- **Логика**:

1. Ключ преобразуется в байты (MD5 или hex).
2. hex-строка переводится в байты.
3. Используется AES (ECB, PKCS7), ключ — keyArray.
4. Возвращается строка результата.


#### HashMD5

```csharp
public static string HashMD5(string phrase)
```

- **phrase**: строка для хэширования.
- **Возвращает**: hex-строку MD5-хэша.
- **Логика**: Строка переводится в UTF-8, хэшируется, результат — hex.


#### ByteArrayToHexString

```csharp
internal static string ByteArrayToHexString(byte[] inputArray)
```

- **inputArray**: массив байтов.
- **Возвращает**: строку в hex-виде (заглавные буквы).


#### HexStringToByteArray

```csharp
internal static byte[] HexStringToByteArray(string inputString)
```

- **inputString**: hex-строка.
- **Возвращает**: массив байтов.
- **Особенности**: Проверяет чётность длины, валидность символов.


### Bech32

#### Bech32ToHex

```csharp
public static string Bech32ToHex(string bech32Address)
```

- **bech32Address**: строка-адрес в формате bech32 (только с префиксом `init`).
- **Возвращает**: hex-строку (0x...).
- **Логика**:

1. Проверяет наличие и валидность префикса.
2. Декодирует символы bech32 в байты.
3. Проверяет контрольную сумму.
4. Переводит данные из 5-битного в 8-битный формат.
5. Проверяет длину (20 байт).
6. Возвращает hex-строку с префиксом 0x.


#### HexToBech32

```csharp
public static string HexToBech32(string hexAddress, string prefix = "init")
```

- **hexAddress**: hex-строка (0x... или без префикса, длина 40 символов).
- **prefix**: префикс bech32 (по умолчанию `init`).
- **Возвращает**: строку-адрес в формате bech32.
- **Логика**:

1. Проверяет валидность hex-строки.
2. Переводит hex в байты.
3. Переводит байты из 8-бит в 5-бит.
4. Генерирует контрольную сумму.
5. Кодирует в bech32 с нужным префиксом.


#### Вспомогательные методы Bech32

- `IsHex(string input)`: Проверяет, что строка состоит только из hex-символов.
- `ConvertBits(byte[] data, int fromBits, int toBits, bool pad)`: Преобразует массив байтов между битовыми форматами (5↔8).
- `VerifyChecksum(string hrp, byte[] data)`: Проверяет контрольную сумму bech32.
- `CreateChecksum(string hrp, byte[] data)`: Генерирует контрольную сумму.
- `Expand(this string hrp)`: Преобразует префикс для расчёта контрольной суммы.
- `Polymod(byte[] values)`: Вычисляет контрольную сумму.


