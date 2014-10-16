namespace Tizsoft.Treenet.Interface
{
    public interface IConnectionObserver
    {
        void GetConnectionEvent(IConnection connection, bool isConnect);
    }
}