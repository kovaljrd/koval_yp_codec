using System;
using System.Security.Cryptography;
using System.Text;

namespace koval_yp_codec
{
    /// <summary>
    /// Модуль для создания и проверки цифровых подписей на основе SHA-256 с солью
    /// </summary>
    public static class Signature
    {
        /// <summary>
        /// Создание цифровой подписи для текста
        /// </summary>
        /// <param name="text">Исходный текст</param>
        /// <returns>Подпись в формате "соль:хэш"</returns>
        /// <exception cref="ArgumentException">Выбрасывается при пустом тексте</exception>
        public static string SignText(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Текст для подписи не может быть пустым");

            string salt = GenerateSalt();
            string combined = text + salt;

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
                string hashStr = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return salt + ":" + hashStr;
            }
        }

        /// <summary>
        /// Проверка цифровой подписи
        /// </summary>
        /// <param name="text">Текст для проверки</param>
        /// <param name="signature">Подпись в формате "соль:хэш"</param>
        /// <returns>True если подпись действительна, иначе False</returns>
        public static bool VerifySignature(string text, string signature)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(signature))
                return false;

            string[] parts = signature.Split(':');
            if (parts.Length != 2) return false;

            string salt = parts[0];
            string hashValue = parts[1];
            string combined = text + salt;

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
                string expected = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return hashValue == expected;
            }
        }

        /// <summary>
        /// Генерация случайной соли для подписи
        /// </summary>
        /// <param name="length">Длина соли в байтах</param>
        /// <returns>Строка с солью в hex-формате</returns>
        private static string GenerateSalt(int length = 8)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] saltBytes = new byte[length];
                rng.GetBytes(saltBytes);
                return BitConverter.ToString(saltBytes).Replace("-", "").ToLowerInvariant().Substring(0, length * 2);
            }
        }
    }
}