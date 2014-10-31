using System.IO;
using System.Text;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class Packet : IPacket
    {
        static readonly IPacket NullPacket = new NullPacket();

        static readonly IPacket KeepalivePacket = new NullPacket();

        public static IPacket Null { get { return NullPacket; } }

        public static IPacket Keepalive { get { return KeepalivePacket; } }

        readonly MemoryStream _buffer = new MemoryStream();

        public bool IsNull { get { return false; } }

        public PacketFlags PacketFlags { get; set; }

        public PacketType PacketType { get; set; }

        public byte[] Content
        {
            get { return _buffer.ToArray(); }
            set
            {
                _buffer.SetLength(0);
                if (value != null)
                {
                    _buffer.Write(value, 0, value.Length);
                }
                _buffer.Seek(0, SeekOrigin.Begin);
            }
        }

        public IConnection Connection { get; set; }

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

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("PacketFlags={0}\n", PacketFlags);
            builder.AppendFormat("PacketType={0}\n", PacketType);

            var content = Content;
            builder.Append("Content=");
            if (content != null)
            {
                builder.Append("{ ");
                for (var i = 0; i != content.Length; ++i)
                {
                    builder.Append(content[i].ToString("X"));
                    if (i < content.Length - 1)
                        builder.Append(", ");
                }
                builder.Append(" }");
            }
            else
            {
                builder.AppendLine("<null>");
            }
            builder.AppendLine();

            return builder.ToString();
        }
    }
}