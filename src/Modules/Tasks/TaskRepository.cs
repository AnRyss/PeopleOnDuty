using Microsoft.EntityFrameworkCore;

namespace PeopleOnDuty.src.Modules.Tasks;

    public class TaskRepository(TasksContext tasksContext) : ITaskRepository
    {
        private readonly TasksContext _tasksContext = tasksContext;
        public async Task<DutyTask?> GetAsync(Guid guid, CancellationToken ct)
    {
        return await _tasksContext.Tasks.FirstOrDefaultAsync(t => t.TaskId == guid,ct);
    }
        public async Task<IReadOnlyList<DutyTask>> GetByStatusAsync(TaskStatus status, CancellationToken ct)
        {
            return await _tasksContext.Tasks.Where(t => t.Status == status).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<DutyTask>> GetByTakerAsync(Guid userTakerId, CancellationToken ct)
        {
               return await _tasksContext.Tasks.Where(t => t.TakenBy == userTakerId).ToListAsync(ct);
        }

        public async Task SaveAsync(DutyTask task, CancellationToken ct)
        {   if (_tasksContext.Entry(task).State == EntityState.Detached)
        _tasksContext.Tasks.Update(task);
            await _tasksContext.SaveChangesAsync(ct);
        }
    }