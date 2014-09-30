using System;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.Tests.TestClient
{
    /// <summary>
    /// support "only 1" connection
    /// </summary>
    public class ConnectService : IService
    {
        Connection _connection;
        ClientConfig _config;
        SimpleObjPool<Connection> _connectionPool;
        readonly AsyncSocketConnector _connector;
        readonly BufferManager _bufferManager;
        readonly IPacketContainer _packetContainer;
        readonly ConnectionObserver _connectionObserver;
        readonly PacketHandler _packetHandler;
        bool _isInit = false;

        void InitConnectionPool()
        {
            _connection = new Connection(_bufferManager, _packetContainer);
            _connection.Register(_connectionObserver);
            _connectionPool = new SimpleObjPool<Connection>(1);
            _connectionPool.Push(_connection);
        }

        public ConnectService()
        {
            _connector = new AsyncSocketConnector();
            _packetContainer = new PacketContainer();
            _bufferManager = new BufferManager();
            _connectionObserver = new ConnectionObserver();
            _connector.Register(_connectionObserver);
            _packetHandler = new PacketHandler();
        }

        ~ConnectService()
        {
            Stop();
        }

        public void Send(byte[] contents)
        {
            _connection.Send(contents);
        }

        public Packet Receive()
        {
            return IsWorking ? _packetContainer.NextPacket() : Packet.NullPacket;
        }

        #region IService Members

        public void Start()
        {
            _connector.Connect(_config);
            IsWorking = true;
        }

        public void Setup(EventArgs configArgs)
        {
            _config = (ClientConfig)configArgs;

            if (_config == null)
                throw new InvalidCastException("configArgs");

            if (!_isInit)
            {
                InitConnectionPool();
                _connectionObserver.Setup(_connectionPool);
                _bufferManager.InitBuffer(_config.BufferSize * 2, _config.BufferSize);
                _isInit = true;
            }
        }

        public void Update()
        {
            //if (!IsWorking)
            //    return;

            //var packet = _packetContainer.NextPacket();

            //if (packet.IsNull || packet.Connection.IsNull)
            //    _packetContainer.RecyclePacket(packet);
            //else
            //{
            //    //Logger.Log(string.Format("得到 server 傳回的訊息：{0}", Encoding.UTF8.GetString(packet.Content)));
            //    _packetHandler.Parse(packet);
            //}
        }

        public void Stop()
        {
            _connector.Stop();
            _connectionObserver.Reset();
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion
    }
}