using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public partial class AdminScenario
{
    private async Task StartBroadcastAsync(CallbackQuery callback, UserState state, CancellationToken ct)
    {
        state.WaitingForField = "admin_broadcast_text";
        await userStateRepository.SaveAsync(state, ct);
        await bot.EditMessageText(callback.Message!.Chat.Id, callback.Message.MessageId, "Введите текст сообщения для рассылки:", cancellationToken: ct);
    }

    private async Task ProcessBroadcastAsync(Update update, UserState state, string text, CancellationToken ct)
    {
        state.WaitingForField = null;
        logger.LogWarning("Админ {TgId} запустил рассылку!", state.TelegramUserId);
        await userStateRepository.SaveAsync(state, ct);
        await bot.SendMessage(update.Message!.Chat.Id, "🚀 Рассылка запущена...", cancellationToken: ct);

        int skip = 0;
        const int batchSize = 100;
        int success = 0;
        int failed = 0;

        // Fire-and-forget
        _ = Task.Run(async () =>
        {
            while (true)
            {
                var telegramIds = await userService.GetTelegramIdsBatchAsync(skip, batchSize, ct);
                if (telegramIds.Count == 0) break;

                foreach (var tgId in telegramIds)
                {
                    try
                    {
                        await bot.SendMessage(tgId, text, cancellationToken: ct);
                        success++;
                        await Task.Delay(50, ct);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.ErrorCode == 429)
                    {
                        var retryAfter = ex.Parameters?.RetryAfter ?? 5;
                        await Task.Delay(TimeSpan.FromSeconds(retryAfter), CancellationToken.None);
                        failed++;
                    }
                    catch { failed++; }
                }
                skip += batchSize;
            }

            await bot.SendMessage(update.Message.Chat.Id, $"✅ Рассылка завершена.\nДоставлено: {success}\nОшибок: {failed}", cancellationToken: CancellationToken.None);
        }, ct);
    }
}