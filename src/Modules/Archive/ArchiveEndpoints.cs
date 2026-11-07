using PeopleOnDuty.src.Modules.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PeopleOnDuty.src.Modules.Archive;

public static class ArchiveEndpoints
{
    public static IEndpointRouteBuilder MapArchiveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/archive")
            .WithTags("Archive")
            .RequireAuthorization();

        group.MapGet("/", async (string? query, int page, int pageSize, IArchiveTaskService archiveService, CancellationToken ct) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 5 : pageSize;

            var (items, total) = await archiveService.SearchAsync(query, page, pageSize, ct);
            return Results.Ok(new { items, total, page, pageSize });
        });

        group.MapGet("/{id:guid}", async (Guid id, IArchiveTaskService archiveService, CancellationToken ct) =>
        {
            var task = await archiveService.GetByIdAsync(id, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        return app;
    }
}