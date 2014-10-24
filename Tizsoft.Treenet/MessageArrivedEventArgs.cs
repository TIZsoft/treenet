using System;

namespace Tizsoft.Treenet
{
    public class MessageArrivedEventArgs : EventArgs
    {
        public byte[] Message { get; private set; }

        internal MessageArrivedEventArgs(byte[] message)
        {
            Message = message;
        }
    }
}