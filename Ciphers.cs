using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace koval_yp_codec
{
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

        /// <summary>
        /// Шифр Цезаря с поддержкой русского и английского
        /// </summary>
        public static string Caesar(string input, int shift, bool encrypt)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int direction = encrypt ? 1 : -1;
            int effectiveShift = (shift * direction) % 32;
            if (effectiveShift < 0) effectiveShift += 32;

            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                // Английские строчные
                if (c >= 'a' && c <= 'z')
                {
                    int index = c - 'a';
                    int newIndex = (index + effectiveShift) % 26;
                    if (newIndex < 0) newIndex += 26;
                    result.Append((char)('a' + newIndex));
                }
                // Английские заглавные
                else if (c >= 'A' && c <= 'Z')
                {
                    int index = c - 'A';
                    int newIndex = (index + effectiveShift) % 26;
                    if (newIndex < 0) newIndex += 26;
                    result.Append((char)('A' + newIndex));
                }
                // Русские строчные
                else if (c >= 'а' && c <= 'я')
                {
                    int index = c - 'а';
                    int newIndex = (index + effectiveShift) % 32;
                    if (newIndex < 0) newIndex += 32;
                    result.Append((char)('а' + newIndex));
                }
                // Русские заглавные
                else if (c >= 'А' && c <= 'Я')
                {
                    int index = c - 'А';
                    int newIndex = (index + effectiveShift) % 32;
                    if (newIndex < 0) newIndex += 32;
                    result.Append((char)('А' + newIndex));
                }
                // Ё и ё обрабатываем отдельно
                else if (c == 'ё')
                {
                    result.Append('ё');
                }
                else if (c == 'Ё')
                {
                    result.Append('Ё');
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// ROT-n для всех печатных символов (коды ASCII 32-126)
        /// </summary>
        /// <param name="input">Входной текст</param>
        /// <param name="shift">Величина сдвига</param>
        /// <param name="encrypt">true - шифрование, false - дешифрование</param>
        /// <returns>Преобразованный текст</returns>
        public static string Rot(string input, int shift, bool encrypt)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int direction = encrypt ? 1 : -1;
            int start = 32;  // пробел
            int end = 126;   // тильда ~
            int range = end - start + 1; // 95 символов

            // Корректируем сдвиг
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
                    // Символы вне диапазона (например, русские буквы) оставляем без изменений
                    result.Append(c);
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

        // Таблица транслитерации для русских букв
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

        /// <summary>
        /// Азбука Морзе с проверкой формата
        /// </summary>
        public static string MorseEncode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            var result = new StringBuilder();

            foreach (char c in input)
            {
                // Пробуем прямое соответствие
                if (MorseMap.ContainsKey(char.ToUpperInvariant(c)))
                {
                    result.Append(MorseMap[char.ToUpperInvariant(c)] + " ");
                }
                // Транслитерация русских букв
                else if (TranslitMap.ContainsKey(c))
                {
                    char latin = TranslitMap[c];
                    result.Append(MorseMap[char.ToUpperInvariant(latin)] + " ");
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
        /// A1Z26 (A=1, B=2, ...)
        /// </summary>
        public static string A1Z26Encode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            var result = new List<string>();
            foreach (char c in input.ToUpperInvariant())
            {
                if (c >= 'A' && c <= 'Z')
                {
                    result.Add((c - 'A' + 1).ToString());
                }
                else if (TranslitMap.ContainsKey(c))
                {
                    char latin = TranslitMap[c];
                    result.Add((char.ToUpperInvariant(latin) - 'A' + 1).ToString());
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
        /// Base32
        /// </summary>
        public static string Base32Encode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Base32.ToBase32String(bytes);
        }

        public static string Base32Decode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Введите текст для дешифрования");
            }

            try
            {
                byte[] bytes = Base32.FromBase32String(input);
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

        // Вспомогательный класс для Base32
        private static class Base32
        {
            private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

            public static string ToBase32String(byte[] input)
            {
                if (input == null || input.Length == 0) return "";

                int charCount = (int)Math.Ceiling(input.Length * 8 / 5.0);
                var result = new StringBuilder(charCount);
                byte nextChar = 0, bitsRemaining = 5;
                int index = 0;

                while (index < input.Length)
                {
                    nextChar = (byte)(nextChar | (input[index] >> (8 - bitsRemaining)));
                    result.Append(Base32Chars[nextChar]);

                    if (bitsRemaining < 4)
                    {
                        nextChar = (byte)((input[index] >> (3 - bitsRemaining)) & 31);
                        result.Append(Base32Chars[nextChar]);
                        bitsRemaining += 5;
                    }
                    bitsRemaining -= 3;
                    nextChar = (byte)((input[index] << bitsRemaining) & 31);
                    index++;
                }

                if (bitsRemaining > 0)
                {
                    result.Append(Base32Chars[nextChar]);
                }
                return result.ToString();
            }

            public static byte[] FromBase32String(string base32)
            {
                if (string.IsNullOrEmpty(base32)) return new byte[0];

                base32 = base32.Trim().ToUpperInvariant();

                // Проверка символов
                foreach (char c in base32)
                {
                    if (!Base32Chars.Contains(c))
                        throw new FormatException($"Недопустимый символ '{c}' в Base32 строке");
                }

                int byteCount = base32.Length * 5 / 8;
                byte[] result = new byte[byteCount];
                int buffer = 0;
                int bitsLeft = 8;
                int resultIndex = 0;

                foreach (char c in base32)
                {
                    int value = Base32Chars.IndexOf(c);
                    if (bitsLeft > 5)
                    {
                        buffer = (buffer << 5) | value;
                        bitsLeft -= 5;
                    }
                    else
                    {
                        buffer = (buffer << 5) | value;
                        int shift = bitsLeft + 5 - 8;
                        result[resultIndex++] = (byte)((buffer >> shift) & 0xFF);
                        buffer &= (1 << shift) - 1;
                        bitsLeft = 8 - shift;
                    }
                }
                return result;
            }
        }
    }
}