namespace Tizsoft.Treenet.Interface
{
    public interface IPacket
    {
        bool IsNull { get; }

        PacketFlags PacketFlags { get; }

        PacketType PacketType { get; }

        byte[] Content { get; }

        IConnection Connection { get; }

        void SetContent(IConnection connection, byte[] content, PacketType packetType);

        void Clear();
    }
}