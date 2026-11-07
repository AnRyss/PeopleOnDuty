using PeopleOnDuty.src.Modules.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PeopleOnDuty.src.Modules.Tasks;

public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization();

        group.MapGet("/open", async (ITaskService taskService, CancellationToken ct) =>
        {
            var tasks = await taskService.GetByStatusAsync(TaskStatus.OPENED, ct);
            return Results.Ok(tasks);
        });

        group.MapGet("/{id:guid}", async (Guid id, ITaskService taskService, CancellationToken ct) =>
        {
            var task = await taskService.GetTaskAsync(id, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPost("/", async (CreateTaskRequest request, ITaskService taskService, ICurrentUser currentUser, CancellationToken ct) =>
        {
            var createdTask = new TaskBuilder()
                .WithName(request.Name)
                .WithDescription(request.Description)
                .WithReward(request.Reward)
                .WithPublicy(request.IsPublic)
                .Build(currentUser.UserId);

            var taskId = await taskService.CreateTaskAsync(createdTask, ct);
            return Results.Created($"/api/tasks/{taskId}", new { id = taskId });
        });

        group.MapPost("/{id:guid}/take", async (Guid id, ITaskService taskService, ICurrentUser currentUser, CancellationToken ct) =>
        {
            await taskService.TakeTaskAsync(id, currentUser.UserId, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/complete", async (Guid id, ITaskService taskService, CancellationToken ct) =>
        {
            await taskService.CompleteTaskAsync(id, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, ITaskService taskService, CancellationToken ct) =>
        {
            await taskService.CancelTaskAsync(id, ct);
            return Results.NoContent();
        });

        return app;
    }
}

public record CreateTaskRequest(string Name, string? Description, string Reward, bool IsPublic);