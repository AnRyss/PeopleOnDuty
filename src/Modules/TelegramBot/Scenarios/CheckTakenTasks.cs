using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios
{
    public class CheckTakenTasks(ITaskService taskService, IUserService userService, ITelegramBotClient bot, IUserStateRepository userStateRepository) : IScenario
    {
        public bool CanHandle(StateType state, Update update) => 
        state is StateType.CheckTakenTasks;
       


        public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
        {

            if (update.CallbackQuery is not { } callback) return;

            var chatId = callback.Message!.Chat.Id;
            var messageId = callback.Message.MessageId;
            var data = callback.Data;
        await RenderTakenTasks(chatId,state,ct);
        state.CurrentState = StateType.MainMenu;
        await userStateRepository.SaveAsync(state, ct);
      
        }
         public async Task RenderTakenTasks(long chatId, UserState state, CancellationToken ct)
{               
    var user =  await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
           
    var takenTasks = await taskService.GetMyTakenTasksAsync(user.UserId,ct);

   

    var buttons = takenTasks
        .Select(t => new[]
        {
            InlineKeyboardButton.WithCallbackData(
                $"{t.Name} — {t.Reward}",
                $"taken_task:{t.TaskId}")
        })
        .ToArray();

    var keyboard = new InlineKeyboardMarkup(buttons);

    await bot.SendMessage(
        chatId,
        "Выберите задачу, которую хотите посмотреть:",
        replyMarkup: keyboard,
        cancellationToken: ct);
}
        }
    }
