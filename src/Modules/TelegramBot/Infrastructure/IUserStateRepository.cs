namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public interface IUserStateRepository
    {
        Task<UserState> GetOrCreateAsync(long userId, CancellationToken ct);
        Task SaveAsync(UserState state, CancellationToken ct);
        Task DeleteAsync(long userId, CancellationToken ct);
    }
}