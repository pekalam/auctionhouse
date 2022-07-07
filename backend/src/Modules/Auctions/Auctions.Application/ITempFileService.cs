namespace Core.Common
{
    public interface ITempFileService
    {
        string SaveAsTempFile(Stream stream);
    }
}
