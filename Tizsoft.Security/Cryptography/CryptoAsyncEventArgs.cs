using System;

namespace Tizsoft.Security.Cryptography
{
    public class CryptoAsyncEventArgs : EventArgs
    {
        public byte[] Bytes { get; private set; }

        internal CryptoAsyncEventArgs(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}