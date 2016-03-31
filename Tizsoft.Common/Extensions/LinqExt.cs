using System;
using System.Collections.Generic;
using System.Linq;

namespace Tizsoft.Extensions
{
    public static class LinqExt
    {
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return from element in source
                let elementValue = keySelector(element)
                where seenKeys.Add(elementValue)
                select element;
        }
    }
}