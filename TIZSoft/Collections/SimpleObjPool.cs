using System;
using System.Collections.Generic;

namespace Tizsoft.Collections
{
	public class SimpleObjPool<T> where T: IDisposable
	{
		Stack<T> _pool;

		// Initializes the object pool to the specified size
		//
		// The "capacity" parameter is the maximum number of 
		// objects the pool can hold
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

		// Add an object instance to the pool
		//
		//The "item" parameter is the T instance 
		// to add to the pool
		public void Push(T item)
		{
			if (item == null) { throw new ArgumentNullException("Items added to a SimpleObjPool cannot be null"); }
			lock (_pool)
			{
				if (!_pool.Contains(item))
					_pool.Push(item);
			}
		}

		// Removes a object instance from the pool
		// and returns the object removed from the pool
		public T Pop()
		{
			lock (_pool)
			{
				return _pool.Pop();
			}
		}

		// The number of object instances in the pool
		public int Count
		{
			get { return _pool.Count; }
		}
	}
}
