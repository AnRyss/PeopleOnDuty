using System;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public class MainMenuScenario( ITelegramBotClient _bot,  MainMenuRenderer _mainMenuRenderer) : IScenario
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
                    await _mainMenuRenderer.SendAsync(update.Message.Chat.Id, ct);
                    break;
                case "/tell_about":
                    await _bot.SendMessage(update.Message.Chat.Id, "Бот гарантирует анонимность, но не гарантирует выполнение обязательств", cancellationToken: ct);
                    break;
                case "/help":
                    await _bot.SendMessage(update.Message.Chat.Id, "Доступные команды: /menu, /help, /tell_about", cancellationToken: ct);
                    break;
                default:
                    await _bot.SendMessage(update.Message.Chat.Id, "Неизвестная команда. Нажмите /menu.", cancellationToken: ct);
                    break;
            }
            return;
        }

        // Кнопки
        if (update.CallbackQuery is { } callback)
        {
            if (callback.Data == "review")
            {
                await _bot.AnswerCallbackQuery(callback.Id, "Пока в разработке!", showAlert: true, cancellationToken: ct);
            }
            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }
    }
}