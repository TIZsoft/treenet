using System.Net.Sockets;

namespace Tizsoft.Treenet.Interface
{
    public interface IConnectionSubject
    {
        void Register(IConnectionObserver observer);

        void Unregister(IConnectionObserver observer);

        void Notify(Socket connection, bool isConnect);
    }
}