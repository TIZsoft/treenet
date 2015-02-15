using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Tizsoft.Database.Helper_Interface;
using Tizsoft.Log;

namespace Tizsoft.Database
{
    public class TizMySql
    {
        readonly StringBuilder _queryBuilder = new StringBuilder();
        readonly string _connectionString;

        static async void DisconnectAsync(MySqlConnection connection)
        {
            if (connection != null)
                await connection.CloseAsync();
        }

        static void Disconnect(IDbConnection connection)
        {
            if (connection != null)
                connection.Close();
        }

        void ResetQueryBuilder()
        {
            _queryBuilder.Remove(0, _queryBuilder.Length);
        }

        public TizMySql(EventArgs configArgs)
        {
            if (configArgs == null)
                throw new ArgumentNullException("configArgs");

            var config = (DatabaseConfig) configArgs;

            if (config == null)
                throw new InvalidCastException("configArgs");

            _connectionString = string.Format("server={0};port={1};uid={2};pwd={3};database={4};Charset=utf8;{5}",
                    config.HostName, config.Port, config.UserName, config.Password, config.DataBase, config.Option);
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

        static MySqlCommand CreateStoredProcedureCommand(MySqlConnection connection, IMySqlStoredProcedureHelper helper)
        {
            var cmd = new MySqlCommand(helper.Function, connection) {CommandType = CommandType.StoredProcedure};
            foreach (var parameter in helper.Parameters())
                cmd.Parameters.AddWithValue(string.Format("@{0}", parameter.Key), parameter.Value);
            return cmd;
        }

        static List<Dictionary<string, object>> FetchSqlResultToList(IDataReader dataReader)
        {
            var result = new List<Dictionary<string, object>>();

            try
            {
                while (dataReader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (var i = 0; i < dataReader.FieldCount; ++i)
                        row.Add(dataReader.GetName(i), dataReader[i]);
                    result.Add(row);
                }
                return result;
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
                return result;
            }
        }

        public async Task ExecuteNonQueryAsync(string query)
        {
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var cmd = new MySqlCommand(query, connection);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("execute query \"{0}\" get mysql exception {1} with number {2}", query, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("execute query \"{0}\" get exception {1}", query, exception);
            }
            finally
            {
                DisconnectAsync(connection);
            }
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query)
        {
            List<Dictionary<string, object>> result = null;
            MySqlConnection connection = null;
            DbDataReader dataReader = null;

            try
            {
                connection = await ConnectAsync();
                var cmd = new MySqlCommand(query, connection);
                dataReader = await cmd.ExecuteReaderAsync();
                result = FetchSqlResultToList(dataReader);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("execute query \"{0}\" get mysql exception {1} with number {2}", query, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("execute query \"{0}\" get exception {1}", query, exception);
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
                DisconnectAsync(connection);
            }

            return result;
        }

        public async Task<JArray> ExecuteQueryJsonAsync(string query)
        {
            var result = new JArray();

            try
            {
                result = JArray.FromObject(await ExecuteQueryAsync(query));
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("execute query \"{0}\" get mysql exception {1} with number {2}", query, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("execute query \"{0}\" get exception {1}", query, exception);
            }

            return result;
        }

        public async Task ExecuteNonQueryStoredProcedureAsync(IMySqlStoredProcedureHelper helper)
        {
            if (helper == null)
                throw new NullReferenceException("helper can't be null");

            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync();
                var cmd = CreateStoredProcedureCommand(connection, helper);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get mysql exception {1} with number {2}", helper.Function, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get exception {1}", helper.Function, exception);
            }
            finally
            {
                DisconnectAsync(connection);
            }
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryStoredProcedureAsync(
            IMySqlStoredProcedureHelper helper)
        {
            if (helper == null)
                throw new NullReferenceException("helper can't be null");

            var result = new List<Dictionary<string, object>>();
            MySqlConnection connection = null;
            DbDataReader dataReader = null;

            try
            {
                connection = await ConnectAsync();
                var cmd = CreateStoredProcedureCommand(connection, helper);
                dataReader = await cmd.ExecuteReaderAsync();
                result = FetchSqlResultToList(dataReader);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get mysql exception {1} with number {2}", helper.Function, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get exception {1}", helper.Function, exception);
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
                DisconnectAsync(connection);
            }

            return result;
        }

        public async Task<JArray> ExecuteQueryJsonStoredProcedureAsync(IMySqlStoredProcedureHelper helper)
        {
            var result = new JArray();

            try
            {
                result = JArray.FromObject(await ExecuteQueryStoredProcedureAsync(helper));
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get mysql exception {1} with number {2}", helper.Function, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get exception {1}", helper.Function, exception);
            }

            return result;
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
            }
            finally
            {
                Disconnect(connection);
            }
        }

