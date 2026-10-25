using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public record  UserState // Точно не sctruct, кэш полетит
    {
      
      public long TelegramUserId { get; set; }
    
    // Временные данные для создания задачи
    public string? DraftName { get; set; }
    public string? DraftDescription { get; set; }
    public string? DraftReward { get; set; }
    public bool DraftIsPublic { get; set; } = false; 
    public string? WaitingForField { get; set; } 
    public void resetDraft()
    {
     DraftName = null;
    DraftDescription = null;
    DraftReward = null;
    DraftIsPublic = false;
    WaitingForField = null;
    }
    }
}