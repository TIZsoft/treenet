using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class Packet : INullObj
    {
        MemoryStream _buffer;
        int _packetType;

        public void SetContent(Connection connection, SocketAsyncEventArgs asyncArgs)
        {
            if (_buffer == null)
                _buffer = new MemoryStream();

            _buffer.Write(asyncArgs.Buffer, asyncArgs.Offset, asyncArgs.Count);
            _buffer.Seek(0, SeekOrigin.Begin);

            try
            {
                using (var reader = new BinaryReader(_buffer, Encoding.UTF8, true))
                {
                    Header = reader.ReadBytes(Network.PacketHeaderSize);
                    _packetType = reader.ReadInt32();
                    var length = reader.ReadInt32();
                    Content = reader.ReadBytes(length);
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            finally
            {
                _buffer.SetLength(0);
            }

            Connection = connection;
        }

        public void Clear()
        {
            _buffer.SetLength(0);
            Array.Clear(Header, 0, Header.Length);
            Array.Clear(Content, 0, Content.Length);
            Connection = Connection.NullConnection;
        }

        public byte[] Header { get; private set; }

        public PacketType PacketType
        {
            get
            {
                return Enum.IsDefined(typeof(PacketType), _packetType) ? (PacketType)_packetType : PacketType.Undefine;
            }
        }

        public byte[] Content { get; private set; }

        public Connection Connection { get; private set; }

        #region INullObj Members

        public bool IsNull
        {
            get { return false; }
        }

        #endregion
    }
}