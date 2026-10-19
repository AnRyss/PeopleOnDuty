using Microsoft.EntityFrameworkCore;

namespace PeopleOnDuty.src.Modules.Users
{
    public class UserRepository(UserContext userContext) : IUserRepository
    {
        private UserContext _userContext = userContext;
         public async Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct = default)
        {
          return await _userContext.Users.FirstOrDefaultAsync(u => u.UserId == userGuid,ct);
        }
        public async Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
        {
          return await _userContext.Users.Where(u => guids.Contains( u.UserId)).ToListAsync(ct);
        }
    }
}