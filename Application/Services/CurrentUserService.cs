// HungNgo96

using Application.Interfaces;

namespace Application.Services
{
    public interface ICurrentUserService : IScopedService
    {
    }

    public sealed class CurrentUserService : ICurrentUserService
    {
    }
}
