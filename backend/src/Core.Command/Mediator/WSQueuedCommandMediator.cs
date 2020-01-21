using System.Threading.Tasks;
using Core.Common;
using Core.Common.Command;

namespace Core.Command.Mediator
{
    public class WSQueuedCommandMediator : CommandMediator
    {
        public WSQueuedCommandMediator(EventBusCommandHandlerMediator mediator, IImplProvider implProvider) : base(mediator, implProvider)
        {
        }

        public override Task<RequestStatus> Send(CommandBase commandBase)
        {
            commandBase.WSQueued = true;
            commandBase.HttpQueued = false;
            return base.Send(commandBase);
        }
    }
}