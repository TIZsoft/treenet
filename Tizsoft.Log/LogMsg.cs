using System.Drawing;

namespace Tizsoft.Log
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
}