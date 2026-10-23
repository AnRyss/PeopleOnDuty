namespace PeopleOnDuty.src.Modules.Tasks
{
    public record struct TakenTaskDto
    {   public Guid TaskId;
        public string Name;
        public string? Description;
        public TaskStatus Status;

        public string Reward;
        public DateTime? TakenAt;

        public TakenTaskDto(string name, string? description, string reward, TaskStatus status, DateTime? takenAt, Guid taskId)
        {
            Description = description;
            Name = name;
            Reward = reward;
            Status = status;
            TakenAt = takenAt;
            TaskId = taskId;

        }




    }
    public record struct OpenedTaskDto
    {
        public Guid TaskId;
        public string Name;
        public string? Description;
        public string Reward;
        public string AuthorName;


        public OpenedTaskDto(string name, string? description, string reward, string authorName, Guid taskId)
        {
            Description = description;
            Name = name;
            Reward = reward;
            AuthorName = authorName;
            TaskId = taskId;


        }




    }
}