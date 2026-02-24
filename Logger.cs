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
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Запись действия в журнал
        /// </summary>
        /// <param name="action">Тип действия</param>
        /// <param name="details">Детали действия</param>
        public static void Log(string action, string details)
        {
            try
            {
                lock (_lockObject)
                {
                    string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}: {details}";
                    File.AppendAllText(LogFile, line + Environment.NewLine);
                }
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
                lock (_lockObject)
                {
                    string[] allLines = File.ReadAllLines(LogFile);
                    int start = Math.Max(0, allLines.Length - lines);
                    return string.Join(Environment.NewLine, allLines, start, allLines.Length - start);
                }
            }
            catch
            {
                return "Ошибка чтения журнала";
            }
        }

        /// <summary>
        /// Очистка журнала операций
        /// </summary>
        public static void ClearLog()
        {
            try
            {
                lock (_lockObject)
                {
                    if (File.Exists(LogFile))
                    {
                        File.Delete(LogFile);
                        Log("LOG_CLEAR", "Журнал операций очищен");
                    }
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки хотя бы попытаемся записать в лог (если ещё возможно)
                try
                {
                    Log("LOG_CLEAR_ERROR", ex.Message);
                }
                catch { }
            }
        }

        /// <summary>
        /// Получение размера журнала в байтах
        /// </summary>
        public static long GetLogSize()
        {
            try
            {
                if (File.Exists(LogFile))
                    return new FileInfo(LogFile).Length;
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Получение количества строк в журнале
        /// </summary>
        public static int GetLogLineCount()
        {
            try
            {
                if (!File.Exists(LogFile)) return 0;
                string[] lines = File.ReadAllLines(LogFile);
                return lines.Length;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Экспорт журнала в указанный файл
        /// </summary>
        /// <param name="destinationPath">Путь для сохранения копии</param>
        /// <returns>True если экспорт успешен, иначе False</returns>
        public static bool ExportLog(string destinationPath)
        {
            try
            {
                if (!File.Exists(LogFile)) return false;

                lock (_lockObject)
                {
                    File.Copy(LogFile, destinationPath, true);
                    Log("LOG_EXPORT", $"Журнал экспортирован в {destinationPath}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log("LOG_EXPORT_ERROR", ex.Message);
                return false;
            }
        }
    }
}