using TestFormApp.User;
using Tizsoft.Caching.Cache;

namespace TestFormApp
{
    public class CacheUserData
    {
        readonly Cache<UserData> _cache;

        public CacheUserData()
        {
            _cache = new Cache<UserData>();
        }

        public UserData Get(string guid)
        {
            UserData data;
            if (_cache.TryGet(guid, out data))
            {
                return data;
            }
            return null;
        }

        public void Add(UserData data)
        {
            _cache.Add(data.Guid, data);
        }

        public UserData Update(UserData data)
        {
            if (_cache.Contains(data.Guid))
            {
                _cache.Update(data.Guid, data);
                return _cache.Get(data.Guid);
            }

            return null;
        }

        public void Remove(string guid)
        {
            _cache.Remove(guid);
        }
    }
}
