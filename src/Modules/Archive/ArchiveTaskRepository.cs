using Microsoft.EntityFrameworkCore;


namespace PeopleOnDuty.src.Modules.Archive.Infrastructure;

public class ArchiveTaskRepository(ArchiveTaskContext context) : IArchiveTaskRepository
{
    private readonly ArchiveTaskContext _context = context;

    public async Task AddAsync(ArchivedTask task, CancellationToken ct)
    {
        _context.ArchiveTasks.Add(task);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ArchivedTask?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.ArchiveTasks
            .FirstOrDefaultAsync(x => x.Id == id, ct);
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
}