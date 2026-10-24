using PeopleOnDuty.src.Modules.Tasks;

namespace PeopleOnDuty.src.Modules.Shared
{
    public interface ITaskService
    {
         public  Task AbandonTaskAsync(Guid taskId, Guid userId, CancellationToken ct);
        public  Task CancelTaskAsync(Guid taskId, CancellationToken ct);
        public  Task CompleteTaskAsync(Guid taskId, CancellationToken ct);

        public  Task<Guid> CreateTaskAsync(DutyTask task, CancellationToken ct);
        public  Task<IReadOnlyList<TaskDto>> GetTakenTasksByUserAsync(Guid userId, CancellationToken ct);
        public  Task<IReadOnlyList<TaskDto>> GetByStatusAsync(Tasks.TaskStatus status , CancellationToken ct);
        public  Task TakeTaskAsync(Guid taskId, Guid userId, CancellationToken ct);
        public Task<IReadOnlyList<TaskDto>> GetTasksAsync(IReadOnlyCollection<Guid> tasksIds, CancellationToken ct);
        public Task<TaskDto?> GetTaskAsync(Guid taskId, CancellationToken ct);
        public  Task<IReadOnlyList<TaskDto>> GetByAuthorAsync(Guid userId, CancellationToken ct);
    }
}