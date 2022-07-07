using Core.DomainFramework;
using System.Collections;

namespace Auctions.Domain
{
    public class AuctionImages : IEnumerable<AuctionImage?>
    {
        private AuctionImage?[] _images = new AuctionImage[AuctionConstantsFactory.MaxImages];


        public IEnumerable<string?> Size1Ids => _images.Select(i => i?.Size1Id);
        public IEnumerable<string?> Size2Ids => _images.Select(i => i?.Size2Id);
        public IEnumerable<string?> Size3Ids => _images.Select(i => i?.Size3Id);

        public static AuctionImages FromSizeIds(string?[] Size1Ids, string?[] Size2Ids, string?[] Size3Ids)
        {
            if(Size1Ids.Length != Size2Ids.Length || Size1Ids.Length != Size3Ids.Length || Size2Ids.Length != Size3Ids.Length)
            {
                throw new ArgumentException();
            }
            if(Size1Ids.Length != AuctionConstantsFactory.MaxImages)
            {
                throw new ArgumentException();
            }
            var images = new AuctionImage[AuctionConstantsFactory.MaxImages];

            for (int i = 0; i < Size1Ids.Length; i++)
            {
                if(Size1Ids[i] is not null)
                {
                    images[i] = new AuctionImage(Size1Ids[i] ?? throw new ArgumentException(), Size2Ids[i] ?? throw new ArgumentException(), Size3Ids[i] ?? throw new ArgumentException());
                }
            }
            return new AuctionImages { _images = images };
        }

        internal void ClearAll()
        {
            _images = new AuctionImage[AuctionConstantsFactory.MaxImages];
        }

        public int AddImage(AuctionImage image)
        {
            for (int i = 0; i < AuctionConstantsFactory.MaxImages; i++)
            {
                if (_images[i] is null)
                {
                    this[i] = image;
                    return i;
                }
            }
            throw new DomainException("Could not add auction image");
        }

        public AuctionImage? this[int imageNum]
        {
            get
            {
                ThrowIfImageNumIsInvalid(imageNum);
                return _images[imageNum];
            }
            set
            {
                ThrowIfImageNumIsInvalid(imageNum);
                _images[imageNum] = value;
            }
        }

        private static void ThrowIfImageNumIsInvalid(int imageNum)
        {
            if (imageNum >= AuctionConstantsFactory.MaxImages || imageNum < 0) throw new DomainException("Invalid image number");
        }

        public int Count() => _images.Where(x => x is not null).Count();

        public IEnumerator<AuctionImage?> GetEnumerator()
        {
            return _images.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}