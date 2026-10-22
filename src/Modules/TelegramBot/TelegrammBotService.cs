using System;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot;

public class TelegramBotService(ITelegramBotClient bot, IServiceProvider provider) : BackgroundService
{
    private readonly IServiceProvider _provider = provider;
    private readonly ITelegramBotClient _bot = bot;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var commands = new[]
{
    new BotCommand { Command = "start", Description = "Запустить бота / Главное меню" },
    new BotCommand { Command = "tell_about", Description = "Помощь" }
    // Добавь сюда свои команды, если они будут (например, "my_tasks")
};
         Console.WriteLine("TelegramBotService STARTED");
         await _bot.SetMyCommands(commands, cancellationToken: ct);
        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: (bot,ex,ct) =>
              {
            Console.WriteLine($"BOT ERROR: {ex.Message}");
            return Task.CompletedTask;
        },
            cancellationToken: ct
        );
        await Task.Delay(Timeout.Infinite, ct);
    }
     private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
            Console.WriteLine($"UPDATE RECEIVED: {update.Message?.Text ?? update.CallbackQuery?.Data}");
        using var scope = _provider.CreateScope();  
        var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>(); // DI scoped в singletone
        await handler.HandleUpdateAsync(update, ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        // Логирование
        return Task.CompletedTask;
    }
}