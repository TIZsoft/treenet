namespace Tizsoft.Treenet.Interface
{
    public interface IPacket
    {
        bool IsNull { get; }

        PacketFlags PacketFlags { get; set; }

        PacketType PacketType { get; set; }

        byte[] Content { get; set; }

        IConnection Connection { get; set; }

        void Clear();
    }
}