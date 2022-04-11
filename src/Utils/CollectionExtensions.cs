
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

        public static void AddRange<TSource>(this ICollection<TSource> destination, IEnumerable<TSource> source)
        {
            List<TSource> list = destination.ToList();

            if (list != null)
            {
                list.AddRange(source);
            }
            else
            {
                foreach (TSource item in source)
                {
                    destination.Add(item);
                }
            }
        }
    }
}