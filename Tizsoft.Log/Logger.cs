using System;
using System.Collections.Generic;
using log4net;

namespace Tizsoft.Log
{
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