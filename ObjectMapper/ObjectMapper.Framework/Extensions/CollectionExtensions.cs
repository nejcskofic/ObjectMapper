using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    public static class CollectionExtensions
    {
        public static void CopyFrom<T>(this List<T> target, IEnumerable<T> source)
        {
            if (target == null || source == null) return;
            target.Clear();
            target.AddRange(source);
        }

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
