using System.Collections.Generic;

namespace Tizsoft.Caching.Cache
{
    public class Cache<T> where T: class, new()
    {
        readonly Dictionary<string, CacheData<T>> _cacheObjects;

        public Cache()
        {
            _cacheObjects = new Dictionary<string, CacheData<T>>();
        }

        public void Add(string key, T obj)
        {
            var cacheObj = new CacheData<T>(obj);
            _cacheObjects.Add(key, cacheObj);
        }

        public T Get(string key)
        {
            if (!_cacheObjects.ContainsKey(key))
            {
                return default(T);
            }

            _cacheObjects[key].UpdateCount();
            return _cacheObjects[key].CachedObject;
        }

        public void Remove(string key)
        {
            if (!_cacheObjects.ContainsKey(key))
            {
                return;
            }

            _cacheObjects.Remove(key);
        }
    }
}
