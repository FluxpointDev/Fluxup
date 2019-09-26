using System;
using System.Runtime.CompilerServices;

namespace Fluxup.Core.Logging
{
    public class Logger
    {
        public static event EventHandler<LogArgs> NewLog;
        public string LoggerName { get; }

        public Logger(string loggerName)
        {
            LoggerName = loggerName;
        }

        public void Debug(string message, [CallerMemberName] string objName = "")
        {
            NewLog.Invoke(objName, new LogArgs(message, LogLevel.Debug, LoggerName));
        }

        public void Error(Exception exception, [CallerMemberName] string objName = "")
        {
            var message = $"[{LoggerName}]\r\nMessage: {exception.Message}\r\nStackTrace: {exception.StackTrace}\r\nSource: {exception.Source}";
            NewLog.Invoke(objName, new LogArgs(message, LogLevel.Error, LoggerName));
        }

        public void Error(string message, [CallerMemberName] string objName = "")
        {
            NewLog.Invoke(objName, new LogArgs(message, LogLevel.Error, LoggerName));
        }

        public void Warning(string message, [CallerMemberName] string objName = "")
        {
            NewLog.Invoke(objName, new LogArgs(message, LogLevel.Warning, LoggerName));
        }

        public void Information(string message, [CallerMemberName] string objName = "")
        {
            NewLog.Invoke(objName, new LogArgs(message, LogLevel.Info, LoggerName));
        }
    }
}