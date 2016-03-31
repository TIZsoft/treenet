using System;

namespace Tizsoft.Collections
{
    /// <summary>
    /// Represents a wrapper of object for <see cref="IPool{T}"/>.
    /// </summary>
    /// <remarks>
    /// Invoke <see cref="IDisposable.Dispose"/> method will release the object.
    /// </remarks>
    /// <typeparam name="T">The type of the object.</typeparam>
    public interface IPoolObject<out T> : IDisposable
    {
        /// <summary>
        /// Gets the object. (readonly)
        /// </summary>
        T Value { get; }
    }
}