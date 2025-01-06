using MediatR;

namespace Contract.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
