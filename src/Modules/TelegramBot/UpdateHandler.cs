using System.Collections.Concurrent;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using src.Modules.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot;


public class UpdateHandler(ScenarioRouter router, IUserStateRepository userStateRepository, ILogger<UpdateHandler> logger, UserLockProvider userLockProvider)
{


    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        if (userId is null) return;
        var userLock = userLockProvider.GetOrAdd(userId.Value);
        await userLock.WaitAsync(ct);
        try
        {
            var state = await userStateRepository.GetOrCreateAsync(userId.Value, ct);
            await router.RouteAsync(update, state, ct);
            await userStateRepository.SaveAsync(state, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке апдейта пользователя {UserId}", userId);
        }
        finally
        {
            userLock.Release();
        }
    }


}


