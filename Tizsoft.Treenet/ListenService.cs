using System;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ListenService : IService
    {
        SimpleObjPool<Connection> _asyncOpPool;
        readonly BufferManager _bufferManager;
        readonly AsyncSocketListener _socketListener;
        readonly ConnectionObserver _connectionObserver;
        readonly IPacketContainer _packetContainer;
        readonly PacketHandler _packetHandler;
        bool _isInit = false;

        void InitConnectionPool(int maxConnections, IPacketContainer packetContainer, IConnectionObserver connectionObserver)
        {
            _asyncOpPool = new SimpleObjPool<Connection>(maxConnections);

            for (var i = 0; i < maxConnections; ++i)
            {
                var connection = new Connection(_bufferManager, packetContainer);
                connection.Register(connectionObserver);
                _asyncOpPool.Push(connection);
            }
        }

        public ListenService()
        {
            _bufferManager = new BufferManager();
            _socketListener = new AsyncSocketListener();
            _connectionObserver = new ConnectionObserver();
            _socketListener.Register(_connectionObserver);
            _packetContainer = new PacketContainer();
            _packetHandler = new PacketHandler();
        }

        public void Send(Connection connection, byte[] contents)
        {
            connection.Send(contents);
        }

        public Packet Receive()
        {
            return IsWorking ? _packetContainer.NextPacket() : Packet.NullPacket;
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

            if (!_isInit)
            {
                _bufferManager.InitBuffer(config.MaxConnections * config.BufferSize * 2, config.BufferSize);
                InitConnectionPool(config.MaxConnections, _packetContainer, _connectionObserver);
                _connectionObserver.Setup(_asyncOpPool);
                _isInit = true;
            }
            
            _socketListener.Setup(config);
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
            //    _packetHandler.Parse(packet);
            //}
        }

        public void Stop()
        {
            _socketListener.Stop();
            _connectionObserver.Reset();
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion
    }
}