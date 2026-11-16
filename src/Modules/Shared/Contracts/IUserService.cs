using PeopleOnDuty.src.Modules.Users;
using src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Shared;

public interface IUserService
{
    Task<AuthResponse> Login(LoginRequest loginRequest, CancellationToken ct);
    Task<AuthResponse> Register(RegisterRequest registerRequest, CancellationToken ct);
    Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct);
    Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct);
    Task<User> GetOrCreateByTelegramIdAsync(long telegramId, CancellationToken ct);
    Task<int> GetAllUsersCountAsync(CancellationToken ct);
    Task<IReadOnlyList<long>> GetTelegramIdsBatchAsync(int skip, int take, CancellationToken ct);
    Task AttachTelegramIdAsync(Guid userId, long telegramId, CancellationToken ct);
}
