using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Tizsoft.Log;

namespace Tizsoft.Database
{
    public class DatabaseConnector
    {
        readonly StringBuilder _queryBuilder = new StringBuilder();
        readonly string _connectionString;

        async void DisconnectAsync(MySqlConnection connection)
        {
            if (connection != null)
                await connection.CloseAsync();
        }

        void Disconnect(MySqlConnection connection)
        {
            if (connection != null)
                connection.Close();
        }

        void ResetQueryBuilder()
        {
            _queryBuilder.Remove(0, _queryBuilder.Length);
        }

        public DatabaseConnector(EventArgs configArgs)
        {
            if (configArgs == null)
                throw new ArgumentNullException("configArgs");

            var config = (DatabaseConfig) configArgs;

            if (config == null)
                throw new InvalidCastException("configArgs");

            _connectionString = string.Format("server={0};port={1};uid={2};pwd={3};database={4};Charset=utf8;{5}",
                    config.HostName, config.Port, config.UserName, config.Password, config.DataBase, config.Option);

            GLogger.Debug(config.Option);
        }

        async Task<MySqlConnection> ConnectAsync()
        {
            try
            {
                var mySqlConnection = new MySqlConnection(_connectionString);
                await mySqlConnection.OpenAsync();
                return mySqlConnection;
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("exception number: {0}\n{1}", mySqlException.Number, mySqlException);
            }
            catch (Exception exception)
            {
                GLogger.Fatal(exception);
            }

            return null;
        }

        MySqlConnection Connect()
        {
            try
            {
                var mySqlConnection = new MySqlConnection(_connectionString);
                mySqlConnection.Open();
                return mySqlConnection;
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("exception number: {0}\n{1}", mySqlException.Number, mySqlException);
            }
            catch (Exception exception)
            {
                GLogger.Fatal(exception);
            }

            return null;
        }

        public async Task CreateAsync(string table, List<string> columns, List<object> values)
        {
            await CreateOnDuplicateAsync(table, columns, values, string.Empty);
        }

        public void Create(string table, List<string> columns, List<object> values)
        {
            CreateOnDuplicate(table, columns, values, string.Empty);
        }

        public async Task CreateOnDuplicateAsync(string table, List<string> columns, List<object> values,
            string duplicateKeyClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null)
                return;

