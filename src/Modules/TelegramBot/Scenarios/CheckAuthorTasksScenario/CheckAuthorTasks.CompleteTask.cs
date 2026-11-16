using System;
using PeopleOnDuty.src.Modules.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot.Scenarios;

public partial class CheckAuthorTasksScenario
{
    private async Task HandleCompleteTaskAsync( //callback
        long chatId, int messageId, Guid taskId, UserState state, CancellationToken ct)
    {
        var user = await userService.GetOrCreateByTelegramIdAsync(state.TelegramUserId, ct);
        var task = await taskService.GetTaskAsync(taskId, ct);
        if (task is null || task.Value.AuthorId != user.UserId)
        {
            await bot.AnswerCallbackQuery("Задача не найдена или у вас нет прав.", showAlert: true, cancellationToken: ct);
            return;
        }
        if (task.Value.IsPublic)
        {
            state.WaitingForField = "archive_answer";
            state.PendingTaskId = task.Value.TaskId.ToString();
            await userStateRepository.SaveAsync(state, ct);

            await bot.EditMessageText(
                chatId,
                messageId,
                "Введите ответ на задачу (он будет сохранён в архиве):",
                cancellationToken: ct);
            return;
        }
        else
        {
            await taskService.CompleteTaskAsync(taskId, ct);
            await bot.EditMessageText(chatId, messageId, "✅ Задача завершена!", cancellationToken: ct);
        }



    }
    private async Task HandleArchiveAnswerAsync( // Input
       Update update, UserState state, string answer, CancellationToken ct)
    {
        if (state.PendingTaskId is not { } taskIdString) return;
        if (!Guid.TryParse(taskIdString, out var taskId)) return;

        var task = await taskService.GetTaskAsync(taskId, ct);
        if (task is null) return;

        var dto = new ArchivedTaskDto
        {
            OriginalTaskId = task.Value.TaskId,
            TaskName = task.Value.Name,
            TaskDescription = task.Value.Description,
            Reward = task.Value.Reward,
            AuthorId = task.Value.AuthorId,
            Answer = answer,
            CompletedAt = DateTime.UtcNow
        };

        var savedArchiveId = await archiveTaskService.SaveAsync(dto, ct);
        try
        {
            await taskService.CompleteTaskAsync(taskId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Непредвиденная ошибка при сохранении в архив задачи {taskId}", dto.OriginalTaskId);
            await archiveTaskService.DeleteAsync(savedArchiveId, ct);
            await bot.SendMessage(update.Message!.Chat.Id, "Ошибка при завершении задачи. Попробуйте позже.", cancellationToken: ct);
            return;
        }
        state.WaitingForField = null;
        state.PendingTaskId = null;
        await userStateRepository.SaveAsync(state, ct);
        logger.LogInformation("Задача {OriginalTaskId} сохранена в архив с айди {ArchiveId}", dto.OriginalTaskId, savedArchiveId);
        await bot.SendMessage(update.Message!.Chat.Id, "✅ Задача завершена и сохранена в архив!", cancellationToken: ct);
    }
}
