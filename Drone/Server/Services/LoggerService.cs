using System;
using System.Configuration;
using System.IO;

namespace Server.Services
{
    public class LoggerService : IDisposable
    {
        string _logFilePath;
        StreamWriter _fileWriter;
        bool _disposed = false;

        public LoggerService()
        {
            string logFileName = ConfigurationManager.AppSettings["LogFilePath"];
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName.TrimStart('\\'));

            string directory = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _fileWriter = new StreamWriter(_logFilePath, append: true, encoding: System.Text.Encoding.UTF8);
        }

        public void LogMessage(string message)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LoggerService));

            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            Console.WriteLine(logEntry);

            try
            {
                _fileWriter.WriteLine(logEntry);
                _fileWriter.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        public void LogWarning(string message)
        {
            LogMessage($"[WARNING] {message}");
        }

        public void LogError(string message)
        {
            LogMessage($"[ERROR] {message}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_fileWriter != null)
                    {
                        _fileWriter.Flush();
                        _fileWriter.Dispose();
                        _fileWriter = null;
                    }
                }
                _disposed = true;
            }
        }

        ~LoggerService()
        {
            Dispose(false);
        }
    }
}
