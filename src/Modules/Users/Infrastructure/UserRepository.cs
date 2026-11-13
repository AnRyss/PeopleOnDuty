using Microsoft.EntityFrameworkCore;
using src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Users
{
  public class UserRepository(UserContext userContext) : IUserRepository
  {
    private UserContext userContext = userContext;
    public async Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct = default)
    {
      return await userContext.Users.FirstOrDefaultAsync(u => u.UserId == userGuid, ct);
    }
    public async Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
    {
      return await userContext.Users.Where(u => guids.Contains(u.UserId)).ToListAsync(ct);
    }
    public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct)
    {
      return await userContext.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
    }
    public async Task AddAsync(User user, CancellationToken ct)
    {
      await userContext.Users.AddAsync(user, ct);
      await userContext.SaveChangesAsync();
    }
    public async Task<int> GetCountAsync(CancellationToken ct)
    {
      return await userContext.Users.CountAsync(ct);
    }
    public async Task<IReadOnlyList<long>> GetTelegramIdsBatchAsync(int skip, int take, CancellationToken ct)
    {
      return await userContext.Users
          .Where(u => u.TelegramId != null)
          .OrderBy(u => u.UserId)
          .Skip(skip)
          .Take(take)
          .Select(u => u.TelegramId!.Value)
          .ToListAsync(ct);
    }
    public async Task<User> CreateNew(string name, string passwordHash, CancellationToken ct)
    {
      var exists = await userContext.Users.FirstOrDefaultAsync(u => u.Name == name, ct);
      if (exists is null)
      {
        var user = new User(name, passwordHash);
        await AddAsync(user, ct);
        return user;
      }
      else
      {
        throw new InvalidOperationException("Пользователь с таким именем уже существует");
      }
    }
   
  }
}