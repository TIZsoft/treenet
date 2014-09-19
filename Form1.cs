using System;
using System.Text;
using System.Windows.Forms;
using TIZServer;
using TIZServer.TestClient;
using TIZSoft.VS;

namespace TIZServerForm
{
	public partial class MainForm : Form
	{
		private ServerConfig _serverConfig;
		private LogPrinter _logPrinter;
		private ServerMain _main;
		private TestClient _testClient;

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

		ClientConfig GetTestClientConfig()
		{
			ClientConfig config = new ClientConfig();
			config.Address = AddressTextBox.Text;
			config.Port = int.Parse(PortTextBox.Text);
			config.BufferSize = int.Parse(BufferSizeTextBox.Text);
			return config;
		}

		public MainForm()
		{
			InitializeComponent();
			ReadServerConfig();
			_logPrinter = new LogPrinter(LogMsgrichTextBox);
			_main = new ServerMain();
			_testClient = new TestClient();
		}

		private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !Char.IsDigit(e.KeyChar);
		}

		private void StartBtn_Click(object sender, EventArgs e)
		{
			bool isClient = IsClientCheckBox.Checked;

			if (isClient)
			{
				ClientConfig config = GetTestClientConfig();
				_testClient.Setup(config);
				_testClient.Start();
			}
			else
			{
				SaveServerConfig();
				_main.Setup(_serverConfig);
				_main.Start();
			}
		}

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
			if (IsClientCheckBox.Checked)
				_testClient.Connection.Send(Encoding.UTF8.GetBytes("Hello World!"));

			//SaveServerConfig();
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