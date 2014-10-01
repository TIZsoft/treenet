using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Interface;

namespace TestFormApp
{
    public class ParseJsonPacket : IPacketProcessor
    {
        Action<JObject, Connection> _action;

        public ParseJsonPacket(Action<JObject, Connection> action)
        {
            _action = action;
        }

        #region IPacketProcessor Members

        public void Process(Packet packet)
        {
            var jsonStr = Encoding.UTF8.GetString(packet.Content);
            JObject jsonObject = null;

            try
            {
                jsonObject = JObject.Parse(jsonStr);
                var jtoken = (string)jsonObject.SelectToken("function");

                if (string.IsNullOrEmpty(jtoken))
                    jsonObject = null;
            }
            finally
            {
                _action(jsonObject, packet.Connection);
            }
        }

        #endregion
    }
}