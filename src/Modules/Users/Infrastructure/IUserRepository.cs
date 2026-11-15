namespace PeopleOnDuty.src.Modules.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByName(string name, CancellationToken ct);
        Task<User> CreateNew(string name, string passwordHash, CancellationToken ct);
        Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct);
        Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct);
        Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct);
        Task CreateNew(User user, CancellationToken ct);
        Task<int> GetCountAsync(CancellationToken ct);
        Task<IReadOnlyList<long>> GetTelegramIdsBatchAsync(int skip, int take, CancellationToken ct);
        Task SaveAsync(User user,CancellationToken ct);
    }
}