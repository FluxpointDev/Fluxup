using System;

namespace Fluxup.Core.Logging
{
    public class LogArgs : EventArgs
    {
        internal LogArgs(string message, LogLevel logLevel, string loggerName)
        {
            Message = message;
            LogLevel = logLevel;
            LoggerName = loggerName;
        }
        
        public string Message { get; }
        public LogLevel LogLevel { get; }
        public string LoggerName { get; }
    }
}