using System;

namespace Common.Application
{
    public interface IImplProvider
    {
        T Get<T>() where T : class;
        object Get(Type t);
    }
}
