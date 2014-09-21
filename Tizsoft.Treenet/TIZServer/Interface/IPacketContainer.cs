using System.Net.Sockets;

namespace TIZServer.Interface
{
	public interface IPacketContainer
	{
		void AddPacket(TizConnection connection, SocketAsyncEventArgs aysncArgs);
		void RecyclePacket(TizPacket packet);
		//int PacketCount { get; }
		TizPacket NextPacket();
	}
}