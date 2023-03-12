using Auctions.Application.Sagas;
using Users.Application.Sagas;

namespace Auctionhouse.Command
{
    internal static class SagaTypeSerialization //TODO temporary solution - consider changing serialization logic in chronicle.integrations or change library
    {
        public static Type GetSagaType(string sagaType) => sagaType switch
        {
            nameof(BuyNowSaga) => typeof(BuyNowSaga),
            nameof(CreateAuctionSaga) => typeof(CreateAuctionSaga),
            nameof(SignUpSaga) => typeof(SignUpSaga),
            _ => throw new NotImplementedException(),
        };
    }
}
