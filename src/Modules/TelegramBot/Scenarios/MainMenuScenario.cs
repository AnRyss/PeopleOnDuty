using System;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using PeopleOnDuty.src.Modules.Users.Application;
using Telegram.Bot;
using Telegram.Bot.Types;
namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public class MainMenuScenario(ITelegramBotClient bot, MainMenuRenderer mainMenuRenderer,
 IUserStateRepository userStateRepository, IUserService userService, ILinkCodeService linkCodeService,
  ILogger<MainMenuScenario> logger) : IScenario
{
    public IEnumerable<string> HandledCallbacks => new[] { "review" };
    public bool CanHandleInput(string? waitingForField) => false;

    public async Task HandleAsync(Update update, UserState state, CancellationToken ct)
    {
        // Текстовые команды
        if (update.Message?.Text is { } text)
        {

            switch (text)
            {
                case "/menu":
                case "/start":
                    state.Reset();
                    await userStateRepository.SaveAsync(state, ct);
                    await mainMenuRenderer.SendAsync(update.Message.Chat.Id, ct);
                    break;
                case "/tell_about":
                    state.Reset();
                    await userStateRepository.SaveAsync(state, ct);
                    await bot.SendMessage(update.Message.Chat.Id, "Бот гарантирует анонимность, но не гарантирует выполнение обязательств", cancellationToken: ct);
                    break;
                case "/help":
                    state.Reset();
                    await userStateRepository.SaveAsync(state, ct);
                    await bot.SendMessage(update.Message.Chat.Id, "Доступные команды: /menu, /help, /tell_about", cancellationToken: ct);
                    break;
                case var t when t.StartsWith("/link "):
                    await LinkTelegram(update,ct);
                    break;
                default:
                    state.Reset();
                    await userStateRepository.SaveAsync(state, ct);
                    await bot.SendMessage(update.Message.Chat.Id, "Неизвестная команда. Нажмите /menu.", cancellationToken: ct);
                    break;
            }
            return;
        }

        // Кнопки
        if (update.CallbackQuery is { } callback)
        {


            if (callback.Data == "review")
            {
                await bot.AnswerCallbackQuery(callback.Id, "Пока в разработке!", showAlert: true, cancellationToken: ct);
            }
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }
    }
    private async Task LinkTelegram(Update update, CancellationToken ct)
    {
        var code = update!.Message!.Text!.Substring(6).Trim().ToUpper();
        var telegramId = update.Message.From!.Id;
        var chatId = update.Message.Chat.Id;
        var userId = linkCodeService.Consume(code);
        if (userId == null)
        {
            await bot.SendMessage(chatId, "❌ Код неверный или истек.", cancellationToken: ct);
            return;
        }
        try
        {
            await userService.AttachTelegramIdAsync(userId.Value, telegramId, ct);
            await bot.SendMessage(chatId, "✅ Telegram привязан!", cancellationToken: ct);
        }
        catch (InvalidOperationException ex)
        {

            await bot.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при привязке Telegram");
            await bot.SendMessage(chatId, "❌ Внутренняя ошибка сервера.", cancellationToken: ct);
        }
    }
}