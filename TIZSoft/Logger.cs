using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tizsoft
{
    public struct LogMsg
    {
        public string Msg;
        public Color MsgColor;

        public LogMsg(string msg, Color color)
        {
            Msg = msg;
            MsgColor = color;
        }
    }

    public class Logger
    {
        static Queue<string> _msgQueue = new Queue<string>();

        public static void Log(string msg)
        {
            _msgQueue.Enqueue(msg);
        }

        public static void LogWarning(string msg)
        {
            _msgQueue.Enqueue("warning: " + msg);
        }

        public static void LogError(string msg)
        {
            _msgQueue.Enqueue("error: " + msg);
        }

        public static void LogException(Exception e)
        {
            _msgQueue.Enqueue("exception: " + e.Message);
        }

        public static Queue<string> Msgs { get { return _msgQueue; } }
    }
}