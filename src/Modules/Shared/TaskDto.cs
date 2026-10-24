namespace PeopleOnDuty.src.Modules.Tasks
{
    public record struct TaskDto
    {   public TaskStatus TaskStatus;
        public Guid TaskId;
        public string Name;
        public string? Description;
        public string Reward;
        public Guid AuthorId;


        public TaskDto(string name, string? description, string reward, Guid authorName, Guid taskId, TaskStatus taskStatus)
        {
            Description = description;
            Name = name;
            Reward = reward;
            AuthorId = authorName;
            TaskId = taskId;
            TaskStatus  = taskStatus;

        }




    }
}