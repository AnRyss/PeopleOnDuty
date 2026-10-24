using Microsoft.EntityFrameworkCore;

namespace PeopleOnDuty.src.Modules.Tasks.Infrastructure
{
    public class CompletedTaskContext : DbContext
    {
         public CompletedTaskContext(DbContextOptions<CompletedTaskContext> options) : base(options)
    {

    }
    public DbSet<DutyTask> CompletedTasks => Set<DutyTask>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Явно указываем, что TaskId — это первичный ключ
        modelBuilder.Entity<DutyTask>()
            .HasKey(t => t.TaskId);
    }
    }
}