        string BuildCreateOnDuplicateQueryString(string table, IReadOnlyList<string> columns, IReadOnlyList<object> values, string duplicateKeyClause)
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
            }
            finally
            {
                Disconnect(connection);
            }
        }

        string BuildMultiCreateOnDuplicateQueryString(string table, IReadOnlyList<string> columns, IReadOnlyList<List<object>> multiValueLists, string duplicateKeyClause)
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

                if (valueList == null || valueList.Count <= 0) 
                    continue;

                _queryBuilder.Append("(");

                for (var j = 0; j < minColumnCount; ++j)
                    _queryBuilder.AppendFormat(@"'{0}'{1}", valueList[j],
                        j == minColumnCount - 1 ? ")" : ",");

                _queryBuilder.Append(i == multiValueLists.Count - 1 ? " " : ",");
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
                result = FetchSqlResultToList(dataReader);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
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

        /// <summary>
        /// Request data async with "WHERE `key`='value'" condition.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="singleCondition"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> RequestAsync(string table, List<string> columns,
            KeyValuePair<string, object> singleCondition)
        {
            return
                await
                    RequestAsync(table, columns, SingleKeyValueWhereClause(singleCondition));
        }

        /// <summary>
        /// Request data(in json format returned) async with "WHERE `key`='value'" condition.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="singleCondition"></param>
        /// <returns></returns>
        public async Task<JArray> RequestJsonAsync(string table, List<string> columns,
            KeyValuePair<string, object> singleCondition)
        {
            return await RequestJsonAsync(table, columns, SingleKeyValueWhereClause(singleCondition));
        }

        static string SingleKeyValueWhereClause(KeyValuePair<string, object> singleCondition)
        {
            return string.Format("`{0}`='{1}'", singleCondition.Key, singleCondition.Value);
        }

        public async Task<JArray> RequestJsonAsync(string table, List<string> columns, string whereClause)
        {
            var result = new JArray();
            try
            {
                result = JArray.FromObject(await RequestAsync(table, columns, whereClause));
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
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
                result = FetchSqlResultToList(dataReader);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
                DisconnectAsync(connection);
            }

            return result;
        }

        /// <summary>
        /// Request data with "WHERE `key`='value'" condition.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="singleCondition"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Request(string table, List<string> columns,
            KeyValuePair<string, object> singleCondition)
        {
            return Request(table, columns, SingleKeyValueWhereClause(singleCondition));
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
                result = FetchSqlResultToList(dataReader);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
                Disconnect(connection);
            }
            
            return result;
        }

        string BuildRequestQueryString(string table, IReadOnlyList<string> columns, string whereClause)
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

        string BuildRequestJoinQueryString(IReadOnlyList<string> tables, IReadOnlyList<string> columns, string whereClause)
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

        /// <summary>
        /// Update data async with single condition with "WHERE `key`='value'".
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <param name="singleCondition"></param>
        /// <returns></returns>
        public async Task UpdateAsync(string table, List<string> columns, List<object> values,
            KeyValuePair<string, object> singleCondition)
        {
            await UpdateAsync(table, columns, values, SingleKeyValueWhereClause(singleCondition));
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

        /// <summary>
        /// Update data with single condition with "WHERE `key`='value'".
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <param name="singleCondition"></param>
        public void Update(string table, List<string> columns, List<object> values,
            KeyValuePair<string, object> singleCondition)
        {
            Update(table, columns, values, SingleKeyValueWhereClause(singleCondition));
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

        string BuildUpdateQueryString(string table, IReadOnlyList<string> columns, IReadOnlyList<object> values, string whereClause)
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

        /// <summary>
        /// Delete data async with only one condition to create a `WHERE key`='value' query.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="singleCondition"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string table, KeyValuePair<string, object> singleCondition)
        {
            await DeleteAsync(table, SingleKeyValueWhereClause(singleCondition));
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

        /// <summary>
        /// Delete data with only one condition to create a `WHERE key`='value' query.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="singleCondition"></param>
        /// <returns></returns>
        public void Delete(string table, KeyValuePair<string, object> singleCondition)
        {
            Delete(table, SingleKeyValueWhereClause(singleCondition));
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