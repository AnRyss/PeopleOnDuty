using System;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot;

public class TelegrammBotService(TelegramBotClient bot, IServiceProvider provider) : BackgroundService
{
    private readonly IServiceProvider _provider = provider;
    private readonly TelegramBotClient _bot = bot;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            cancellationToken: stoppingToken
        );
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
     private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
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