using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Interface;

namespace TestFormApp
{
    public class ParseJsonPacket : IPacketParser
    {
//        const string SchemaJson = @"{
//            'description': 'client send format',
//            'type': 'object',
//            'properties':
//            {
//                'function': {'type':'string'},
//                'param': 
//                {
//                    'type': 'array', 
//                    'items': {'type':'string'}
//                }
//            }}";

        Action<JObject> _action;

        public ParseJsonPacket(Action<JObject> action)
        {
            _action = action;
        }

        #region IPacketParser Members

        public void Parse(Packet packet)
        {
            string jsonStr = BitConverter.ToString(packet.Content);
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
                _action(jsonObject);
            }
        }

        #endregion
    }
}