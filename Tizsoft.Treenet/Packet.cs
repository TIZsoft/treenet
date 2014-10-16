using System.IO;

namespace Tizsoft.Treenet
{
    public class Packet : IPacket
    {
        static readonly IPacket NullPacket = new NullPacket();

        public static IPacket Null { get { return NullPacket; } }

        readonly MemoryStream _buffer = new MemoryStream();

        public bool IsNull { get { return false; } }

        public PacketType PacketType { get; protected set; }

        public byte[] Content { get { return _buffer.ToArray(); } }

        public IConnection Connection { get; protected set; }

        public Packet()
        {
            Connection = Treenet.Connection.Null;
        }

        public void SetContent(IConnection connection, byte[] contents, PacketType packetType)
        {
            _buffer.SetLength(0);

            if (contents != null)
                _buffer.Write(contents, 0, contents.Length);

            _buffer.Seek(0, SeekOrigin.Begin);
            PacketType = packetType;
            Connection = connection;
        }

        public void Clear()
        {
            _buffer.SetLength(0);
            Connection = Treenet.Connection.Null;
        }
    }
}