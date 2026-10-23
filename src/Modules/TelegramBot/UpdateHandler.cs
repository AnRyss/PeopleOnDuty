using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot;


public class UpdateHandler(ScenarioRouter _router, IUserStateRepository userStateRepository, ITelegramBotClient bot)
{


    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {

        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        if (userId is null) return;
        var state = await userStateRepository.GetOrCreateAsync(userId.Value, ct);
       
        // Обработка глобальных команд
        if (update.Message?.Text is { } text && text.StartsWith("/"))
    {
        await HandleGlobalCommandAsync(update, state, ct);
        await userStateRepository.SaveAsync(state, ct);
        return;
    }


        await _router.RouteAsync(update, state, ct);
        await userStateRepository.SaveAsync(state, ct);
    }



private async Task HandleGlobalCommandAsync(Update update, UserState state, CancellationToken ct)
{
    var text = update.Message!.Text;
    var chatId = update.Message.Chat.Id;
var mainkeyboardmenu =  new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("Создать задачу", "create_task") },
                    new[] { InlineKeyboardButton.WithCallbackData("Взять задачу", "take_task") },
                    new[] { InlineKeyboardButton.WithCallbackData("Взятые задачи", "taken_tasks") }
                });
    switch (text)
    {
        
        case "/start":
            state.CurrentState = StateType.MainMenu;
            await bot.SendMessage(chatId, "Привет! Что будем делать?",
                replyMarkup: mainkeyboardmenu, cancellationToken: ct);
            break;

        case "/tell_about":
            await bot.SendMessage(chatId, "Бот гарантирует...", cancellationToken: ct);
            break;

        case "/help":
            await bot.SendMessage(chatId, "Доступные команды: /start, /help, /tell_about", cancellationToken: ct);
            break;

        default:
            await bot.SendMessage(chatId, "Неизвестная команда. Нажмите /start.", cancellationToken: ct);
            break;
    }
   
            return;
        }

        }


