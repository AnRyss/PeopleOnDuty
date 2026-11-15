using System.Security.Claims;
using PeopleOnDuty.src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Host
{
    public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        public Guid UserId
        {
            get
            {
                // Достаём пользователя из текущего HTTP-запроса
                var user = httpContextAccessor.HttpContext?.User;

                // Ищем claim, в который мы клали UserId при генерации токена
                var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Если claim нет или он не парсится в Guid — значит, пользователь не аутентифицирован
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                throw new UnauthorizedAccessException("Пользователь не аутентифицирован");
            }
        }

    }
}