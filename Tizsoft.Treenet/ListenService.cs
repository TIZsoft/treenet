using System;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ListenService : IService, IConnectionSubject
    {
        FixedSizeObjPool<Connection> _connectionPool;
        readonly BufferManager _receiveBufferManager;
        readonly BufferManager _sendBufferManager;
        readonly AsyncSocketListener _socketListener;
        readonly IPacketContainer _packetContainer;
        readonly PacketHandler _packetHandler;
        readonly PacketSender _packetSender;

        void InitConnectionPool(int maxConnections, IPacketContainer packetContainer)
        {
            _connectionPool = new FixedSizeObjPool<Connection>(maxConnections);

            for (var i = 0; i < maxConnections; ++i)
            {
                var connection = new Connection(_receiveBufferManager, packetContainer, _packetSender);
                connection.Register(_socketListener);
                _connectionPool.Push(connection);
            }
        }

        public ListenService()
        {
            _receiveBufferManager = new BufferManager();
            _sendBufferManager = new BufferManager();
            _socketListener = new AsyncSocketListener();
            _packetContainer = new PacketContainer();
            _packetHandler = new PacketHandler();
            _packetSender = new PacketSender();
        }

        public void AddParser(PacketType type, IPacketProcessor processor)
        {
            _packetHandler.AddParser(type, processor);
        }

        #region IService Members

        public void Start()
        {
            _socketListener.Start();
            IsWorking = true;
        }

        public void Setup(EventArgs configArgs)
        {
            var config = (ServerConfig) configArgs;

            if (config == null)
                throw new InvalidCastException("configArgs");

            _receiveBufferManager.InitBuffer(config.MaxConnections * config.BufferSize, config.BufferSize);
            InitConnectionPool(config.MaxConnections, _packetContainer);
            var sendConnection = Math.Max(1, config.MaxConnections / 10);
            _sendBufferManager.InitBuffer(sendConnection * config.BufferSize, config.BufferSize);
            _packetSender.Setup(_sendBufferManager, config.MaxConnections / 10);
            
            _socketListener.Setup(config, _connectionPool);
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
            _socketListener.Stop();
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion

        #region IConnectionSubject Members

        public void Register(IConnectionObserver observer)
        {
            _socketListener.Register(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            _socketListener.Unregister(observer);
        }

        public void Notify(Connection connection, bool isConnect)
        {
            _socketListener.Notify(connection, isConnect);
        }

        #endregion
    }
}