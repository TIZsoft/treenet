using System.Net.Sockets;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

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

        public override void Register(IConnectionObserver observer)
        {
        }

        public override void Unregister(IConnectionObserver observer)
        {
        }

        public override void Notify(Socket socket, bool isConnect)
        {
        }

        public override void Send(byte[] content)
        {
        }

        #region IDisposable Members

        public override void Dispose()
        {
            // Purposefully provides no behaviour.
            Logger.LogWarning("Dispose in NullConnection");
        }

        #endregion

        #region INullObj Members

        public override bool IsNull
        {
            get { return true; }
        }

        #endregion
    }
}
