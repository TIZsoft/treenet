using System.Net.Sockets;
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

        public PacketProtocol PacketProtocol { get; set; }

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

        public int Count { get { return 0; } }

        public void SetConnection(Socket socket)
        {
        }

        public void Send(byte[] content, PacketType packetType)
        {

        }

        public double IdleTime { get; set; }

        public bool IsActive { get; private set; }

        public bool DisconnectAfterSend { get; set; }
    }
}