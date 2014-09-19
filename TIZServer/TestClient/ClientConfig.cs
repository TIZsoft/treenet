using System.Net.Sockets;

namespace TIZServer.TestClient
{
	public class ClientConfig
	{
		public string Address { get; set; }
		public int Port { get; set; }
		public int BufferSize { get; set; }
		public SocketType TransferType { get; set; }
		public ProtocolType UseProtocol { get; set; }

		//private static string CONFIG_FILENAME = "ServerConfig.json";

		//private static string ConfigFullPath(string appPath)
		//{
		//	return string.Format(@"{0}\{1}", appPath, CONFIG_FILENAME);
		//}

		public ClientConfig()
		{
			Address = "127.0.0.1";
			TransferType = SocketType.Stream;
			UseProtocol = ProtocolType.Tcp;
		}

		//public static ServerConfig Read(string appPath)
		//{
		//	ServerConfig config = new ServerConfig();

		//	if (File.Exists(ConfigFullPath(appPath)))
		//	{
		//		using (var configFile = File.OpenText(ConfigFullPath(appPath)))
		//		{
		//			string configString = configFile.ReadToEnd();

		//			if (!string.IsNullOrEmpty(configString))
		//				config = JsonConvert.DeserializeObject<ServerConfig>(configString);
		//		}
		//	}

		//	return config;
		//}

		//public static void Save(string appPath, ServerConfig config)
		//{
		//	string jsonStr = JsonConvert.SerializeObject(config);
		//	File.WriteAllText(ConfigFullPath(appPath), jsonStr, Encoding.UTF8);
		//	Logger.Log(jsonStr);
		//}
	}
}