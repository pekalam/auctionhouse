using System;

namespace Core.Common
{
    public interface IImplProvider
    {
        T Get<T>() where T : class;
        object Get(Type t);
    }
}
