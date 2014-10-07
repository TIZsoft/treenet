namespace Tizsoft.Treenet.Interface
{
    public interface IPacketContainer
    {
        void AddPacket(Connection connection, byte[] contents, PacketType packetType);

        void RecyclePacket(Packet packet);

        void Clear();

        Packet NextPacket();
    }
}