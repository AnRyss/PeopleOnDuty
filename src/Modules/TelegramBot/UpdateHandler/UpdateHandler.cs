using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot{

    public partial class UpdateHandler(IUserStateRepository userStateRepository, ITelegramBotClient bot, ITaskService taskService, IUserService userService)
{
    private readonly  IUserService _userService = userService;
    private readonly ITaskService _taskService = taskService;
    private readonly ITelegramBotClient _bot = bot;
    private readonly IUserStateRepository _userStateRepository = userStateRepository;
    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {

        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        var text = update.Message?.Text;
        var callbackData = update.CallbackQuery?.Data;

        if (userId == null) return;


        var state = await _userStateRepository.GetOrCreateAsync(userId.Value, ct);


       switch (state.CurrentState)
{
    case "MainMenu": 
        await HandleMainMenuAsync(update, state, ct);
        break;
    case "CreatingTask_Menu":
        await HandleTaskCreationMenuAsync(update, state, ct); // Нажатия на кнопки
        break;
    case "CreatingTask_WaitingInput":
        await HandleTaskCreationInputAsync(update, state, ct); // Ввод текста
        break;
    case "" :
    break;
}
    }

    private async Task RenderTaskCreationMenuAsync(long chatId, int messageId, UserState state, CancellationToken ct)
    {
        var text = $"""
        **Создание новой задачи**
        
        Имя: {state.DraftName ?? "не задано"}
        Описание: {state.DraftDescription ?? "не задано"}
        Награда: {state.DraftReward ?? "не задано"}
        Публичная: {(state.DraftIsPublic ? "Да" : "Нет")}
        """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
        new[] { InlineKeyboardButton.WithCallbackData("Поменять имя", "edit_name") },
        new[] { InlineKeyboardButton.WithCallbackData("Поменять описание", "edit_desc") },
        new[] { InlineKeyboardButton.WithCallbackData("Поменять награду", "edit_reward") },
        new[] { InlineKeyboardButton.WithCallbackData($"Публичность: {(state.DraftIsPublic ? "Да" : "Нет")}", "toggle_public") },
        new[] { InlineKeyboardButton.WithCallbackData("✅ Создать задачу", "confirm_create") },
        new[] { InlineKeyboardButton.WithCallbackData("❌ Отмена", "cancel_create") }
    });

        await _bot.EditMessageText(chatId, messageId, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);

    }
}}
