using System;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ServerMain
    {
        SimpleObjPool<Connection> _asyncOpPool;
        readonly BufferManager _bufferManager;
        readonly AsyncSocketListener _socketListener;
        readonly ConnectionMonitor _connectionMonitor;
        readonly IPacketContainer _packetContainer;
        readonly PacketHandler _packetHandler;

        IPacketParser CreatePacketParser(PacketType type)
        {
            switch (type)
            {
                default:
                    return new ParseDefaultEchoPacket();
            }
        }

        void InitPacketHandler()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                _packetHandler.AddParser((int)type, null);
            }
        }

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

        public ServerMain()
        {
            _bufferManager = new BufferManager();
            _socketListener = new AsyncSocketListener();
            _connectionMonitor = new ConnectionMonitor();
            _socketListener.Register(_connectionMonitor);
            _packetContainer = new PacketContainer();
            _packetHandler = new PacketHandler();
        }

        public void Setup(ServerConfig config)
        {
            _bufferManager.InitBuffer(config.MaxConnections * config.BufferSize * 2, config.BufferSize);
            InitConnectionPool(config.MaxConnections, _packetContainer, _connectionMonitor);
            _connectionMonitor.Setup(config.MaxConnections, _asyncOpPool);
            _socketListener.Setup(config);
        }

        public void Start()
        {
            _socketListener.Start();
        }

        public void Stop()
        {
            _socketListener.Stop();
        }

        public void Update()
        {
            var packet = _packetContainer.NextPacket();

            if (packet.IsNull || packet.Connection.IsNull)
                _packetContainer.RecyclePacket(packet);
            else
            {
                _packetHandler.Parse(packet);
            }
        }
    }
}