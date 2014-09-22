using System.Text;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.Tests.TestClient
{
    /// <summary>
    /// support "only 1" connection
    /// </summary>
    public class TestClient : IConnectionObserver
    {
        readonly AsyncSocketConnector _connector;
        readonly Connection _connection;
        readonly BufferManager _bufferManager;
        readonly IPacketContainer _packetContainer;
        ClientConfig _config;

        public TestClient()
        {
            _connector = new AsyncSocketConnector();
            _bufferManager = new BufferManager();
            _packetContainer = new PacketContainer();
            _connection = new Connection(_bufferManager, _packetContainer);
            _connector.Register(this);
            _connection.Register(this);
        }

        public void Setup(ClientConfig config)
        {
            _config = config;
            _bufferManager.InitBuffer(_config.BufferSize * 2, _config.BufferSize);
        }

        public void Start()
        {
            _connector.Connect(_config);
        }

        public void Stop()
        {
            if (!IsConnected)
                _connector.Stop();
            else
                _connection.Dispose();
        }

        public void Update()
        {
            var packet = _packetContainer.NextPacket();

            if (packet.IsNull || packet.Connection.IsNull)
                _packetContainer.RecyclePacket(packet);
            else
            {
                Logger.Log(string.Format("得到 server 傳回的訊息：{0}", Encoding.UTF8.GetString(packet.Content)));
            }
        }

        public bool IsConnected { get; private set; }

        public Connection Connection { get { return _connection; } }

        #region IConnectionObserver Members

        public bool GetConnectionEvent(System.Net.Sockets.Socket socket, bool isConnect)
        {
            switch (isConnect)
            {
                case false:
                    Logger.LogError("連線失敗!");
                    break;

                default:
                    _connection.SetConnection(socket);
                    break;
            }

            IsConnected = isConnect;
            return true;
        }

        #endregion
    }
}