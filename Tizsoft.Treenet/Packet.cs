using System.IO;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class Packet : INullObj
    {
        readonly MemoryStream _buffer;

        public Packet()
        {
            _buffer = new MemoryStream();
            Connection = Connection.NullConnection;
        }

        public virtual void SetContent(Connection connection, byte[] contents, PacketType packetType)
        {
            _buffer.SetLength(0);

            if (contents != null)
                _buffer.Write(contents, 0, contents.Length);

            _buffer.Seek(0, SeekOrigin.Begin);
            PacketType = packetType;
            Connection = connection;
        }

        public virtual void Clear()
        {
            _buffer.SetLength(0);
            Connection = Connection.NullConnection;
        }

        public virtual PacketType PacketType { get; protected set; }

        public virtual byte[] Content { get { return _buffer.ToArray(); } }

        public Connection Connection { get; protected set; }

        public static Packet NullPacket { get { return Treenet.NullPacket.Instance; } }

        #region INullObj Members

        public virtual bool IsNull
        {
            get { return false; }
        }

        #endregion
    }
}