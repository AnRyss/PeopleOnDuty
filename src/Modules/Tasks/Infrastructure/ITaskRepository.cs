namespace PeopleOnDuty.src.Modules.Tasks
{
    public interface ITaskRepository
    {
     Task<DutyTask?> GetAsync(Guid id, CancellationToken ct);
       Task<IReadOnlyList<DutyTask>> GetManyAsync(IReadOnlyCollection<Guid> taskIds, CancellationToken ct);
    Task<IReadOnlyList<DutyTask>> GetByTakerAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<DutyTask>> GetByStatusAsync(TaskStatus status, CancellationToken ct);
        Task<IReadOnlyList<DutyTask>> GetByAuthorAsync(Guid authorId, CancellationToken ct);
    Task SaveAsync(DutyTask task, CancellationToken ct);
     Task AddAsync(DutyTask task, CancellationToken ct);
    Task<bool> TryTakeAsync(Guid taskId, Guid userId, CancellationToken ct);
    }
}