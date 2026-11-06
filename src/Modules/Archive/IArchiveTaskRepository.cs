namespace PeopleOnDuty.src.Modules.Archive
{
    public interface IArchiveTaskRepository
    {
        Task<Guid> AddAsync(ArchivedTask task, CancellationToken ct);
        Task DeleteAsync(Guid archiveTaskId, CancellationToken ct);
        Task<ArchivedTask?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<ArchivedTask?> GetByOriginalTaskIdAsync(Guid originalTaskId, CancellationToken ct);
        Task<IReadOnlyList<ArchivedTask>> GetByAuthorAsync(Guid authorId, CancellationToken ct);
        Task<IReadOnlyList<ArchivedTask>> GetAllAsync(CancellationToken ct);
        Task<(IReadOnlyList<ArchivedTask> Items, int Total)> SearchAsync(string? query, int skip, int take, CancellationToken ct);
        
    }
}