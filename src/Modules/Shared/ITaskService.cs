using PeopleOnDuty.src.Modules.Tasks;

namespace PeopleOnDuty.src.Modules.Shared
{
    public interface ITaskService
    {
         public  Task AbandonTaskAsync(Guid taskId, Guid userId, CancellationToken ct);
        public  Task CancelTaskAsync(Guid taskId, CancellationToken ct);
        public  Task CompleteTaskAsync(Guid taskId, CancellationToken ct);

        public  Task<Guid> CreateTaskAsync(DutyTask task, CancellationToken ct);
        public  Task<IReadOnlyList<TakenTaskDto>> GetMyTakenTasksAsync(Guid userId, CancellationToken ct);
        public  Task<IReadOnlyList<OpenedTaskDto>> GetOpenTasksAsync(CancellationToken ct);
        public  Task TakeTaskAsync(Guid taskId, Guid userId, CancellationToken ct);
       
    }
}