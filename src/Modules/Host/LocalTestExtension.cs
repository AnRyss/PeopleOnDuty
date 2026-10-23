using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using PeopleOnDuty.src.Modules.Users;

namespace PeopleOnDuty.src.Modules.Host
{
    public static class LocalTestExtension
    {
        public static IServiceCollection AddLocalContext(this IServiceCollection services)
        {
            // Для локальной разработки — файл SQLite
            services.AddDbContext<TasksContext>(opt =>
                opt.UseSqlite("Data Source=peopleonduty.db"));
            services.AddDbContext<UserContext>(opt =>
                opt.UseSqlite("Data Source=peopleonduty.db"));
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}