using MediatR;

namespace Contract.Abstractions.Messaging
{
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }

    public interface ICommand : IRequest
    {
    }
}
