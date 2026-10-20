namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public record  UserState // Точно не sctruct, кэш полетит
    {
      
    public long TelegramUserId { get; set; } 
    public string CurrentState { get; set; } = "MainMenu";
    public string? TempData { get; set; }
    }
}