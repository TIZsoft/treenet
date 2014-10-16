namespace Tizsoft.Treenet.Interface
{
    public interface IPacketContainer
    {
        void AddPacket(IConnection connection, byte[] contents, PacketType packetType);

        void RecyclePacket(IPacket packet);

        void Clear();

        IPacket NextPacket();
    }
}