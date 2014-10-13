using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tizsoft;
using Tizsoft.Database;
using Tizsoft.Log;

namespace TestFormApp.Database
{
    public class DatabaseQuery
    {
        DatabaseConnector _dbConnector;

        public DatabaseQuery(EventArgs args)
        {
            _dbConnector = new DatabaseConnector();
            _dbConnector.Connect(args);
        }

        public T GetUserData<T>(string guid)
        {
            return GetUserDataByToken<T>(guid, AccountType.Guid);
        }

        public T GetUserData<T>(Guid guid)
        {
            T userData;
            return HasUserData(GuidUtil.ToBase64(guid), AccountType.Guid, out userData) ? userData : default(T);
        }

        public bool HasUserData<T>(string account, AccountType type, out T userData)
        {
            userData = default(T);

            if (_dbConnector.Count(SchemaConst.AccountTable, new KeyValuePair<string, string>(AccountField(type), account)) == 0)
                return false;

            userData = GetUserDataByToken<T>(account, type);
            return true;
        }

        public T CreateNewUser<T>(string guid)
        {
            return CreateNewUser<T>(guid, AccountType.Guid);
        }

        public T CreateNewUser<T>(Guid guid)
        {
            return CreateNewUser<T>(GuidUtil.ToBase64(guid), AccountType.Guid);
        }

        public T CreateNewUser<T>(string account, AccountType type)
        {
            var fieldColumns = new List<string>() { AccountField(type) };
            var valueColumns = new List<object>() { account };

            if (type != AccountType.Guid)
            {
                fieldColumns.Add(SchemaConst.GuidField);
                valueColumns.Add(GuidUtil.ToBase64(GuidUtil.New()));
            }

            _dbConnector.Create(SchemaConst.AccountTable, fieldColumns, valueColumns);
            return GetUserDataByToken<T>(account, type);
        }

        string AccountField(AccountType type)
        {
            switch (type)
            {
                case AccountType.Facebook:
                    return SchemaConst.FbIdField;

                default:
                    return SchemaConst.GuidField;
            }
        }

        public T GetUserDataByToken<T>(string account, AccountType type)
        {
            if (_dbConnector.Count(SchemaConst.AccountTable, new KeyValuePair<string, string>(AccountField(type), account)) == 0)
                return default(T);

            string json = string.Empty;
            MySqlDataReader dataReader = null;

            try
            {
                dataReader = _dbConnector.Request(SchemaConst.AccountTable, null,
                    string.Format(@"`{0}`='{1}'", AccountField(type), account));

                dataReader.Read();
                var dictionary = SchemaConst.AccountFields.ToDictionary(accountField => accountField, accountField => dataReader[accountField]);
                //var dictionary = new Dictionary<string, object>();
                //foreach (var accountField in SchemaConst.AccountFields)
                //    dictionary.Add(accountField, dataReader[accountField]);

                json = JsonConvert.SerializeObject(dictionary);
            }
            catch (Exception exception)
            {
                GLogger.Fatal(exception);
                throw;
            }
            finally
            {
                if (dataReader != null)
                    dataReader.Close();
            }

            return JsonConvert.DeserializeObject<T>(json);
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

                _dbConnector.Update(SchemaConst.AccountTable, SchemaConst.AccountFields, values, string.Format("`{0}`='{1}'", SchemaConst.GuidField, userdataJObject[SchemaConst.GuidField]));
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}