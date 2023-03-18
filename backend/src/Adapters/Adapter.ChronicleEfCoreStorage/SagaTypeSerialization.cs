using Auctions.Application.Sagas;
using Users.Application.Sagas;

namespace ChronicleEfCoreStorage
{
    internal static class SagaTypeSerialization
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
