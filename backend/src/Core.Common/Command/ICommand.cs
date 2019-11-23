using MediatR;

namespace Core.Common.Command
{
    public interface ICommand : IRequest<RequestStatus>
    {
    }
}
