using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class NullPacket : IPacket
    {
        public bool IsNull { get { return true; } }

        public PacketFlags PacketFlags { get; private set; }

        public PacketType PacketType
        {
            get { return default(PacketType); }
        }

        public byte[] Content
        {
            get { return null; }
        }

        public IConnection Connection
        {
            get { return null; }
        }

        public void SetContent(IConnection connection, byte[] content, PacketType packetType)
        {
            
        }

        public void Clear()
        {
            
        }
    }
}