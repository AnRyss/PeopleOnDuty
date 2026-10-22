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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Явно указываем, что TaskId — это первичный ключ
        modelBuilder.Entity<DutyTask>()
            .HasKey(t => t.TaskId);
    }
}
