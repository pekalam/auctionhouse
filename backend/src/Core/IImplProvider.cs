namespace Core.Common
{
    public interface IImplProvider
    {
        T Get<T>();
    }
}
