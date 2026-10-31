namespace PeopleOnDuty.src.Modules.Tasks
{
    public record struct TaskDto
    {   public TaskStatus TaskStatus;
        public Guid TaskId;
        public string Name;
        public string? Description;
        public string Reward;
        public Guid AuthorId;
        public bool IsPublic;

        public TaskDto(string name, string? description, string reward, Guid authorName, Guid taskId, TaskStatus taskStatus)
        {
            IsPublic = false;
            Description = description;
            Name = name;
            Reward = reward;
            AuthorId = authorName;
            TaskId = taskId;
            TaskStatus  = taskStatus;

        }

public TaskDto(string name, string? description, string reward, Guid authorName, Guid taskId, TaskStatus taskStatus, bool isPublic)
        {
            IsPublic = isPublic;
            Description = description;
            Name = name;
            Reward = reward;
            AuthorId = authorName;
            TaskId = taskId;
            TaskStatus  = taskStatus;

        }


    }
}