using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    /// <summary>
    /// Marks method as mapping method. Method should accept two parameters (source and target) and returns void.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ObjectMapperMethodAttribute : Attribute
    {
    }
}
