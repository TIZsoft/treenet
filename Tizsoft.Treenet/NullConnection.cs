﻿using System.Net.Sockets;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class NullConnection : IConnection
    {
        public bool IsNull { get { return true; } }

        public string DestAddress
        {
            get { return null; }
        }

        public Socket ConnectSocket
        {
            get { return null; }
        }

        public void Dispose()
        {

        }

        public void Register(IConnectionObserver observer)
        {
            
        }

        public void Unregister(IConnectionObserver observer)
        {
            
        }

        public void Notify(IConnection connection, bool isConnected)
        {
            
        }

        public void SetConnection(Socket socket)
        {

        }

        public void Send(byte[] content, PacketType packetType)
        {

        }
    }
}