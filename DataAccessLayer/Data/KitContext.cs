using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    /// <summary>
    /// Unable to create a 'DbContext' of type 'KitContext'.
    /// The exception 'Unable to resolve service for type 
    /// 'Microsoft.EntityFrameworkCore.DbContextOptions`1[DataAccessLayer.ApplicationDbContext]' 
    /// while attempting to activate 'DataAccessLayer.ApplicationDbContext'.' was thrown while attempting to create an instance.
    /// For the different patterns supported at design time, see https://go.microsoft.com/fwlink/?linkid=851728
    /// </summary>
    public class KitContext : DbContext
    {

        //public KitContext(DbContextOptions<KitContext> options) : base(options)
        //{
        //}

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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Ensure correct path
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

                string connectionString = config.GetConnectionString("KitContext");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
