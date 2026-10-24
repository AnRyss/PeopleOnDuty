using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks.Infrastructure;
using PeopleOnDuty.src.Modules.Tasks.Infrastructure.CompletedTasks;

namespace PeopleOnDuty.src.Modules.Tasks
{
    public static class TaskModuleExtension
    {
        public static IServiceCollection AddTaskModule(this IServiceCollection services, string connectionString)
        {
             services.AddDbContext<TasksContext>(opt => opt.UseNpgsql(connectionString));
             services.AddScoped<ITaskRepository,TaskRepository>();
             services.AddScoped<ITaskService, TaskService>(); 

             // Completed Tasks
             services.AddDbContext<CompletedTaskContext>(opt => opt.UseNpgsql(connectionString));
             services.AddScoped<ICompletedTaskRepository,CompletedTaskRepository>();
             //services.AddScoped<ITaskService, TaskService>();  Кажется, делать тупую обертку над хранилищем готовых заявок не надо
               return services;
        }
    }
}