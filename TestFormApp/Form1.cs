using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Log;

namespace TestFormApp
{
    public partial class Form1 : Form
    {
        Guid _guid;
        DatabaseConnector _dbConnector;
        TestUserData _testUser = new TestUserData();

        public Form1()
        {
            InitializeComponent();

            _dbConnector = new DatabaseConnector();
            _dbConnector.Connect(new DatabaseConfig("1.34.115.165", 3306, "test", "Treenet", "speedrunning", string.Empty));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _guid = GuidUtil.New();
            //var userDataStr = _dbConnector.GetUserData(_guid);
            //_testUser = JsonConvert.DeserializeObject<TestUserData>(userDataStr);
            //richTextBox1.AppendText(userDataStr + Environment.NewLine);
            _testUser = _dbConnector.GetUserData<TestUserData>(_guid);
            richTextBox1.AppendText(_testUser.ToString() + Environment.NewLine);
            button2.Text = string.Format("Query\n{0}", GuidUtil.ToBase64(_guid));
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _testUser = _dbConnector.GetUserData<TestUserData>(_guid);
            richTextBox1.AppendText(_testUser.ToString() + Environment.NewLine);
            //richTextBox1.AppendText(_dbConnector.GetUserData(_guid) + Environment.NewLine);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Logger.Msgs.Count > 0)
            {
                richTextBox1.AppendText(Logger.Msgs.Dequeue() + Environment.NewLine);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _testUser.level = 10;
            _dbConnector.WriteUserData(_testUser);
            _testUser = _dbConnector.GetUserData<TestUserData>(_testUser.guid);
            //_testUser = JsonConvert.DeserializeObject<TestUserData>(userDataStr);
            richTextBox1.AppendText(_testUser.ToString() + Environment.NewLine);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _testUser = new TestUserData();
            _testUser.level = 20;
            string json = JsonConvert.SerializeObject(_testUser);
            TestUserData user = JsonConvert.DeserializeObject<TestUserData>(json);
            richTextBox1.AppendText(user.ToString() + Environment.NewLine);
        }
    }
}