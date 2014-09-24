using System;

namespace Tizsoft
{
    public class DatabaseConfig : EventArgs
    {
        public DatabaseConfig(string host, int port, string user, string pass, string db, string opt)
        {
            HostName = host;
            Port = port;
            UserName = user;
            Password = pass;
            DataBase = db;
            Option = opt;
        }

        public string HostName { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DataBase { get; set; }

        public string Option { get; set; }
    }
}