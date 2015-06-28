using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
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
        readonly string _connectionString;

        static async void DisconnectAsync(MySqlConnection connection)
        {
            if (connection != null)
                await connection.CloseAsync().ConfigureAwait(false);
        }

        static void Disconnect(IDbConnection connection)
        {
            if (connection != null)
                connection.Close();
        }

        #region build mysql query string

        string BuildCreateOnDuplicateQueryString(string table, IReadOnlyList<string> columns, IReadOnlyList<object> values, string duplicateKeyClause)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("INSERT INTO `{0}` (", table);

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                builder.AppendFormat("`{0}`{1}", columns[i], i == columns.Count - 1 ? string.Empty : ",");

            builder.Append(") VALUES(");

            for (var i = 0; i < Math.Min(columns.Count, values.Count); ++i)
                builder.AppendFormat(@"'{0}'{1}", values[i], i == values.Count - 1 ? string.Empty : ",");

            builder.Append(") ");

            if (!string.IsNullOrEmpty(duplicateKeyClause))
            {
                if (!duplicateKeyClause.ToUpper().Contains("ON DUPLICATE KEY UPDATE"))
                    builder.Append("ON DUPLICATE KEY UPDATE ");

                builder.Append(duplicateKeyClause);
            }

            var result = builder.ToString();
            return result;
        }

        string BuildMultiCreateOnDuplicateQueryString(string table, IReadOnlyList<string> columns, IReadOnlyList<List<object>> multiValueLists, string duplicateKeyClause)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("INSERT INTO `{0}` (", table);
            var minColumnCount = Math.Min(columns.Count, multiValueLists[0].Count);
            for (var i = 0; i < minColumnCount; ++i)
                builder.AppendFormat("`{0}`{1}", columns[i], i == minColumnCount - 1 ? ") " : ",");

            builder.Append("VALUES");

            for (var i = 0; i < multiValueLists.Count; ++i)
            {
                var valueList = multiValueLists[i];

                if (valueList == null || valueList.Count <= 0)
                    continue;

                builder.Append("(");

                for (var j = 0; j < minColumnCount; ++j)
                    builder.AppendFormat(@"'{0}'{1}", valueList[j],
                        j == minColumnCount - 1 ? ")" : ",");

                builder.Append(i == multiValueLists.Count - 1 ? " " : ",");
            }

            if (!string.IsNullOrEmpty(duplicateKeyClause))
            {
                if (!duplicateKeyClause.ToUpper().Contains("ON DUPLICATE KEY UPDATE"))
                    builder.Append("ON DUPLICATE KEY UPDATE ");

                builder.Append(duplicateKeyClause);
            }

            var result = builder.ToString();
            return result;
        }

        string BuildRequestQueryString(string table, IReadOnlyList<string> columns, string conditions)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ");

            if (columns == null || columns.Count == 0)
                builder.Append("* ");
            else
            {
                for (var i = 0; i < columns.Count; ++i)
                    builder.AppendFormat("`{0}`{1}", columns[i], i == columns.Count - 1 ? " " : ",");
            }

            builder.AppendFormat("FROM `{0}` ", table);

            if (!string.IsNullOrEmpty(conditions))
            {
                if (!conditions.ToUpper().Contains("WHERE"))
                    builder.Append("WHERE ");

                builder.Append(conditions);
            }

            var result = builder.ToString();
            return result;
        }

        string BuildRequestJoinQueryString(IReadOnlyList<string> tables, IReadOnlyList<string> columns, string conditions)
        {
            var builder = new StringBuilder();

            // SELECT
            builder.Append("SELECT ");
            if (columns == null || columns.Count == 0)
                builder.Append("* ");
            else
            {
                for (var i = 0; i < columns.Count; ++i)
                    builder.AppendFormat("{0}{1}", columns[i], i == columns.Count - 1 ? " " : ",");
            }

            // FROM
            builder.Append("FROM ");
            for (var i = 0; i < tables.Count; ++i)
            {
                builder.AppendFormat("{0}{1}", tables[i], i == tables.Count - 1 ? " " : ",");
            }

            // WHERE
            if (!string.IsNullOrEmpty(conditions))
            {
                if (!conditions.ToUpper().Contains("WHERE"))
                    builder.Append("WHERE ");

                builder.Append(conditions);
            }

            var result = builder.ToString();
            return result;
        }

        string BuildUpdateQueryString(string table, IReadOnlyList<string> columns, IReadOnlyList<object> values, string conditions)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("UPDATE `{0}` SET ", table);
            var bound = Math.Min(columns.Count, values.Count);

            for (var i = 0; i < bound; ++i)
                builder.AppendFormat(@"`{0}` = '{1}'{2}", columns[i], values[i], i == bound - 1 ? " " : ",");

            if (!string.IsNullOrEmpty(conditions))
            {
                if (!conditions.ToUpper().Contains("WHERE"))
                    builder.Append("WHERE ");

                builder.Append(conditions);
            }

            var result = builder.ToString();
            return result;
        }

        string BuildDeleteQueryString(string table, string conditions)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("DELETE FROM `{0}` ", table);

            if (!string.IsNullOrEmpty(conditions))
            {
                if (!conditions.ToUpper().Contains("WHERE"))
                    builder.Append("WHERE ");

                builder.Append(conditions);
            }

            var result = builder.ToString();
            return result;
        }

        string BuildCountQueryString(string table, string conditions)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("SELECT COUNT(*) FROM {0} ", table);

            if (!string.IsNullOrEmpty(conditions))
            {
                if (!conditions.ToUpper().Contains("WHERE"))
                    builder.Append("WHERE ");

                builder.Append(conditions);
            }

            var result = builder.ToString();
            return result;
        }

        string BuildWhereClauseString(params KeyValuePair<string, object>[] whereClauses)
        {
            if (whereClauses.Length <= 0) 
                return string.Empty;

            var builder = new StringBuilder();
            builder.Append("WHERE ");
            builder.Append(string.Join(" AND ",
                whereClauses.Select(pair => string.Format("{0}=@{0}", pair.Key))));

            var result = builder.ToString();
            return result;
        }

        #endregion

        #region Execute command in sync mode

        bool ExecuteNonQueryCommand(string queryString, params KeyValuePair<string, object>[] parameters)
        {
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var cmd = CreateMySqlCommand(queryString, connection, parameters);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
                return false;
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                return false;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        int ExecuteScalarCommand(string queryString, params KeyValuePair<string, object>[] parameters)
        {
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var cmd = CreateMySqlCommand(queryString, connection, parameters);
                var count = cmd.ExecuteScalar();
                return Convert.ToInt32(count);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
                return 0;
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                return 0;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        List<Dictionary<string, object>> ExecuteReaderCommand(string queryString,
            params KeyValuePair<string, object>[] conditions)
        {
            var result = new List<Dictionary<string, object>>();
            MySqlDataReader dataReader = null;
            MySqlConnection connection = null;

            try
            {
                connection = Connect();
                var requestCommand = CreateMySqlCommand(queryString, connection, conditions);
                dataReader = requestCommand.ExecuteReader();
                return FetchSqlResultToList(dataReader);
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

        #endregion

        #region Execute command in async mode

        async Task<bool> ExecuteNonQueryCommandAsync(string queryString, params KeyValuePair<string, object>[] parameters)
        {
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync().ConfigureAwait(false);
                var cmd = CreateMySqlCommand(queryString, connection, parameters);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                return true;
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
                return false;
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                return false;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        async Task<int> ExecuteScalarCommandAsync(string queryString, params KeyValuePair<string, object>[] parameters)
        {
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync().ConfigureAwait(false);
                var cmd = CreateMySqlCommand(queryString, connection, parameters);
                var count = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                return Convert.ToInt32(count);
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("query \"{0}\" get mysql exception {1} with number {2}", queryString, mySqlException, mySqlException.Number);
                return 0;
            }
            catch (Exception exception)
            {
                GLogger.Fatal("query \"{0}\" get exception {1}", queryString, exception);
                return 0;
            }
            finally
            {
                Disconnect(connection);
            }
        }

        async Task<List<Dictionary<string, object>>> ExecuteReaderCommandAsync(string queryString,
            params KeyValuePair<string, object>[] conditions)
        {
            var result = new List<Dictionary<string, object>>();
            DbDataReader dataReader = null;
            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync().ConfigureAwait(false);
                var cmd = CreateMySqlCommand(queryString, connection, conditions);
                dataReader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                return FetchSqlResultToList(dataReader);
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

        #endregion

        public TizMySql(EventArgs configArgs)
        {
            if (configArgs == null)
                throw new ArgumentNullException("configArgs");

            var config = (DatabaseConfig) configArgs;

            if (config == null)
                throw new InvalidCastException("configArgs");

            _connectionString = string.Format("server={0};port={1};uid={2};pwd={3};database={4};Charset=utf8;ConvertZeroDateTime=true;{5}",
                    config.HostName, config.Port, config.UserName, config.Password, config.DataBase, config.Option);
        }

        async Task<MySqlConnection> ConnectAsync()
        {
            try
            {
                var mySqlConnection = new MySqlConnection(_connectionString);
                await mySqlConnection.OpenAsync().ConfigureAwait(false);
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

        static MySqlCommand CreateMySqlCommand(string query, MySqlConnection connection,
            params KeyValuePair<string, object>[] parameters)
        {
            var cmd = new MySqlCommand(query, connection);

            if (parameters.Length <= 0) 
                return cmd;

            foreach (var parameter in parameters)
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

        public async Task<int> ExecuteScalarAsync(string query)
        {
            return await ExecuteScalarCommandAsync(query).ConfigureAwait(false);
        }

        public async Task<bool> ExecuteNonQueryAsync(string query)
        {
            return await ExecuteNonQueryCommandAsync(query).ConfigureAwait(false);
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query)
        {
            return await ExecuteReaderCommandAsync(query).ConfigureAwait(false);
        }

        public async Task<JArray> ExecuteQueryJsonAsync(string query)
        {
            return JArray.FromObject(await ExecuteQueryAsync(query).ConfigureAwait(false));
        }

        public async Task<bool> ExecuteNonQueryStoredProcedureAsync(IMySqlStoredProcedureHelper helper)
        {
            if (helper == null)
                throw new NullReferenceException("helper can't be null");

            MySqlConnection connection = null;

            try
            {
                connection = await ConnectAsync().ConfigureAwait(false);
                var cmd = CreateStoredProcedureCommand(connection, helper);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                return true;
            }
            catch (MySqlException mySqlException)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get mysql exception {1} with number {2}", helper.Function, mySqlException, mySqlException.Number);
                return false;
            }
            catch (Exception exception)
            {
                GLogger.Fatal("execute stored procedure \"{0}\" get exception {1}", helper.Function, exception);
                return false;
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
                connection = await ConnectAsync().ConfigureAwait(false);
                var cmd = CreateStoredProcedureCommand(connection, helper);
                dataReader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
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
            return JArray.FromObject(await ExecuteQueryStoredProcedureAsync(helper).ConfigureAwait(false));
        }

        public async Task<bool> CreateAsync(string table, List<string> columns, List<object> values)
        {
            return await CreateOnDuplicateAsync(table, columns, values).ConfigureAwait(false);
        }

        public async Task<bool> CreateOnDuplicateAsync(string table, List<string> columns, List<object> values,
            string duplicateKeyClause = "")
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null)
                return false;

            var queryString = BuildCreateOnDuplicateQueryString(table, columns, values, duplicateKeyClause);
            return await ExecuteNonQueryCommandAsync(queryString).ConfigureAwait(false);
        }

        public bool Create(string table, List<string> columns, List<object> values)
        {
            return CreateOnDuplicate(table, columns, values);
        }

        public bool CreateOnDuplicate(string table, List<string> columns, List<object> values, string duplicateKeyClause = "")
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null)
                return false;

            var queryString = BuildCreateOnDuplicateQueryString(table, columns, values, duplicateKeyClause);
            return ExecuteNonQueryCommand(queryString);
        }

        public async Task<bool> MultiCreateAsync(string table, List<string> columns,
            List<List<object>> multiValueLists)
        {
            return await MultiCreateOnDuplicateAsync(table, columns, multiValueLists).ConfigureAwait(false);
        }

        public async Task<bool> MultiCreateOnDuplicateAsync(string table, List<string> columns,
            List<List<object>> multiValueLists, string duplicateKeyClause = "")
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || multiValueLists == null || columns.Count == 0 || multiValueLists.Count == 0)
                return false;

            var queryString = BuildMultiCreateOnDuplicateQueryString(table, columns, multiValueLists, duplicateKeyClause);
            return await ExecuteNonQueryCommandAsync(queryString).ConfigureAwait(false);
        }

        public bool MultiCreate(string table, List<string> columns, List<List<object>> multiValueLists)
        {
            return MultiCreateOnDuplicate(table, columns, multiValueLists);
        }

        public bool MultiCreateOnDuplicate(string table, List<string> columns, List<List<object>> multiValueLists, string duplicateKeyClause = "")
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || multiValueLists == null || columns.Count == 0 || multiValueLists.Count == 0)
                return false;

            var queryString = BuildMultiCreateOnDuplicateQueryString(table, columns, multiValueLists, duplicateKeyClause);
            return ExecuteNonQueryCommand(queryString);
        }

        public async Task<List<Dictionary<string, object>>> RequestJoinAsync(List<string> tables, 
            List<string> columns, params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new Exception("Connection doesn't establish yet.");
            }

            var queryString = BuildRequestJoinQueryString(tables, columns, BuildWhereClauseString(conditions));
            return await ExecuteReaderCommandAsync(queryString, conditions).ConfigureAwait(false);
        }

        /// <summary>
        /// Request data(in json format returned) async with "WHERE `key`='value'" condition.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public async Task<JArray> RequestJsonAsync(string table, List<string> columns,
            params KeyValuePair<string, object>[] conditions)
        {
            return JArray.FromObject(await RequestAsync(table, columns, conditions).ConfigureAwait(false));
        }

        public async Task<JArray> RequestJsonAsync(string table, List<string> columns, string conditions)
        {
            return JArray.FromObject(await RequestAsync(table, columns, conditions).ConfigureAwait(false));
        }

        /// <summary>
        /// Request data async with "WHERE `key`='value'" condition.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> RequestAsync(string table, List<string> columns, params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildRequestQueryString(table, columns, BuildWhereClauseString(conditions));
            return await ExecuteReaderCommandAsync(queryString, conditions).ConfigureAwait(false);
        }

        public async Task<List<Dictionary<string, object>>> RequestAsync(string table, List<string> columns, string conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildRequestQueryString(table, columns, conditions);
            return await ExecuteReaderCommandAsync(queryString).ConfigureAwait(false);
        }

        /// <summary>
        /// Request data with "WHERE `key`='value'" condition.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Request(string table, List<string> columns,
            params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildRequestQueryString(table, columns, BuildWhereClauseString(conditions));
            return ExecuteReaderCommand(queryString, conditions);
        }

        /// <summary>
        /// Update data async with single condition with "WHERE `key`='value'".
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(string table, List<string> columns, List<object> values,
            params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return false;

            var queryString = BuildUpdateQueryString(table, columns, values, BuildWhereClauseString(conditions));
            return await ExecuteNonQueryCommandAsync(queryString, conditions).ConfigureAwait(false);
        }

        public async Task<bool> UpdateAsync(string table, List<string> columns, List<object> values, string conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return false;

            var queryString = BuildUpdateQueryString(table, columns, values, conditions);
            return await ExecuteNonQueryCommandAsync(queryString).ConfigureAwait(false);
        }

        /// <summary>
        /// Update data with conditions with multiple "WHERE `key`='value'".
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <param name="conditions"></param>
        public bool Update(string table, List<string> columns, List<object> values,
            params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return false;

            var queryString = BuildUpdateQueryString(table, columns, values, BuildWhereClauseString(conditions));
            return ExecuteNonQueryCommand(queryString, conditions);
        }

        public bool Update(string table, List<string> columns, List<object> values,
            string conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return false;

            var queryString = BuildUpdateQueryString(table, columns, values, conditions);
            return ExecuteNonQueryCommand(queryString);
        }

        /// <summary>
        /// Delete data async with condition(s) to create a `WHERE key`='value' query.
        /// If condition(s) is not set, DeleteAsync will return false to avoid delete all unintentionally.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string table, params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (conditions.Length == 0)
                return false;

            var queryString = BuildDeleteQueryString(table, BuildWhereClauseString(conditions));
            return await ExecuteNonQueryCommandAsync(queryString, conditions).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete data async with condition string passed from application.
        /// If condition string is not set, DeleteAsync will return false to avoid delete all unintentionally.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string table, string conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (string.IsNullOrEmpty(conditions))
                return false;

            var queryString = BuildDeleteQueryString(table, conditions);
            return await ExecuteNonQueryCommandAsync(queryString).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete data with condition(s) to create a `WHERE key`='value' query.
        /// If condition(s) is not set, Delete will return false to avoid delete all unintentionally.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public bool Delete(string table, params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (conditions.Length == 0)
                return false;

            var queryString = BuildDeleteQueryString(table, BuildWhereClauseString(conditions));
            return ExecuteNonQueryCommand(queryString, conditions);
        }

        public bool Delete(string table, string conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            if (string.IsNullOrEmpty(conditions))
                return false;

            var queryString = BuildDeleteQueryString(table, conditions);
            return ExecuteNonQueryCommand(queryString);
        }

        public async Task<int> CountAsync(string table, params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildCountQueryString(table, BuildWhereClauseString(conditions));
            return await ExecuteScalarCommandAsync(queryString, conditions).ConfigureAwait(false);
        }

        public async Task<int> CountAsync(string table, string conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildCountQueryString(table, conditions);
            return await ExecuteScalarCommandAsync(queryString).ConfigureAwait(false);
        }

        public int Count(string table, params KeyValuePair<string, object>[] conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildCountQueryString(table, BuildWhereClauseString(conditions));
            return ExecuteScalarCommand(queryString, conditions);
        }

        public int Count(string table, string conditions)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection doesn't establish yet.");

            var queryString = BuildCountQueryString(table, conditions);
            return ExecuteScalarCommand(queryString);
        }

        public static string CreateDuplicateOption(IReadOnlyList<string> columns)
        {
            if (columns.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();
            builder.Append("ON DUPLICATE KEY UPDATE ");
            builder.Append(string.Join(", ", columns.Select(col => string.Format("`{0}`=VALUES(`{0}`)", col))));

            var result = builder.ToString();
            return result;
        }
    }
}