using System.Net.Sockets;

namespace TIZServer.Interface
{
	public interface IConnectionObserver
	{
		bool GetConnection(Socket socket, bool isConnect);
	}
}