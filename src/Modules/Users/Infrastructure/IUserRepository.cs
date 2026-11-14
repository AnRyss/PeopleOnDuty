namespace PeopleOnDuty.src.Modules.Users
{
    public interface IUserRepository
    {
        public Task<User> CreateNew(string name, string passwordHash, CancellationToken ct);
        public Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct);
        public Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct);
        public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct);
        public Task CreateNew(User user, CancellationToken ct);
        Task<int> GetCountAsync(CancellationToken ct);
        Task<IReadOnlyList<long>> GetTelegramIdsBatchAsync(int skip, int take, CancellationToken ct);
    }
}