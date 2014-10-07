using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Tizsoft.Log;

namespace Tizsoft.Treenet
{
    public class ServerConfig : EventArgs
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public int MaxConnections { get; set; }

        public int BufferSize { get; set; }

        public SocketType TransferType { get; set; }

        public ProtocolType UseProtocol { get; set; }

        const string ConfigFilename = "ServerConfig.json";

        private static string ConfigFullPath(string appPath)
        {
            return string.Format(@"{0}\{1}", appPath, ConfigFilename);
        }

        public ServerConfig()
        {
            Address = "127.0.0.1";
        }

        public static ServerConfig Read(string appPath)
        {
            var config = new ServerConfig();

            if (File.Exists(ConfigFullPath(appPath)))
            {
                using (var configFile = File.OpenText(ConfigFullPath(appPath)))
                {
                    var configString = configFile.ReadToEnd();

                    if (!string.IsNullOrEmpty(configString))
                        config = JsonConvert.DeserializeObject<ServerConfig>(configString);
                }
            }

            return config;
        }

        public static void Save(string appPath, ServerConfig config)
        {
            var jsonStr = JsonConvert.SerializeObject(config);
            File.WriteAllText(ConfigFullPath(appPath), jsonStr, Encoding.UTF8);
            GLogger.Debug(jsonStr);
        }
    }
}