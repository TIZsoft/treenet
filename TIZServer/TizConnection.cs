﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using TIZServer.Interface;
using TIZSoft;

namespace TIZServer
{
	public class TizConnection : IDisposable, INullObj, IConnectionSubject
	{
		private SocketAsyncEventArgs _receiveAsyncArgs;
		private SocketAsyncEventArgs _sendAsyncArgs;
		private readonly BufferManager _bufferManager;
		private readonly IPacketContainer _packetContainer;
		private List<IConnectionObserver> _observers;

		void OnAsyncComplete(object sender, SocketAsyncEventArgs args)
		{
			Logger.Log(string.Format("async {0} complete with result {1}", args.LastOperation, args.SocketError));

			switch (args.LastOperation)
			{
				case SocketAsyncOperation.Receive:
					ReceiveResult(args);
					break;

				case SocketAsyncOperation.Send:
					SendResult(args);
					break;

				default:
					break;
			}
		}

		// This method is invoked when an asynchronous receive operation completes.  
		// If the remote host closed the connection, then the socket is closed.   
		void ReceiveResult(SocketAsyncEventArgs args)
		{
			// check if the remote host closed the connection
			if (args.SocketError == SocketError.Success)
			{
				//means client has disconnect
				if (args.BytesTransferred == 0)
				{
					CloseAsyncSocket(args);
					return;
				}
				else if (args.BytesTransferred > TizNetwork.PacketMinSize)
				{
					_packetContainer.AddPacket(this, args);
				}
			}
			else
			{
				CloseAsyncSocket(args);
			}
		}

		// This method is invoked when an asynchronous send operation completes.   
		// The method issues another receive on the socket to read any additional  
		// data sent from the client 
		// 
		// <param name="e"></param>
		void SendResult(SocketAsyncEventArgs args)
		{
			if (args.SocketError == SocketError.Success)
			{
				Logger.Log(string.Format("already send <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color>", args.BytesTransferred, args.AcceptSocket.RemoteEndPoint));
				//StartReceive(args);
			}
			else
			{
				CloseAsyncSocket(args);
			}
		}

		void StartReceive()
		{
			if (!_receiveAsyncArgs.AcceptSocket.ReceiveAsync(_receiveAsyncArgs))
				ReceiveResult(_receiveAsyncArgs);
		}

		void StartSend()
		{
			if (!_sendAsyncArgs.AcceptSocket.SendAsync(_sendAsyncArgs))
				SendResult(_sendAsyncArgs);
		}

		void CloseAsyncSocket(SocketAsyncEventArgs args)
		{
			Notify(args.AcceptSocket, false);
		}

		void InitSocketAsyncEventArgs(ref SocketAsyncEventArgs asyncArgs, BufferManager bufferManager, Socket socket)
		{
			asyncArgs = new SocketAsyncEventArgs();
			asyncArgs.AcceptSocket = socket;
			asyncArgs.Completed += OnAsyncComplete;

			if (bufferManager != null)
				bufferManager.SetBuffer(asyncArgs);
		}

		void ClearSocketAsyncEventArgs(ref SocketAsyncEventArgs asyncArgs)
		{
			if (_bufferManager != null)
			{
				_bufferManager.FreeBuffer(asyncArgs);
				asyncArgs.Dispose();
			}
		}

		public TizConnection(BufferManager bufferManager, IPacketContainer packetContainer) : this()
		{
			_bufferManager = bufferManager;
			_packetContainer = packetContainer;
		}

		public TizConnection()
		{
			DestAddress = string.Empty;
			_observers = new List<IConnectionObserver>();
		}

		public void SetConnection(Socket socket)
		{
			DestAddress = socket.RemoteEndPoint.ToString();
			InitSocketAsyncEventArgs(ref _receiveAsyncArgs, _bufferManager, socket);
			InitSocketAsyncEventArgs(ref _sendAsyncArgs, _bufferManager, socket);
			StartReceive();
		}

		public void Send(byte[] content)
		{
			_sendAsyncArgs.SetBuffer(_sendAsyncArgs.Offset, content.Length);

			if (!_sendAsyncArgs.AcceptSocket.SendAsync(_sendAsyncArgs))
				SendResult(_sendAsyncArgs);
		}

		public string DestAddress { get; private set; }

		public static TizConnection NullConnection {get { return NullTizConnection.Instance; }}

		#region IDisposable Members

		public void Dispose()
		{
			Notify(_receiveAsyncArgs.AcceptSocket, false);
			ClearSocketAsyncEventArgs(ref _receiveAsyncArgs);
			ClearSocketAsyncEventArgs(ref _sendAsyncArgs);
		}

		#endregion

		#region INullObj Members

		public bool IsNull
		{
			get { return false; }
		}

		#endregion

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

		public void Notify(Socket connection, bool isConnect)
		{
			if (connection == null)
				return;

			RemoveNullObservers();

			foreach (IConnectionObserver observer in _observers)
				observer.GetConnection(connection, isConnect);
		}

		#endregion
	}
}