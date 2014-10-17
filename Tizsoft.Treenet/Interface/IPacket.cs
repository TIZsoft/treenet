namespace Tizsoft.Treenet.Interface
{
    public interface IPacket
    {
        bool IsNull { get; }

        PacketType PacketType { get; }

        byte[] Content { get; }

        IConnection Connection { get; }

        void SetContent(IConnection connection, byte[] contents, PacketType packetType);

        void Clear();
    }
}