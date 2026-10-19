using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Tasks
{
    public static class TaskModuleExtension
    {
        public static IServiceCollection AddTaskModule(this IServiceCollection services, string connectionString)
        {
             services.AddDbContext<TasksContext>(opt => opt.UseNpgsql(connectionString));
             services.AddScoped<ITaskRepository,TaskRepository>();
             services.AddScoped<ITaskService, TaskService>(); 
               return services;
        }
    }
}