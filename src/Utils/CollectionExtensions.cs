
namespace NGroot
{
    public static class CollectionExtensions
    {

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