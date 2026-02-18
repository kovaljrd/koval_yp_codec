using System;
using System.Security.Cryptography;
using System.Text;

namespace koval_yp_codec
{
    public static class Signature
    {
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

        public static bool VerifySignature(string text, string signature)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(signature))
                return false;

            var parts = signature.Split(':');
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