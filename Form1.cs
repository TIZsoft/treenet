using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Newtonsoft.Json;
using TIZServer;
using TIZSoft;
using TIZSoft.VS;

namespace TIZServerForm
{
	public partial class MainForm : Form
	{
		private ServerConfig _serverConfig;
		private LogPrinter _logPrinter;
		private ServerMain _main;

		void ReadServerConfig()
		{
			_serverConfig = ServerConfig.Read(Application.StartupPath);
			AddressTextBox.Text = _serverConfig.Address ?? string.Empty;
			PortTextBox.Text = _serverConfig.Port.ToString();
			MaxConnsTextBox.Text = _serverConfig.MaxConnections.ToString();
			BufferSizeTextBox.Text = _serverConfig.BufferSize.ToString();
		}

		void SaveServerConfig()
		{
			_serverConfig.Address = AddressTextBox.Text;
			_serverConfig.Port = int.Parse(PortTextBox.Text);
			_serverConfig.MaxConnections = int.Parse(MaxConnsTextBox.Text);
			_serverConfig.BufferSize = int.Parse(BufferSizeTextBox.Text);
			ServerConfig.Save(Application.StartupPath, _serverConfig);
		}

		public MainForm()
		{
			InitializeComponent();
			ReadServerConfig();
			_logPrinter = new LogPrinter(LogMsgrichTextBox);
			_main = new ServerMain();
		}

		private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !Char.IsDigit(e.KeyChar);
		}

		private void StartBtn_Click(object sender, EventArgs e)
		{
			SaveServerConfig();
			_main.Setup(_serverConfig);
			_main.Start();

			//if (_server == null)
			//	CreateServer();

			//if (_server.IsRunning)
			//	_server.StopServer();
			//else
			//{
			//	GameDataLoader.LoadAllFiles(Application.StartupPath + @"\GameData");
			//	_server.StartServer(GetNetConfig(), DBHosttextBox.Text, DBUsertextBox.Text, DBPwdtextBox.Text);
			//}
		}

		private void CreateServer()
		{
			//_server = new Server();
		}

		//private NetConfig GetNetConfig()
		//{
		//	string address = AddressTextBox.Text;
		//	int port = 0;

		//	if (!int.TryParse(PortTextBox.Text, out port))
		//	{
		//		Logger.LogError(string.Format("parse Port fail! use {0} instead!", TIZNetwork.DEFAULT_PORT_NUMBER));
		//		port = TIZNetwork.DEFAULT_PORT_NUMBER;
		//	}

		//	int maxConn = 0;

		//	if (!int.TryParse(MaxConnsTextBox.Text, out maxConn))
		//	{
		//		Logger.LogError(string.Format("parse Max Conns. fail! use {0} instead!", TIZNetwork.DEFAULT_SERVER_MAX_CONN));
		//		maxConn = TIZNetwork.DEFAULT_SERVER_MAX_CONN;
		//	}

		//	int bufferSize = 0;

		//	if (!int.TryParse(BufferSizeTextBox.Text, out bufferSize))
		//	{
		//		Logger.LogError(string.Format("parse Content Size fail! use {0} instead!", TIZNetwork.DEFAULT_NET_BUFFER_SIZE));
		//		bufferSize = TIZNetwork.DEFAULT_NET_BUFFER_SIZE;
		//	}

		//	int listenNumber = 0;

		//	if (!int.TryParse(ListenNoTextBox.Text, out listenNumber))
		//	{
		//		Logger.LogError(string.Format("parse Listen No. fail! use {0} instead!", TIZNetwork.DEFAULT_SERVER_LISTEN_NUM));
		//		listenNumber = TIZNetwork.DEFAULT_SERVER_LISTEN_NUM;
		//	}

		//	return new NetConfig(address, port, SocketType.Stream, ProtocolType.Tcp, maxConn, bufferSize, listenNumber);
		//}

		//private void CheckServerStatus()
		//{
			//StartBtn.Text = _server.IsRunning ? "Stop" : "Start";

			//if (_server.IsRunning)
			//{
			//	if (StatusprogressBar.MarqueeAnimationSpeed != StatusprogressBar.Maximum)
			//		StatusprogressBar.MarqueeAnimationSpeed = StatusprogressBar.Maximum;

			//	StatusprogressBar.Value = (StatusprogressBar.Value + 1) % StatusprogressBar.Maximum;
			//}
			//else
			//{
			//	if (StatusprogressBar.MarqueeAnimationSpeed != StatusprogressBar.Minimum)
			//		StatusprogressBar.MarqueeAnimationSpeed = StatusprogressBar.Minimum;

			//	StatusprogressBar.Value = StatusprogressBar.Minimum;
			//}
		//}

		private void statusTimer_Tick(object sender, EventArgs e)
		{
			//if (_server != null)
			//{
			//	CheckServerStatus();
			//	ConnectionCountLabel.Text = string.Format("連線數：{0}", _server.UserCount);
			//}

			_logPrinter.Print();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			SaveServerConfig();
			//string json = File.ReadAllText(Application.StartupPath + @"\Award_S.json");
			//GameData<Award> datas = new GameData<Award>(json);

			//IEnumerable<Award> query = from data in datas.Data select data;

			//foreach (Award data in query)
			//    Logger.Log(data.ID.ToString());

			//GameDataLoader.LoadAllFiles(Application.StartupPath + @"\GameData");
			//MySQLConnector.Instance.Setup(DBHosttextBox.Text, DBUsertextBox.Text, DBPwdtextBox.Text);
			//UserDataManager udm = new UserDataManager();
			//udm.GetUserData("123");
		}

		private void GameUpdateTimer_Tick(object sender, EventArgs e)
		{
			//if (_server != null && _server.IsRunning)
			//	_server.Update();
		}
	}
}