using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    public interface IObjectMapper<T>
    {
        void MapObject(T target);
    }
}
