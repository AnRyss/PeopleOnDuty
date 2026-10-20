using Telegram.Bot;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public partial class UpdateHandler(IUserStateRepository userStateRepository,ITelegramBotClient bot )
    { 
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
            case "CreatingTask_WaitingName":
                await HandleCreatingTaskNameAsync(update, state, ct);
                break;
          
        }
    }
    }
}