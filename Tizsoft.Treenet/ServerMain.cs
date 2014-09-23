using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ServerMain : IService
    {
        SimpleObjPool<Connection> _asyncOpPool;
        readonly BufferManager _bufferManager;
        readonly AsyncSocketListener _socketListener;
        readonly ConnectionObserver _connectionObserver;
        readonly IPacketContainer _packetContainer;
        readonly PacketHandler _packetHandler;

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
            _connectionObserver = new ConnectionObserver();
            _socketListener.Register(_connectionObserver);
            _packetContainer = new PacketContainer();
            _packetHandler = new PacketHandler();
        }

        public void Setup(ServerConfig config)
        {
            _bufferManager.InitBuffer(config.MaxConnections * config.BufferSize * 2, config.BufferSize);
            InitConnectionPool(config.MaxConnections, _packetContainer, _connectionObserver);
            _connectionObserver.Setup(_asyncOpPool);
            _socketListener.Setup(config);
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

        #region IService Members

        public void Start()
        {
            _socketListener.Start();
            IsWorking = true;
        }

        public void Stop()
        {
            _socketListener.Stop();
            _connectionObserver.Reset();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion
    }
}