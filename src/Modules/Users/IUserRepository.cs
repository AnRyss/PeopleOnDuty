namespace PeopleOnDuty.src.Modules.Users
{
    public interface IUserRepository
    {
          public  Task<User?> GetUserAsync(Guid userGuid, CancellationToken ct);
          public  Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct);
    }
}