using System;
using TIZServer.Interface;
using TIZServer.TIZServer;
using TIZSoft;

namespace TIZServer
{
	public class ServerMain
	{
		private SimpleObjPool<TizConnection> _asyncOpPool;
		private BufferManager _bufferManager;
		private AsyncSocketListener _socketListener;
		private ConnectionMonitor _connectionMonitor;
		private IPacketContainer _packetContainer;
		private PacketHandler _packetHandler;

		IPacketParser createPacketParser(PacketType type)
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
			_asyncOpPool = new SimpleObjPool<TizConnection>(maxConnections);

			for (int i = 0; i < maxConnections; ++i)
			{
				TizConnection connection = new TizConnection(_bufferManager, packetContainer);
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
			TizPacket packet = _packetContainer.NextPacket();

			if (packet.IsNull || packet.Connection.IsNull)
				_packetContainer.RecyclePacket(packet);
			else
			{
				_packetHandler.Parse(packet);
			}
		}
	}
}