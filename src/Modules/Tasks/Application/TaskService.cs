using System.Runtime.ExceptionServices;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Users;

namespace PeopleOnDuty.src.Modules.Tasks
{
    public class TaskService(ITaskRepository taskRepository, IUserService userService) : ITaskService

    {
        private readonly IUserService _userService = userService;

        private readonly ITaskRepository _taskRepository = taskRepository;
        public async Task AbandonTaskAsync(Guid taskId, Guid userId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId,ct) ?? throw new KeyNotFoundException("No such Task");
            if (task.TakenBy != userId)
                throw new Exception("Ony user that took task can abandon it");
            task.Abandon();
            await _taskRepository.SaveAsync(task, ct);

        }

        public async Task CancelTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId,ct) ?? throw new KeyNotFoundException("No such Task");
            task.Cancel();
            await _taskRepository.SaveAsync(task, ct);
        }

        public async Task CompleteTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId,ct) ?? throw new KeyNotFoundException("No such Task");
            task.Complete();
            await _taskRepository.SaveAsync(task, ct);
        }

        public async Task<Guid> CreateTaskAsync(DutyTask task, CancellationToken ct)
        {
            await _taskRepository.AddAsync(task, ct);
            return task.TaskId;
        }

        public async Task<IReadOnlyList<TaskDto>> GetTakenTasksByUserAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByTakerAsync(userId, ct);
            return tasks
       .Select(t => new TaskDto(t.Name, t.Description, t.Reward, t.AuthorId, t.TaskId,t.Status))
       .ToList();
        }

        public async Task<IReadOnlyList<TaskDto>> GetByStatusAsync(TaskStatus status,CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByStatusAsync(status, ct);
            return tasks.Select(t => new TaskDto(t.Name, t.Description, t.Reward, t.AuthorId, t.TaskId, t.Status))
            .ToList();
        }

        public async Task TakeTaskAsync(Guid taskId, Guid userId, CancellationToken ct)
        {   
            

            var task = await _taskRepository.GetAsync(taskId, ct)
        ?? throw new Exception($"Task {taskId} not found");

            if (task.AuthorId == userId)
            throw new InvalidOperationException("Author can not take his own task");

            task.Take(userId);

            // Атомарный захват 
            var taken = await _taskRepository.TryTakeAsync(taskId, userId, ct);
            if (!taken)
                throw new InvalidOperationException("Task already taken by someone else");
        }

        public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(IReadOnlyCollection<Guid> TasksId, CancellationToken ct)
        {
             var tasks = await _taskRepository.GetManyAsync(TasksId, ct);
            return tasks.Select(t => new TaskDto(t.Name, t.Description, t.Reward, t.AuthorId, t.TaskId, t.Status))
            .ToList();
        }

        public async Task<TaskDto?> GetTaskAsync(Guid taskId, CancellationToken ct)
        {
          var task = await _taskRepository.GetAsync(taskId, ct);
            return new TaskDto(name: task.Name, description: task.Description, reward: task.Reward, authorName: task.AuthorId, taskId: task.TaskId, taskStatus: task.Status);
        }

        public async Task<IReadOnlyList<TaskDto>> GetByAuthorAsync(Guid  userId, CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByAuthorAsync(userId, ct);
            return tasks.Select(t => new TaskDto(t.Name, t.Description, t.Reward, t.AuthorId, t.TaskId, t.Status))
            .ToList();
        }
    }
}
