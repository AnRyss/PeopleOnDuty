using System;

namespace PeopleOnDuty.src.Modules.Users;

public class User(string name)
{
    public Guid UserId{get;private set;} =   Guid.NewGuid();
    public string Name{get;init;} = name;
    public long? TelegramId { get; init;}
    public User(long telegramId) : this(telegramId.ToString())
    {   
        TelegramId =  telegramId;
    }
}

