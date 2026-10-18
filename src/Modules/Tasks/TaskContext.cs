using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace PeopleOnDuty.src.Modules.Tasks;

public class TasksContext : DbContext
{
public TasksContext(DbContextOptions<TasksContext> options) : base(options)
    {
         
    }
    public DbSet<DutyTask> Tasks => Set<DutyTask>();
}
