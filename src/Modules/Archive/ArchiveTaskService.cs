using PeopleOnDuty.src.Modules.Archive;
using PeopleOnDuty.src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.ArchiveTask
{
    public class ArchiveTaskService(IArchiveTaskRepository repository) : IArchiveTaskService
    {
        private readonly IArchiveTaskRepository _repository = repository;

        public async Task SaveAsync(ArchivedTaskDto dto, CancellationToken ct)
        {
            var existing = await _repository.GetByOriginalTaskIdAsync(dto.OriginalTaskId, ct);
            if (existing is not null)
                throw new InvalidOperationException($"Archive task for {dto.OriginalTaskId} already exists");

            var archiveTask = new ArchivedTask(
                originalTaskId: dto.OriginalTaskId,
                name: dto.TaskName,
                description: dto.TaskDescription,
                reward: dto.Reward,
                authorId: dto.AuthorId,
                answer: dto.Answer);

            await _repository.AddAsync(archiveTask, ct);


        }

        public async Task<ArchivedTaskDto?> GetByOriginalTaskIdAsync(Guid originalTaskId, CancellationToken ct)
        {
            var task = await _repository.GetByOriginalTaskIdAsync(originalTaskId, ct);
            return task is null ? null : ToDto(task);
        }

        public async Task<IReadOnlyList<ArchivedTaskDto>> GetByAuthorAsync(Guid authorId, CancellationToken ct)
        {
            var tasks = await _repository.GetByAuthorAsync(authorId, ct);
            return tasks.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<ArchivedTaskDto>> GetAllAsync(CancellationToken ct)
        {
            var tasks = await _repository.GetAllAsync(ct);
            return tasks.Select(ToDto).ToList();
        }

        private static ArchivedTaskDto ToDto(ArchivedTask task) =>
            new()
            {
                ArchiveTaskId = task.Id,
                OriginalTaskId = task.OriginalTaskId,
                TaskName = task.Name,
                TaskDescription = task.Description,
                Reward = task.Reward,
                AuthorId = task.AuthorId,
                Answer = task.Answer,
                CompletedAt = task.CompletedAt
            };
        public async Task<(IReadOnlyList<ArchivedTaskDto> Items, int TotalCount)> SearchAsync(
        string? query, int page, int pageSize, CancellationToken ct)
        {
            var skip = (page - 1) * pageSize;
            var (items, totalCount) = await repository.SearchAsync(query, skip, pageSize, ct);
            var dtos = items.Select(ToDto).ToList();
            return (dtos, totalCount);
        }
        public async Task<ArchivedTaskDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var task = await _repository.GetByIdAsync(id, ct);
            return task is null ? null : ToDto(task);
        }
    }

}