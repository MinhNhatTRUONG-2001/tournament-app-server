using Microsoft.EntityFrameworkCore;
using tournament_app_server.Models;

namespace tournament_app_server
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<StageFormat> StageFormats { get; set; }
        public DbSet<MatchSe> MatchSes { get; set; }
        public DbSet<MatchRr> MatchRrs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("data"); // Configure table schema
            modelBuilder.Entity<Tournament>().ToTable("tournaments");
            modelBuilder.Entity<Stage>().ToTable("stages");
            modelBuilder.Entity<StageFormat>().ToTable("stage_format");
            modelBuilder.Entity<MatchSe>().ToTable("matches_se");
            modelBuilder.Entity<MatchRr>().ToTable("matches_rr");
            base.OnModelCreating(modelBuilder);
        }
    }
}
