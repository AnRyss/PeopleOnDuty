using System;
using System.Collections.Concurrent;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PeopleOnDuty.src.Modules.TelegramBot;

public class TelegramBotService(ITelegramBotClient bot, IServiceProvider provider
, ILogger<TelegramBotService> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var commands = new[]
{
    new BotCommand { Command = "menu", Description = "Запустить бота / Главное меню" },
    new BotCommand { Command = "tell_about", Description = "Помощь" }

};
        logger.LogInformation("TelegramBotService запущен");
        await bot.SetMyCommands(commands, cancellationToken: ct);
        bot.StartReceiving(
           updateHandler: (bot, update, ct) =>
    {
        // Многопоточная обработка
        _ = Task.Run(() => HandleUpdateAsync(bot, update, ct), ct);
        return Task.CompletedTask;
    },
            errorHandler: (bot, ex, ct) =>
              {

                  logger.LogError("Ошибка бота: {exc}", ex.Message);
                  return Task.CompletedTask;
              },
            cancellationToken: ct
        );
        await Task.Delay(Timeout.Infinite, ct);
    }
    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        Console.WriteLine($"UPDATE RECEIVED: {update.Message?.Text ?? update.CallbackQuery?.Data}");
        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        if (userId == null) return;


        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
        await handler.HandleUpdateAsync(update, ct);


    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        logger.LogError("Ошибка бота: {exc}", ex.Message);
        return Task.CompletedTask;
    }
}