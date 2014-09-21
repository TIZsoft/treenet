using System.Net.Sockets;

namespace TIZServer.Interface
{
	public interface IConnectionObserver
	{
		bool GetConnectionEvent(Socket socket, bool isConnect);
	}
}