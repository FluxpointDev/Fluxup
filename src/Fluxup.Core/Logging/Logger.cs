using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Fluxup.Updater", AllInternalsVisible = true)]
namespace Fluxup.Core.Logging
{
    //TODO: Have a public log level, allowing this to filter out logs that the dev doesn't need
    /// <summary>
    /// Logger for logging! 
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Event that is fired when a new log has been made
        /// </summary>
        public static event EventHandler<LogArgs> NewLog;
        
        /// <summary>
        /// Makes a <see cref="Logger"/>
        /// </summary>
        /// <param name="loggerName">The name of the logger</param>
        public Logger(string loggerName)
        {
            LoggerName = loggerName;
        }

        /// <summary>
        /// The name of the logger
        /// </summary>
        public string LoggerName { get; }

        /// <summary>
        /// Debug log
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objName">CallerMemberName</param>
        public void Debug(string message, [CallerMemberName] string objName = "")
        {
            NewLog?.Invoke(objName, new LogArgs(message, LogLevel.Debug, LoggerName));
        }

        /// <summary>
        /// Error log
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="objName">CallerMemberName</param>
        public void Error(Exception exception, [CallerMemberName] string objName = "")
        {
            var message = $"[{LoggerName}]\r\nMessage: {exception.Message}\r\nStackTrace: {exception.StackTrace}" +
                          $"\r\nSource: {exception.Source}";
            NewLog?.Invoke(objName, new LogArgs(message, LogLevel.Error, LoggerName));
        }

        /// <summary>
        /// Error log
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objName">CallerMemberName</param>
        public void Error(string message, [CallerMemberName] string objName = "")
        {
            NewLog?.Invoke(objName, new LogArgs(message, LogLevel.Error, LoggerName));
        }

        /// <summary>
        /// Warning log
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objName">CallerMemberName</param>
        public void Warning(string message, [CallerMemberName] string objName = "")
        {
            NewLog?.Invoke(objName, new LogArgs(message, LogLevel.Warning, LoggerName));
        }

        /// <summary>
        /// Information log
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objName">CallerMemberName</param>
        public void Information(string message, [CallerMemberName] string objName = "")
        {
            NewLog?.Invoke(objName, new LogArgs(message, LogLevel.Info, LoggerName));
        }
    }
}