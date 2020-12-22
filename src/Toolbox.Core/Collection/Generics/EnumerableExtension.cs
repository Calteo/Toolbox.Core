using System;
using System.Collections.Generic;

namespace Toolbox.Collection.Generics
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Creates a foreach loop over every element.
        /// </summary>
        /// <typeparam name="T">Type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection itself.</param>
        /// <param name="action">Action to be performed.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}
