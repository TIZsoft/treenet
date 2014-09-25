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
            _dbConnector.Connect(new DatabaseConfig("1.34.115.165", 3306, "test", "Treenet", "speedrunning", string.Empty));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _guid = GuidUtil.New();
            richTextBox1.AppendText(_dbConnector.GetUserData(_guid) + Environment.NewLine);
            button2.Text = string.Format("Query\n{0}", GuidUtil.ToBase64(_guid));
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText(_dbConnector.GetUserData(_guid) + Environment.NewLine);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Logger.Msgs.Count > 0)
            {
                richTextBox1.AppendText(Logger.Msgs.Dequeue() + Environment.NewLine);
            }
        }
    }
}