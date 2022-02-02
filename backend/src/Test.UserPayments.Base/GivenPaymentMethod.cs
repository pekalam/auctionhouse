using UserPayments.Domain;

namespace Test.UserPaymentsBase
{
    public class TestPaymentMethodConstants
    {
        public const string Name = "test";
    }

    public class GivenPaymentMethod
    {
        private string _name = TestPaymentMethodConstants.Name;

        public PaymentMethod Build()
        {
            return new(_name);
        }
    }
}