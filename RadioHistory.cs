using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace koval_yp_codec
{
    public class RadioEntry
    {
        public double Frequency { get; set; }
        public string OperationType { get; set; } // ENCRYPT, DECRYPT, SIGN
        public string CipherName { get; set; }
        public string Preview { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public static class RadioHistory
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "radio_history.json");
        private static List<RadioEntry> _history = new List<RadioEntry>();

        static RadioHistory()
        {
            Load();
        }

        public static void AddEntry(string operationType, string cipherName, string text)
        {
            var entry = new RadioEntry
            {
                Frequency = new Random().NextDouble() * 10 + 140, // 140-150 MHz
                OperationType = operationType,
                CipherName = cipherName,
                Preview = text.Length > 30 ? text.Substring(0, 30) + "..." : text,
                Timestamp = DateTime.Now
            };
            _history.Add(entry);
            Save();
        }

        public static List<RadioEntry> GetAll() => _history;

        private static void Load()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    _history = JsonConvert.DeserializeObject<List<RadioEntry>>(json) ?? new List<RadioEntry>();
                }
                catch { _history = new List<RadioEntry>(); }
            }
        }

        private static void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_history, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }
    }
}