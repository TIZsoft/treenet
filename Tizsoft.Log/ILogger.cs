namespace Tizsoft.Log
{
    public interface ILogger
    {
        void Debug(object message);
        void Debug(string format, params object[] args);
        void Error(object message);
        void Error(string format, params object[] args);
        void Fatal(object message);
        void Fatal(string format, params object[] args);
        void Info(object message);
        void Info(string format, params object[] args);
        void Warn(object message);
        void Warn(string format, params object[] args);
    }
}
