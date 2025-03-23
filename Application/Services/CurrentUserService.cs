// HungNgo96

using Contract.Interfaces;

namespace Application.Services
{
    public interface ICurrentUserService : IScopedService
    {
    }

    public sealed class CurrentUserService : ICurrentUserService
    {
    }
}
