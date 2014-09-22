using System.Net.Sockets;

namespace Tizsoft.Treenet.Interface
{
	public interface IConnectionObserver
	{
		bool GetConnectionEvent(Socket socket, bool isConnect);
	}
}