using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    /// <summary>
    /// Implementing object can be mapped to object of class T.
    /// </summary>
    /// <typeparam name="T">Type of target object</typeparam>
    public interface IObjectMapper<T>
    {
        /// <summary>
        /// Maps this object to target.
        /// </summary>
        /// <param name="target">The target.</param>
        void MapObject(T target);
    }
}
