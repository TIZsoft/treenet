using System;
using System.Collections.Generic;
using log4net;

namespace Tizsoft.Log
{
    public static class LoggerEx
    {
        static ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Debug(object message)
        {
            _log.Debug(message);
        }

        public static void DebugFormat(string format, params object[] args)
        {
            _log.DebugFormat(format, args);
        }

        public static void Error(object message)
        {
            _log.Error(message);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }

        public static void Fatal(object message)
        {
            _log.Fatal(message);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            _log.FatalFormat(format, args);
        }

        public static void Info(object message)
        {
            _log.Info(message);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        public static void Warn(object message)
        {
            _log.Warn(message);
        }

        public static void WarnFormat(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
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