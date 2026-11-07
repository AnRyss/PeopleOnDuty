using PeopleOnDuty.src.Modules.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PeopleOnDuty.src.Modules.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IUserService userService, CancellationToken ct) =>
        {
            var user = await userService.GetUserAsync(id, ct);
            if (user is null) return Results.NotFound(new { error = "Пользователь не найден" });
            
            return Results.Ok(new { user.UserId, user.Name });
        });

        group.MapPost("/batch", async (IEnumerable<Guid> ids, IUserService userService, CancellationToken ct) =>
        {
            var users = await userService.GetUsersAsync(ids, ct);
            return Results.Ok(users.Select(u => new { u.UserId, u.Name }));
        });

        return app;
    }
}