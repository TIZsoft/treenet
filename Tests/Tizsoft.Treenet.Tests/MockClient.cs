using System;
using System.Net.Sockets;

namespace Tizsoft.Treenet.Tests
{
    class MockClient : IDisposable
    {
        ClientConfig _config;
        Socket _socket;
        SocketAsyncEventArgs _socketOperation = new SocketAsyncEventArgs();
        byte[] _buffer;
        bool _isDisposed;

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public MockClient(ClientConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _config = config;
            _buffer = new byte[config.BufferSize];
            _socketOperation.SetBuffer(_buffer, 0, _buffer.Length);
            _socketOperation.Completed += IO_Completed;

            var endPoint = Network.GetIpEndPoint(config.Address, config.Port);
            _socket = new Socket(endPoint.AddressFamily, config.TransferType, config.UseProtocol);
        }

        ~MockClient()
        {
            Dispose(false);
        }

        public void StartConnect()
        {
            ThrowExceptionIfDisposed();

            var willRaiseEvent = _socket.ConnectAsync(_socketOperation);

            if (willRaiseEvent)
            {
                return;
            }

            ProcessConnect(_socketOperation);
        }

        public void StartDisconnect()
        {
            ThrowExceptionIfDisposed();

            var willRaiseEvent = _socket.DisconnectAsync(_socketOperation);

            if (willRaiseEvent)
            {
                return;
            }

            ProcessDisconnect(_socketOperation);
        }

        void IO_Completed(object sender, SocketAsyncEventArgs socketOperation)
        {
            switch (socketOperation.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(socketOperation);
                    break;

                case SocketAsyncOperation.Disconnect:
                    ProcessDisconnect(socketOperation);
                    break;
            }
        }

        void ProcessConnect(SocketAsyncEventArgs connectOperation)
        {
            OnConnected();
        }

        void ProcessDisconnect(SocketAsyncEventArgs disconnectOperation)
        {
            OnDisconnected();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (_socketOperation != null)
            {
                _socketOperation.Dispose();
                _socketOperation = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        void ThrowExceptionIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("MockClient");
            }
        }

        void OnConnected()
        {
            if (Connected != null)
                Connected(this, EventArgs.Empty);
        }

        void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }
    }
}