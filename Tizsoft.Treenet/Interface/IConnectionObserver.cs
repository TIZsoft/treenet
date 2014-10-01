namespace Tizsoft.Treenet.Interface
{
    public interface IConnectionObserver
    {
        void GetConnectionEvent(Connection connection, bool isConnect);
    }
}