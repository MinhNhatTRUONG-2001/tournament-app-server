using Microsoft.EntityFrameworkCore;
using tournament_app_server.Models;

namespace tournament_app_server
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tournament> Tournaments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("data"); // Configure table schema
            modelBuilder.Entity<Tournament>().ToTable("tournaments");
            base.OnModelCreating(modelBuilder);
        }
    }
}
