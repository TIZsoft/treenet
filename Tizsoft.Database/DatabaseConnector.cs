using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Tizsoft.Log;

namespace Tizsoft.Database
{
    public class DatabaseConnector
    {
        readonly StringBuilder _queryBuilder = new StringBuilder();
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
                GLogger.Fatal(exception);
            }
        }

        public void Create(string table, List<string> columns, List<object> values)
        {
            CreateOnDuplicate(table, columns, values, string.Empty);
        }

        public void CreateOnDuplicate(string table, List<string> columns, List<object> values, string duplicateKeyClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null)
                return;

            ResetQueryBuilder();
            _queryBuilder.AppendFormat("INSERT INTO `{0}` (", table);

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                _queryBuilder.AppendFormat("`{0}`{1}", columns[i], i == columns.Count - 1 ? string.Empty : ",");

            _queryBuilder.Append(") VALUES(");

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                _queryBuilder.AppendFormat(@"'{0}'{1}", values[i], i == values.Count - 1 ? string.Empty : ",");

            _queryBuilder.Append(") ");

            if (!string.IsNullOrEmpty(duplicateKeyClause))
                _queryBuilder.Append(duplicateKeyClause);

            var createCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            createCommand.ExecuteNonQueryAsync();
        }

        public void MultiCreate(string table, List<string> columns, List<List<object>> multiValueLists)
        {
            MultiCreateOnDuplicate(table, columns, multiValueLists, string.Empty);
        }

        public void MultiCreateOnDuplicate(string table, List<string> columns, List<List<object>> multiValueLists, string duplicateKeyClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || multiValueLists == null || columns.Count == 0 || multiValueLists.Count == 0)
                return;

            ResetQueryBuilder();
            _queryBuilder.AppendFormat("INSERT INTO `{0}` (", table);
            var minColumnCount = Math.Min(columns.Count, multiValueLists[0].Count);
            for (var i = 0; i < minColumnCount; ++i)
                _queryBuilder.AppendFormat("`{0}`{1}", columns[i], i == minColumnCount - 1 ? ") " : ",");

            _queryBuilder.Append("VALUES");

            for (var i = 0; i < multiValueLists.Count; ++i)
            {
                var valueList = multiValueLists[i];

                if (valueList != null && valueList.Count > 0)
                {
                    _queryBuilder.Append("(");

                    for (var j = 0; j < minColumnCount; ++j)
                        _queryBuilder.AppendFormat(@"'{0}'{1}", valueList[j],
                            j == minColumnCount - 1 ? ")" : ",");

                    _queryBuilder.Append(i == multiValueLists.Count - 1 ? " " : ",");
                }
            }

            if (!string.IsNullOrEmpty(duplicateKeyClause))
                _queryBuilder.AppendFormat(@"ON DUPLICATE KEY UPDATE {0}", duplicateKeyClause);

            var createCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            createCommand.ExecuteNonQueryAsync();
        }

        public List<Dictionary<string, object>> Request(string table, List<string> columns, string whereClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

            ResetQueryBuilder();
            _queryBuilder.Append("SELECT ");

            if (columns == null || columns.Count == 0)
                _queryBuilder.Append("* ");
            else
            {
                for (var i = 0; i < columns.Count; ++i)
                    _queryBuilder.AppendFormat("`{0}`{1}", columns[i], i == columns.Count - 1 ? " " : ",");
            }

            _queryBuilder.AppendFormat("FROM `{0}` ", table);

            if (!string.IsNullOrEmpty(whereClause))
                _queryBuilder.AppendFormat("WHERE {0}", whereClause);

            var result = new List<Dictionary<string, object>>();
            var requestCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            var dataReader = requestCommand.ExecuteReader();

            if (!dataReader.HasRows)
            {
                dataReader.Close();
                return result;
            }

            while (dataReader.Read())
            {
                if (columns != null)
                {
                    var row = columns.ToDictionary(column => column, column => dataReader[column]);
                    result.Add(row);
                }
                else
                {
                    var row = new Dictionary<string, object>();
                    for (var i = 0; i < dataReader.FieldCount; ++i)
                        row.Add(dataReader.GetName(i), dataReader[i]);
                    result.Add(row);
                }
            }
            dataReader.Close();
            return result;
        }

        public void Update(string table, List<string> columns, List<object> values, string whereClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return;

            ResetQueryBuilder();
            _queryBuilder.AppendFormat("UPDATE `{0}` SET ", table);
            var bound = Math.Min(columns.Count, values.Count);

            for (var i = 0; i < bound; ++i)
                _queryBuilder.AppendFormat(@"`{0}` = '{1}'{2}", columns[i], values[i], i == bound - 1 ? " " : ",");

            if (!string.IsNullOrEmpty(whereClause))
                _queryBuilder.AppendFormat("WHERE {0}", whereClause);

            var updateCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            updateCommand.ExecuteNonQuery();
        }

        public void Delete(string table, string whereClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

            if (string.IsNullOrEmpty(whereClause))
                return;

            ResetQueryBuilder();

            _queryBuilder.AppendFormat("DELETE FROM `{0}` WHERE {1}", table, whereClause);
            var deleteCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            deleteCommand.ExecuteNonQuery();
        }

        public int Count(string table, KeyValuePair<string, string> whereClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

            ResetQueryBuilder();
            _queryBuilder.AppendFormat(@"SELECT COUNT(*) FROM `{0}` WHERE `{1}`='{2}'", table, whereClause.Key, whereClause.Value);

            try
            {
                MySqlCommand countCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
                var count = countCommand.ExecuteScalar();
                return Convert.ToInt32(count);
            }
            catch (Exception exception)
            {
                GLogger.Fatal(exception);
            }

            return 0;
        }
    }
}