using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public record  UserState // Точно не sctruct, кэш полетит
    {
      
      public long TelegramUserId { get; set; }
    public StateType CurrentState { get; set; } = StateType.MainMenu;
    
    // Временные данные для создания задачи
    public string? DraftName { get; set; }
    public string? DraftDescription { get; set; }
    public string? DraftReward { get; set; }
    public bool DraftIsPublic { get; set; } = false; 
    public string? WaitingForField { get; set; } 
    }
}