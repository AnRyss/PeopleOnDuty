using PeopleOnDuty.src.Modules.Tasks;

namespace PeopleOnDuty.src.Modules.Shared
{
    public interface ITaskService
    {
        Task AbandonTaskAsync(Guid taskId, Guid userId, CancellationToken ct);
        Task CancelTaskAsync(Guid taskId, CancellationToken ct);
        Task CompleteTaskAsync(Guid taskId, CancellationToken ct);
        Task<Guid> CreateTaskAsync(DutyTask task, CancellationToken ct);
        Task<IReadOnlyList<TaskDto>> GetTakenTasksByUserAsync(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<TaskDto>> GetByStatusAsync(Tasks.TaskStatus status, CancellationToken ct);
        Task TakeTaskAsync(Guid taskId, Guid userId, CancellationToken ct);
        Task<IReadOnlyList<TaskDto>> GetTasksAsync(IReadOnlyCollection<Guid> tasksIds, CancellationToken ct);
        Task<TaskDto?> GetTaskAsync(Guid taskId, CancellationToken ct);
        Task<IReadOnlyList<TaskDto>> GetByAuthorAsync(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<TaskDto>> GetAllTasksAsync(CancellationToken ct);
        Task ChangeDescription(Guid taskId, string desc, CancellationToken ct);
    }
}