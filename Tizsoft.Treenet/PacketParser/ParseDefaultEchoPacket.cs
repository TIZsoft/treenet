using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.PacketParser
{
    public class ParseDefaultEchoPacket : IPacketParser
    {
        #region IPacketParser Members

        public void Parse(Packet packet)
        {
            Logger.Log("parse by default: echo back!");
            packet.Connection.Send(packet.Content);
        }

        #endregion
    }
}