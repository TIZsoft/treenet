#define CONCURRENT
#if !CONCURRENT
    #define USE_LOCK
#endif

using System;
#if CONCURRENT
using System.Collections.Concurrent;
#else
using System.Collections.Generic;
#endif

namespace Tizsoft.Collections
{
    /// <summary>
    /// Represents a simple thread-safe object pool.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be stored in pool.</typeparam>
    public class Pool<T> : IPool<T>
    {
        sealed class PoolObject : IPoolObject<T>
        {
            readonly T _value;
            readonly Pool<T> _pool;
            bool _isDisposed;

            public T Value
            {
                get
                {
                    if (_isDisposed)
                    {
                        throw new ObjectDisposedException("Pool object has been disposed.");
                    }

                    return _value;
                }
            }

            public PoolObject(T value, Pool<T> pool)
            {
                _value = value;
                _pool = pool;
            }

            ~PoolObject()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
                _pool.Release(_value);
            }
        }

#if CONCURRENT
        readonly ConcurrentBag<T> _objects;
#else
        readonly Queue<T> _objects;
#endif
        readonly Func<T> _objectGenerator;

        /// <summary>
        /// Gets the number of objects contained in the <see cref="Pool{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return _objects.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{T}"/> class.
        /// </summary>
        /// <param name="objectGenerator">A function of object generator.</param>
        public Pool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
            {
                throw new ArgumentNullException("objectGenerator");
            }

#if CONCURRENT
            _objects = new ConcurrentBag<T>();
#else
            _objects = new Queue<T>();
#endif
            _objectGenerator = objectGenerator;
        }

        /// <summary>
        /// Acquire object from pool.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>T.</returns>
        public IPoolObject<T> Acquire()
        {
            T item;

#if CONCURRENT
            item = _objects.TryTake(out item)
                ? item
                : _objectGenerator();
#else
#if USE_LOCK
            lock (_objects)
            {
#endif
                if (_objects.Count > 0)
                {
                    item = _objects.Dequeue();
                }
                else
                {
                    item = _objectGenerator();
                }
#if USE_LOCK
            }
#endif
#endif
            return new PoolObject(item, this);
        }

        void Release(T item)
        {
#if CONCURRENT
            _objects.Add(item);
#else
#if USE_LOCK
            lock (_objects)
            {
#endif
                _objects.Enqueue(item);
#if USE_LOCK
            }
#endif
#endif
        }
    }
}
