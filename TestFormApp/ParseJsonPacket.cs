using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace TestFormApp
{
    public class ParseJsonPacket : IPacketProcessor
    {
        readonly Action<JObject, IConnection> _action;

        public ParseJsonPacket(Action<JObject, IConnection> action)
        {
            _action = action;
        }

        #region IPacketProcessor Members

        public void Process(IPacket packet)
        {
            var jsonStr = Encoding.UTF8.GetString(packet.Content);
            JObject jsonObject = null;

            try
            {
                jsonObject = JObject.Parse(jsonStr);
                var jtoken = (string) jsonObject.SelectToken("function");

                if (string.IsNullOrEmpty(jtoken))
                    jsonObject = null;
            }
            catch (Exception e)
            {
                GLogger.Debug(string.Format("invalidate json string <color=cyan>{0}</color> caused exception <color=cyan>{1}</color>", jsonStr, e.Message));
            }
            finally
            {
                _action(jsonObject, packet.Connection);
            }
        }

        #endregion
    }
}