            var queryString = BuildCreateOnDuplicateQueryString(table, columns, values, duplicateKeyClause);
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var createCommand = new MySqlCommand(queryString, connection);
                await createCommand.ExecuteNonQueryAsync();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                DisconnectAsync(connection);
            }
        }

        public void CreateOnDuplicate(string table, List<string> columns, List<object> values, string duplicateKeyClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null)
                return;

            var queryString = BuildCreateOnDuplicateQueryString(table, columns, values, duplicateKeyClause);
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var createCommand = new MySqlCommand(queryString, connection);
                createCommand.ExecuteNonQuery();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        string BuildCreateOnDuplicateQueryString(string table, List<string> columns, List<object> values, string duplicateKeyClause)
        {
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

            return _queryBuilder.ToString();
        }

        public async Task MultiCreateAsync(string table, List<string> columns, List<List<object>> multiValueLists)
        {
            await MultiCreateOnDuplicateAsync(table, columns, multiValueLists, string.Empty);
        }

        public void MultiCreate(string table, List<string> columns, List<List<object>> multiValueLists)
        {
            MultiCreateOnDuplicate(table, columns, multiValueLists, string.Empty);
        }

        public async Task MultiCreateOnDuplicateAsync(string table, List<string> columns,
            List<List<object>> multiValueLists, string duplicateKeyClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || multiValueLists == null || columns.Count == 0 || multiValueLists.Count == 0)
                return;

            var queryString = BuildMultiCreateOnDuplicateQueryString(table, columns, multiValueLists, duplicateKeyClause);
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var createCommand = new MySqlCommand(queryString, connection);
                await createCommand.ExecuteNonQueryAsync();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                DisconnectAsync(connection);
            }
        }

        public void MultiCreateOnDuplicate(string table, List<string> columns, List<List<object>> multiValueLists, string duplicateKeyClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || multiValueLists == null || columns.Count == 0 || multiValueLists.Count == 0)
                return;

            var queryString = BuildMultiCreateOnDuplicateQueryString(table, columns, multiValueLists, duplicateKeyClause);
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var createCommand = new MySqlCommand(queryString, connection);
                createCommand.ExecuteNonQuery();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        string BuildMultiCreateOnDuplicateQueryString(string table, List<string> columns, List<List<object>> multiValueLists, string duplicateKeyClause)
        {
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

            return _queryBuilder.ToString();
        }

        public async Task<List<Dictionary<string, object>>> RequestJoinAsync(List<string> tables, 
            List<string> columns, string whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new Exception("Connection doesn't establish yet.");
            }

            var queryString = BuildRequestJoinQueryString(tables, columns, whereClause);
            var result = new List<Dictionary<string, object>>();
            DbDataReader dataReader = null;
            MySqlConnection connection = null;
            try
            {
                connection = await ConnectAsync();
                var requestCommand = new MySqlCommand(queryString, connection);
                dataReader = await requestCommand.ExecuteReaderAsync();

                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    DisconnectAsync(connection);
                    return result;
                }

                while (dataReader.Read())
                {
                    var row = new Dictionary<string, object>();
                    if (columns != null)
                    {
                        for (var i = 0; i < dataReader.FieldCount; ++i)
                        {
                            var name = dataReader.GetName(i);
                            var idx = columns.FindIndex(s => string.CompareOrdinal(s, name) == 0);
                            if (idx == -1)
                            {
                                continue;
                            }

                            if (row.ContainsKey(name))
                            {
                                continue;
                            }

                            row.Add(name, dataReader.GetValue(i));
                        }
                    }
                    else
                    {
                        for (var i = 0; i < dataReader.FieldCount; ++i)
                        {
                            var name = dataReader.GetName(i);
                            if (row.ContainsKey(name))
                            {
                                continue;
                            }
                            row.Add(name, dataReader.GetValue(i));
                        }
                    }
                    result.Add(row);
                }
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                }
                DisconnectAsync(connection);
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> RequestAsync(string table, List<string> columns, string whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildRequestQueryString(table, columns, whereClause);

            var result = new List<Dictionary<string, object>>();
            DbDataReader dataReader = null;
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var requestCommand = new MySqlCommand(queryString, connection);
                dataReader = await requestCommand.ExecuteReaderAsync();

                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    DisconnectAsync(connection);
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
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
                DisconnectAsync(connection);
            }

            return result;
        }

        public List<Dictionary<string, object>> Request(string table, List<string> columns, string whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildRequestQueryString(table, columns, whereClause);

            var result = new List<Dictionary<string, object>>();
            MySqlDataReader dataReader = null;
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var requestCommand = new MySqlCommand(queryString, connection);
                dataReader = requestCommand.ExecuteReader();

                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    Disconnect(connection);
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
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
                Disconnect(connection);
            }
            
            return result;
        }

        string BuildRequestQueryString(string table, List<string> columns, string whereClause)
        {
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

            return _queryBuilder.ToString();
        }

        string BuildRequestJoinQueryString(List<string> tables, List<string> columns, string whereClause)
        {
            ResetQueryBuilder();
            
            // SELECT
            _queryBuilder.Append("SELECT ");
            if (columns == null || columns.Count == 0)
                _queryBuilder.Append("* ");
            else
            {
                for (var i = 0; i < columns.Count; ++i)
                    _queryBuilder.AppendFormat("{0}{1}", columns[i], i == columns.Count - 1 ? " " : ",");
            }
            
            // FROM
            _queryBuilder.Append("FROM ");
            for (var i = 0; i < tables.Count; ++i)
            {
                _queryBuilder.AppendFormat("{0}{1}", tables[i], i == tables.Count - 1 ? " " : ",");
            }
            
            // WHERE
            if (!string.IsNullOrEmpty(whereClause))
                _queryBuilder.AppendFormat("WHERE {0}", whereClause);

            return _queryBuilder.ToString();
        }

        public async Task UpdateAsync(string table, List<string> columns, List<object> values, string whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return;

            var queryString = BuildUpdateQueryString(table, columns, values, whereClause);
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var updateCommand = new MySqlCommand(queryString, connection);
                await updateCommand.ExecuteNonQueryAsync();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                DisconnectAsync(connection);
            }
        }

        public void Update(string table, List<string> columns, List<object> values, string whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return;

            var queryString = BuildUpdateQueryString(table, columns, values, whereClause);
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var updateCommand = new MySqlCommand(queryString, connection);
                updateCommand.ExecuteNonQuery();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        string BuildUpdateQueryString(string table, List<string> columns, List<object> values, string whereClause)
        {
            ResetQueryBuilder();
            _queryBuilder.AppendFormat("UPDATE `{0}` SET ", table);
            var bound = Math.Min(columns.Count, values.Count);

            for (var i = 0; i < bound; ++i)
                _queryBuilder.AppendFormat(@"`{0}` = '{1}'{2}", columns[i], values[i], i == bound - 1 ? " " : ",");

            if (!string.IsNullOrEmpty(whereClause))
                _queryBuilder.AppendFormat("WHERE {0}", whereClause);

            return _queryBuilder.ToString();
        }

        public async Task DeleteAsync(string table, string whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (string.IsNullOrEmpty(whereClause))
                return;

            var queryString = BuildDeleteQueryString(table, whereClause);
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var deleteCommand = new MySqlCommand(queryString, connection);
                await deleteCommand.ExecuteNonQueryAsync();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                DisconnectAsync(connection);
            }
        }

        public void Delete(string table, string whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (string.IsNullOrEmpty(whereClause))
                return;

            var queryString = BuildDeleteQueryString(table, whereClause);
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var deleteCommand = new MySqlCommand(queryString, connection);
                deleteCommand.ExecuteNonQuery();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        string BuildDeleteQueryString(string table, string whereClause)
        {
            ResetQueryBuilder();
            _queryBuilder.AppendFormat("DELETE FROM `{0}` WHERE {1}", table, whereClause);
            return _queryBuilder.ToString();
        }

        public async Task<int> CountAsync(string table, KeyValuePair<string, string> whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildCountQueryString(table, whereClause);
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var countCommand = new MySqlCommand(queryString, connection);
                var count = await countCommand.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                DisconnectAsync(connection);
            }

            return 0;
        }

        public int Count(string table, KeyValuePair<string, string> whereClause)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildCountQueryString(table, whereClause);
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var countCommand = new MySqlCommand(queryString, connection);
                var count = countCommand.ExecuteScalar();
                return Convert.ToInt32(count);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                throw;
            }
            finally
            {
                Disconnect(connection);
            }

            return 0;
        }

        string BuildCountQueryString(string table, KeyValuePair<string, string> whereClause)
        {
            ResetQueryBuilder();
            _queryBuilder.AppendFormat(@"SELECT COUNT(*) FROM `{0}` WHERE `{1}`='{2}'", table, whereClause.Key,
                whereClause.Value);
            return _queryBuilder.ToString();
        }
    }
}