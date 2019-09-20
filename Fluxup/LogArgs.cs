using System;

namespace Fluxup.Updater
{
    public class LogArgs : EventArgs
    {
        internal LogArgs(string message, LogLevel logLevel)
        {
            Message = message;
            LogLevel = logLevel;
        }
        
        public string Message { get; }
        public LogLevel LogLevel { get; }
    }
}