using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot.Handlers
{
  public class PendingActionQueue
{
    private readonly ConcurrentQueue<PendingAction> _queue = new();

    public void Enqueue(PendingAction action) => _queue.Enqueue(action);
    public void Enqueue(StateType stateType,Update update, UserState userState)
        {
             _queue.Enqueue(new PendingAction(stateType,update,userState));
        }

    public bool TryDequeue(out PendingAction? action) => _queue.TryDequeue(out action);

    public bool IsEmpty => _queue.IsEmpty;
}
}
