using System;
using System.IO;

namespace koval_yp_codec
{
    /// <summary>
    /// Модуль для журналирования всех действий пользователя
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "journal.log");

        /// <summary>
        /// Запись действия в журнал
        /// </summary>
        /// <param name="action">Тип действия</param>
        /// <param name="details">Детали действия</param>
        public static void Log(string action, string details)
        {
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}: {details}";
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки записи в лог
            }
        }

        /// <summary>
        /// Получение последних записей из журнала
        /// </summary>
        /// <param name="lines">Количество запрашиваемых строк</param>
        /// <returns>Строка с последними записями</returns>
        public static string GetLogs(int lines = 50)
        {
            if (!File.Exists(LogFile)) return "";

            try
            {
                string[] allLines = File.ReadAllLines(LogFile);
                int start = Math.Max(0, allLines.Length - lines);
                return string.Join(Environment.NewLine, allLines, start, allLines.Length - start);
            }
            catch
            {
                return "Ошибка чтения журнала";
            }
        }
    }
}