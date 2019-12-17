using Core.Common;

namespace Core.Command.Mediator
{
    public class QueuedCommandMediator : CommandMediator
    {
        public QueuedCommandMediator(EventBusCmdHandlerMediator mediator, IImplProvider implProvider) : base(mediator, implProvider)
        {
        }
    }
}