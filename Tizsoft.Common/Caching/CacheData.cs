﻿using System;

namespace Tizsoft.Caching
{
    class CacheData<T> where T : class
    {
        public T CachedObject { get; private set; }
        public long Time { get; private set; }
        public uint Count { get; private set; }

        public CacheData(T obj)
        {
            UpdateCachedObject(obj);
            Time = DateTime.UtcNow.Ticks;
            Count = 0;
        }

        public void UpdateCachedObject(T obj)
        {
            CachedObject = obj;
        }

        public void UpdateCount()
        {
            Count++;
            Time = DateTime.UtcNow.Ticks;
        }
    }
}
