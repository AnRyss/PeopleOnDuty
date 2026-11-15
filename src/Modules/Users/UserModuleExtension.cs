using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Users.Application;
using src.Modules.Users.Application;

namespace PeopleOnDuty.src.Modules.Users
{
    public static class UserModuleExtension
    {
        public static IServiceCollection AddUserModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<UserContext>(opt => opt.UseNpgsql(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<TokenService>();
            services.AddSingleton<ILinkCodeService, InMemoryLinkCodeService>();
            return services;
        }
    }
}