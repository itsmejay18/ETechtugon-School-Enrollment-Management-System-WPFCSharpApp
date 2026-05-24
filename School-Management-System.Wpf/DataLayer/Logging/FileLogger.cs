using System;
using System.IO;
using System.Text;

namespace School_Management_System.DataLayer.Logging
{
    public static class FileLogger
    {
        private static readonly object Sync = new object();

        public static void LogError(string context, Exception ex)
        {
            try
            {
                var baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SchoolManagementSystem",
                    "logs");

                Directory.CreateDirectory(baseDir);

                var logPath = Path.Combine(baseDir, "app.log");
                var sb = new StringBuilder();
                sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ERROR");
                if (!string.IsNullOrWhiteSpace(context))
                {
                    sb.AppendLine("Context: " + context);
                }
                sb.AppendLine(ex.ToString());
                sb.AppendLine(new string('-', 80));

                lock (Sync)
                {
                    File.AppendAllText(logPath, sb.ToString());
                }
            }
            catch
            {
                // Never throw from logger.
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }

                var baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SchoolManagementSystem",
                    "logs");

                Directory.CreateDirectory(baseDir);
                var logPath = Path.Combine(baseDir, "app.log");

                lock (Sync)
                {
                    File.AppendAllText(logPath, "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message + Environment.NewLine);
                }
            }
            catch
            {
                // Never throw from logger.
            }
        }
    }
}

