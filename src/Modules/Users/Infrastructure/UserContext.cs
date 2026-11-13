using Microsoft.EntityFrameworkCore;

namespace PeopleOnDuty.src.Modules.Users
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {

        }
        public DbSet<User> Users => Set<User>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
    .HasIndex(u => u.Name)
    .IsUnique();
            base.OnModelCreating(modelBuilder);
        }

    }

}