using Core.Common;

namespace Auctionhouse.Command.Adapters
{
    internal class TempFileService : ITempFileService
    {
        public string SaveAsTempFile(Stream stream)
        {
            var tempFile = Path.GetTempFileName();
            using (var fs = File.OpenWrite(tempFile))
            {
                stream.CopyTo(fs);
            }
            return tempFile;
        }
    }
}
