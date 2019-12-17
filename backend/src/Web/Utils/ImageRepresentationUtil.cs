using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Common.Domain.Auctions;
using Microsoft.AspNetCore.Http;

namespace Web.Utils
{
    public class ImageRepresentationUtil
    {
        public static AuctionImageRepresentation GetImageRepresentationFromFormFile(IFormFile formFile)
        {
            var buffer = new byte[1024 * 1024 * 5];
            using (var stream = new MemoryStream(buffer))
            {
                formFile.CopyTo(stream);
                var imageRepresentation = new AuctionImageRepresentation()
                {
                    Img = stream.ToArray(),
                    Metadata = new AuctionImageMetadata()
                };
                return imageRepresentation;
            }
        }

    }
}
