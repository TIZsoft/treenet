namespace Tizsoft.Treenet.Interface
{
    public interface IConnectionSubject
    {
        void Register(IConnectionObserver observer);

        void Unregister(IConnectionObserver observer);

        void Notify(Connection connection, bool isConnect);
    }
}