using System;
using System.Text;
using System.IO;

namespace ServerFanControlLib.Loggers
{
    internal interface LoggerBase
    {
        void Log(params String[] messageParts);
    }

    internal class Logger : LoggerBase
    {
        private readonly String filePath;
        private static readonly Object m_logLock = new Object();
        private Int32 logCounter = 0;

        private const int MaxFileSize = 10485760;

        internal Logger(String filePath)
        {
            this.filePath = filePath;
        }

        public void Log(params String[] messageParts)
        {
            lock (m_logLock)
            {
                //using (var sw = new StreamWriter(filePath, true, Encoding.UTF8))
                //{
                //    sw.WriteAsync(DateTime.UtcNow.ToString("s"));
                //    foreach (var part in messageParts)
                //    {
                //        sw.WriteAsync("|");
                //        sw.WriteAsync(part);
                //    }

                //    sw.WriteLineAsync();
                //}

                Console.Write(DateTime.UtcNow.ToString("s"));
                foreach (var part in messageParts)
                {
                    Console.Write("|");
                    Console.Write(part);
                }

                Console.WriteLine();
            }

            logCounter++;
        }

        public Boolean IsFull()
        {
            if (logCounter % 25 != 0)
            {
                // Don't check every time
                return false;
            }

            lock (m_logLock)
            {
                var v = new System.IO.FileInfo(filePath);
                return v.Length > MaxFileSize;
            }
        }

        public void MoveFile(String from, String to)
        {
            lock (m_logLock)
            {
                if (!File.Exists(from))
                {
                    return;
                }

                try
                {
                    File.Move(from, to);
                }
                catch { }
            }
        }
    }

    public abstract class LogHandler
    {
        private readonly Logger m_logger;
        private readonly String m_baseName;
        private readonly String m_extension;
        private const Int32 HistoryCount = 2;
        private readonly Object m_logLock = new Object();
        private Boolean m_isMovingFiles = false;

        public LogHandler(string baseName, string extension)
        {
            m_baseName = baseName;
            m_extension = extension;

            m_logger = new Logger(FileName(0));
        }
        
        protected void TryLog(params String[] messageParts)
        {
            try
            {
                m_logger.Log(messageParts);
                CheckSizeAndUpdate();
            }
            catch
            {
            }
        }

        private void CheckSizeAndUpdate()
        {
            if (!m_logger.IsFull())
            {
                return;
            }

            if (m_isMovingFiles)
            {
                return;
            }

            lock (m_logLock)
            {
                m_isMovingFiles = true;

                for (var i = HistoryCount - 1; i >= 0; i--)
                {
                    m_logger.MoveFile(FileName(i), FileName(i + 1));
                }

                m_isMovingFiles = false;
            }
        }

        private string FileName(Int32 historyIndex)
        {
            return $"{m_baseName}_{historyIndex}.{m_extension}";
        }
    }

    public class DebugLogger : LogHandler
    {
        private static DebugLogger instance;

        private DebugLogger() : base("Debug", "txt")
        {
        }

        public static DebugLogger Instance => instance ?? (instance = new DebugLogger());

        public void Log(params String[] messageParts)
        {
            try
            {
                base.TryLog(messageParts);
            }
            catch (Exception e)
            {
                ErrorLogger.Instance.Log(e);
            }
        }
    }

    public class EventLogger : LogHandler
    {
        private static EventLogger instance;

        private EventLogger() : base("Events", "txt")
        {
        }

        public static EventLogger Instance => instance ?? (instance = new EventLogger());

        public void Log(params String[] messageParts)
        {
            try
            {
                base.TryLog(messageParts);
            }
            catch (Exception e)
            {
                ErrorLogger.Instance.Log(e);
            }
        }
    }

    public class ErrorLogger : LogHandler
    {
        private static ErrorLogger instance;

        private ErrorLogger() : base("Errors", "txt")
        {
        }

        public static ErrorLogger Instance => instance ?? (instance = new ErrorLogger());

        public void Log(Exception e)
        {
            try
            {
                base.TryLog("EX", e.Message, e.StackTrace);
            }
            catch
            { }
        }

        public void Log(String where, String message)
        {
            try
            {
                base.TryLog("MSG", where, message);
            }
            catch
            { }
        }
    }
}
