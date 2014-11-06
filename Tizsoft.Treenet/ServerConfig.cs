using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Tizsoft.Log;

namespace Tizsoft.Treenet
{
    // TODO: Remove duplicated.
    public class ServerConfig : EventArgs
    {
        public const int DefaultBufferSize = 8 * 1024;

        public const int DefaultMaxMessageSize = 8 * 1024;

        /// <summary>
        ///     Gets or sets the address of the local host.
        /// </summary>
        /// <example>
        /// var config = new ClientConfig();
        /// config.Address = "192.168.1.1";
        /// config.Address = "127.0.0.1";
        /// config.Address = "localhost";
        /// </example>
        public string Address { get; set; }

        public int Port { get; set; }

        /// <summary>
        ///     Gets or sets the maximum length of the pending connections queue.
        /// </summary>
        /// <remarks>
        ///     See <see cref="Socket.Listen"/>.
        /// </remarks>
        public int Backlog { get; set; }

        public int MaxConnections { get; set; }

        /// <summary>
        ///     Gets or sets the maximum message size.
        /// </summary>
        /// <remarks>
        ///     We strongly recommend the buffer size should be 8KB (default value).
        /// </remarks>
        public int BufferSize { get; set; }

        /// <summary>
        ///     Gets or sets the maximum message size.
        /// </summary>
        /// <remarks>
        ///     Default value is 8KB.
        /// </remarks>
        public int MaxMessageSize { get; set; }

        /// <summary>
        ///     Gets or sets the socket type.
        /// </summary>
        /// <remarks>
        ///     <see cref="SocketType.Stream"/> is recommended.
        /// </remarks>
        public SocketType TransferType { get; set; }

        /// <summary>
        ///     Gets or sets the protocol type of socket.
        /// </summary>
        /// <remarks>
        ///     <see cref="ProtocolType.Tcp"/> is recommended.
        /// </remarks>
        public ProtocolType UseProtocol { get; set; }

        public double TimeOut { get; set; }

        public PacketProtocolSettings PacketProtocolSettings { get; set; }

        const string ConfigFilename = "ServerConfig.json";

        static string ConfigFullPath(string appPath)
        {
            return string.Format(@"{0}\{1}", appPath, ConfigFilename);
        }

        public ServerConfig()
        {
            Address = "127.0.0.1";
            Backlog = (int)SocketOptionName.MaxConnections;
            TimeOut = Network.DefaultTimeOut;
            BufferSize = DefaultBufferSize;
            MaxMessageSize = DefaultMaxMessageSize;
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
            GLogger.Debug((object) jsonStr);
        }
    }
}