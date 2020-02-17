using System;
using System.Collections.Generic;

namespace Niusys.Extensions.TypeFinders
{
    public interface ITypeFinder
    {
        List<Type> FindAllType<TInterface>();
        List<Type> FindAllType(Type type);
        Type FindFirstOrDefalt(Type type);
    }
}
