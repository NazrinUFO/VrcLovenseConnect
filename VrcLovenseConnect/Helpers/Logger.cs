using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect.Helpers
{
    internal static class Logger
    {
        private static FileStream? log;
        private static StreamWriter? logWriter;
        private static bool disposedValue;

        public static void OpenLog(string path)
        {
            log = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            logWriter = new StreamWriter(log);
            logWriter.AutoFlush = true;
        }

        public static void LogException(Exception ex)
        {
            logWriter?.WriteLine($"[{DateTime.Now}] {ex.Message}");

            if (ex.StackTrace != null)
                logWriter?.WriteLine(ex.StackTrace);
        }

        private static void CloseLog(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    logWriter?.Dispose();
                    log?.Dispose();
                }

                disposedValue = true;
            }
        }

        public static void CloseLog()
        {
            CloseLog(true);
        }
    }
}
