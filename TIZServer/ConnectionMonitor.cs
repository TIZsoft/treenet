using System.Collections.Generic;
using System.Net.Sockets;
using TIZServer.Interface;
using TIZSoft;

namespace TIZServer.TIZServer
{
	public class ConnectionMonitor : IConnectionObserver
	{
		private Dictionary<Socket, TizConnection> _workingConnections;
		private SimpleObjPool<TizConnection> _connectionPool;

		public ConnectionMonitor()
		{
			_workingConnections = new Dictionary<Socket, TizConnection>();
		}

		public void Setup(int maxConnection, SimpleObjPool<TizConnection> connectionPool)
		{
			_connectionPool = connectionPool;
		}

		#region IConnectionObserver Members

		public bool GetConnectionEvent(Socket acceptSocket, bool isConnect)
		{
			TizConnection connection;

			if (isConnect)
			{
				if (_connectionPool.Count <= 0)
				{
					Logger.LogWarning("連線數已達上限!");
					return false;
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
					connection.Dispose();
					Logger.Log(string.Format("IP: <color=cyan>{0}</color> 已斷線", connection.DestAddress));
				}

				_workingConnections.Remove(acceptSocket);
				Logger.Log(string.Format("目前連線數: {0}", _workingConnections.Count));
			}

			return true;
		}

		#endregion
	}
}