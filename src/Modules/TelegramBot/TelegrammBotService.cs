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
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _userLocks = new();
    private readonly SemaphoreSlim _globalLock = new(initialCount: 20);
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
            updateHandler: HandleUpdateAsync,
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
        await _globalLock.WaitAsync(ct);
        var userLock = _userLocks.GetOrAdd(userId.Value, _ => new SemaphoreSlim(1, 1));

        await userLock.WaitAsync(ct);

        try
        {

            await _globalLock.WaitAsync(ct);

            try
            {
              
                using var scope = provider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
                await handler.HandleUpdateAsync(update, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке апдейта пользователя {UserId}", userId);
            }
            finally
            {
                // Освобождаем глобальный лок только если дошли сюда 
                _globalLock.Release();
            }
        }
        finally
        {
            // Освобождаем лок пользователя в любом случае (мы его точно взяли)
            userLock.Release();
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        logger.LogError("Ошибка бота: {exc}", ex.Message);
        return Task.CompletedTask;
    }
}