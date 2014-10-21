using System;
using System.Collections.Concurrent;

namespace Tizsoft.Collections
{
    public class FixedSizeObjPool<T> where T : class 
    {
        readonly ConcurrentDictionary<int, T> _pool = new ConcurrentDictionary<int, T>();
        readonly ConcurrentStack<int> _hash = new ConcurrentStack<int>();
        readonly int _capacity;
        
        /// <summary>
        /// Initializes the object pool to the specified size.<br />
        /// </summary>
        /// <param name="capacity">The maximum number of objects the pool can hold.</param>
        public FixedSizeObjPool(int capacity)
        {
            _capacity = capacity;
        }

        /// <summary>
        /// Add an object instance to the pool.
        /// </summary>
        /// <param name="item">The T instance to add to the pool.</param>
        public void Push(T item)
        {
            if (ReferenceEquals(item, null))
            {
                throw new ArgumentNullException("item", "Items added to a FixedSizeObjPool cannot be null.");
            }

            if (_pool.Count > _capacity)
            {
                return;
            }
            
            var hash = item.GetHashCode();
            if (_pool.ContainsKey(hash))
            {
                return;
            }

            _hash.Push(hash);
            _pool[hash] = item;
        }

        /// <summary>
        /// Removes a object instance from the pool and returns the object removed from the pool.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            int lastHash;
            if (_hash.TryPop(out lastHash))
            {
                T item;
                _pool.TryRemove(lastHash, out item);
                return item;
            }

            throw new InvalidOperationException("Pop operation failure. The pool is already empty.");
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