namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public interface IUserStateRepository // Телеграм кэш
    {
        Task<UserState> GetOrCreateAsync(long TelegramUserId, CancellationToken ct);
        Task SaveAsync(UserState state, CancellationToken ct);
        Task DeleteAsync(long TelegramUserId, CancellationToken ct);
    }
}