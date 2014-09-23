using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ConnectionObserver : IConnectionObserver
    {
        readonly Dictionary<Socket, Connection> _workingConnections;
        SimpleObjPool<Connection> _connectionPool;

        public ConnectionObserver()
        {
            _workingConnections = new Dictionary<Socket, Connection>();
        }

        public void Setup(SimpleObjPool<Connection> connectionPool)
        {
            _connectionPool = connectionPool;
        }

        public void Reset()
        {
            foreach (var connection in _workingConnections.Values.ToArray())
            {
                connection.Dispose();
            }
        }

        #region IConnectionObserver Members

        public void GetConnectionEvent(Socket acceptSocket, bool isConnect)
        {
            Connection connection;

            if (isConnect)
            {
                if (_connectionPool.Count <= 0)
                {
                    Logger.LogWarning("連線數已達上限!");
                    return;
                }

                if (!_workingConnections.TryGetValue(acceptSocket, out connection))
                {
                    connection = _connectionPool.Pop();
                    _workingConnections.Add(acceptSocket, connection);
                }

                connection.SetConnection(acceptSocket);
                Logger.Log(string.Format("IP: <color=cyan>{0}</color> 已連線", connection.DestAddress));
                Logger.Log(string.Format("目前連線數: {0}", _workingConnections.Count));
            }
            else
            {
                if (_workingConnections.TryGetValue(acceptSocket, out connection))
                {
                    Logger.Log(string.Format("IP: <color=cyan>{0}</color> 已斷線", connection.DestAddress));
                    _connectionPool.Push(connection);
                }

                _workingConnections.Remove(acceptSocket);
                Logger.Log(string.Format("目前連線數: {0}", _workingConnections.Count));
            }
        }

        #endregion
    }
}