using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Log;

namespace Tizsoft.Treenet
{
    public class PacketSender
    {
        FixedSizeObjPool<SocketAsyncEventArgs> _asyncSendOpPool;
        readonly Queue<KeyValuePair<Connection, byte[]>> _sendQueue = new Queue<KeyValuePair<Connection, byte[]>>();
        List<SocketAsyncEventArgs> _workingAsyncSendOps;

        void OnAsyncComplete(object sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    SendResult(args);
                    break;
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.
        /// The method issues another receive on the socket to read any additional data sent from the client.
        /// </summary>
        void SendResult(SocketAsyncEventArgs args)
        {
            var connection = (Connection)args.UserToken;

            if (args.SocketError == SocketError.Success)
            {
                GLogger.Debug(string.Format("already send <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color>", args.BytesTransferred, connection.DestAddress));
            }
            else
            {
                GLogger.Error(String.Format("send msg to <color=cyan>{0}</color> faild due to <color=cyan>{1}</color>", connection.DestAddress, args.SocketError));
                connection.Dispose();
            }

            if (!TrySendNextMsg(args))
            {
                args.UserToken = null;
                args.AcceptSocket = null;
                _asyncSendOpPool.Push(args);
                _workingAsyncSendOps.Remove(args);
            }
        }

        bool TrySendNextMsg(SocketAsyncEventArgs args)
        {
            if (_sendQueue.Count > 0)
            {
                var nextMsg = _sendQueue.Dequeue();
                StartSend(nextMsg.Key, nextMsg.Value);
                return true;
            }

            return false;
        }

        void StartSend(Connection connection, byte[] msg)
        {
            var asyncSendOp = _asyncSendOpPool.Pop();
            asyncSendOp.SetBuffer(asyncSendOp.Offset, msg.Length);
            Buffer.BlockCopy(msg, 0, asyncSendOp.Buffer, asyncSendOp.Offset, msg.Length);
            asyncSendOp.UserToken = connection;
            _workingAsyncSendOps.Add(asyncSendOp);
            if (!connection.Connector.SendAsync(asyncSendOp))
                SendResult(asyncSendOp);
        }

        void FreeWorkingAsyncSendOps()
        {
            if (_workingAsyncSendOps == null)
                return;

            foreach (var socketAsyncEventArgse in _workingAsyncSendOps)
            {
                if (socketAsyncEventArgse == null)
                    continue;

                try
                {
                    if (socketAsyncEventArgse.AcceptSocket != null)
                    {
                        socketAsyncEventArgse.AcceptSocket.Shutdown(SocketShutdown.Both);
                        socketAsyncEventArgse.AcceptSocket.Dispose();
                    }
                }
                catch (Exception e)
                {
                    GLogger.Fatal(e);
                    throw;
                }
                finally
                {
                    socketAsyncEventArgse.AcceptSocket = null;
                }
            }

            _workingAsyncSendOps.Clear();
        }

        public void Setup(BufferManager bufferManager, int asyncCount)
        {
            _sendQueue.Clear();
            FreeWorkingAsyncSendOps();

            _asyncSendOpPool = new FixedSizeObjPool<SocketAsyncEventArgs>(asyncCount);

            for (var i = 0; i < asyncCount; ++i)
            {
                var asyncSend = new SocketAsyncEventArgs();
                bufferManager.SetBuffer(asyncSend);
                asyncSend.Completed += OnAsyncComplete;
                _asyncSendOpPool.Push(asyncSend);
            }

            _workingAsyncSendOps = new List<SocketAsyncEventArgs>(asyncCount);
        }

        public void SendMsg(Connection connection, byte[] msg)
        {
            if (_asyncSendOpPool.Count == 0)
            {
                _sendQueue.Enqueue(new KeyValuePair<Connection, byte[]>(connection, msg));
                return;
            }

            StartSend(connection, msg);
        }
    }
}