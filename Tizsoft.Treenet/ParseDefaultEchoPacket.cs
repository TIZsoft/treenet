using System.IO;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ParseDefaultEchoPacket : IPacketParser
    {
        #region IPacketParser Members

        public void Parse(Packet packet)
        {
            Logger.Log("parse by default");
            packet.Connection.Send(packet.Content);
        }

        #endregion
    }
}