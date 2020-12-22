using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Collection.Generics
{
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}
