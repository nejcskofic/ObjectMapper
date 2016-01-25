using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    public static class ObjectMapperExtensions
    {
        /// <summary>
        /// Creates new object of type T from mapper object.
        /// </summary>
        /// <typeparam name="T">Type of target</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <returns>New object with properties mapped from mapper.</returns>
        public static T CreateMappedObject<T>(this IObjectMapper<T> mapper) where T : new()
        {
            T newObject = new T();
            mapper.MapObject(newObject);
            return newObject;
        }

        /// <summary>
        /// Creates new object of type T from mapper adapter object.
        /// </summary>
        /// <typeparam name="T">Type of source object</typeparam>
        /// <typeparam name="U">Type of target object</typeparam>
        /// <param name="adapter">The adapter.</param>
        /// <param name="source">The source.</param>
        /// <returns>New object with properties mapped from mapper adapter.</returns>
        public static U CreateMappedObject<T, U>(this IObjectMapperAdapter<T, U> adapter, T source) where U : new()
        {
            U newObject = new U();
            adapter.MapObject(source, newObject);
            return newObject;
        }

        /// <summary>
        /// Creates new object of type U from mapper adapter object.
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <typeparam name="U">Type of source object</typeparam>
        /// <param name="adapter">The adapter.</param>
        /// <param name="source">The source.</param>
        /// <returns>New object with properties mapped from mapper adapter.</returns>
        public static T CreateMappedObject<T, U>(this IObjectMapperAdapter<T, U> adapter, U source) where T : new()
        {
            T newObject = new T();
            adapter.MapObject(source, newObject);
            return newObject;
        }
    }
}
