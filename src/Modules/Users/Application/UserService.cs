using PeopleOnDuty.src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Users
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        public async Task<User?> GetUserAsync(Guid guid, CancellationToken ct)
        {
            return await _userRepository.GetUserAsync(guid, ct);
        }
        public async Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
        {
            return await _userRepository.GetUsersAsync(guids, ct);
        }
        public async Task<User> GetOrCreateByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId, ct);
            if (user is not null) return user;

            // Создаем нового
            var newUser = new User(telegramId);
            await _userRepository.AddAsync(newUser, ct);
            return newUser;
        }
    }
}