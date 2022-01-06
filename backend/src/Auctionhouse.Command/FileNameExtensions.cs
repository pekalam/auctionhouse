namespace Auctionhouse.Command.Controllers
{
    public static class FileNameExtensions
    {
        public static string GetFileExtensionOrThrow400(this string fileName)
        {
            var arr = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 0)
            {
                throw new Exception("Invalid file extension");
                //TODO
                //throw new ApiException(HttpStatusCode.BadRequest, "Invalid file extension");
            }
            else
            {
                return arr.Last();
            }
        }
    }
}