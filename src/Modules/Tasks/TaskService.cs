using PeopleOnDuty.src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Tasks
{
    public class TaskService(ITaskRepository taskRepository, ICurrentUser currentUser)

    {
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
            return tasks.Select(t => new OpenedTaskDto( t.Name,t.Description, t.Reward,t.AuthorId));
        }

        public Task TakeTaskAsync(Guid taskId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

    }
    }
}