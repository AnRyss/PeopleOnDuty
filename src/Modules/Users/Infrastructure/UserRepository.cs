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
    public async Task CreateNew(User user, CancellationToken ct)
    {
      if (String.IsNullOrEmpty(user.Name))
        throw new InvalidDataException("Укажите имя");
      var exists = await userContext.Users.FirstOrDefaultAsync(u => u.Name == user.Name, ct);
      if (exists is not null)
        throw new InvalidOperationException("Пользователь с таким именем уже существует");

      try
      {
        await userContext.Users.AddAsync(user, ct);
        await userContext.SaveChangesAsync(ct);
      }
      catch (DbUpdateException)
      {
        throw new InvalidOperationException("Пользователь с таким именем уже существует");
      }

    }
    public async Task<User> CreateNew(string name, string passwordHash, CancellationToken ct)
    {
      var user = new User(name, passwordHash);
      await CreateNew(user, ct);
      return user;


    }

    public async Task<User?> GetByName(string name, CancellationToken ct)
    {
      return await userContext.Users.FirstOrDefaultAsync(u => u.Name == name, ct);

    }
    public async Task SaveAsync(User user, CancellationToken ct)
    {
      if (userContext.Entry(user).State == EntityState.Detached)
      {
        userContext.Users.Update(user);
      }

      await userContext.SaveChangesAsync(ct);
    }
  }
}