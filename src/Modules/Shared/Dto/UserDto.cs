namespace src.Modules.Shared
{
    public record RegisterRequest(string Name, string Password);
    public record LoginRequest(string Name, string Password);
    public record AuthResponse(string Token, Guid UserId);
    public record UserDto(string UserId, string Name);
}