using System;
using System.Windows.Forms;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Tests.TestClient;

namespace TestFormApp
{
    public partial class Form1 : Form
    {
        Guid _guid;
        DatabaseConnector _dbConnector;
        TestUserData _testUser = new TestUserData();
        private ServerConfig _serverConfig;
        private LogPrinter _logPrinter;
        private ServerMain _server;
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

        void InitDatabaseConnector()
        {
            string databaseAddress = DBHosttextBox.Text;
            string user = DBUsertextBox.Text;
            string password = DBPwdtextBox.Text;

            _dbConnector = new DatabaseConnector();
            _dbConnector.Connect(new DatabaseConfig(databaseAddress, 3306, user, password, "speedrunning", string.Empty));
        }

        public Form1()
        {
            InitializeComponent();
            ReadServerConfig();
            _logPrinter = new LogPrinter(LogMsgrichTextBox);
            _server = new ServerMain();
            _testClient = new TestClient();
            InitDatabaseConnector();
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
                if (_testClient.IsWorking)
                    _testClient.Stop();
                else
                {
                    ClientConfig config = GetTestClientConfig();
                    _testClient.Setup(config);
                    _testClient.Start();
                }
            }
            else
            {
                if (_server.IsWorking)
                    _server.Stop();
                else
                {
                    SaveServerConfig();
                    _server.Setup(_serverConfig);
                    _server.Start();	
                }
            }
        }
        
        private void CheckServerStatus()
        {
            StartBtn.Text = (IsClientCheckBox.Checked ? _testClient.IsWorking : _server.IsWorking) ? "Stop" : "Start";

            if (_server.IsWorking)
            {
                StatusprogressBar.MarqueeAnimationSpeed = StatusprogressBar.Maximum;
                StatusprogressBar.Value = (StatusprogressBar.Value + 1) % StatusprogressBar.Maximum;
            }
            else
            {
                StatusprogressBar.MarqueeAnimationSpeed = StatusprogressBar.Minimum;
                StatusprogressBar.Value = StatusprogressBar.Minimum;
            }
        }

        private void NewGuidBtn_Click(object sender, EventArgs e)
        {
            _guid = GuidUtil.New();
            _testUser = _dbConnector.GetUserData<TestUserData>(_guid);
            LogMsgrichTextBox.AppendText(_testUser.ToString() + Environment.NewLine);
            QueryGuidBtn.Text = string.Format("Query\n{0}", GuidUtil.ToBase64(_guid));
            QueryGuidBtn.Enabled = true;
            SetLevelBtn.Enabled = true;
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            if (_server != null)
            {
                CheckServerStatus();
            }

            _logPrinter.Print();
        }
        
        private void GameUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_testClient != null && _testClient.IsWorking)
                _testClient.Update();

            if (_server != null && _server.IsWorking)
                _server.Update();
        }

        private void QueryGuidBtn_Click(object sender, EventArgs e)
        {
            _testUser = _dbConnector.GetUserData<TestUserData>(_guid);
            LogMsgrichTextBox.AppendText(_testUser.ToString() + Environment.NewLine);
        }

        private void SetLevelBtn_Click(object sender, EventArgs e)
        {
            _testUser.level = 10;
            _dbConnector.WriteUserData(_testUser);
            _testUser = _dbConnector.GetUserData<TestUserData>(_testUser.guid);
            LogMsgrichTextBox.AppendText(_testUser.ToString() + Environment.NewLine);
        }
    }
}