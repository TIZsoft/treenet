using System;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.Factory
{
    public class ConnectionFactory
    {
        readonly BufferManager _bufferManager;
        readonly IPacketContainer _packetContainer;
        readonly PacketSender _packetSender;
        readonly PacketProtocol _packetProtocol;
        readonly int _maxMessageSize;
        readonly bool _disconnectAfterSend;

        public ConnectionFactory(BufferManager bufferManager, IPacketContainer packetContainer,
                                 PacketSender packetSender, PacketProtocol packetProtocol, int maxMessageSize, bool disconnectAfterSend = false)
        {
            _bufferManager = bufferManager;
            _packetContainer = packetContainer;
            _packetSender = packetSender;
            _packetProtocol = packetProtocol;
            _maxMessageSize = maxMessageSize;
            _disconnectAfterSend = disconnectAfterSend;
        }

        public IConnection NewConnection()
        {
            try
            {
                var connection = new Connection(_bufferManager, _packetContainer, _packetSender, _maxMessageSize)
                {
                    PacketProtocol = _packetProtocol,
                    DisconnectAfterSend = _disconnectAfterSend
                };
                return connection;
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
                return Connection.Null;
            }
        }
    }
}