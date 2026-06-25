using System;
using System.IO;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public static class LoggerService
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly object LockObject = new object();

       
        static LoggerService()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch { }
        }

       //log type: info
        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

     // log type: error
        public static void LogError(string message, Exception ex = null)
        {
            string fullMessage = ex != null ? $"{message} | Exception: {ex.Message} \nStackTrace: {ex.StackTrace}" : message;
            WriteLog("ERROR", fullMessage);
        }

       // writing log to file
        private static void WriteLog(string level, string message)
        {
            string fileName = $"log_{DateTime.Now:dd_MM_yyyy}.log";
            string logFilePath = Path.Combine(LogDirectory, fileName);

            string logLine = $"[{DateTime.Now:dd_MM_yyyy HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

            lock (LockObject)
            {
                try
                {
                    File.AppendAllText(logFilePath, logLine);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}