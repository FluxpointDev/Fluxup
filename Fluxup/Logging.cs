using System;

namespace Fluxup.Updater
{
    public static class Logging
    {
        public static event EventHandler<LogArgs> NewLog;

        internal static void TriggerLog(LogArgs logArgs)
        {
            NewLog?.Invoke(null, logArgs);
        }
        
        internal static void TriggerLog(string message, LogLevel logLevel)
        {
            NewLog?.Invoke(null, new LogArgs(message, logLevel));
        }
    }
}