namespace Tizsoft.Collections
{
    /// <summary>
    /// Represents an abstract object pool interface.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be stored in pool.</typeparam>
    public interface IPool<out T>
    {
        /// <summary>
        /// Gets the number of objects contained in the <see cref="IPool{T}"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Acquire object from pool.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>T.</returns>
        IPoolObject<T> Acquire();
    }
}
