using System;
using System.Collections.Generic;
using System.Text;
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
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

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

        public MySqlDataReader Request(string table, List<string> columns, string whereClause)
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
                    _queryBuilder.AppendFormat("{0}{1}", columns[i], i == columns.Count - 1 ? " " : ",");
            }

            _queryBuilder.AppendFormat("FROM {0} ", table);

            if (!string.IsNullOrEmpty(whereClause))
                _queryBuilder.AppendFormat("WHERE {0}", whereClause);

            var requestCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            return requestCommand.ExecuteReader();
        }

        public void Update(string table, List<string> columns, List<object> values, string whereClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

            if (columns == null || values == null || columns.Count == 0 || values.Count == 0)
                return;

            ResetQueryBuilder();
            _queryBuilder.AppendFormat("UPDATE {0} SET ", table);
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

            _queryBuilder.AppendFormat("DELETE FROM {0} WHERE {1}", table, whereClause);
            var deleteCommand = new MySqlCommand(_queryBuilder.ToString(), _mySqlConnection);
            deleteCommand.ExecuteNonQuery();
        }

        public int Count(string table, KeyValuePair<string, string> whereClause)
        {
            if (_mySqlConnection == null)
                throw new Exception("Connection doesn't establish yet.");

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
                GLogger.Fatal(exception);
            }

            return 0;
        }

        //public T GetUserData<T>(string guid)
        //{
        //    return GetUserDataByToken<T>(guid, AccountType.Guid);
        //}

        //public T GetUserData<T>(Guid guid)
        //{
        //    T userData;
        //    return HasUserData(GuidUtil.ToBase64(guid), AccountType.Guid, out userData) ? userData : default(T);
        //}

        //public bool HasUserData<T>(string account, AccountType type, out T userData)
        //{
        //    userData = default(T);

        //    if (Count(SchemaConst.AccountTable, new KeyValuePair<string, string>(AccountField(type), account)) == 0)
        //        return false;

        //    userData = GetUserDataByToken<T>(account, type);
        //    return true;
        //}

        //public T CreateNewUser<T>(string guid)
        //{
        //    return CreateNewUser<T>(guid, AccountType.Guid);
        //}

        //public T CreateNewUser<T>(Guid guid)
        //{
        //    return CreateNewUser<T>(GuidUtil.ToBase64(guid), AccountType.Guid);
        //}

        //public T CreateNewUser<T>(string account, AccountType type)
        //{
        //    if (_mySqlConnection == null)
        //        throw new Exception("not connect yet!");

        //    var fieldColumns = new List<string>() { AccountField(type) };
        //    var valueColumns = new List<object>() { account };

        //    if (type != AccountType.Guid)
        //    {
        //        fieldColumns.Add(SchemaConst.GuidField);
        //        valueColumns.Add(GuidUtil.ToBase64(GuidUtil.New()));
        //    }

        //    Create(SchemaConst.AccountTable, fieldColumns, valueColumns);
        //    return GetUserDataByToken<T>(account, type);
        //}

        //string AccountField(AccountType type)
        //{
        //    switch (type)
        //    {
        //        case AccountType.Facebook:
        //            return SchemaConst.FbIdField;

        //        default:
        //            return SchemaConst.GuidField;
        //    }
        //}

        //public T GetUserDataByToken<T>(string account, AccountType type)
        //{
        //    if (_mySqlConnection == null)
        //        throw new Exception("not connect yet!");

        //    if (Count(SchemaConst.AccountTable, new KeyValuePair<string, string>(AccountField(type), account)) == 0)
        //        return default(T);

        //    string json = string.Empty;
        //    MySqlDataReader dataReader = null;

        //    try
        //    {
        //        dataReader = Request(SchemaConst.AccountTable, null,
        //            string.Format(@"`{0}`='{1}'", AccountField(type), account));

        //        dataReader.Read();
        //        var dictionary = SchemaConst.AccountFields.ToDictionary(accountField => accountField, accountField => dataReader[accountField]);
        //        //var dictionary = new Dictionary<string, object>();
        //        //foreach (var accountField in SchemaConst.AccountFields)
        //        //    dictionary.Add(accountField, dataReader[accountField]);

        //        json = JsonConvert.SerializeObject(dictionary);
        //    }
        //    catch (Exception exception)
        //    {
        //        GLogger.Fatal(exception);
        //        throw;
        //    }
        //    finally
        //    {
        //        if (dataReader != null)
        //            dataReader.Close();
        //    }

        //    return JsonConvert.DeserializeObject<T>(json);
        //}

        //public void WriteUserData(object userData)
        //{
        //    try
        //    {
        //        string jsonStr = JsonConvert.SerializeObject(userData);
        //        var userdataJObject = JObject.Parse(jsonStr);
        //        var values = new List<object>(SchemaConst.AccountFields.Count);

        //        foreach (var accountField in SchemaConst.AccountFields)
        //            values.Add(userdataJObject[accountField]);

        //        Update(SchemaConst.AccountTable, SchemaConst.AccountFields, values, string.Format("`{0}`='{1}'", SchemaConst.GuidField, userdataJObject[SchemaConst.GuidField]));
        //    }
        //    catch (Exception exception)
        //    {
        //        throw exception;
        //    }
        //}
    }
}