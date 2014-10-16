using System;
using System.Net.Sockets;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public interface IConnection : IDisposable, IConnectionSubject
    {
        bool IsNull { get; }

        string DestAddress { get; }

        Socket ConnectSocket { get; }

        void SetConnection(Socket socket);

        void Send(byte[] content, PacketType packetType);
    }
}