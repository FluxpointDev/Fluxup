using System;

namespace Fluxup.Core.Logging
{
    /// <summary>
    /// Arguments from a log
    /// </summary>
    public class LogArgs : EventArgs
    {
        internal LogArgs(string message, LogLevel logLevel, string loggerName)
        {
            Message = message;
            LogLevel = logLevel;
            LoggerName = loggerName;
        }
        
        /// <summary>
        /// Message of the log
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The level of the log
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// The logger name that triggered this log
        /// </summary>
        public string LoggerName { get; }
    }
}