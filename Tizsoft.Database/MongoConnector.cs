using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Tizsoft.Log;

namespace Tizsoft.Database
{
    public class MongoConnector
    {
        MongoDatabase _mongoDatabase;

        public MongoConnector(EventArgs args)
        {
            var config = (DatabaseConfig) args;

            if (config == null)
                throw new InvalidCastException("args");

            var connectionString = string.Format("mongodb://{0}:{1}/{2}", config.HostName, config.Port, config.DataBase);

            try
            {
                var mongoClient = new MongoClient(connectionString);
                _mongoDatabase = mongoClient.GetServer().GetDatabase(config.DataBase);
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
                throw;
            }
        }

        public void Create<T>(string collectionName, T data)
        {
            try
            {
                var collections = _mongoDatabase.GetCollection<T>(collectionName);
                collections.Insert(data);
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
                throw;
            }
        }

        public void Create<T>(string collectionName, List<T> dataList)
        {
            try
            {
                var collections = _mongoDatabase.GetCollection<T>(collectionName);
                collections.InsertBatch(dataList);
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
                throw;
            }
        }

        public List<T> Find<T>(string collectionName, IMongoQuery query)
        {
            try
            {
                var result = new List<T>();
                var collections = _mongoDatabase.GetCollection<T>(collectionName);
                foreach (var data in collections.FindAs<T>(query))
                    result.Add(data);
                return result;
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
                throw;
            }
        }

        //public void Update<T>(string collectionName, T data)
        //{
        //    try
        //    {
        //        var collections = _mongoDatabase.GetCollection<T>(collectionName);
        //    }
        //    catch (Exception exception)
        //    {
        //        GLogger.Error(exception);
        //        throw;
        //    }
        //}
    }
}
