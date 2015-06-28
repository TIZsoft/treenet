using System;
using System.Collections.Concurrent;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    // TODO: Reuse packets.
    public class PacketContainer : IPacketContainer
    {
        readonly ConcurrentQueue<IPacket> _queueingPackets = new ConcurrentQueue<IPacket>();
        readonly ConcurrentQueue<IPacket> _unusedPackets = new ConcurrentQueue<IPacket>();
        
        IPacket GetUnusedPacket()
        {
            IPacket packet;
            return _unusedPackets.TryDequeue(out packet) ? packet : new Packet();
        }

        #region IPacketContainer Members

        public void AddPacket(IConnection connection, byte[] content, PacketType packetType)
        {
            var packet = GetUnusedPacket();
            packet.Connection = connection;
            packet.Content = content;
            packet.PacketType = packetType;
            _queueingPackets.Enqueue(packet);
        }

        public void AddPacket(IPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            _queueingPackets.Enqueue(packet);
        }

        public void RecyclePacket(IPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            packet.Clear();

            if (packet != Packet.Null)
            {
                _unusedPackets.Enqueue(packet);
            }
        }

        public void Clear()
        {
            while (_queueingPackets.Count > 0)
            {
                IPacket packet;
                if (_queueingPackets.TryDequeue(out packet))
                {
                    RecyclePacket(packet);
                }
            }
        }

        public void ClearDisconnectedPacket(IConnection connection)
        {
            while (_queueingPackets.Count > 0)
            {
                {
                    IPacket packet;
                    if (_queueingPackets.TryDequeue(out packet))
                    {
                        if (packet.Connection == connection)
                            RecyclePacket(packet);
                        else
                            _queueingPackets.Enqueue(packet);
                    }
                }
            }
        }

        public IPacket NextPacket()
        {
            IPacket packet;
            return _queueingPackets.TryDequeue(out packet) ? packet : Packet.Null;
        }

        #endregion
    }
}