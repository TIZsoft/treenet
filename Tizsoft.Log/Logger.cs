using System;
using System.Collections.Generic;
using log4net;

namespace Tizsoft.Log
{
    public class LoggerEx : ILogger
    {
        readonly ILog _log;

        public LoggerEx(Type type)
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

        public void DebugFormat(string format, params object[] args)
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

        public void ErrorFormat(string format, params object[] args)
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

        public void FatalFormat(string format, params object[] args)
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

        public void InfoFormat(string format, params object[] args)
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

        public void WarnFormat(string format, params object[] args)
        {
            if (_log.IsWarnEnabled)
            {
                _log.WarnFormat(format, args);
            }
        }
    }

    public class Logger
    {
        static readonly Queue<string> MsgQueue = new Queue<string>();

        public static void Log(string msg)
        {
            MsgQueue.Enqueue(msg);
        }

        public static void LogWarning(string msg)
        {
            MsgQueue.Enqueue("warning: " + msg);
        }

        public static void LogError(string msg)
        {
            MsgQueue.Enqueue("error: " + msg);
        }

        public static void LogException(Exception e)
        {
            MsgQueue.Enqueue("exception: " + e.Message);
        }

        public static Queue<string> Msgs { get { return MsgQueue; } }
    }
}