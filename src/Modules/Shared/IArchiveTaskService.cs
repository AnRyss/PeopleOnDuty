using PeopleOnDuty.src.Modules.Archive;

namespace PeopleOnDuty.src.Modules.Shared
{
    public interface IArchiveTaskService
    {

        Task SaveAsync(ArchivedTaskDto archivedTaskDto, CancellationToken ct);

        Task<ArchivedTaskDto?> GetByOriginalTaskIdAsync(Guid originalTaskId, CancellationToken ct);
        Task<IReadOnlyList<ArchivedTaskDto>> GetByAuthorAsync(Guid authorId, CancellationToken ct);
        Task<IReadOnlyList<ArchivedTaskDto>> GetAllAsync(CancellationToken ct);
        Task<(IReadOnlyList<ArchivedTaskDto> Items, int TotalCount)> SearchAsync(
        string? query, int page, int pageSize, CancellationToken ct);
    }
}
