using PeopleOnDuty.src.Modules.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace PeopleOnDuty.src.Modules.Archive;

public static class ArchiveEndpoints
{
    public static IEndpointRouteBuilder MapArchiveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/archive")
            .WithTags("Archive")
            .RequireAuthorization();

       group.MapGet("/", async (
            [FromQuery] string? query, 
            [FromQuery] int page = 1,      
            [FromQuery] int pageSize = 5,  
            [FromServices] IArchiveTaskService archiveService = null!, 
            CancellationToken ct = default) =>
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