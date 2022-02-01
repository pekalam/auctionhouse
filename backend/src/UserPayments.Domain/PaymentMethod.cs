using Core.Common.Domain;

namespace UserPayments.Domain
{
    public class PaymentMethod : ValueObject
    {
        public string Name { get; }

        public PaymentMethod(string name)
        {
            Name = name;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
        }
    }
}
