using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Archive;
using PeopleOnDuty.src.Modules.Archive.Infrastructure;
using PeopleOnDuty.src.Modules.ArchiveTask;
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
            services.AddDbContext<ArchiveTaskContext>((opt =>
            opt.UseSqlite("Data Source=peopleonduty.db")));

            services.AddDbContext<TasksContext>(opt =>
                opt.UseSqlite("Data Source=peopleonduty.db"));
            services.AddDbContext<UserContext>(opt =>
                opt.UseSqlite("Data Source=peopleonduty.db"));
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IArchiveTaskRepository, ArchiveTaskRepository>();
            services.AddScoped<IArchiveTaskService, ArchiveTaskService>();
            return services;
        }
    }
}