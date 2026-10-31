namespace PeopleOnDuty.src.Modules.Shared
{
   public record ArchivedTaskDto
{
    public Guid OriginalTaskId { get; init; }
    public required string TaskName { get; init; }
    public string? TaskDescription { get; init; }
    public required string Reward { get; init; }
    public Guid AuthorId { get; init; }
    public required string Answer { get; init; }
    public DateTime CompletedAt { get; init; }
}
}