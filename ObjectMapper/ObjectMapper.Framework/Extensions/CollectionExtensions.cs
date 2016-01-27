using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Copies elements from source enumerable to list. List is cleared before new elements are copied over.
        /// </summary>
        /// <typeparam name="T">Type of objects in collections.</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyFrom<T>(this List<T> target, IEnumerable<T> source)
        {
            if (target == null || source == null) return;
            target.Clear();
            target.AddRange(source);
        }

        /// <summary>
        /// Copies elements from source enumerable to collection. Collection is cleared before new elements are copied over.
        /// </summary>
        /// <typeparam name="T">Type of objects in collections.</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyFrom<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null || source == null || target.IsReadOnly) return;
            target.Clear();
            foreach (var element in source)
            {
                target.Add(element);
            }
        }
    }
}
