using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace koval_yp_codec
{
    /// <summary>
    /// Представляет запись в истории операций с уникальной "частотой"
    /// </summary>
    public class RadioEntry
    {
        /// <summary>Уникальная частота в диапазоне 140-150 MHz</summary>
        public double Frequency { get; set; }

        /// <summary>Тип операции (ENCRYPT, DECRYPT, SIGN)</summary>
        public string OperationType { get; set; }

        /// <summary>Название использованного шифра</summary>
        public string CipherName { get; set; }

        /// <summary>Превью текста (первые 30 символов)</summary>
        public string Preview { get; set; }

        /// <summary>Временная метка операции</summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Менеджер для хранения и загрузки истории операций
    /// </summary>
    public static class RadioHistory
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "radio_history.json");
        private static readonly List<RadioEntry> _history = new List<RadioEntry>();
        private static readonly Random _random = new Random();

        /// <summary>
        /// Статический конструктор - загружает историю при первом обращении
        /// </summary>
        static RadioHistory()
        {
            Load();
        }

        /// <summary>
        /// Добавление новой записи в историю
        /// </summary>
        /// <param name="operationType">Тип операции</param>
        /// <param name="cipherName">Название шифра</param>
        /// <param name="text">Исходный текст</param>
        public static void AddEntry(string operationType, string cipherName, string text)
        {
            var entry = new RadioEntry
            {
                Frequency = _random.NextDouble() * 10 + 140, // 140-150 MHz
                OperationType = operationType,
                CipherName = cipherName,
                Preview = text.Length > 30 ? text.Substring(0, 30) + "..." : text,
                Timestamp = DateTime.Now
            };

            _history.Add(entry);
            Save();
        }

        /// <summary>
        /// Получение всех записей истории
        /// </summary>
        public static IReadOnlyList<RadioEntry> GetAll() => _history.AsReadOnly();

        /// <summary>
        /// Загрузка истории из JSON-файла
        /// </summary>
        private static void Load()
        {
            if (!File.Exists(FilePath)) return;

            try
            {
                string json = File.ReadAllText(FilePath);
                var loaded = JsonConvert.DeserializeObject<List<RadioEntry>>(json);
                if (loaded != null)
                {
                    _history.Clear();
                    _history.AddRange(loaded);
                }
            }
            catch
            {
                // При ошибке загрузки оставляем пустую историю
            }
        }

        /// <summary>
        /// Сохранение истории в JSON-файл
        /// </summary>
        private static void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_history, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // Игнорируем ошибки сохранения
            }
        }
    }
}