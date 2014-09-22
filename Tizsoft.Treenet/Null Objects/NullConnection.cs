using System.Net.Sockets;

namespace Tizsoft.Treenet
{
    public class NullConnection : Connection
    {
        static NullConnection _instance;

        public static NullConnection Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NullConnection();

                return _instance;
            }
        }

        NullConnection()
        {
        }

        public void SetConnection(Socket socket, bool connected)
        {
            // Purposefully provides no behaviour.
            Logger.LogWarning("SetConnection in NullConnection");
        }

        public bool IsConnected { get; private set; }

        #region IDisposable Members

        public new void Dispose()
        {
            // Purposefully provides no behaviour.
            Logger.LogWarning("Dispose in NullConnection");
        }

        #endregion

        #region INullObj Members

        public new bool IsNull
        {
            get { return true; }
        }

        #endregion
    }
}
