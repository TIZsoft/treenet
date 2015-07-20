using System;

namespace Tizsoft.Treenet
{
    public class MessageArrivedEventArgs : EventArgs
    {
        public MessageFramingErrorCode ErrorCode { get; private set; }

        public byte[] Message { get; private set; }

        internal MessageArrivedEventArgs(MessageFramingErrorCode errorCode, byte[] message)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
}