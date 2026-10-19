using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Users
{
    public static class UserModuleExtension
    {
        public static IServiceCollection AddUserModule(this IServiceCollection services, string connectionString)
        {
              services.AddDbContext<UserContext>(opt => opt.UseNpgsql(connectionString));
             services.AddScoped<IUserRepository,UserRepository>();
             services.AddScoped<IUserService, UserService>(); 
             return services;
        }
    }
}