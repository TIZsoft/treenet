using System.Collections.Generic;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketHandler
    {
        readonly Dictionary<PacketType, IPacketProcessor> _parsers;

        public PacketHandler()
        {
            _parsers = new Dictionary<PacketType, IPacketProcessor>();
        }

        public void Parse(IPacket packet)
        {
            IPacketProcessor processor;

            if (_parsers.TryGetValue(packet.PacketType, out processor))
            {
                if (processor != null)
                    processor.Process(packet);
            }
            else
            {
                GLogger.Warn(string.Format("封包類型 {0} 沒有指定處理方式!", packet.PacketType));
            }
        }

        public void AddParser(PacketType packetType, IPacketProcessor processor)
        {
            if (processor == null)
                return;

            IPacketProcessor existProcessor;

            if (_parsers.TryGetValue(packetType, out existProcessor))
                _parsers.Remove(packetType);

            _parsers.Add(packetType, processor);
        }
    }
}