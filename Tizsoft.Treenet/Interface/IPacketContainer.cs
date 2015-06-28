namespace Tizsoft.Treenet.Interface
{
    public interface IPacketContainer
    {
        void AddPacket(IConnection connection, byte[] content, PacketType packetType);

        void AddPacket(IPacket packet);

        void RecyclePacket(IPacket packet);

        void Clear();

        void ClearDisconnectedPacket(IConnection connection);

        IPacket NextPacket();
    }
}