// See http://dzmitryhuba.blogspot.tw/2011/05/concurrent-object-pool.html

using System;

namespace Tizsoft.Collections.Concurrent
{
    /// <summary>
    /// Represents disposable wrapper around pooled object
    /// that is used to return object back to the pool.
    /// </summary>
    /// <typeparam name="T">Type of item.</typeparam>
    public sealed class PoolObject<T> : IDisposable
    {
        // TODO: Implements IDisposable is a little strange in this scenario.

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
}