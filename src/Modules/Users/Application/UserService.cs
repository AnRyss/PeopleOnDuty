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
    }
}