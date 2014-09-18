using System.Collections.Generic;
using System.Net.Sockets;
using TIZServer.Interface;

namespace TIZServer
{
	public class AsyncSocketConnector : IConnectionSubject
	{
		private List<IConnectionObserver> _connectionObservers;
		private SocketAsyncEventArgs _connectArgs;
		private readonly BufferManager _bufferManager;

		void OnConnectComplete(object sender, SocketAsyncEventArgs args)
		{
			
		}

		void InitConnectArgs(string address, int port)
		{

		}

		public AsyncSocketConnector(BufferManager bufferManager, string address, int port)
		{
			_connectionObservers = new List<IConnectionObserver>();
			_bufferManager = bufferManager;
			InitConnectArgs(address, port);
		}

		#region IConnectionSubject Members

		public void Register(IConnectionObserver observer)
		{
			if (observer != null && !_connectionObservers.Contains(observer))
				_connectionObservers.Add(observer);
		}

		public void Unregister(IConnectionObserver observer)
		{
			_connectionObservers.Remove(observer);
		}

		public void Notify(Socket connection, bool isConnect)
		{
		}

		#endregion
	}
}