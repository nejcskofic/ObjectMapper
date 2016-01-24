using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.Framework
{
    public interface IObjectMapperAdapter<T, U>
    {
        void MapObject(T source, U target);
        void MapObject(U source, T target);
    }
}
