using Microsoft.EntityFrameworkCore;
using Server.Data.Entities;

namespace Server.Data.DBManager
{
    public class DBSetup : DbContext
    {
        public DBSetup(DbContextOptions<DBSetup> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ScheduledEvent> ScheduledEvents { get; set; }
        public DbSet<IoTEvent> IoTEvents { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<TriggeredEvent> TriggeredEvents { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Default connection string");
            }
        }
    }


}

