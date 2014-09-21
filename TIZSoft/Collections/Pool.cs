using System;
using System.Collections.Concurrent;

namespace Tizsoft.Collections
{
    /// <summary>
    /// Represents a generic and thread-safe pool of objects.
    /// </summary>
    /// <typeparam name="T">Type of item.</typeparam>
    public class Pool<T> : IDisposable
    {
        readonly ConcurrentBag<T> _objects;
        readonly Func<T> _objectGenerator;

        bool _isDisposed;

        public int Count
        {
            get { return _objects.Count; }
        }

        public bool IsEmpty
        {
            get { return _objects.IsEmpty; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{T}"/> class.
        /// </summary>
        /// <param name="objectGenerator">A function of object generation.</param>
        public Pool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
            {
                throw new ArgumentNullException("objectGenerator");
            }

            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
        }

        ~Pool()
        {
            Dispose(false);
        }

        /// <summary>
        /// Removes and returns an object from the <see cref="Pool{T}"/>.<br />
        /// If the pool is empty, then create a new instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <returns>An object.</returns>
        public T Get()
        {
            T item;
            if (_objects.TryTake(out item))
            {
                return item;
            }
            
            return _objectGenerator();
        }

        /// <summary>
        /// Puts an object into the pool.
        /// </summary>
        /// <param name="item"></param>
        public void Put(T item)
        {
            _objects.Add(item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManaged)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposeManaged)
            {
                foreach (var item in _objects)
                {
                    var disposable = item as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }

            _isDisposed = true;
        }
    }
}
