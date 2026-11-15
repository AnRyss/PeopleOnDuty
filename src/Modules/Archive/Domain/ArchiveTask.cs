namespace PeopleOnDuty.src.Modules.Archive{

public class ArchivedTask
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OriginalTaskId { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public string Reward { get; init; }
    public Guid AuthorId { get; init; }
    public string Answer { get; init; }
    public DateTime CompletedAt { get; init; }

    public ArchivedTask(
        Guid originalTaskId,
        string name,
        string? description,
        string reward,
        Guid authorId,
        string answer)
    {
        OriginalTaskId = originalTaskId;
        Name = name;
        Description = description;
        Reward = reward;
        AuthorId = authorId;
        Answer = answer;
        CompletedAt = DateTime.UtcNow;
    }

}
}