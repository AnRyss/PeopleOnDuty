using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot;


public class UpdateHandler(ScenarioRouter _router, IUserStateRepository userStateRepository)
{


    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        if (userId is null) return;
        var state = await userStateRepository.GetOrCreateAsync(userId.Value, ct);
        await _router.RouteAsync(update, state, ct);
        await userStateRepository.SaveAsync(state, ct);
    }


}


