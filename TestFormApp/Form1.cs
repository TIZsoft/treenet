using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Log;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Interface;

namespace TestFormApp
{
    public static class DataBasePath
    {
        public const string ImportData = "/ImportData";
    }

    public partial class Form1 : Form
    {
        Guid _guid;
        DatabaseConnector _dbConnector;
        TestUserData _testUser = new TestUserData();
        private ServerConfig _serverConfig;
        private LogPrinter _logPrinter;
        private ListenService _listen;
        private ConnectService _connectService;
        CacheUserData _cacheUserData;
        TizIdManager _idManager;

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

        ClientConfig GetConnectServiceConfig(bool autoReconnect)
        {
            return new ClientConfig()
            {
                Address = AddressTextBox.Text,
                Port = int.Parse(PortTextBox.Text),
                BufferSize = int.Parse(BufferSizeTextBox.Text),
                AutoReConnect = autoReconnect
            };
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
                validateArgs.Connection.Send(Encoding.UTF8.GetBytes(responseStr));
                return;
            }

            var validateResultJobject = JObject.Parse(args.Result);
            var fbid = (string)validateResultJobject.SelectToken("id");

            TestUserData userData;

            if (!_dbConnector.HasUserData<TestUserData>(fbid, AccountType.Facebook, out userData))
                userData = _dbConnector.CreateNewUser<TestUserData>(fbid, AccountType.Facebook);

            response.Add("param", new Dictionary<string, object>
            {
                {"user", JsonConvert.SerializeObject(userData)}
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
            var functionToken = (string)jsonObject.SelectToken("function");

            switch (functionToken.ToLower())
            {
                case "login":
                    var guid = (string)jsonObject.SelectToken("param.guid");
                    var fbtoken = (string)jsonObject.SelectToken("param.fbtoken");

                    if (string.IsNullOrEmpty(guid))
                    {
                        if (string.IsNullOrEmpty(fbtoken))
                        {
                            var userData = _cacheUserData.Get(guid);
                            if (null == userData)
                            {
                                userData = _dbConnector.CreateNewUser<TestUserData>(GuidUtil.New());
                            }
                            response.Add("param", new Dictionary<string, object>
                            {
                                {"user", JsonConvert.SerializeObject(userData)}
                            });
                        }
                        else
                        {
                            ValidateFacebookTokenAsync(connection, fbtoken, response);
                            return;
                        }
                    }
                    else
                    {
                        TestUserData user;
                        response.Add("param", _dbConnector.HasUserData(guid, AccountType.Guid, out user) ?
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
                    Logger.Log(responseStr);
                    connection.Send(Encoding.UTF8.GetBytes(responseStr));
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

        IPacketProcessor CreatePacketParser(PacketType type)
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
                _listen.AddParser(type, CreatePacketParser(type));
            }
        }

        void AppClose(object sender, EventArgs args)
        {
            if (IsClientCheckBox.Checked)
            {
                if (_connectService != null)
                    _connectService.Stop();
            }
            else
            {
                if (_listen != null)
                    _listen.Stop();
            }
        }

        public Form1()
        {
            InitializeComponent();
            ReadServerConfig();
            _logPrinter = new LogPrinter(LogMsgrichTextBox);
            _listen = new ListenService();
            _cacheUserData = new CacheUserData();
            _idManager = new TizIdManager();
            InitPacketParser();
            _connectService = new ConnectService();
            InitDatabaseConnector();
            Application.ApplicationExit += AppClose;
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
                if (_connectService.IsWorking)
                {
                    _connectService.Setup(GetConnectServiceConfig(false));
                    _connectService.Stop();
                }
                else
                {
                    ClientConfig config = GetConnectServiceConfig(true);
                    _connectService.Setup(config);
                    _connectService.Start();
                }
            }
            else
            {
                if (_listen.IsWorking)
                    _listen.Stop();
                else
                {
                    SaveServerConfig();
                    _listen.Setup(_serverConfig);
                    _listen.Start();
                }
            }
        }

        private void CheckServiceStatus()
        {
            StartBtn.Text = (IsClientCheckBox.Checked ? _connectService.IsWorking : _listen.IsWorking) ? "Stop" : "Start";

            if (_listen.IsWorking)
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
            if (_listen != null)
            {
                CheckServiceStatus();
            }

            _logPrinter.Print();
        }

        private void GameUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_connectService != null && _connectService.IsWorking)
                _connectService.Update();

            if (_listen != null && _listen.IsWorking)
                _listen.Update();
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