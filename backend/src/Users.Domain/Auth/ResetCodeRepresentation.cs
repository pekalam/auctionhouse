using System;
using Core.DomainFramework;

namespace Users.Domain.Auth
{
    public class ResetCodeRepresentation
    {
        public const int MAX_MINUTES_SINCE_CREATED = 10;

        public long Id { get; }
        public ResetCode ResetCode { get; }
        public DateTime DateCreated { get; }
        public bool Checked { get; private set; }
        public string Email { get; }

        public ResetCodeRepresentation(long id, string resetCode, DateTime dateCreated, bool @checked, string email)
        {
            Id = id;
            ResetCode = resetCode;
            DateCreated = dateCreated;
            Checked = @checked;
            Email = email;
        }

        public void MarkAsChecked()
        {
            if (Checked)
            {
                throw new DomainException("Reset code was previously checked");
            }
            Checked = true;
        }

        public bool IsExpired => DateTime.UtcNow.Subtract(DateCreated).Minutes > MAX_MINUTES_SINCE_CREATED;
    }
}