using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Users;

namespace PeopleOnDuty.src.Modules.Tasks
{
    public class TaskService(ITaskRepository taskRepository, ICurrentUser currentUser, IUserService userService) : ITaskService

    {   
            private IUserService _userService = userService;
            private ICurrentUser _currentUser = currentUser;
            private ITaskRepository _taskRepository = taskRepository;
          public async Task AbandonTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId) ?? throw new KeyNotFoundException("No such Task");
            if (task.TakenBy != _currentUser.UserId)
                throw new Exception("Ony user that took task can abandon it");
            task.Abandon();
          await _taskRepository.SaveAsync(task,ct);
           
        }

        public async Task CancelTaskAsync(Guid taskId, CancellationToken ct)
        {
           var task = await _taskRepository.GetAsync(taskId) ?? throw new KeyNotFoundException("No such Task");
           task.Cancel();
            await _taskRepository.SaveAsync(task,ct);
        }

        public async Task CompleteTaskAsync(Guid taskId, CancellationToken ct)
        {
         var task = await _taskRepository.GetAsync(taskId) ?? throw new KeyNotFoundException("No such Task");
           task.Complete();
            await _taskRepository.SaveAsync(task,ct);
        }

        public async Task<Guid> CreateTaskAsync(DutyTask task, CancellationToken ct)
        {
            await _taskRepository.SaveAsync(task,ct);
            return task.TaskId;
        }

        public async Task<IReadOnlyList<TakenTaskDto>> GetMyTakenTasksAsync(CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByTakerAsync(_currentUser.UserId,ct);
             return tasks
        .Select(t => new TakenTaskDto( t.Name,t.Description, t.Reward, t.Status, t.TakenAt))
        .ToList();
        }

        public async Task<IReadOnlyList<OpenedTaskDto>> GetOpenTasksAsync(CancellationToken ct)
        {
            var tasks = await _taskRepository.GetByStatusAsync(TaskStatus.OPENED, ct);
                var authorIds = tasks.Select(t => t.AuthorId).Distinct().ToList();
    
    var authors = await _userService.GetUsersAsync(authorIds, ct);
    var authorMap = authors.ToDictionary(a => a.UserId, a => a.Name);
            return tasks.Select(t => new OpenedTaskDto( t.Name,t.Description, t.Reward, authorMap.GetValueOrDefault(t.AuthorId, "Unknown")))
            .ToList();
        }

        public async Task TakeTaskAsync(Guid taskId, CancellationToken ct)
        {
            var task = await _taskRepository.GetAsync(taskId, ct)
        ?? throw new Exception($"Task {taskId} not found");
        task.Take(_currentUser.UserId);

    // Атомарный захват 
    var taken = await _taskRepository.TryTakeAsync(taskId, _currentUser.UserId, ct);
    if (!taken)
        throw new ConflictException("Task already taken by someone else");
        }

    }
    }
