using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class CreditsTransfered : Event
    {
        public CreditsTransfered() : base("creditsTransfered")
        {

        }
    }
}
