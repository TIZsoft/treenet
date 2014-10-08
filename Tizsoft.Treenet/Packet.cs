using System;
using System.IO;
using System.Net.Sockets;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class Packet : INullObj
    {
        readonly MemoryStream _buffer;
        PacketType _packetType;

        public Packet()
        {
            _buffer = new MemoryStream();
            _packetType = PacketType.Stream;
            Connection = Connection.NullConnection;
        }

        public virtual void SetContent(Connection connection, SocketAsyncEventArgs asyncArgs)
        {
            _buffer.SetLength(0);
            _buffer.Write(asyncArgs.Buffer, asyncArgs.Offset, asyncArgs.BytesTransferred);
            _buffer.Seek(0, SeekOrigin.Begin);

            Connection = connection;
        }

        public virtual void Clear()
        {
            _buffer.SetLength(0);
            Connection = Connection.NullConnection;
        }

        public virtual PacketType PacketType
        {
            get { return Enum.IsDefined(typeof(PacketType), _packetType) ? _packetType : PacketType.Stream; }
            protected set { _packetType = value; }
        }

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