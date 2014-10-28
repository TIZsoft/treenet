using System.IO;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class Packet : IPacket
    {
        static readonly IPacket NullPacket = new NullPacket();

        static readonly IPacket KeepalivePacket = new NullPacket();

        public static IPacket Null { get { return NullPacket; } }

        public static IPacket Keepalive { get { return KeepalivePacket; }}

        readonly MemoryStream _buffer = new MemoryStream();

        public bool IsNull { get { return false; } }

        public PacketFlags PacketFlags { get; internal set; }

        public PacketType PacketType { get; internal set; }

        public byte[] Content
        {
            get { return _buffer.ToArray(); }
            internal set
            {
                _buffer.SetLength(0);
                if (value != null)
                {
                    _buffer.Write(value, 0, value.Length);
                }
                _buffer.Seek(0, SeekOrigin.Begin);
            }
        }

        public IConnection Connection { get; protected set; }

        public Packet()
        {
            Connection = Treenet.Connection.Null;
        }

        public void SetContent(IConnection connection, byte[] content, PacketType packetType)
        {
            Content = content;
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