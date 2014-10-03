using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tizsoft.Collections.Concurrent
{
    /// <summary>
    /// Represents a simple thread-safe object pool.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be stored in pool.</typeparam>
    public class ConcurrentPool<T> : IPool<T>
    {
        sealed class PoolObject : IPoolObject<T>
        {
            readonly T _value;
            readonly ConcurrentPool<T> _pool;
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

            public PoolObject(T value, ConcurrentPool<T> pool)
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

        // Do not use BlockingCollection to implement pool. It makes amazing performance issue.
        readonly IProducerConsumerCollection<T> _collection;
        readonly Func<T> _objectGenerator;

        /// <summary>
        /// Gets the number of objects contained in the <see cref="ConcurrentPool{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return _collection.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentPool{T}"/> class.
        /// </summary>
        /// <param name="objectGenerator">A function of object generator.</param>
        public ConcurrentPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
            {
                throw new ArgumentNullException("objectGenerator");
            }

            // Performance test results:
            // Queue > Bag >> Stack (Testing failed)
            _collection = new ConcurrentQueue<T>();
            _objectGenerator = objectGenerator;
        }

        /// <summary>
        /// Acquire object from pool.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>T.</returns>
        public IPoolObject<T> Acquire()
        {
            T item;

            if (_collection.TryTake(out item))
            {
                return new PoolObject(item, this);
            }

            return new PoolObject(_objectGenerator(), this);
        }

        void Release(T item)
        {
            var spin = new SpinWait();

            while (true)
            {
                if (_collection.TryAdd(item))
                {
                    break;
                }

                spin.SpinOnce();
            }
        }
    }
}
