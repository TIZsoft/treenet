using System;
using System.Collections.Generic;
using log4net;

namespace Tizsoft.Log
{
    public class Logger : ILogger
    {
        readonly ILog _log;

        public Logger(Type type)
        {
            _log = LogManager.GetLogger(type);
        }

        public void Debug(object message)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(message);
            }
        }

        public void Debug(string format, params object[] args)
        {
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat(format, args);
            }
        }

        public void Error(object message)
        {
            if (_log.IsErrorEnabled)
            {
                _log.Error(message);
            }
        }

        public void Error(string format, params object[] args)
        {
            if (_log.IsErrorEnabled)
            {
                _log.ErrorFormat(format, args);
            }
        }

        public void Fatal(object message)
        {
            if (_log.IsFatalEnabled)
            {
                _log.Fatal(message);
            }
        }

        public void Fatal(string format, params object[] args)
        {
            if (_log.IsFatalEnabled)
            {
                _log.FatalFormat(format, args);
            }
        }

        public void Info(object message)
        {
            if (_log.IsInfoEnabled)
            {
                _log.Info(message);
            }
        }

        public void Info(string format, params object[] args)
        {
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat(format, args);
            }
        }

        public void Warn(object message)
        {
            if (_log.IsWarnEnabled)
            {
                _log.Warn(message);
            }
        }

        public void Warn(string format, params object[] args)
        {
            if (_log.IsWarnEnabled)
            {
                _log.WarnFormat(format, args);
            }
        }
    }

    public static class GLogger
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(GLogger));

        public static void Debug(object message)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(message);
            }
        }

        public static void Debug(string format, params object[] args)
        {
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat(format, args);
            }
        }

        public static void Error(object message)
        {
            if (Log.IsErrorEnabled)
            {
                Log.Error(message);
            }
        }

        public static void Error(string format, params object[] args)
        {
            if (Log.IsErrorEnabled)
            {
                Log.ErrorFormat(format, args);
            }
        }

        public static void Fatal(object message)
        {
            if (Log.IsFatalEnabled)
            {
                Log.Fatal(message);
            }
        }

        public static void Fatal(string format, params object[] args)
        {
            if (Log.IsFatalEnabled)
            {
                Log.FatalFormat(format, args);
            }
        }

        public static void Info(object message)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info(message);
            }
        }

        public static void Info(string format, params object[] args)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat(format, args);
            }
        }

        public static void Warn(object message)
        {
            if (Log.IsWarnEnabled)
            {
                Log.Warn(message);
            }
        }

        public static void Warn(string format, params object[] args)
        {
            if (Log.IsWarnEnabled)
            {
                Log.WarnFormat(format, args);
            }
        }
    }
}