using Microsoft.EntityFrameworkCore;

namespace PeopleOnDuty.src.Modules.Archive
{
    public class ArchiveTaskContext : DbContext
    {
         public ArchiveTaskContext(DbContextOptions<ArchiveTaskContext> options) : base(options)
    {

    }
    public DbSet<ArchivedTask> ArchiveTasks => Set<ArchivedTask>();
     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("archive");

        modelBuilder.Entity<ArchivedTask>(e =>
        {
            e.ToTable("ArchiveTasks");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OriginalTaskId).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Reward).HasMaxLength(200).IsRequired();
            e.Property(x => x.Answer).IsRequired();
            e.HasIndex(x => x.AuthorId);
        });
    }
    }
}