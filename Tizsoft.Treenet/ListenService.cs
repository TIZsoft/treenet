using System;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ListenService : IService, IConnectionSubject
    {
        FixedSizeObjPool<Connection> _connectionPool;
        readonly BufferManager _bufferManager;
        readonly AsyncSocketListener _socketListener;
        readonly IPacketContainer _packetContainer;
        readonly PacketHandler _packetHandler;

        void InitConnectionPool(int maxConnections, IPacketContainer packetContainer)
        {
            _connectionPool = new FixedSizeObjPool<Connection>(maxConnections);

            for (var i = 0; i < maxConnections; ++i)
            {
                var connection = new Connection(_bufferManager, packetContainer);
                connection.Register(_socketListener);
                _connectionPool.Push(connection);
            }
        }

        public ListenService()
        {
            _bufferManager = new BufferManager();
            _socketListener = new AsyncSocketListener();
            _packetContainer = new PacketContainer();
            _packetHandler = new PacketHandler();
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

            _bufferManager.InitBuffer(config.MaxConnections * config.BufferSize * 2, config.BufferSize);
            InitConnectionPool(config.MaxConnections, _packetContainer);
            
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