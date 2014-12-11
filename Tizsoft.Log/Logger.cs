using NLog;

namespace Tizsoft.Log
{
    public static class GLogger
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Trace(string msg)
        {
            _logger.Trace(msg);
        }

        public static void Trace(string msg, params object[] args)
        {
            _logger.Trace(msg, args);
        }

        public static void Trace(object msg)
        {
            _logger.Trace(msg);
        }

        public static void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public static void Debug(string msg, params object[] args)
        {
            _logger.Debug(msg, args);
        }

        public static void Debug(object msg)
        {
            _logger.Debug(msg);
        }

        public static void Info(string msg)
        {
            _logger.Info(msg);
        }

        public static void Info(string msg, params object[] args)
        {
            _logger.Info(msg, args);
        }

        public static void Info(object msg)
        {
            _logger.Info(msg);
        }

        public static void Warn(string msg)
        {
            _logger.Warn(msg);
        }

        public static void Warn(string msg, params object[] args)
        {
            _logger.Warn(msg, args);
        }

        public static void Warn(object msg)
        {
            _logger.Warn(msg);
        }

        public static void Error(string msg)
        {
            _logger.Error(msg);
        }

        public static void Error(string msg, params object[] args)
        {
            _logger.Error(msg, args);
        }

        public static void Error(object msg)
        {
            _logger.Error(msg);
        }

        public static void Fatal(string msg)
        {
            _logger.Fatal(msg);
        }

        public static void Fatal(string msg, params object[] args)
        {
            _logger.Fatal(msg, args);
        }

        public static void Fatal(object msg)
        {
            _logger.Fatal(msg);
        }
    }
}