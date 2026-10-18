namespace PeopleOnDuty.src.Modules.Tasks
{
    public record struct TakenTaskDto
    {
        public string Name;
        public string? Description;
        public TaskStatus Status;
      
         public string Reward;
       public DateTime? TakenAt;

        public TakenTaskDto( string name, string? description, string reward, TaskStatus status,  DateTime? takenAt) {
            Description = description;
            Name = name;
            Reward = reward;
            Status = status;
            TakenAt = takenAt;
            

        }

     
       

    }
    public record struct OpenedTaskDto
    {
        public string Name;
        public string? Description;      
         public string Reward;
         public string AuthorName;
       

        public OpenedTaskDto( string name, string? description, string reward, string authorName) {
            Description = description;
            Name = name;
            Reward = reward;
           AuthorName = authorName;
            

        }

     
       

    }
}