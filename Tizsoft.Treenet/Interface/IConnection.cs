using System;
using System.Net.Sockets;

namespace Tizsoft.Treenet.Interface
{
    public interface IConnection : IDisposable, IConnectionSubject
    {
        bool IsNull { get; }

        string DestAddress { get; }

        Socket ConnectSocket { get; }

        PacketProtocol PacketProtocol { get; set; }

        void SetConnection(Socket socket);

        void Send(byte[] content, PacketType packetType);

        double IdleTime { get; set;}

        bool IsActive { get; }
    }
}