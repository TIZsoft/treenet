﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms.VisualStyles;
using TIZServer.Interface;
using TIZServer.TestClient;
using TIZSoft;

namespace TIZServer
{
	public class AsyncSocketConnector : IConnectionSubject
	{
		private List<IConnectionObserver> _connectionObservers;
		private SocketAsyncEventArgs _connectArgs;

		void OnConnectComplete(object sender, SocketAsyncEventArgs args)
		{
			switch (args.LastOperation)
			{
				case SocketAsyncOperation.Connect:
					ConnectResult(args);
					break;

				default:
					break;
			}
		}

		void ConnectResult(SocketAsyncEventArgs args)
		{
			switch (args.SocketError)
			{
				case SocketError.Success:
					Notify(args.AcceptSocket, true);
					break;

				default:
					Logger.Log(string.Format("因為 {0} ，所以無法連線", args.SocketError));
					break;
			}
		}

		void InitConnectArgs(ClientConfig config)
		{
			if (_connectArgs != null)
				_connectArgs.Dispose();

			_connectArgs = new SocketAsyncEventArgs();
			_connectArgs.AcceptSocket = new Socket(AddressFamily.InterNetwork, config.TransferType, config.UseProtocol);
			IPAddress ipAddress = IPAddress.Parse(config.Address);
			_connectArgs.RemoteEndPoint = new IPEndPoint(ipAddress, config.Port);
			_connectArgs.Completed += OnConnectComplete;
		}

		public AsyncSocketConnector()
		{
			_connectionObservers = new List<IConnectionObserver>();
		}

		public void Connect(ClientConfig config)
		{
			InitConnectArgs(config);

			if (!_connectArgs.AcceptSocket.ConnectAsync(_connectArgs))
				ConnectResult(_connectArgs);
		}

		public void Stop()
		{
			if (_connectArgs != null)
				_connectArgs.Dispose();
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