using System.Collections.Generic;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketHandler
    {
        readonly Dictionary<int, List<IPacketParser>> _parsers;

        public PacketHandler()
        {
            _parsers = new Dictionary<int, List<IPacketParser>>();
        }

        public void Parse(Packet packet)
        {
            List<IPacketParser> parsers;

            if (_parsers.TryGetValue((int)packet.PacketType, out parsers))
            {
                foreach (var parser in parsers)
                {
                    if (parser != null)
                        parser.Parse(packet);
                }
            }
        }

        public void AddParser(int packetType, IPacketParser parser)
        {
            if (parser == null)
                return;

            List<IPacketParser> parsers;

            if (!_parsers.TryGetValue(packetType, out parsers))
            {
                parsers = new List<IPacketParser>();
                _parsers.Add(packetType, parsers);
            }

            if (!parsers.Contains(parser))
                parsers.Add(parser);
        }

        public void RemoveParser(int packetType, IPacketParser parser)
        {
            List<IPacketParser> parsers;

            if (_parsers.TryGetValue(packetType, out parsers))
                parsers.Remove(parser);
        }
    }
}