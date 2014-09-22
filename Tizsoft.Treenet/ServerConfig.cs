using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Tizsoft.Treenet
{
	public class ServerConfig
	{
		public string Address { get; set; }
		public int Port { get; set; }
		public int MaxConnections { get; set; }
		public int BufferSize { get; set; }
		public SocketType TransferType { get; set; }
		public ProtocolType UseProtocol { get; set; }

		private static string CONFIG_FILENAME = "ServerConfig.json";

		private static string ConfigFullPath(string appPath)
		{
			return string.Format(@"{0}\{1}", appPath, CONFIG_FILENAME);
		}

		public ServerConfig()
		{
			Address = "127.0.0.1";
		}

		public static ServerConfig Read(string appPath)
		{
			ServerConfig config = new ServerConfig();

			if (File.Exists(ConfigFullPath(appPath)))
			{
				using (var configFile = File.OpenText(ConfigFullPath(appPath)))
				{
					string configString = configFile.ReadToEnd();

					if (!string.IsNullOrEmpty(configString))
						config = JsonConvert.DeserializeObject<ServerConfig>(configString);
				}
			}

			return config;
		}

		public static void Save(string appPath, ServerConfig config)
		{
			string jsonStr = JsonConvert.SerializeObject(config);
			File.WriteAllText(ConfigFullPath(appPath), jsonStr, Encoding.UTF8);
			Logger.Log(jsonStr);
		}
	}
}