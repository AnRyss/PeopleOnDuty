using System;
using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Archive.Infrastructure;
using PeopleOnDuty.src.Modules.ArchiveTask;
using PeopleOnDuty.src.Modules.Shared;

namespace PeopleOnDuty.src.Modules.Archive;

public static class ArchiveModuleExtension
{
    public static IServiceCollection AddArchiveModule(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ArchiveTaskContext>(opt => opt.UseNpgsql(connectionString));
        services.AddScoped<IArchiveTaskRepository, ArchiveTaskRepository>();
        services.AddScoped<IArchiveTaskService, ArchiveTaskService>();
        return services;
    }
}