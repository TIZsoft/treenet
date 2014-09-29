using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Event.Default;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Log;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Interface;
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

        void FacebookValidateHandler(object sender, DownloadStringCompletedEventArgs args)
        {
            var validateArgs = (FacebookValidateArgs) args.UserState;
            var response = validateArgs.Response;

            if (args.Error != null)
            {
                var responseStream = ((WebException) args.Error).Response.GetResponseStream();
                using (var reader = new StreamReader(responseStream))
                {
                    response.Add("param", new Dictionary<string, object>()
                    {
                        {"error", reader.ReadToEnd()}
                    });
                }

                var responseStr = JsonConvert.SerializeObject(response);
                validateArgs.Connection.Send(Encoding.UTF8.GetBytes(responseStr));
                return;
            }

            var validateResultJobject = JObject.Parse(args.Result);
            var fbid = (string)validateResultJobject.SelectToken("id");
            var user = _dbConnector.GetUserDataByToken<TestUserData>(fbid, TokenType.Facebook);
            response.Add("param", new Dictionary<string, object>()
            {
                {"user", JsonConvert.SerializeObject(user)}
            });
            validateArgs.Connection.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
        }

        void CheckJsonContent(JObject jsonObject, Connection connection)
        {
            if (jsonObject == null)
                return;

            var response = new Dictionary<string, object>()
            {
                {"result", "login"},
            };
            var functionToken = (string) jsonObject.SelectToken("function");

            switch (functionToken.ToLower())
            {
                case "login":
                    var guid = (string) jsonObject.SelectToken("param.guid");
                    var fbtoken = (string) jsonObject.SelectToken("param.fbtoken");

                    if (string.IsNullOrEmpty(guid))
                    {
                        if (string.IsNullOrEmpty(fbtoken))
                        {
                            var userData = _dbConnector.GetUserData<TestUserData>(GuidUtil.New());
                            response.Add("param", new Dictionary<string, object>()
                            {
                                {"user", JsonConvert.SerializeObject(userData)}
                            });
                            var responseStr = JsonConvert.SerializeObject(response);
                            Logger.Log(responseStr);
                            connection.Send(Encoding.UTF8.GetBytes(responseStr));
                        }
                        else
                        {
                            ValidateFacebookTokenAsync(connection, fbtoken, response);
                        }
                    }
                        
                    break;

                default:
                    Logger.LogWarning(string.Format("未定義的function: <color=cyan>{0}</color>", functionToken));
                    break;
            }
        }

        void ValidateFacebookTokenAsync(Connection connection, string fbtoken, Dictionary<string, object> response)
        {
            using (var wc = new WebClient())
            {
                var userToken = new FacebookValidateArgs();
                userToken.Connection = connection;
                userToken.FbToken = fbtoken;
                userToken.Response = response;

                wc.Encoding = Encoding.UTF8;
                wc.DownloadStringCompleted += FacebookValidateHandler;
                wc.DownloadStringAsync(new Uri("https://graph.facebook.com/me/?access_token=" + fbtoken), userToken);
            }
        }

        IPacketParser CreatePacketParser(PacketType type)
        {
            switch (type)
            {
                default:
                    return new ParseJsonPacket(CheckJsonContent);
            }
        }

        void InitPacketParser()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                _server.PacketHandler.AddParser(type, CreatePacketParser(type));
            }
        }

        public Form1()
        {
            InitializeComponent();
            ReadServerConfig();
            _logPrinter = new LogPrinter(LogMsgrichTextBox);
            _server = new ServerMain();
            InitPacketParser();
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

        private void button1_Click(object sender, EventArgs e)
        {
            string json = "{\"function\": \"login\", \"param\": { \"guid\": \"\", \"fbtoken\": \"12345\"}}";
            JObject jObject = JObject.Parse(json);
            CheckJsonContent(jObject, Connection.NullConnection);
        }
    }
}