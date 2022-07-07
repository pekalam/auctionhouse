using MediatR;

namespace Common.Application.Commands
{
    public interface ICommand : IRequest<RequestStatus>
    {

    }
}
