using System;
using System.Linq;
using System.Reflection;

namespace Fluxup.Core.Logging
{
    /// <summary>
    /// Logger extensions
    /// </summary>
    public static class LoggerEx
    {
        /// <summary>
        /// Logs the message and then returns <see cref="T"/>
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message to log</param>
        /// <typeparam name="T">Type to return</typeparam>
        /// <returns><see cref="T"/></returns>
        public static T ErrorAndReturnDefault<T>(this Logger logger, string message) 
        {
            logger.Error(message);
            var constructors = typeof(T).GetConstructors(BindingFlags.NonPublic);
            return (T)Activator.CreateInstance(typeof(T),!constructors.Any(x => x.IsPublic));
        }

        /// <summary>
        /// Logs the message and then returns <see cref="T"/>
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="exception">Exception to log</param>
        /// <typeparam name="T">Type to return</typeparam>
        /// <returns><see cref="T"/></returns>
        public static T ErrorAndReturnDefault<T>(this Logger logger, Exception exception)
        {
            logger.Error(exception);
            var constructors = typeof(T).GetConstructors();
            return (T)Activator.CreateInstance(typeof(T),!constructors.Any(x => x.IsPublic));
        }
    }
}
