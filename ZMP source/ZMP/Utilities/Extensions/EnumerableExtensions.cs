namespace ZMP.Utilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ZMP.Decorate;

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> original, T next)
        {
            foreach (T item in original)
            {
                yield return item;
            }

            yield return next;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> original, params T[] next)
        {
            foreach (T item in original)
            {
                yield return item;
            }

            foreach (T item in next)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> original, IEnumerable<T> next)
        {
            foreach (T item in original)
            {
                yield return item;
            }

            foreach (T item in next)
            {
                yield return item;
            }
        }
    }
}
