using System;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    /// <summary>
    /// support "only 1" connection
    /// </summary>
    public class ConnectService : IService, IConnectionSubject
    {
        Connection _connection;
        ClientConfig _config;
        FixedSizeObjPool<Connection> _connectionPool;
        readonly AsyncSocketConnector _connector;
        readonly BufferManager _receiveBufferManager;
        readonly BufferManager _sendBufferManager;
        readonly IPacketContainer _packetContainer;
        readonly PacketHandler _packetHandler;
        readonly PacketSender _packetSender;

        void InitConnectionPool()
        {
            _connection = new Connection(_receiveBufferManager, _packetContainer, _packetSender);
            _connection.Register(_connector);
            _connectionPool = new FixedSizeObjPool<Connection>(1);
            _connectionPool.Push(_connection);
        }

        public ConnectService()
        {
            _connector = new AsyncSocketConnector();
            _packetContainer = new PacketContainer();
            _receiveBufferManager = new BufferManager();
            _sendBufferManager = new BufferManager();
            _packetHandler = new PacketHandler();
            _packetSender = new PacketSender();
        }

        public void Send(byte[] contents)
        {
            _connection.Send(contents);
        }

        public void AddParser(PacketType type, IPacketProcessor processor)
        {
            _packetHandler.AddParser(type, processor);
        }

        #region IService Members

        public void Start()
        {
            _connector.Connect();
            IsWorking = true;
        }

        public void Setup(EventArgs configArgs)
        {
            _config = (ClientConfig)configArgs;

            if (_config == null)
                throw new InvalidCastException("configArgs");

            _receiveBufferManager.InitBuffer(_config.BufferSize, _config.BufferSize);
            _sendBufferManager.InitBuffer(_config.BufferSize, _config.BufferSize);
            _packetSender.Setup(_sendBufferManager, 1);
            InitConnectionPool();
            _connector.Setup(_config, _connectionPool);
        }

        public void Update()
        {
            if (IsWorking)
            {
                var packet = _packetContainer.NextPacket();

                if (!packet.IsNull)
                    _packetHandler.Parse(packet);
            }
        }

        public void Stop()
        {
            _connector.Stop();
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion

        #region IConnectionSubject Members

        public void Register(IConnectionObserver observer)
        {
            _connector.Register(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            _connector.Unregister(observer);
        }

        public void Notify(Connection connection, bool isConnect)
        {
            _connector.Notify(connection, isConnect);
        }

        #endregion
    }
}