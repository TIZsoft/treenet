using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Interface;

namespace TestFormApp.JsonCommand
{
    public class UpdatePlayerDataCmd : IJsonCommand
    {
        CacheUserData _cacheUserData;
        IConnection _connection;

        public UpdatePlayerDataCmd(CacheUserData cacheUserData, IConnection connection)
        {
            _cacheUserData = cacheUserData;
            _connection = connection;
        }

        public void Do(JObject jObject)
        {
            var response = new Dictionary<string, object>
            {
                {"result", "updateplayerdata"}
            };

            var type = (string) jObject.SelectToken("param.Types");
            var guid = (string) jObject.SelectToken("param.Guid");
            var userData = _cacheUserData.Get(guid);
            var success = true;

            if (userData != null)
            {
                try
                {
                    switch (type)
                    {
                        case "levelcompleteupdate":
                            userData.Score = (uint) jObject.SelectToken("param.score");
                            userData.Money = (uint) jObject.SelectToken("param.money");
                            userData.Exp = (uint) jObject.SelectToken("param.exp");
                            break;
                    }
                }
                catch (Exception)
                {
                    success = false;
                    throw;
                }

                response.Add("type", success ?
                    new Dictionary<string, object>
                    {
                        {"result", "true"}
                    }
                    :
                    new Dictionary<string, object>
                    {
                        {"error", string.Format("invalid value type for {0}", type)}
                    });

                var responseStr = JsonConvert.SerializeObject(response);
                _connection.Send(Encoding.UTF8.GetBytes(responseStr), PacketType.KeyValue);
            }
        }
    }
}