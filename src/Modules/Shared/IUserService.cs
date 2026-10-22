using PeopleOnDuty.src.Modules.Users;

namespace PeopleOnDuty.src.Modules.Shared
{
    public interface IUserService
    {
          public  Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct);
          public  Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct);
          public  Task<User> GetOrCreateByTelegramIdAsync(long telegramId, CancellationToken ct);
    }
}