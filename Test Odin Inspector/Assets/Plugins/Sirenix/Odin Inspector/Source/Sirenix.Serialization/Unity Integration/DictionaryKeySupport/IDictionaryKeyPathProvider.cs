//-----------------------------------------------------------------------
// <copyright file="IDictionaryKeyPathProvider.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public interface IDictionaryKeyPathProvider
    {
        /// <summary>
        /// Gets the provider identifier.
        /// </summary>
        string ProviderID { get; }

        /// <summary>
        /// Gets the path string from key.
        /// </summary>
        /// <param name="key">The key.</param>
        string GetPathStringFromKey(object key);

        /// <summary>
        /// Gets the key from path string.
        /// </summary>
        /// <param name="pathStr">The path string.</param>
        object GetKeyFromPathString(string pathStr);

        /// <summary>
        /// Compares the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        int Compare(object x, object y);
    }

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public interface IDictionaryKeyPathProvider<T> : IDictionaryKeyPathProvider
    {
        /// <summary>
        /// Gets the path string from key.
        /// </summary>
        /// <param name="key">The key.</param>
        string GetPathStringFromKey(T key);

        /// <summary>
        /// Gets the key from path string.
        /// </summary>
        /// <param name="pathStr">The path string.</param>
        new T GetKeyFromPathString(string pathStr);

        /// <summary>
        /// Compares the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        int Compare(T x, T y);
    }
}