using System;
using System.Net.Sockets;

namespace Tizsoft.Treenet
{
    public class ClientConfig : EventArgs
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public int BufferSize { get; set; }

        public SocketType TransferType { get; set; }

        public ProtocolType UseProtocol { get; set; }

        public ClientConfig()
        {
            Address = "127.0.0.1";
            TransferType = SocketType.Stream;
            UseProtocol = ProtocolType.Tcp;
        }
    }
}