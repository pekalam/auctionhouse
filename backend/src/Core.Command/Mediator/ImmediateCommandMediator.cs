using Core.Common;

namespace Core.Command.Mediator
{
    public class ImmediateCommandMediator : CommandMediator
    {
        public ImmediateCommandMediator(MediatRCommandHandlerMediator mediator, IImplProvider implProvider) : base(mediator, implProvider)
        {
        }
    }
}