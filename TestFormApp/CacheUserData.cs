using Tizsoft.Caching.Cache;

namespace TestFormApp
{
    public class CacheUserData
    {
        readonly Cache<TestUserData> _cache;

        public CacheUserData()
        {
            _cache = new Cache<TestUserData>();
        }

        public TestUserData Get(string guid)
        {
            TestUserData data;
            if (_cache.TryGet(guid, out data))
            {
                return data;
            }
            return null;
        }

        public void Add(TestUserData data)
        {
            _cache.Add(data.guid, data);
        }

        public TestUserData Update(TestUserData data)
        {
            if (_cache.Contains(data.guid))
            {
                _cache.Update(data.guid, data);
                return _cache.Get(data.guid);
            }

            return null;
        }

        public void Remove(string guid)
        {
            _cache.Remove(guid);
        }
    }
}
