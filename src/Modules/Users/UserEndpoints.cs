using Microsoft.AspNetCore.Mvc;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Users.Application;
using src.Modules.Shared;
using src.Modules.Users.Application;

namespace PeopleOnDuty.src.Modules.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth")
        .WithTags("Users");
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IUserService userService, CancellationToken ct) =>
        {
            var user = await userService.GetUserAsync(id, ct);
            if (user is null) return Results.NotFound(new { error = "Пользователь не найден" });

            return Results.Ok(new { user.UserId, user.Name });
        });

        group.MapPost("/batchList", async ([FromBody] IEnumerable<Guid> ids, IUserService userService, CancellationToken ct) =>
        {
            var users = await userService.GetUsersAsync(ids, ct);
            return Results.Ok(users.Select(u => new { u.UserId, u.Name }));
        });
        authGroup.MapPost("/login", async (
     LoginRequest request,
     IUserService userService,
     HttpContext http,
     CancellationToken ct) =>
 {
     var auth = await userService.Login(request, ct);

     http.Response.Cookies.Append("jwt_token", auth.Token, new CookieOptions
     {
         HttpOnly = true,
         Secure = true,
         SameSite = SameSiteMode.Lax,
         Expires = DateTime.UtcNow.AddHours(24),
         Path = "/"
     });

     return Results.Ok(new { auth.UserId });
 });
        authGroup.MapPost("/register", async (
            RegisterRequest request,
            IUserService userService,
            CancellationToken ct,
            HttpContext http) =>
        {
            var auth = await userService.Register(request, ct);
            http.Response.Cookies.Append("jwt_token", auth.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(24),
                Path = "/"
            });
            return Results.Ok(new { auth.UserId });
        });
        authGroup.MapPost("/link-telegram", (
    ILinkCodeService linkService,
    ICurrentUser currentUser) =>
{
    var code = linkService.Generate(currentUser.UserId);
    return Results.Ok(new { code });
}).RequireAuthorization();
        return app;
    }
}