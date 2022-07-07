using System.Linq;
using Core.Common.Exceptions;

namespace Core.Common.Auth
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

        public override bool Equals(object obj) => obj is ResetCode && ((ResetCode)obj).Value.Equals(this.Value);
        public override int GetHashCode() => this.Value.GetHashCode();
        public override string ToString() => this.Value.ToString();

        public static implicit operator ResetCode(string value) => new ResetCode(value);
        public static implicit operator string(ResetCode resetCode) => resetCode.Value;
    }
}