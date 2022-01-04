using Auctions.Application;

namespace Auctionhouse.Command.Adapters
{
    internal class FileStreamAccessor : IFileStreamAccessor
    {
        private readonly IFormFile _formFile;

        public FileStreamAccessor(IFormFile formFile)
        {
            //this._formFile = formFile ?? throw new ApiException(HttpStatusCode.BadRequest, "Null file"); //TODO
            _formFile = formFile ?? throw new NullReferenceException();
        }

        public Stream GetStream()
        {
            return _formFile.OpenReadStream();
        }
    }
}
