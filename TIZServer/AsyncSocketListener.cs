﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TIZServer.Interface;
using TIZSoft;

namespace TIZServer
{
	public class AsyncSocketListener : IConnectionSubject
	{
		private Socket _listenSocket;
		private Semaphore _maxNumberAcceptedClients;
		private SocketAsyncEventArgs _acceptAsyncOp;
		private ServerConfig _config;
		private List<IConnectionObserver> _observers;

		// Begins an operation to accept a connection request from the client  
		// 
		// <param name="acceptEventArg">The context object to use when issuing 
		// the accept operation on the server's listening socket</param> 
		void StartAccept(SocketAsyncEventArgs acceptEventArg)
		{
			if (acceptEventArg == null)
			{
				acceptEventArg = new SocketAsyncEventArgs();
				acceptEventArg.Completed += OnAcceptComplete;
			}
			else
			{
				// socket must be cleared since the context object is being reused
				acceptEventArg.AcceptSocket = null;
			}

			_maxNumberAcceptedClients.WaitOne();

			if (!_listenSocket.AcceptAsync(acceptEventArg))
				AcceptResult(acceptEventArg);
		}

		// This method is called whenever a receive or send operation is completed on a socket  
		// <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
		void OnAcceptComplete(object sender, SocketAsyncEventArgs args)
		{
			if (args.LastOperation != SocketAsyncOperation.Accept)
				return;

			Logger.Log(string.Format("process async <color=cyan>{0}</color> get result <color=cyan>{1}</color>", args.LastOperation, args.SocketError));
			AcceptResult(args);
		}

		void AcceptResult(SocketAsyncEventArgs args)
		{
			if (args.SocketError == SocketError.Success)
			{
				Notify(args.AcceptSocket, true);
			}
			else
			{
				Notify(args.AcceptSocket, false);

				//server close on purpose
				if (args.SocketError == SocketError.OperationAborted)
					return;
			}

			// Accept the next connection request
			StartAccept(args);
		}

		void Free()
		{
			if (_listenSocket != null)
				_listenSocket.Dispose();

			if (_acceptAsyncOp != null)
				_acceptAsyncOp.Dispose();

			if (_maxNumberAcceptedClients != null)
				_maxNumberAcceptedClients.Dispose();
		}

		public AsyncSocketListener()
		{
			_observers = new List<IConnectionObserver>();
		}

		public void Setup(ServerConfig config)
		{
			_config = config;
			_maxNumberAcceptedClients = new Semaphore(config.MaxConnections, config.MaxConnections);
			IPEndPoint endPoint = TizNetwork.GetIpEndPoint(config.Address, config.Port);

			if (_listenSocket != null)
				Free();

			_listenSocket = new Socket(endPoint.AddressFamily, config.TransferType, config.UseProtocol);
			_listenSocket.Bind(endPoint);
		}

		public void Start()
		{
			_listenSocket.Listen(_config.MaxConnections);
			//_listenSocket.IOControl(IOControlCode.KeepAliveValues, TIZNetwork.GetKeepAliveSetting(1, 5000, 5000), null);
			StartAccept(_acceptAsyncOp);
		}

		public void Stop()
		{
			Free();
		}

		#region IConnectionSubject Members

		public void Register(IConnectionObserver observer)
		{
			if (observer == null)
				return;
			
			if (!_observers.Contains(observer))
				_observers.Add(observer);
		}

		public void Unregister(IConnectionObserver observer)
		{
			_observers.Remove(observer);
		}

		void RemoveNullObservers()
		{
			foreach (IConnectionObserver observer in _observers.ToArray())
			{
				if (observer == null)
					_observers.Remove(observer);
			}
		}

		public void Notify(Socket socket, bool isConnect)
		{
			if (socket == null)
				return;

			RemoveNullObservers();

			foreach (IConnectionObserver observer in _observers)
				observer.GetConnection(socket, isConnect);
		}

		#endregion
	}
}