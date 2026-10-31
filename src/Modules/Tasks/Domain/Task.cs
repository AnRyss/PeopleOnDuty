
namespace PeopleOnDuty.src.Modules.Tasks;

public class DutyTask
{
public Guid AuthorId { get; init; }
public Guid TaskId { get; init; } = Guid.NewGuid();
public string? Description{get;private set;}
public  string Name{get;init;}
public string Reward {get;init;}
public bool IsPublic{get;init;}
public DateTime? UntilTime {get;init;}
public TaskStatus Status { get; private set; } = TaskStatus.OPENED;
public Guid? TakenBy{get;private set;} = null;
public DateTime? TakenAt { get; private set; } = null;


    public DutyTask(string description, string name, string reward, DateTime? untilTime, Guid authorId, bool isPublic)
    {
        IsPublic = isPublic;
        Description = description;
        Name = name;
        Reward = reward;
        UntilTime = untilTime;
        AuthorId = authorId;
    }
    public DutyTask( string name, string reward, DateTime? untilTime, Guid authorId,bool isPublic) {
           IsPublic = isPublic;
        Description = null;
        Name = name;
        Reward = reward;
        UntilTime = untilTime;
        AuthorId = authorId;
    }
    public void ChangeTakenBy(Guid newTakenById)
    {
        if (Status == TaskStatus.COMPLETED)
        throw new InvalidOperationException("Cannot change TakenBy field for completed task");
        TakenBy = newTakenById;
    }
    public void AddDescription(string desc)
    {
        if (!String.IsNullOrEmpty(Description)) 
        throw new InvalidOperationException("Description is already there");
        Description = desc;
    }
    public void Take(Guid userId)
{
    if (Status != TaskStatus.OPENED)
        throw new InvalidOperationException("Task is not open");
    if (AuthorId == userId)
        throw new InvalidOperationException("Cannot take your own task");
    
    TakenBy = userId;
    TakenAt = DateTime.UtcNow;
    Status = TaskStatus.TAKEN;
}

public void Complete()
{
    if (Status != TaskStatus.TAKEN)
        throw new InvalidOperationException("Task is not taken");
    Status = TaskStatus.COMPLETED;
}

public void Cancel()
{
    if (Status == TaskStatus.COMPLETED)
        throw new InvalidOperationException("Cannot cancel completed task");

    Status = TaskStatus.CANCELLED;
}
public void Abandon()
{
   if (Status != TaskStatus.TAKEN)
        throw new InvalidOperationException("Cannot cancel completed task");
    TakenBy = null;
    TakenAt = null;
    Status = TaskStatus.OPENED;
}
}
public enum TaskStatus
{
    OPENED, TAKEN, CANCELLED, COMPLETED, ADANDONED
}
