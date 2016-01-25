using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    public static class ObjectMapperExtensions
    {
        public static T CreateMappedObject<T>(this IObjectMapper<T> mapper) where T : new()
        {
            T newObject = new T();
            mapper.MapObject(newObject);
            return newObject;
        }

        public static U CreateMappedObject<T, U>(this IObjectMapperAdapter<T, U> adapter, T source) where U : new()
        {
            U newObject = new U();
            adapter.MapObject(source, newObject);
            return newObject;
        }

        public static T CreateMappedObject<T, U>(this IObjectMapperAdapter<T, U> adapter, U source) where T : new()
        {
            T newObject = new T();
            adapter.MapObject(source, newObject);
            return newObject;
        }
    }
}
