using Core.DomainFramework;
using System.Linq;

namespace Users.Domain.Auth
{
    public class ResetCode
    {
        public const int CODE_LENGTH = 6;
        public string Value { get; }

        public ResetCode(string value)
        {
            if (value.Length != CODE_LENGTH)
            {
                throw new DomainException("Invalid reset code length");
            }
            if (!value.All(char.IsDigit))
            {
                throw new DomainException("Invalid reset code");
            }
            Value = value;
        }

        public override bool Equals(object obj) => obj is ResetCode && ((ResetCode)obj).Value.Equals(Value);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static implicit operator ResetCode(string value) => new ResetCode(value);
        public static implicit operator string(ResetCode resetCode) => resetCode.Value;
    }
}