using System;
using System.Windows.Forms;
using Tizsoft;
using Tizsoft.Database;

namespace TestFormApp
{
    public partial class Form1 : Form
    {
        Guid _guid;
        DatabaseConnector _dbConnector;

        public Form1()
        {
            InitializeComponent();

            _dbConnector = new DatabaseConnector();
            _dbConnector.Connect(new DatabaseConfig("localhost", 3306, "root", "1234", "speedrunning", string.Empty));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _guid = GuidUtil.New();
            richTextBox1.AppendText(_dbConnector.GetUserData(_guid, "account"));
            button2.Text = string.Format("Query\n{0}", GuidUtil.ToBase64(_guid));
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText(_dbConnector.GetUserData(_guid, "account"));
        }
    }
}