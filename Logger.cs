using System;
using System.IO;

namespace koval_yp_codec
{
    public static class Logger
    {
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "journal.log");

        public static void Log(string action, string details)
        {
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}: {details}";
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
            catch { }
        }

        public static string GetLogs(int lines = 50)
        {
            if (!File.Exists(LogFile)) return "";
            var allLines = File.ReadAllLines(LogFile);
            int start = Math.Max(0, allLines.Length - lines);
            return string.Join(Environment.NewLine, allLines, start, allLines.Length - start);
        }
    }
}