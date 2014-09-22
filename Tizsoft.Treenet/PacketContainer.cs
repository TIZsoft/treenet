using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketContainer : IPacketContainer
    {
        public PacketContainer()
        {
            _waitToParsePackets = new Queue<Packet>();
            _unusedPackets = new Queue<Packet>();
        }

        readonly Queue<Packet> _waitToParsePackets;
        readonly Queue<Packet> _unusedPackets;

        Packet GetUnusedPacket()
        {
            return _unusedPackets.Count != 0 ? _unusedPackets.Dequeue() : new Packet();
        }

        #region IPacketContainer Members

        public void AddPacket(Connection connection, SocketAsyncEventArgs asyncArgs)
        {
            var packet = GetUnusedPacket();
            packet.SetContent(connection, asyncArgs);
            _waitToParsePackets.Enqueue(packet);
        }

        public void RecyclePacket(Packet packet)
        {
            if (packet == null)
                return;

            packet.Clear();
            _unusedPackets.Enqueue(packet);
        }

        public Packet NextPacket()
        {
            return _waitToParsePackets.Count > 0 ? _waitToParsePackets.Dequeue() : NullPacket;
        }

        #endregion

        public static Packet NullPacket { get { return Treenet.NullPacket.Instance; } }
    }
}