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
        { var userEntity =  modelBuilder.Entity<User>();
            userEntity
    .HasIndex(u => u.Name)
    .IsUnique();
    userEntity.HasIndex(u => u.TelegramId)
    .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

    }

}