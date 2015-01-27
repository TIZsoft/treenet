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
            if (!_cacheObjects.ContainsKey(key))
            {
                _cacheObjects.Add(key, cacheObj);
            }
            else
            {
                _cacheObjects[key] = cacheObj;
            }
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

        public bool TryGet(string key, out T cachedObject)
        {
            CacheData<T> data;
            if (_cacheObjects.TryGetValue(key, out data))
            {
                data.UpdateCount();
                cachedObject = data.CachedObject;
                return true;
            }

            cachedObject = default(T);
            return false;
        }

        public void Update(string key, T obj)
        {
            if (!_cacheObjects.ContainsKey(key))
            {
                return;
            }

            _cacheObjects[key].UpdateCachedObject(obj);
        }

        public void Remove(string key)
        {
            if (!_cacheObjects.ContainsKey(key))
            {
                return;
            }

            _cacheObjects.Remove(key);
        }

        public bool Contains(string key)
        {
            return _cacheObjects.ContainsKey(key);
        }
    }
}
