using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestFormApp.User;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Log;
using Tizsoft.Treenet;
using Tizsoft.Treenet.Interface;
using Tizsoft.Treenet.PacketParser;

namespace TestFormApp
{
    public partial class Form1 : Form
    {
        Guid _guid;
        DatabaseQuery _dbQuery;
        UserData _user = new UserData();
        private ServerConfig _serverConfig;
        private LogPrinter _logPrinter;
        private ListenService _listenService;
        private ConnectService _connectService;
        readonly CacheUserData _cacheUserData;
        TizIdManager _idManager;
        byte[] _largeBytes;
        bool _largeTest;

        void ReadServerConfig()
        {
            _serverConfig = ServerConfig.Read(Application.StartupPath);
            AddressTextBox.Text = _serverConfig.Address ?? string.Empty;
            PortTextBox.Text = _serverConfig.Port.ToString();
            MaxConnsTextBox.Text = _serverConfig.MaxConnections.ToString();
            BufferSizeTextBox.Text = _serverConfig.BufferSize.ToString();
            TimeOutTextBox.Text = _serverConfig.TimeOut.ToString();
        }

        void SaveServerConfig()
        {
            _serverConfig.Address = AddressTextBox.Text;
            _serverConfig.Port = int.Parse(PortTextBox.Text);
            _serverConfig.MaxConnections = int.Parse(MaxConnsTextBox.Text);
            _serverConfig.BufferSize = int.Parse(BufferSizeTextBox.Text);
            _serverConfig.TimeOut = int.Parse(TimeOutTextBox.Text);
            ServerConfig.Save(Application.StartupPath, _serverConfig);
        }

        void InitTizIdManager()
        {
            if (null == _idManager)
            {
                _idManager = new TizIdManager();
            }
            _idManager.Read(Application.StartupPath + DatabasePath.ImportData);
        }

        void SaveTizIdManager()
        {
            _idManager.Save(Application.StartupPath + DatabasePath.ImportData);
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

            _dbQuery = new DatabaseQuery(new DatabaseConfig(databaseAddress, 3306, user, password, "speedrunning", string.Empty));
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

        void CheckJsonContent(JObject jsonObject, IConnection connection)
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
                            ValidateFacebookTokenAsync(connection, fbtoken, response);
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
                    connection.Send(Encoding.UTF8.GetBytes(responseStr), PacketType.KeyValue);
                    break;

                default:
                    GLogger.Warn(string.Format("未定義的function: <color=cyan>{0}</color>", functionToken));
                    break;
            }
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
                wc.DownloadStringAsync(new Uri("https://graph.facebook.com/me/?access_token=" + fbtoken), userToken);
            }
        }

        IPacketProcessor CreatePacketParser(PacketType type)
        {
            switch (type)
            {
                case PacketType.KeyValue:
                    return new ParseJsonPacket(CheckJsonContent);

                default:
                    return new ParseDefaultEchoPacket();
            }
        }

        void InitListenerPacketParser()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                _listenService.AddParser(type, CreatePacketParser(type));
            }
        }

        void InitConnectorPacketParser()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                _connectService.AddParser(type, CreatePacketParser(type));
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
                if (_listenService != null)
                    _listenService.Stop();
            }
            SaveTizIdManager();
        }

        public Form1()
        {
            InitializeComponent();
            ReadServerConfig();
            _logPrinter = new LogPrinter(LogMsgrichTextBox, 100);
            InitTizIdManager();
            _cacheUserData = new CacheUserData();
            _listenService = new ListenService();
            InitListenerPacketParser();
            _connectService = new ConnectService();
            InitConnectorPacketParser();
            InitDatabaseConnector();
            Application.ApplicationExit += AppClose;
            _largeBytes = new byte[512];
        }

        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Char.IsDigit(e.KeyChar);
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            var isClient = IsClientCheckBox.Checked;
            IService service = isClient ? _connectService as IService : _listenService as IService;

            if (isClient)
            {
                if (service.IsWorking)
                {
                    service.Setup(GetConnectServiceConfig(false));
                    service.Stop();
                }
                else
                {
                    service.Setup(GetConnectServiceConfig(true));
                    service.Start();
                }
            }
            else
            {
                if (service.IsWorking)
                    service.Stop();
                else
                {
                    SaveServerConfig();
                    SaveTizIdManager();
                    service.Setup(_serverConfig);
                    service.Start();
                }
            }
        }

        private void CheckServiceStatus()
        {
            var service = IsClientCheckBox.Checked ? _connectService as IService : _listenService as IService;
            StartBtn.Text = service.IsWorking ? "Stop" : "Start";
            StatusprogressBar.Value = service.IsWorking ? (StatusprogressBar.Value + 1) % StatusprogressBar.Maximum : StatusprogressBar.Minimum;
        }

        private void NewGuidBtn_Click(object sender, EventArgs e)
        {
            _guid = GuidUtil.New();
            _user = _dbQuery.GetUserData("123123123123");
            LogMsgrichTextBox.AppendText(_user.ToString() + Environment.NewLine);
            QueryGuidBtn.Text = string.Format("Query\n{0}", GuidUtil.ToBase64(_guid));
            QueryGuidBtn.Enabled = true;
            SetLevelBtn.Enabled = true;
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            if (_listenService != null)
            {
                CheckServiceStatus();
            }

            if (_connectService.IsWorking && _largeTest)
            {
                _connectService.Send(_largeBytes, PacketType.Stream);
            }
        }

        private void GameUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_connectService != null && _connectService.IsWorking)
                _connectService.Update();

            if (_listenService != null && _listenService.IsWorking)
                _listenService.Update();
        }

        private void QueryGuidBtn_Click(object sender, EventArgs e)
        {
            _user = _dbQuery.GetUserData(_guid);
            LogMsgrichTextBox.AppendText(_user.ToString() + Environment.NewLine);
        }

        private void SetLevelBtn_Click(object sender, EventArgs e)
        {
            _user.Money = 10;
            _dbQuery.WriteUserData(_user);
            _user = _dbQuery.GetUserData(_user.Guid);
            LogMsgrichTextBox.AppendText(_user.ToString() + Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TestLargePacket();
            //TestCreateAccount();
            //TestSendEchoPack();
            //TestWriteUserDataBack();
        }

        void TestLargePacket()
        {
            _largeTest = !_largeTest;
        }

        void TestWriteUserDataBack()
        {
            UserData user = _dbQuery.CreateNewUser(GuidUtil.New());
            user.Ap = 0;
            user.Attributes[0].Level = 10;
            user.Characters[0].Level = 3;
            user.Items[3].Level = 2;
            user.SkateBoards.Add(new IdLevelData() {Id = 2, Level = 2});
            user.Treasures.Add(new IdLevelData() {Id = 5, Level = 10});
            _dbQuery.WriteUserData(user);
        }

        void TestSendEchoPack()
        {
            string test = "hello world!";

            if (IsClientCheckBox.Checked)
            {
                var packetType = PacketTypeListBox.SelectedIndex == -1
                    ? PacketType.Echo
                    : (PacketType) PacketTypeListBox.SelectedIndex;
                _connectService.Send(Encoding.UTF8.GetBytes(test), packetType);
            }
        }

        void TestCreateAccount()
        {
            //string json = "{\"function\": \"login\", \"param\": { \"guid\": \"123123123123\", \"fbtoken\": \"\"}}";
            string json = "{\"function\": \"login\", \"param\": { \"guid\": \"\", \"fbtoken\": \"\"}}";
            JObject jObject = JObject.Parse(json);
            CheckJsonContent(jObject, Connection.Null);
        }
    }
}