using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class KitContext : DbContext
    {

        public KitContext(DbContextOptions<KitContext> options) : base(options)
        {
        }

        public DbSet<Profile> Profile { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<EventType> EventType { get; set; }
        public DbSet<ProfileAspNetUsers> ProfileAspNetUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>().ToTable("Profile");
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<EventType>().ToTable("EventType");
            modelBuilder.Entity<ProfileAspNetUsers>().ToTable("ProfileAspnetUsers");
        }
    }
}
