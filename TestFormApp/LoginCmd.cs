using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestFormApp.User;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Log;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Interface;

namespace TestFormApp
{
    public class LoginCmd : IJsonCommand
    {
        CacheUserData _cacheUserData;
        DatabaseQuery _dbQuery;
        DownloadStringCompletedEventHandler _eventHandler;
        IConnection _connection;

        static string CreateFbTokenValidateUrl(string fbtoken)
        {
            return "https://graph.facebook.com/me/?access_token=" + fbtoken;
        }

        void FacebookValidateHandler(object sender, DownloadStringCompletedEventArgs args)
        {
            var validateArgs = (FacebookValidateArgs)args.UserState;
            var response = validateArgs.Response;

            if (args.Error != null)
            {
                var responseStream = ((WebException)args.Error).Response.GetResponseStream();
                using (var reader = new StreamReader(responseStream))
                {
                    response.Add("param", new Dictionary<string, object>
                    {
                        {"error", reader.ReadToEnd()}
                    });
                }

                var responseStr = JsonConvert.SerializeObject(response);
                validateArgs.Connection.Send(Encoding.UTF8.GetBytes(responseStr), PacketType.KeyValue);
                return;
            }

            var validateResultJobject = JObject.Parse(args.Result);
            var fbid = (string)validateResultJobject.SelectToken("id");

            UserData userData;

            if (!_dbQuery.HasUserData(fbid, AccountType.Facebook, out userData))
                userData = _dbQuery.CreateNewUser(fbid, AccountType.Facebook);

            response.Add("param", new Dictionary<string, object>
            {
                {"user", JsonConvert.SerializeObject(userData)}
            });
            validateArgs.Connection.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)), PacketType.KeyValue);
        }

        void ValidateFacebookTokenAsync(IConnection connection, string fbtoken, Dictionary<string, object> response)
        {
            using (var wc = new WebClient())
            {
                var userToken = new FacebookValidateArgs();
                userToken.Connection = connection;
                userToken.FbToken = fbtoken;
                userToken.Response = response;

                wc.Encoding = Encoding.UTF8;
                wc.DownloadStringCompleted += FacebookValidateHandler;
                wc.DownloadStringAsync(new Uri(CreateFbTokenValidateUrl(fbtoken)), userToken);
            }
        }

        public LoginCmd(CacheUserData cacheUserData, DatabaseQuery databaseQuery, IConnection connection)
        {
            _cacheUserData = cacheUserData;
            _dbQuery = databaseQuery;
            _connection = connection;
        }

        public void Do(JObject jObject)
        {
            var response = new Dictionary<string, object>()
            {
                {"result", "login"},
            };

            var guid = (string)jObject.SelectToken("param.guid");
            var fbtoken = (string)jObject.SelectToken("param.fbtoken");

            if (string.IsNullOrEmpty(guid))
            {
                if (string.IsNullOrEmpty(fbtoken))
                {
                    var userData = _cacheUserData.Get(guid);
                    if (null == userData)
                    {
                        userData = _dbQuery.CreateNewUser(GuidUtil.New());
                        _cacheUserData.Add(userData);
                    }
                    response.Add("param", new Dictionary<string, object>
                            {
                                {"user", JsonConvert.SerializeObject(userData)}
                            });
                }
                else
                {
                    ValidateFacebookTokenAsync(_connection, fbtoken, response);
                    return;
                }
            }
            else
            {
                UserData user;
                response.Add("param", _dbQuery.HasUserData(guid, AccountType.Guid, out user) ?
                    new Dictionary<string, object>
                            {
                                 {"user", JsonConvert.SerializeObject(user)}
                            }
                    :
                    new Dictionary<string, object>
                            {
                                {"error", "wrong guid"}
                            });
            }

            var responseStr = JsonConvert.SerializeObject(response);
            GLogger.Debug(responseStr);
            _connection.Send(Encoding.UTF8.GetBytes(responseStr), PacketType.KeyValue);
        }
    }
}