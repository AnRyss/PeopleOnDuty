using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Users;
using PeopleOnDuty.src.Modules.Tasks;

namespace PeopleOnDuty.src.Modules.Tasks
{
    public class TaskService(ITaskRepository taskRepository,IConfiguration config) : ITaskService
    {
        public async Task AbandonTaskAsync(Guid taskId, Guid userId, CancellationToken ct)
        {
            var task = await taskRepository.GetAsync(taskId, ct) ?? throw new KeyNotFoundException("No such Task");
            if (task.TakenBy != userId)
                throw new InvalidOperationException("Only user that took task can abandon it");
            task.Abandon();
            await taskRepository.SaveAsync(task, ct);
        }

        public async Task CancelTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await taskRepository.GetAsync(taskId, ct) ?? throw new KeyNotFoundException("No such Task");
            task.Cancel();
            await taskRepository.SaveAsync(task, ct);
        }

        public async Task CompleteTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await taskRepository.GetAsync(taskId, ct) ?? throw new KeyNotFoundException("No such Task");
            task.Complete();
            await taskRepository.SaveAsync(task, ct);
        }

        public async Task<Guid> CreateTaskAsync(DutyTask task, CancellationToken ct)
        {
            var cooldownHours  = config.GetValue<int>("Tasks:CreationCooldownHours", 1);
            if (cooldownHours  < 0 ) cooldownHours  = -cooldownHours ;
            var timeAgo = DateTime.UtcNow.AddHours(-cooldownHours );
            var hasRecentTask = await taskRepository.HasUserCreatedTaskSinceAsync(task.AuthorId, timeAgo, ct);

            if (hasRecentTask)
            {
                throw new InvalidOperationException($"Вы можете создавать только одну задачу в {cooldownHours} ч. Попробуйте позже.");
            }
            await taskRepository.AddAsync(task, ct);
            return task.TaskId;
        }

        public async Task<IReadOnlyList<TaskDto>> GetTakenTasksByUserAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await taskRepository.GetByTakerAsync(userId, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<TaskDto>> GetByStatusAsync(TaskStatus status, CancellationToken ct)
        {
            var tasks = await taskRepository.GetByStatusAsync(status, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task TakeTaskAsync(Guid taskId, Guid userId, CancellationToken ct)
        {
            var task = await taskRepository.GetAsync(taskId, ct)
                ?? throw new KeyNotFoundException($"Task {taskId} not found");

            task.Take(userId);

            var taken = await taskRepository.TryTakeAsync(taskId, userId, ct);
            if (!taken)
                throw new InvalidOperationException("Task already taken by someone else");
        }

        public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(IReadOnlyCollection<Guid> tasksId, CancellationToken ct)
        {
            var tasks = await taskRepository.GetManyAsync(tasksId, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task<TaskDto?> GetTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await taskRepository.GetAsync(taskId, ct);
            if (task is null) return null;

            return ToDto(task);
        }

        public async Task<IReadOnlyList<TaskDto>> GetByAuthorAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await taskRepository.GetByAuthorAsync(userId, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<TaskDto>> GetAllTasksAsync(CancellationToken ct)
        {
            var tasks = await taskRepository.GetAllAsync(ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task ChangeDescription(Guid taskId, string desc, CancellationToken ct)
        {
            await taskRepository.ChangeDescription(taskId, desc, ct);
        }

        private static TaskDto ToDto(DutyTask task)
        {
            return new TaskDto(
                name: task.Name,
                description: task.Description,
                reward: task.Reward,
                authorName: task.AuthorId,
                taskId: task.TaskId,
                taskStatus: task.Status,
                isPublic: task.IsPublic,
                createdAt: task.CreatedAt
            );
        }
    }
}