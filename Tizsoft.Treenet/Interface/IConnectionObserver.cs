using System.Net.Sockets;

namespace Tizsoft.Treenet.Interface
{
    public interface IConnectionObserver
    {
        void GetConnectionEvent(Socket socket, bool isConnect);
    }
}