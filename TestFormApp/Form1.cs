using System;
using System.Windows.Forms;
using Tizsoft;
using Tizsoft.Database;

namespace TestFormApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DatabaseConnector connector = new DatabaseConnector();
            connector.Connect(new DatabaseConfig("localhost", 3306, "root", "1234", "speedrunning", string.Empty));
            //connector.GetUserData("")
        }
    }
}