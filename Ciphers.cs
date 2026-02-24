using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace koval_yp_codec
{
    /// <summary>
    /// Режим обработки раскладки клавиатуры
    /// </summary>
    public enum KeyboardLayoutMode
    {
        Auto,       // Автоматическое определение
        Cyrillic,   // Только кириллица
        Latin       // Только латиница
    }

    /// <summary>
    /// Модуль, содержащий реализацию всех алгоритмов шифрования и кодирования
    /// </summary>
    public static class Ciphers
    {
        // Алфавит для шифра Цезаря (английский)
        private const string LowercaseAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Русские алфавиты
        private const string RussianLowercase = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        private const string RussianUppercase = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        // Таблица транслитерации для русских букв (кириллица -> латиница)
        private static readonly Dictionary<char, char> TranslitMap = new Dictionary<char, char>
        {
            {'А', 'A'}, {'Б', 'B'}, {'В', 'V'}, {'Г', 'G'}, {'Д', 'D'},
            {'Е', 'E'}, {'Ё', 'E'}, {'Ж', 'Z'}, {'З', 'Z'}, {'И', 'I'},
            {'Й', 'I'}, {'К', 'K'}, {'Л', 'L'}, {'М', 'M'}, {'Н', 'N'},
            {'О', 'O'}, {'П', 'P'}, {'Р', 'R'}, {'С', 'S'}, {'Т', 'T'},
            {'У', 'U'}, {'Ф', 'F'}, {'Х', 'H'}, {'Ц', 'C'}, {'Ч', 'C'},
            {'Ш', 'S'}, {'Щ', 'S'}, {'Ъ', '\''}, {'Ы', 'Y'}, {'Ь', '\''},
            {'Э', 'E'}, {'Ю', 'U'}, {'Я', 'Y'},
            {'а', 'a'}, {'б', 'b'}, {'в', 'v'}, {'г', 'g'}, {'д', 'd'},
            {'е', 'e'}, {'ё', 'e'}, {'ж', 'z'}, {'з', 'z'}, {'и', 'i'},
            {'й', 'i'}, {'к', 'k'}, {'л', 'l'}, {'м', 'm'}, {'н', 'n'},
            {'о', 'o'}, {'п', 'p'}, {'р', 'r'}, {'с', 's'}, {'т', 't'},
            {'у', 'u'}, {'ф', 'f'}, {'х', 'h'}, {'ц', 'c'}, {'ч', 'c'},
            {'ш', 's'}, {'щ', 's'}, {'ъ', '\''}, {'ы', 'y'}, {'ь', '\''},
            {'э', 'e'}, {'ю', 'u'}, {'я', 'y'}
        };

        // Таблица обратной транслитерации (латиница -> кириллица)
        private static readonly Dictionary<char, char> ReverseTranslitMap = new Dictionary<char, char>
        {
            {'A', 'А'}, {'B', 'Б'}, {'V', 'В'}, {'G', 'Г'}, {'D', 'Д'},
            {'E', 'Е'}, {'Z', 'З'}, {'I', 'И'}, {'K', 'К'}, {'L', 'Л'},
            {'M', 'М'}, {'N', 'Н'}, {'O', 'О'}, {'P', 'П'}, {'R', 'Р'},
            {'S', 'С'}, {'T', 'Т'}, {'U', 'У'}, {'F', 'Ф'}, {'H', 'Х'},
            {'C', 'Ц'}, {'Y', 'Ы'}, {'J', 'Й'}, {'Q', 'Я'}, {'W', 'В'},
            {'X', 'К'}, {'a', 'а'}, {'b', 'б'}, {'v', 'в'}, {'g', 'г'},
            {'d', 'д'}, {'e', 'е'}, {'z', 'з'}, {'i', 'и'}, {'k', 'к'},
            {'l', 'л'}, {'m', 'м'}, {'n', 'н'}, {'o', 'о'}, {'p', 'п'},
            {'r', 'р'}, {'s', 'с'}, {'t', 'т'}, {'u', 'у'}, {'f', 'ф'},
            {'h', 'х'}, {'c', 'ц'}, {'y', 'ы'}, {'j', 'й'}, {'q', 'я'},
            {'w', 'в'}, {'x', 'к'}
        };

        /// <summary>
        /// Вспомогательный метод для обработки латинских символов в шифре Цезаря
        /// </summary>
        private static char ProcessLatinChar(char c, int shift, int direction)
        {
            if (c >= 'a' && c <= 'z')
            {
                int index = c - 'a';
                int newIndex = (index + shift * direction) % 26;
                if (newIndex < 0) newIndex += 26;
                return (char)('a' + newIndex);
            }
            else if (c >= 'A' && c <= 'Z')
            {
                int index = c - 'A';
                int newIndex = (index + shift * direction) % 26;
                if (newIndex < 0) newIndex += 26;
                return (char)('A' + newIndex);
            }
            return c;
        }

        /// <summary>
        /// Вспомогательный метод для обработки русских символов в шифре Цезаря
        /// </summary>
        private static char ProcessCyrillicChar(char c, int shift, int direction)
        {
            if (c >= 'а' && c <= 'я')
            {
                int index = c - 'а';
                int newIndex = (index + shift * direction) % 32;
                if (newIndex < 0) newIndex += 32;
                return (char)('а' + newIndex);
            }
            else if (c >= 'А' && c <= 'Я')
            {
                int index = c - 'А';
                int newIndex = (index + shift * direction) % 32;
                if (newIndex < 0) newIndex += 32;
                return (char)('А' + newIndex);
            }
            else if (c == 'ё')
            {
                return 'ё';
            }
            else if (c == 'Ё')
            {
                return 'Ё';
            }
            return c;
        }

        /// <summary>
        /// Шифр Цезаря с поддержкой русского и английского и выбором раскладки
        /// </summary>
        public static string Caesar(string input, int shift, bool encrypt, KeyboardLayoutMode layoutMode = KeyboardLayoutMode.Auto)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int direction = encrypt ? 1 : -1;
            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                // Определяем, к какому алфавиту относится символ
                bool isLatin = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool isCyrillic = (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';

                // Применяем правила в зависимости от выбранного режима
                if (layoutMode == KeyboardLayoutMode.Latin && isCyrillic)
                {
                    // Если выбран латинский режим, а символ русский - транслитерируем
                    char latinChar = TranslitMap.ContainsKey(c) ? TranslitMap[c] : c;
                    result.Append(ProcessLatinChar(latinChar, shift, direction));
                }
                else if (layoutMode == KeyboardLayoutMode.Cyrillic && isLatin)
                {
                    // Если выбран русский режим, а символ латинский - обратная транслитерация
                    char cyrillicChar = ReverseTranslitMap.ContainsKey(c) ? ReverseTranslitMap[c] : c;
                    result.Append(ProcessCyrillicChar(cyrillicChar, shift, direction));
                }
                else
                {
                    // Автоматический режим или символ соответствует выбранной раскладке
                    if (isLatin)
                        result.Append(ProcessLatinChar(c, shift, direction));
                    else if (isCyrillic)
                        result.Append(ProcessCyrillicChar(c, shift, direction));
                    else
                        result.Append(c);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// ROT-n для всех печатных символов (коды ASCII 32-126)
        /// </summary>
        public static string Rot(string input, int shift, bool encrypt, KeyboardLayoutMode layoutMode = KeyboardLayoutMode.Auto)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int direction = encrypt ? 1 : -1;
            int start = 32;  // пробел
            int end = 126;   // тильда ~
            int range = end - start + 1; // 95 символов

            int effectiveShift = (shift * direction) % range;
            if (effectiveShift < 0) effectiveShift += range;

            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                int asciiCode = (int)c;

                // Обрабатываем только печатные ASCII символы
                if (asciiCode >= start && asciiCode <= end)
                {
                    int newCode = ((asciiCode - start + effectiveShift) % range) + start;
                    result.Append((char)newCode);
                }
                else
                {
                    // Для не-ASCII символов применяем логику раскладки
                    bool isLatin = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                    bool isCyrillic = (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';

                    if (layoutMode == KeyboardLayoutMode.Latin && isCyrillic)
                    {
                        // Транслитерация для ROT-n (применяем ROT к транслитерированному символу)
                        char latinChar = TranslitMap.ContainsKey(c) ? TranslitMap[c] : c;
                        int latinCode = (int)latinChar;
                        if (latinCode >= start && latinCode <= end)
                        {
                            int newCode = ((latinCode - start + effectiveShift) % range) + start;
                            result.Append((char)newCode);
                        }
                        else
                        {
                            result.Append(latinChar);
                        }
                    }
                    else
                    {
                        // В остальных случаях оставляем без изменений
                        result.Append(c);
                    }
                }
            }
            return result.ToString();
        }

        // Таблица азбуки Морзе
        private static readonly Dictionary<char, string> MorseMap = new Dictionary<char, string>
        {
            {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
            {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
            {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
            {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
            {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"},
            {'Z', "--.."},
            {'0', "-----"}, {'1', ".----"}, {'2', "..---"}, {'3', "...--"},
            {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."},
            {'8', "---.."}, {'9', "----."},
            {' ', "/"}, {'.', ".-.-.-"}, {',', "--..--"}, {'?', "..--.."},
            {'!', "-.-.--"}, {'@', ".--.-."}
        };

        /// <summary>
        /// Азбука Морзе с поддержкой выбора раскладки
        /// </summary>
        public static string MorseEncode(string input, KeyboardLayoutMode layoutMode = KeyboardLayoutMode.Auto)
        {
            if (string.IsNullOrEmpty(input)) return "";

            var result = new StringBuilder();

            foreach (char c in input)
            {
                bool isCyrillic = (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
                char processedChar = c;

                // Применяем транслитерацию если нужно
                if (layoutMode == KeyboardLayoutMode.Latin && isCyrillic)
                {
                    processedChar = TranslitMap.ContainsKey(c) ? TranslitMap[c] : c;
                }
                else if (layoutMode == KeyboardLayoutMode.Cyrillic && !isCyrillic && (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z'))
                {
                    processedChar = ReverseTranslitMap.ContainsKey(c) ? ReverseTranslitMap[c] : c;
                }

                char upperChar = char.ToUpperInvariant(processedChar);

                // Пробуем прямое соответствие
                if (MorseMap.ContainsKey(upperChar))
                {
                    result.Append(MorseMap[upperChar] + " ");
                }
                else
                {
                    result.Append("? ");
                }
            }
            return result.ToString().Trim();
        }

        /// <summary>
        /// Дешифрование азбуки Морзе с проверкой формата
        /// </summary>
        public static string MorseDecode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Введите текст для дешифрования");
            }

            // Проверка, что строка содержит только допустимые символы морзянки
            string testString = input.Replace(" ", "").Replace("/", "");
            if (!testString.All(c => ".-".Contains(c)))
            {
                throw new FormatException("Введённый текст не является корректной азбукой Морзе. " +
                                          "Морзе может содержать только символы '.', '-', пробелы и '/' для разделения слов.");
            }

            // Создаём обратный словарь
            var reverseMap = MorseMap.ToDictionary(x => x.Value, x => x.Key);

            var words = input.Split(new string[] { " / " }, StringSplitOptions.None);
            var result = new StringBuilder();

            foreach (var word in words)
            {
                var letters = word.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var l in letters)
                {
                    if (reverseMap.ContainsKey(l))
                        result.Append(reverseMap[l]);
                    else
                        result.Append('?');
                }
                result.Append(' ');
            }
            return result.ToString().Trim();
        }

        /// <summary>
        /// Двоичный код (8 бит на символ)
        /// </summary>
        public static string BinaryEncode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string[] binaryStrings = bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).ToArray();
            return string.Join(" ", binaryStrings);
        }

        /// <summary>
        /// Дешифрование двоичного кода с проверкой формата
        /// </summary>
        public static string BinaryDecode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Введите текст для дешифрования");
            }

            string[] binaryStrings = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Проверка формата
            foreach (string binary in binaryStrings)
            {
                if (binary.Length != 8 || !binary.All(c => c == '0' || c == '1'))
                {
                    throw new FormatException("Введённый текст не является корректным двоичным кодом. " +
                                              "Двоичный код должен состоять из 8-битных последовательностей (0 и 1), разделённых пробелами.");
                }
            }

            byte[] bytes = binaryStrings.Select(b => Convert.ToByte(b, 2)).ToArray();
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// A1Z26 с поддержкой выбора раскладки
        /// </summary>
        public static string A1Z26Encode(string input, KeyboardLayoutMode layoutMode = KeyboardLayoutMode.Auto)
        {
            if (string.IsNullOrEmpty(input)) return "";

            var result = new List<string>();

            foreach (char c in input)
            {
                bool isCyrillic = (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
                char processedChar = c;

                // Применяем транслитерацию если нужно
                if (layoutMode == KeyboardLayoutMode.Latin && isCyrillic)
                {
                    processedChar = TranslitMap.ContainsKey(c) ? TranslitMap[c] : c;
                }
                else if (layoutMode == KeyboardLayoutMode.Cyrillic && !isCyrillic && (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z'))
                {
                    processedChar = ReverseTranslitMap.ContainsKey(c) ? ReverseTranslitMap[c] : c;
                }

                char upperChar = char.ToUpperInvariant(processedChar);

                if (upperChar >= 'A' && upperChar <= 'Z')
                {
                    result.Add((upperChar - 'A' + 1).ToString());
                }
                else
                {
                    result.Add(c.ToString());
                }
            }
            return string.Join("-", result);
        }

        public static string A1Z26Decode(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            string[] parts = input.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            foreach (string part in parts)
            {
                if (int.TryParse(part, out int num) && num >= 1 && num <= 26)
                {
                    result.Append((char)('A' + num - 1));
                }
                else
                {
                    result.Append(part);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Base64
        /// </summary>
        public static string Base64Encode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Введите текст для дешифрования");
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                throw new FormatException("Введённый текст не является корректной Base64 строкой.");
            }
        }

        /// <summary>
        /// Base32 (исправленная версия)
        /// </summary>
        public static string Base32Encode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Base32Converter.ToBase32String(bytes);
        }

        public static string Base32Decode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Введите текст для дешифрования");
            }

            try
            {
                byte[] bytes = Base32Converter.FromBase32String(input);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                throw new FormatException("Введённый текст не является корректной Base32 строкой.");
            }
        }

        /// <summary>
        /// ASCII-коды (десятичные)
        /// </summary>
        public static string AsciiEncode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            byte[] asciiBytes = Encoding.ASCII.GetBytes(input);
            return string.Join(" ", asciiBytes.Select(b => b.ToString()));
        }

        public static string AsciiDecode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Введите текст для дешифрования");
            }

            string[] numbers = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                byte[] bytes = numbers.Select(n => byte.Parse(n)).ToArray();
                return Encoding.ASCII.GetString(bytes);
            }
            catch (FormatException)
            {
                throw new FormatException("Введённый текст не является корректным набором ASCII кодов. " +
                                          "Ожидаются числа от 0 до 255, разделённые пробелами.");
            }
        }
    }

    /// <summary>
    /// Исправленный конвертер для Base32 (RFC 4648)
    /// </summary>
    public static class Base32Converter
    {
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        /// <summary>
        /// Преобразование массива байтов в Base32 строку
        /// </summary>
        public static string ToBase32String(byte[] input)
        {
            if (input == null || input.Length == 0)
                return string.Empty;

            int bitCount = input.Length * 8;
            int resultLength = (bitCount + 4) / 5; // Округление вверх
            StringBuilder result = new StringBuilder(resultLength);

            int buffer = 0;
            int bitsRemaining = 0;
            int index = 0;

            while (index < input.Length || bitsRemaining > 0)
            {
                if (bitsRemaining < 5)
                {
                    if (index < input.Length)
                    {
                        buffer = (buffer << 8) | input[index++];
                        bitsRemaining += 8;
                    }
                    else
                    {
                        buffer <<= 5 - bitsRemaining;
                        bitsRemaining = 5;
                    }
                }

                int value = (buffer >> (bitsRemaining - 5)) & 0x1F;
                result.Append(Base32Chars[value]);
                bitsRemaining -= 5;
            }

            // Добавление padding (длина результата должна быть кратна 8)
            int paddingCount = (8 - (result.Length % 8)) % 8;
            result.Append('=', paddingCount);

            return result.ToString();
        }

        /// <summary>
        /// Преобразование Base32 строки в массив байтов
        /// </summary>
        public static byte[] FromBase32String(string base32)
        {
            if (string.IsNullOrEmpty(base32))
                return new byte[0];

            // Удаление символов padding
            string cleanInput = base32.TrimEnd('=').ToUpperInvariant();

            // Проверка символов
            foreach (char c in cleanInput)
            {
                if (!Base32Chars.Contains(c))
                    throw new FormatException($"Недопустимый символ '{c}' в Base32 строке");
            }

            int bitCount = cleanInput.Length * 5;
            int byteCount = bitCount / 8;
            byte[] result = new byte[byteCount];

            int buffer = 0;
            int bitsRemaining = 0;
            int resultIndex = 0;

            foreach (char c in cleanInput)
            {
                int value = Base32Chars.IndexOf(c);
                buffer = (buffer << 5) | value;
                bitsRemaining += 5;

                if (bitsRemaining >= 8)
                {
                    result[resultIndex++] = (byte)((buffer >> (bitsRemaining - 8)) & 0xFF);
                    bitsRemaining -= 8;
                    buffer &= (1 << bitsRemaining) - 1;
                }
            }

            return result;
        }
    }
}