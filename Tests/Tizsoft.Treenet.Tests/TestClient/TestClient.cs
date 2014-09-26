using System.Net.Sockets;
using System.Text;
using Tizsoft.Collections;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.Tests.TestClient
{
    /// <summary>
    /// support "only 1" connection
    /// </summary>
    public class TestClient : IConnectionObserver, IService
    {
        Connection _connection;
        ClientConfig _config;
        SimpleObjPool<Connection> _connectionPool;
        readonly AsyncSocketConnector _connector;
        readonly BufferManager _bufferManager;
        readonly IPacketContainer _packetContainer;
        readonly ConnectionObserver _connectionObserver;
        readonly PacketHandler _packetHandler;

        void InitConnectionPool()
        {
            _connection = new Connection(_bufferManager, _packetContainer);
            _connection.Register(_connectionObserver);
            _connectionPool = new SimpleObjPool<Connection>(1);
            _connectionPool.Push(_connection);
        }

        public TestClient()
        {
            _connector = new AsyncSocketConnector();
            _packetContainer = new PacketContainer();
            _bufferManager = new BufferManager();
            _connectionObserver = new ConnectionObserver();
            _connector.Register(_connectionObserver);
            _packetHandler = new PacketHandler();
        }

        public void Setup(ClientConfig config)
        {
            _config = config;
            InitConnectionPool();
            _connectionObserver.Setup(_connectionPool);
            _bufferManager.InitBuffer(_config.BufferSize * 2, _config.BufferSize);
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

        public void GetConnectionEvent(Socket socket, bool isConnect)
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
        }

        #endregion

        #region IService Members

        public void Start()
        {
            _connector.Connect(_config);
            IsWorking = true;
        }

        public void Stop()
        {
            if (!IsConnected)
                _connector.Stop();
            else
                _connection.Dispose();

            _connectionObserver.Reset();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion
    }
}