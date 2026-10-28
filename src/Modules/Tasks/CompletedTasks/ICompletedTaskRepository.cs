namespace PeopleOnDuty.src.Modules.Tasks.Infrastructure.CompletedTasks
{
    public interface ICompletedTaskRepository
    {
      Task<DutyTask?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DutyTask>> GetByTakerAsync(Guid userId, CancellationToken ct);
     Task<IReadOnlyList<DutyTask>> GetByAuthorAsync(Guid userId, CancellationToken ct);
    Task SaveAsync(DutyTask task, CancellationToken ct);
     Task AddAsync(DutyTask task, CancellationToken ct);
    
    }
}