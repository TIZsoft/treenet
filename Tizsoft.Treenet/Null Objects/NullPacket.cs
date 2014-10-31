using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class NullPacket : IPacket
    {
        public bool IsNull { get { return true; } }

        public PacketFlags PacketFlags { get; set; }

        public PacketType PacketType
        {
            get { return default(PacketType); }
            set { }
        }

        public byte[] Content
        {
            get { return null; }
            set { }
        }

        public IConnection Connection
        {
            get { return null; }
            set { }
        }

        public void SetContent(IConnection connection, byte[] content, PacketType packetType)
        {
            
        }

        public void Clear()
        {
            
        }
    }
}