using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Tizsoft.Database
{
    public class DatabaseConnector
    {
        StringBuilder _queryBuilder;
        MySqlConnection _mySqlConnection;
        MySqlDataAdapter _accountDataAdapter;
        MySqlCommandBuilder _accountCommandBuilder;

        void CloseConnection()
        {
            if (_mySqlConnection != null)
                _mySqlConnection.Close();
        }

        void ResetQueryBuilder()
        {
            _queryBuilder.Remove(0, _queryBuilder.Length);
        }

        MySqlCommand Request(List<string> columns, string table, string whereClause)
        {
            ResetQueryBuilder();
            _queryBuilder.Append("SELECT ");

            if (columns == null || columns.Count == 0)
                _queryBuilder.Append("* ");
            else
            {
                for (var i = 0; i < columns.Count; ++i)
                    _queryBuilder.AppendFormat("{0}{1}", columns[i], i == columns.Count - 1 ? " " : ",");
            }

            _queryBuilder.AppendFormat("FROM {0} ", table);

            if (!string.IsNullOrEmpty(whereClause))
                _queryBuilder.AppendFormat("WHERE {0}", whereClause);

            MySqlCommand command = new MySqlCommand(_queryBuilder.ToString());
            command.Connection = _mySqlConnection;
            return command;
        }

        MySqlCommand Create(List<string> columns, string table, List<object> values)
        {
            if (columns == null || values == null)
                return new MySqlCommand();

            ResetQueryBuilder();
            _queryBuilder.AppendFormat("INSERT INTO {0} (", table);

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                _queryBuilder.AppendFormat("{0}{1}", columns[i], i == columns.Count - 1 ? string.Empty : ",");

            _queryBuilder.Append(") VALUES(");

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                _queryBuilder.AppendFormat(@"'{0}'{1}", values[i], i == values.Count - 1 ? string.Empty : ",");

            _queryBuilder.Append(")");

            MySqlCommand command = new MySqlCommand(_queryBuilder.ToString());
            command.Connection = _mySqlConnection;
            return command;
        }

        public DatabaseConnector()
        {
            _queryBuilder = new StringBuilder();
        }

        ~DatabaseConnector()
        {
            if (_accountDataAdapter != null)
                _accountDataAdapter.Dispose();

            if (_accountCommandBuilder != null)
                _accountCommandBuilder.Dispose();

            CloseConnection();
        }

        public void Connect(EventArgs configArgs)
        {
            if (configArgs == null)
                throw new ArgumentNullException("configArgs");

            try
            {
                var config = (DatabaseConfig)configArgs;
                var connString = string.Format("server={0};uid={1};pwd={2};database={3};Charset=utf8", config.HostName, config.UserName, config.Password, config.DataBase);

                _mySqlConnection = new MySqlConnection(connString);
                _mySqlConnection.Open();

                _accountDataAdapter = new MySqlDataAdapter();
                _accountCommandBuilder = new MySqlCommandBuilder(_accountDataAdapter);
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                //throw exception;
            }
        }

        public string GetUserData(string guid)
        {
            if (_mySqlConnection == null)
                throw new Exception("not connect yet!");

            DataSet userData = new DataSet();
            _accountDataAdapter.SelectCommand = Request(null, TableConst.AccountTable, string.Format(@"{0}='{1}'", TableConst.GuidField, guid));

            try
            {
                _accountDataAdapter.Fill(userData, TableConst.AccountTable);

                if (userData.Tables[TableConst.AccountTable].Rows.Count == 0)
                {
                    MySqlCommand create = Create(new List<string>() { TableConst.GuidField }, TableConst.AccountTable, new List<object>() { guid });
                    create.ExecuteNonQuery();
                    _accountDataAdapter.Fill(userData, TableConst.AccountTable);
                }
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                throw exception;
            }

            return JsonConvert.SerializeObject(userData);
        }

        public string GetUserData(Guid guid)
        {
            return GetUserData(GuidUtil.ToBase64(guid));
        }

        /// <summary>
        /// write user data back to database
        /// </summary>
        /// <param name="userData">must in json format</param>
        public void WriteUserData(string userData)
        {
            try
            {
                using (var jsonReader = new JsonTextReader(new StringReader(userData)))
                {
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}