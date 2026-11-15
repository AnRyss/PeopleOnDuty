using TaskStatus = PeopleOnDuty.src.Modules.Tasks.TaskStatus;
public readonly record struct TaskDto(
    string Name,
    string? Description,
    string Reward,
    Guid AuthorId,
    Guid TaskId,
    TaskStatus TaskStatus,
    bool IsPublic,
    DateTime CreatedAt
);