namespace ZMP.Utilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class LookupExtensions
    {     
        public static ILookup<TKey, TValue> Union<TKey, TValue>(this ILookup<TKey, TValue> original, ILookup<TKey, TValue> appendee)
        {
            var mergedEnumerable =
                from grouping in original.Union<IGrouping<TKey, TValue>>(appendee)
                from item in grouping
                select new KeyValuePair<TKey, TValue>(grouping.Key, item);

            return mergedEnumerable.ToLookup(p => p.Key, p => p.Value);
        }

        public static ILookup<TKey, TValue> Map<TKey, TValue>(this ILookup<TKey, TValue> original, Func<TKey, TValue, TValue> func)
        {
            var keyValPairs =
                from keyValues in original
                from value in keyValues
                select new KeyValuePair<TKey, TValue>(
                    keyValues.Key,
                    func(keyValues.Key, value));

            return keyValPairs.ToLookup(p => p.Key, p => p.Value);
        }
    }
}
