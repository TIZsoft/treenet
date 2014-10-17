using Tizsoft.Security.Cryptography;

namespace Tizsoft.Treenet.Interface
{
    public interface IPacketContainer
    {
        void Setup(ICryptoProvider crypto);

        void AddPacket(IConnection connection, byte[] contents, PacketType packetType);

        void RecyclePacket(IPacket packet);

        void Clear();

        void ValidatePacket(IConnection connection, byte[] buffer);

        IPacket NextPacket();
    }
}