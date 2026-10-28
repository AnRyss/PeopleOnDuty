using Microsoft.EntityFrameworkCore;

namespace PeopleOnDuty.src.Modules.Tasks.Infrastructure.CompletedTasks
{
    public class CompletedTaskRepository(CompletedTaskContext taskContext) : ICompletedTaskRepository
    {
      
    public async Task<DutyTask?> GetAsync(Guid guid, CancellationToken ct)
    {
        return await taskContext.CompletedTasks.FirstOrDefaultAsync(t => t.TaskId == guid, ct);
    }
   
    public async Task<IReadOnlyList<DutyTask>> GetByTakerAsync(Guid userTakerId, CancellationToken ct)
    {
        return await taskContext.CompletedTasks.Where(t => t.TakenBy == userTakerId).ToListAsync(ct);
    }

    public async Task SaveAsync(DutyTask task, CancellationToken ct)
    {
        await taskContext.SaveChangesAsync(ct);
    }
    public async Task AddAsync(DutyTask task, CancellationToken ct)
    {
        await taskContext.AddAsync(task, ct);
        await taskContext.SaveChangesAsync(ct);
    }

        public async Task<IReadOnlyList<DutyTask>> GetByAuthorAsync(Guid userId, CancellationToken ct)
        {
              return await taskContext.CompletedTasks.Where(t => t.AuthorId == userId).ToListAsync(ct);
        }

    }
}