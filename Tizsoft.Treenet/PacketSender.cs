using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketSender
    {
        FixedSizeObjPool<SocketAsyncEventArgs> _asyncSendOpPool;
        List<SocketAsyncEventArgs> _workingAsyncSendOps;
        IPacketContainer _sendPacketContainer = new PacketContainer();
        byte[] _sendBuffer;
        int _bufferSize;

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
                Logger.Log(string.Format("already send <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color>", args.BytesTransferred, connection.DestAddress));
            }
            else
            {
                Logger.LogError(String.Format("send msg to <color=cyan>{0}</color> faild due to <color=cyan>{1}</color>", connection.DestAddress, args.SocketError));
                connection.Dispose();
            }

            args.UserToken = null;
            args.AcceptSocket = null;
            _asyncSendOpPool.Push(args);
            _workingAsyncSendOps.Remove(args);
            StartSend();
        }

        void StartSend()
        {
            if (_asyncSendOpPool.Count == 0)
                return;

            var packet = _sendPacketContainer.NextPacket();

            if (packet.IsNull || packet.Connection.IsNull)
                return;

            int msgSize;
            using (var stream = new MemoryStream(_sendBuffer))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Network.CheckFlags);
                    writer.Write(false);  //compression now set to false
                    writer.Write((byte)packet.PacketType);
                    var content = packet.Content;
                    writer.Write(content.Length);
                    writer.Write(content);
                    msgSize = (int)stream.Position;
                }
            }

            var asyncSendOp = _asyncSendOpPool.Pop();
            asyncSendOp.SetBuffer(asyncSendOp.Offset, msgSize);
            Buffer.BlockCopy(_sendBuffer, 0, asyncSendOp.Buffer, asyncSendOp.Offset, msgSize);
            asyncSendOp.UserToken = packet.Connection;
            _workingAsyncSendOps.Add(asyncSendOp);

            var connector = packet.Connection.Connector;

            if (!connector.SendAsync(asyncSendOp))
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
                    Logger.LogException(e);
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
            _bufferSize = bufferManager.BufferSize;
            _sendBuffer = new byte[_bufferSize];
            _sendPacketContainer.Clear();
            FreeWorkingAsyncSendOps();

            asyncCount = Math.Max(1, asyncCount);
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

        public void SendMsg(Connection connection, byte[] msg, PacketType packetType)
        {
            _sendPacketContainer.AddPacket(connection, msg, packetType);
            StartSend();
        }
    }
}