using System.Net.Sockets;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class NullPacket : Packet, INullObj
    {
        static NullPacket _instance;

        public static NullPacket Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NullPacket();

                return _instance;
            }
        }

        NullPacket()
        {
            Connection = NullConnection.Instance;
        }

        public new void SetContent(Connection connection, SocketAsyncEventArgs asyncArgs)
        {
        }

        public new void Clear()
        {
        }

        public new byte[] Content { get { return null; } }

        public new Connection Connection { get; private set; }

        #region INullObj Members

        public new bool IsNull
        {
            get { return true; }
        }

        #endregion
    }
}
