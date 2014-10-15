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
                    ProcessSend(args);
                    break;
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.
        /// The method issues another receive on the socket to read any additional data sent from the client.
        /// </summary>
        void ProcessSend(SocketAsyncEventArgs e)
        {
            var connection = (Connection)e.UserToken;

            if (e.SocketError == SocketError.Success)
            {
                GLogger.Debug(string.Format("send <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color> finish!", e.BytesTransferred, connection.DestAddress));
            }
            else
            {
                GLogger.Error(String.Format("send msg to <color=cyan>{0}</color> failed due to <color=cyan>{1}</color>", connection.DestAddress, e.SocketError));
                connection.Dispose();
            }

            e.UserToken = null;
            e.AcceptSocket = null;
            _asyncSendOpPool.Push(e);
            _workingAsyncSendOps.Remove(e);
            StartSend();
        }

        // TODO: “TCP does not operate on packets of data. TCP operates on streams of data.”
        // TODO: Message framing.
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

            // BUG: Possible expected an InvalidOperationException when send operation called too many times at same time.
            var asyncSendOp = _asyncSendOpPool.Pop();
            asyncSendOp.SetBuffer(asyncSendOp.Offset, msgSize);
            Buffer.BlockCopy(_sendBuffer, 0, asyncSendOp.Buffer, asyncSendOp.Offset, msgSize);
            asyncSendOp.UserToken = packet.Connection;
            _workingAsyncSendOps.Add(asyncSendOp);
            
            var connector = packet.Connection.ConnectSocket;

            // BUG: Possible connection is not connected or has been closed/disposed or is a null reference.
            if (!connector.SendAsync(asyncSendOp))
                ProcessSend(asyncSendOp);
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