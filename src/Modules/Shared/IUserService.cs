using PeopleOnDuty.src.Modules.Users;

namespace PeopleOnDuty.src.Modules.Shared
{
    public interface IUserService
    {
          public  Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct);
          public  Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct);
          public  Task<User> GetOrCreateByTelegramIdAsync(long telegramId, CancellationToken ct);
          public Task<int> GetAllUsersCountAsync(CancellationToken ct);
          public  Task<IReadOnlyList<long>> GetTelegramIdsBatchAsync(int skip, int take, CancellationToken ct);
    }
}