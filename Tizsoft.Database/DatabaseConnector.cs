using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tizsoft.Database
{
    public class DatabaseConnector
    {
        StringBuilder _queryBuilder;
        MySqlConnection _mySqlConnection;

        void CloseConnection()
        {
            if (_mySqlConnection != null)
                _mySqlConnection.Close();
        }

        void ResetQueryBuilder()
        {
            _queryBuilder.Remove(0, _queryBuilder.Length);
        }

        void Create(string table, List<string> columns, List<object> values)
        {
            if (columns == null || values == null)
                return;

            ResetQueryBuilder();
            _queryBuilder.AppendFormat("INSERT INTO {0} (", table);

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                _queryBuilder.AppendFormat("{0}{1}", columns[i], i == columns.Count - 1 ? string.Empty : ",");

            _queryBuilder.Append(") VALUES(");

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                _queryBuilder.AppendFormat(@"'{0}'{1}", values[i], i == values.Count - 1 ? string.Empty : ",");

            _queryBuilder.Append(")");

            var createCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            createCommand.ExecuteNonQueryAsync();
        }

        MySqlDataReader Request(string table, List<string> columns, string whereClause)
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

            var requestCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            return requestCommand.ExecuteReader();
        }

        void Update(string table, List<string> columns, List<object> values, string whereClause)
        {
            ResetQueryBuilder();

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return;

            _queryBuilder.AppendFormat("UPDATE {0} SET ", table);
            var bound = Math.Min(columns.Count, values.Count);

            for (var i = 0; i < bound; ++i)
                _queryBuilder.AppendFormat(@"`{0}` = '{1}'{2}", columns[i], values[i], i == bound - 1 ? " " : ",");

            if (!string.IsNullOrEmpty(whereClause))
                _queryBuilder.AppendFormat("WHERE {0}", whereClause);

            var updateCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            updateCommand.ExecuteNonQuery();
        }

        int Count(string table, KeyValuePair<string, string> whereClause)
        {
            ResetQueryBuilder();
            _queryBuilder.AppendFormat(@"SELECT COUNT(*) FROM {0} WHERE `{1}`='{2}'", table, whereClause.Key, whereClause.Value);

            try
            {
                MySqlCommand countCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
                var count = countCommand.ExecuteScalar();
                return Convert.ToInt32(count);
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }

            return 0;
        }

        public DatabaseConnector()
        {
            _queryBuilder = new StringBuilder();
        }

        ~DatabaseConnector()
        {
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
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
        }

        public T GetUserData<T>(string guid)
        {
            if (_mySqlConnection == null)
                throw new Exception("not connect yet!");

            string json = string.Empty;
            MySqlDataReader dataReader = null;

            try
            {
                if (Count(SchemaConst.AccountTable, new KeyValuePair<string, string>(SchemaConst.GuidField, guid)) == 0)
                    Create(SchemaConst.AccountTable, new List<string>() {SchemaConst.GuidField},
                        new List<object>() {guid});

                dataReader = Request(SchemaConst.AccountTable, null,
                    string.Format(@"`{0}`='{1}'", SchemaConst.GuidField, guid));

                dataReader.Read();
                var dictionary = new Dictionary<string, object>();
                foreach (var accountField in SchemaConst.AccountFields)
                    dictionary.Add(accountField, dataReader[accountField]);

                json = JsonConvert.SerializeObject(dictionary);
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                throw exception;
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public T GetUserData<T>(Guid guid)
        {
            return GetUserData<T>(GuidUtil.ToBase64(guid));
        }

        public void WriteUserData(object userData)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(userData);
                var userdataJObject = JObject.Parse(jsonStr);
                var values = new List<object>(SchemaConst.AccountFields.Count);

                foreach (var accountField in SchemaConst.AccountFields)
                    values.Add(userdataJObject[accountField]);

                Update(SchemaConst.AccountTable, SchemaConst.AccountFields, values, string.Format("`{0}`='{1}'", SchemaConst.GuidField, userdataJObject[SchemaConst.GuidField]));
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}