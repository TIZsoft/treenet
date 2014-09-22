using System;
using System.Collections.Generic;

namespace Tizsoft.Collections
{
    public class SimpleObjPool<T> where T : IDisposable
    {
        readonly Stack<T> _pool;

        /// <summary>
        /// Initializes the object pool to the specified size.<br />
        /// </summary>
        /// <param name="capacity">The maximum number of objects the pool can hold.</param>
        public SimpleObjPool(int capacity)
        {
            _pool = new Stack<T>(capacity);
        }

        ~SimpleObjPool()
        {
            lock (_pool)
            {
                while (_pool.Count > 0)
                {
                    IDisposable disposableObj = _pool.Pop();

                    if (disposableObj != null)
                        disposableObj.Dispose();
                }
            }
        }

        /// <summary>
        /// Add an object instance to the pool.
        /// </summary>
        /// <param name="item">The T instance to add to the pool.</param>
        public void Push(T item)
        {
            if (ReferenceEquals(item, null)) { throw new ArgumentNullException("item", "Items added to a SimpleObjPool cannot be null"); }
            lock (_pool)
            {
                if (!_pool.Contains(item))
                    _pool.Push(item);
            }
        }

        /// <summary>
        /// Removes a object instance from the pool and returns the object removed from the pool.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        /// <summary>
        /// The number of object instances in the pool.
        /// </summary>
        public int Count
        {
            get { return _pool.Count; }
        }
    }
}
