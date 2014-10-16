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
        FixedSizeObjPool<IConnection> _connectionPool;
        readonly AsyncSocketConnector _connector = new AsyncSocketConnector();
        readonly BufferManager _receiveBufferManager = new BufferManager();
        readonly BufferManager _sendBufferManager = new BufferManager();
        readonly IPacketContainer _packetContainer = new PacketContainer();
        readonly PacketHandler _packetHandler = new PacketHandler();
        readonly PacketSender _packetSender = new PacketSender();

        void InitConnectionPool()
        {
            _connection = new Connection(_receiveBufferManager, _packetContainer, _packetSender);
            _connection.Register(_connector);
            _connectionPool = new FixedSizeObjPool<IConnection>(1);
            _connectionPool.Push(_connection);
        }

        public void Send(byte[] contents, PacketType packetType)
        {
            _connection.Send(contents, packetType);
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

        public void Notify(IConnection connection, bool isConnected)
        {
            _connector.Notify(connection, isConnected);
        }

        #endregion
    }
}