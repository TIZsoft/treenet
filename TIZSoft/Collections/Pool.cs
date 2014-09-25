using System;
using System.Collections.Concurrent;

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

        readonly ConcurrentBag<T> _objects;
        readonly Func<T> _objectGenerator;

        /// <summary>
        /// Gets the number of objects contained in the <see cref="Pool{T}"/>.
        /// </summary>
        public int Count
        {
            get { return _objects.Count; }
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

            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
        }

        /// <summary>
        /// Acquire object from pool.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>T.</returns>
        public IPoolObject<T> Acquire()
        {
            T item;
            return _objects.TryTake(out item)
                ? new PoolObject(item, this)
                : new PoolObject(_objectGenerator(), this);
        }

        void Release(T item)
        {
            _objects.Add(item);
        }
    }
}
