// See http://dzmitryhuba.blogspot.tw/2011/05/concurrent-object-pool.html

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Tizsoft.Collections.Concurrent
{
    /// <summary>
    /// Represents a thread-safe object pool.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be stored in pool.</typeparam>
    public class ConcurrentPool<T> : IPool<T>
    {
        /// <summary>
        /// Represents a chunk of pooled objects.
        /// </summary>
        sealed class Segment
        {
            readonly int _size;

            // Using stack to store pooled objects assuming that
            // hot objects (recently used) provide better locality.
            readonly Stack<T> _items;

            public Segment(int size, IEnumerable<T> items)
            {
                _size = size;
                _items = new Stack<T>(size);

                if (items == null)
                {
                    return;
                }

                foreach (var item in items)
                {
                    _items.Push(item);
                }
            }

            public bool TryGet(out T item)
            {
                // Pop item if any available.
                if (_items.Count > 0)
                {
                    item = _items.Pop();
                    return true;
                }

                item = default(T);
                return false;
            }

            public Segment Put(T item)
            {
                _items.Push(item);

                // If current segment size is still smaller than
                // twice of original size no need to split.
                if (_items.Count < _size + _size)
                {
                    return null;
                }

                // Otherwise split current segment to get it
                // pushed into global pool.
                var items = new T[_size];

                for (var i = 0; i != _size; ++i)
                {
                    items[i] = _items.Pop();
                }

                return new Segment(_size, items);
            }
        }

        /// <summary>
        /// Represents disposable wrapper around pooled object
        /// that is used to return object back to the pool.
        /// </summary>
        sealed class PoolObject : IPoolObject<T>
        {
            readonly T _value;
            readonly ConcurrentPool<T> _pool;
            bool _isDisposed;

            public T Value
            {
                get
                {
                    // Make sure value can't be obtained (though we can't guarantee that
                    // it is not used) anymore after it is released back to the pool.
                    if (_isDisposed)
                    {
                        throw new ObjectDisposedException("Pool object has been disposed.");
                    }

                    return _value;
                }
            }

            /// <summary>
            /// Get reference to the pool to return value to.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="pool"></param>
            public PoolObject(T value, ConcurrentPool<T> pool)
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

                // As we are disposing pooled object disposal basically
                // equivalent to returning the object back to the pool.
                _isDisposed = true;
                _pool.Release(_value);
            }
        }

        // Generator function that is used from potentionally multiple threads
        // to produce pooled objects and thus must be thread-safe.
        readonly Func<T> _objectGenerator;

        readonly int _segmentSize;
        
        // Thread local pool used without synchronization to reduce costs.
        readonly ThreadLocal<Segment> _localPool;

        // Global pool that is used once there is nothing or too much in local pool.
        readonly ConcurrentStack<Segment> _globalPool;

        /// <summary>
        /// TODO: Is this meaningful?
        /// Gets the number of objects contained in the <see cref="ConcurrentPool{T}"/>.
        /// </summary>
        public int Count { get; private set; }

        public ConcurrentPool(Func<T> objectGenerator, int segmentSize)
        {
            if (objectGenerator == null)
            {
                throw new ArgumentNullException("objectGenerator");
            }

            if (segmentSize < 0)
            {
                throw new ArgumentOutOfRangeException("segmentSize", segmentSize, "Segment size must > 0.");
            }

            _localPool = new ThreadLocal<Segment>();
            _globalPool = new ConcurrentStack<Segment>();
            _objectGenerator = objectGenerator;
            _segmentSize = segmentSize;
        }

        /// <summary>
        /// Acquire object from pool.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public IPoolObject<T> Acquire()
        {
            var local = _localPool.Value;
            T item;

            // Try to acquire pooled object from local pool
            // first to avoid synchronization penalties.
            if (local != null &&
                local.TryGet(out item))
            {
                --Count;
                return new PoolObject(item, this);
            }

            // If failed (either due to empty or not yet initialized
            // local pool) try to acquire segment that will be local
            // pool from global pool.
            if (!_globalPool.TryPop(out local))
            {
                // If failed create new segment using object generator.
                var items = new T[_segmentSize];

                for (var i = 0; i != _segmentSize; ++i)
                {
                    items[i] = _objectGenerator();
                }

                Count += _segmentSize;
                local = new Segment(_segmentSize, items);
            }

            // Eventually get object from local non-empty pool.
            _localPool.Value = local;
            local.TryGet(out item);
            --Count;
            return new PoolObject(item, this);
        }

        /// <summary>
        /// Releases pooled object back to the pool however it is accessible publicly
        /// to avoid multiple releases of the same object.
        /// </summary>
        /// <param name="poolObject"></param>
        void Release(T poolObject)
        {
            ++Count;

            var local = _localPool.Value;

            // Return object back to local pool first.
            var divided = local.Put(poolObject);

            // If local pool has grown beyond threshold
            // return extra segment back to global pool.
            if (divided != null)
            {
                _globalPool.Push(divided);
            }
        }
    }
}
