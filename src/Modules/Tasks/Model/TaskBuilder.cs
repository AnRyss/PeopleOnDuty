namespace PeopleOnDuty.src.Modules.Tasks
{
    public  class TaskBuilder
   {
    private string? _description;
    private string? _name;
    private string? _reward;
    private DateTime? _untilTime;


    public TaskBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TaskBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TaskBuilder WithReward(string reward)
    {
        _reward = reward;
        return this;
    }

    public TaskBuilder WithUntilTime(DateTime untilTime)
    {
        _untilTime = untilTime;
        return this;
    }



    public DutyTask Build(Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Name is required");
        if (string.IsNullOrWhiteSpace(_reward))
            throw new InvalidOperationException("Reward is required");
        if (_untilTime is null)
            throw new InvalidOperationException("UntilTime is required");
        if ( authorId == Guid.Empty)
            throw new InvalidOperationException("AuthorId is required");

        
        return string.IsNullOrWhiteSpace(_description)
            ? new DutyTask(_name!, _reward!, _untilTime.Value, authorId)
            : new DutyTask(_description!, _name!, _reward!, _untilTime.Value, authorId);
    }
    }
}