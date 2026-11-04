using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Users;
using PeopleOnDuty.src.Modules.Tasks;

namespace PeopleOnDuty.src.Modules.Tasks
{
    public class TaskService(ITaskRepository taskRepository, IUserService userService,IConfiguration config) : ITaskService
    {
        private readonly IUserService _userService = userService;
        private readonly ITaskRepository _taskRepository = taskRepository;

        public async Task AbandonTaskAsync(Guid taskId, Guid userId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId, ct) ?? throw new KeyNotFoundException("No such Task");
            if (task.TakenBy != userId)
                throw new InvalidOperationException("Only user that took task can abandon it");
            task.Abandon();
            await _taskRepository.SaveAsync(task, ct);
        }

        public async Task CancelTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId, ct) ?? throw new KeyNotFoundException("No such Task");
            task.Cancel();
            await _taskRepository.SaveAsync(task, ct);
        }

        public async Task CompleteTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId, ct) ?? throw new KeyNotFoundException("No such Task");
            task.Complete();
            await _taskRepository.SaveAsync(task, ct);
        }

        public async Task<Guid> CreateTaskAsync(DutyTask task, CancellationToken ct)
        {
            var cooldownHours  = config.GetValue<int>("Tasks:CreationCooldownHours", 1);
            if (cooldownHours  < 0 ) cooldownHours  = -cooldownHours ;
            var timeAgo = DateTime.UtcNow.AddHours(-cooldownHours );
            var hasRecentTask = await _taskRepository.HasUserCreatedTaskSinceAsync(task.AuthorId, timeAgo, ct);

            if (hasRecentTask)
            {
                throw new InvalidOperationException($"Вы можете создавать только одну задачу в {cooldownHours} ч. Попробуйте позже.");
            }
            await _taskRepository.AddAsync(task, ct);
            return task.TaskId;
        }

        public async Task<IReadOnlyList<TaskDto>> GetTakenTasksByUserAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByTakerAsync(userId, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<TaskDto>> GetByStatusAsync(TaskStatus status, CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByStatusAsync(status, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task TakeTaskAsync(Guid taskId, Guid userId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId, ct)
                ?? throw new KeyNotFoundException($"Task {taskId} not found");

            task.Take(userId);

            var taken = await _taskRepository.TryTakeAsync(taskId, userId, ct);
            if (!taken)
                throw new InvalidOperationException("Task already taken by someone else");
        }

        public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(IReadOnlyCollection<Guid> tasksId, CancellationToken ct)
        {
            var tasks = await _taskRepository.GetManyAsync(tasksId, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task<TaskDto?> GetTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId, ct);
            if (task is null) return null;

            return ToDto(task);
        }

        public async Task<IReadOnlyList<TaskDto>> GetByAuthorAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByAuthorAsync(userId, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<TaskDto>> GetAllTasksAsync(CancellationToken ct)
        {
            var tasks = await _taskRepository.GetAllAsync(ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task ChangeDescription(Guid taskId, string desc, CancellationToken ct)
        {
            await _taskRepository.ChangeDescription(taskId, desc, ct);
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