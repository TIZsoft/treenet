using System.Net.Sockets;

namespace Tizsoft.Treenet.Interface
{
    public interface IPacketContainer
    {
        void AddPacket(Connection connection, SocketAsyncEventArgs aysncArgs);

        void RecyclePacket(Packet packet);

        void Clear();

        Packet NextPacket();
    }
}