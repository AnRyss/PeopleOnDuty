using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
  public record UserState // Точно не sctruct, кэш полетит
  {
    public UserState(long telegramUserId)
    {
      this.TelegramUserId = telegramUserId;
    }
    public UserState()
    {
    }
    public long TelegramUserId { get; set; }

    // Временные данные для создания задачи
    public string? DraftName { get; set; }
    public string? DraftDescription { get; set; }
    public string? DraftReward { get; set; }
    public bool DraftIsPublic { get; set; } = false;
    public string? WaitingForField { get; set; }
    public int? TempMessageId { get; set; }
    public string? PendingTaskId { get; set; }

    public void Reset()
    {
      TempMessageId = null;
      DraftName = null;
      DraftDescription = null;
      DraftReward = null;
      DraftIsPublic = false;
      WaitingForField = null;
      PendingTaskId = null;
    }
    public void ResetAllButDraft()
    {
      WaitingForField = null;
      PendingTaskId = null;
    }
  }
}