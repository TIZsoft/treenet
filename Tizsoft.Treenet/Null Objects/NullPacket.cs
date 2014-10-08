using System.Net.Sockets;

namespace Tizsoft.Treenet
{
    public class NullPacket : Packet
    {
        static Packet _instance;

        public static Packet Instance
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
            Connection = Connection.NullConnection;
        }

        public override void SetContent(Connection connection, SocketAsyncEventArgs asyncArgs)
        {
        }

        public override void Clear()
        {
        }

        public override byte[] Content { get { return null; } }

        #region INullObj Members

        public override bool IsNull
        {
            get { return true; }
        }

        #endregion
    }
}
