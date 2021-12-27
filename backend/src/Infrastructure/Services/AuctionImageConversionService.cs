using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Common.Domain.Auctions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Services
{
    public class AuctionImageConversionService : IAuctionImageConversion
    {
        private static readonly List<string[]> _allowedFirstBytes = new List<string[]>();

        static AuctionImageConversionService()
        {
            var jpg = new string[] {"FF", "D8"};
            var png = new string[] {"89", "50", "4E", "47", "0D", "0A", "1A", "0A"};
            _allowedFirstBytes.Add(jpg);
            _allowedFirstBytes.Add(png);
        }

        private IImageEncoder GetEncoderFromImage(AuctionImageRepresentation imageRepresentation)
        {
            switch (imageRepresentation.Metadata.Extension)
            {
                case "jpg":
                    return new JpegEncoder();
                case "png":
                    return new PngEncoder();
                default:
                    throw new Exception($"Unknown image extension {imageRepresentation.Metadata.Extension}");
            }
        }

        public AuctionImageRepresentation ConvertTo(AuctionImageRepresentation imageRepresentation,
            AuctionImageSize size)
        {
            using (var img = Image.Load(imageRepresentation.Img))
            {
                int destW = size.W;
                int destH = size.H;

                if (img.Width * size.H > size.W * img.Height)
                {
                    destH = (size.W * img.Height) / img.Width;
                }
                else
                {
                    destW = (size.H * img.Width) / img.Height;
                }

                img.Mutate(x => x.Resize(destW, destH));
                using (var mem = new MemoryStream())
                {
                    img.Save(mem, GetEncoderFromImage(imageRepresentation));
                    return new AuctionImageRepresentation(imageRepresentation.Metadata, mem.ToArray());
                }
            }
        }

        private string[] ImgExtensionToFirstBytes(string ext)
        {
            switch (ext)
            {
                case "jpg":
                    return _allowedFirstBytes[0];
                case "png":
                    return _allowedFirstBytes[1];
                default:
                    throw new Exception($"ConverterService does not contain definition of {ext} first bytes");
            }
        }

        public bool ValidateImage(AuctionImageRepresentation imageRepresentation, string[] allowedExtensions)
        {

            //TODO
            var read = new List<string>();
            string[][] firstBytes = allowedExtensions.Select(ImgExtensionToFirstBytes).ToArray();

            for (int i = 0; i < 8; i++)
            {
                var b = imageRepresentation.Img[i].ToString("X2");
                read.Add(b);
                bool isValid = firstBytes.Any(imgBytes => !imgBytes.Except(read).Any());
                if (isValid)
                {
                    return true;
                }
            }

            return false;
        }
    }
}