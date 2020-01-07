using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary
{
    public static class Logger
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void LogInfo(string logMessage)
        {
            _logger.Info(logMessage);
        }

        public static void LogDebug(string logMessage)
        {
            _logger.Debug(logMessage);
        }

        public static void LogError(Exception e)
        {
            _logger.Error(e);
        }

        public static void LogWarn(string logMessage)
        {
            _logger.Warn(logMessage);
        }

        public static void LogTrace(string logMessage)
        {
            _logger.Trace(logMessage);
        }
    }
}
