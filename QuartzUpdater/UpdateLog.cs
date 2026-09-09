using System;
using System.IO;
using System.Text;

namespace QuartzUpdater
{
    internal static class UpdateLog
    {
        private static readonly object SyncRoot = new object();
        private static string _filePath;

        public static string FilePath
        {
            get { return _filePath; }
        }

        public static void Initialize(string filePath, string sessionMessage)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string fullPath = Path.GetFullPath(filePath);
            bool changed;

            lock (SyncRoot)
            {
                changed = !string.Equals(
                    _filePath,
                    fullPath,
                    StringComparison.OrdinalIgnoreCase);

                _filePath = fullPath;
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);
            }

            if (changed && !string.IsNullOrWhiteSpace(sessionMessage))
                Write(sessionMessage);
        }

        public static void Write(string message)
        {
            if (string.IsNullOrEmpty(_filePath))
                return;

            try
            {
                string line = string.Format(
                    "{0:yyyy-MM-dd HH:mm:ss.fff}  {1}{2}",
                    DateTime.Now,
                    message,
                    Environment.NewLine);

                lock (SyncRoot)
                {
                    File.AppendAllText(_filePath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Logging must never make an update fail.
            }
        }

        public static void Write(Exception exception)
        {
            if (exception != null)
                Write(exception.ToString());
        }
    }
}
