using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    /// <summary>
    /// Provides mapping between objects of type T and U.
    /// </summary>
    /// <typeparam name="T">Type of first object.</typeparam>
    /// <typeparam name="U">Type of second object.</typeparam>
    public interface IObjectMapperAdapter<T, U>
    {
        /// <summary>
        /// Maps source object to target object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        void MapObject(T source, U target);

        /// <summary>
        /// Maps source object to target object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        void MapObject(U source, T target);
    }
}
