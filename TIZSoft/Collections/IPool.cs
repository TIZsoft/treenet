namespace Tizsoft.Collections
{
    /// <summary>
    /// Represents an abstract object pool interface.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be stored in pool.</typeparam>
    public interface IPool<out T>
    {
        /// <summary>
        /// Acquire object from pool.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>T.</returns>
        IPoolObject<T> Acquire();
    }
}
