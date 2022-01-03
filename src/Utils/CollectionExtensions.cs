using System;
using System.Collections.Generic;
using System.Linq;

namespace NGroot
{
    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
            => collection == null || !collection.Any();

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
            => collection == null || !collection.Any(predicate);

        public static IEnumerable<T> NotNullDistinct<T>(this IEnumerable<T> collection)
            => collection.Where(i => i != null).Distinct();

        public static TSource? FirstOrDefault<TSource, TObject>(this IEnumerable<TSource> collection, TObject extraElement, Func<TSource, TObject, bool> predicate)
        {
            foreach (TSource element in collection)
            {
                if (predicate(element, extraElement)) return element;
            }
            return default(TSource);
        }
    }
}