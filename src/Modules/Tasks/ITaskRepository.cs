namespace PeopleOnDuty.src.Modules.Tasks
{
    public interface ITaskRepository
    {
       Task<DutyTask?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DutyTask>> GetOpenAsync(CancellationToken ct);
    Task<IReadOnlyList<DutyTask>> GetByTakerAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<DutyTask>> GetByAuthorAsync(Guid userId, CancellationToken ct);
    
    Task<IReadOnlyList<DutyTask>> GetByStatusAsync(TaskStatus status, CancellationToken ct);

    // Запись
    Task AddAsync(DutyTask task, CancellationToken ct);
    Task SaveAsync(DutyTask task, CancellationToken ct);

    // Атомарные операции — то, что нельзя сделать в сущности
    Task<bool> TryTakeAsync(Guid taskId, Guid userId, CancellationToken ct);
    }
}