using System;

namespace Fluxup.Core.Logging
{
    public static class LoggerEx
    {
        public static T ErrorAndReturnDefault<T>(this Logger logger, string message) 
        {
            logger.Error(message);
            return default;
        }

        public static T ErrorAndReturnDefault<T>(this Logger logger, Exception exception)
        {
            logger.Error(exception);
            return default;
        }
    }
}
