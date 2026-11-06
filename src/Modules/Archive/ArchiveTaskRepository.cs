using Microsoft.EntityFrameworkCore;


namespace PeopleOnDuty.src.Modules.Archive.Infrastructure;

public class ArchiveTaskRepository(ArchiveTaskContext context) : IArchiveTaskRepository
{
    private readonly ArchiveTaskContext _context = context;

    public async Task<Guid> AddAsync(ArchivedTask task, CancellationToken ct)
    {       
        _context.ArchiveTasks.Add(task);
        await _context.SaveChangesAsync(ct);
        return task.Id;
    }
    public async Task DeleteAsync(Guid archiveTaskId, CancellationToken ct)
    {
        var task = await GetByIdAsync(archiveTaskId,ct);
        if (task is null) throw new KeyNotFoundException("No such task");
        _context.ArchiveTasks.Remove(task);
        await _context.SaveChangesAsync(ct);
    }
    public async Task<ArchivedTask?> GetByIdAsync(Guid archiveTaskId, CancellationToken ct)
    {
        return await _context.ArchiveTasks
            .FirstOrDefaultAsync(x => x.Id == archiveTaskId, ct);
    }

    public async Task<ArchivedTask?> GetByOriginalTaskIdAsync(Guid originalTaskId, CancellationToken ct)
    {
        return await _context.ArchiveTasks
            .FirstOrDefaultAsync(x => x.OriginalTaskId == originalTaskId, ct);
    }

    public async Task<IReadOnlyList<ArchivedTask>> GetByAuthorAsync(Guid authorId, CancellationToken ct)
    {
        return await _context.ArchiveTasks
            .Where(x => x.AuthorId == authorId)
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ArchivedTask>> GetAllAsync(CancellationToken ct)
    {
        return await _context.ArchiveTasks
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<ArchivedTask> Items, int Total)> SearchAsync(string? query, int skip, int take, CancellationToken ct)
    {
        var q = _context.ArchiveTasks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(t => t.Name.Contains(query));

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(t => t.CompletedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }
}