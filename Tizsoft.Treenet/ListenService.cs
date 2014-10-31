using System;
using Tizsoft.Collections;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ListenService : IService, IConnectionSubject
    {
        FixedSizeObjPool<IConnection> _connectionPool;
        readonly BufferManager _receiveBufferManager = new BufferManager();
        readonly BufferManager _sendBufferManager = new BufferManager();
        readonly AsyncSocketListener _socketListener = new AsyncSocketListener();
        readonly IPacketContainer _packetContainer = new PacketContainer();
        readonly PacketHandler _packetHandler = new PacketHandler();
        readonly PacketSender _packetSender = new PacketSender();

        void InitConnectionPool(int maxConnections, IPacketContainer packetContainer, int maxMessageSize)
        {
            _connectionPool = new FixedSizeObjPool<IConnection>(maxConnections);

            for (var i = 0; i < maxConnections; ++i)
            {
                var connection = new Connection(_receiveBufferManager, packetContainer, _packetSender, maxMessageSize);
                connection.Register(_socketListener);
                _connectionPool.Push(connection);
            }
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

            _receiveBufferManager.InitBuffer(config.MaxConnections, config.BufferSize);
            InitConnectionPool(config.MaxConnections, _packetContainer, config.MaxMessageSize);
            var sendConnection = Math.Max(1, config.MaxConnections / 10);
            _sendBufferManager.InitBuffer(sendConnection, config.BufferSize);
            _packetSender.Setup(_sendBufferManager, config.MaxConnections / 10, new XorCryptoProvider(Network.DefaultXorKey));
            _packetContainer.Setup(new XorCryptoProvider(Network.DefaultXorKey));
            
            _socketListener.Setup(config, _connectionPool);
        }

        public void Update()
        {
            if (IsWorking)
            {
                var packet = _packetContainer.NextPacket();

                if (packet != Packet.Null)
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

        public void Notify(IConnection connection, bool isConnected)
        {
            _socketListener.Notify(connection, isConnected);
        }

        #endregion
    }
}