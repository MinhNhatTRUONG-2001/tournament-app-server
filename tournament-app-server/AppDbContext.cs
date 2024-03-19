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
        public DbSet<StageUserId> StageUserIds { get; set; }
        public DbSet<MatchSeUserId> MatchSeUserIds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("data"); // Configure table schema
            modelBuilder.Entity<Tournament>().ToTable("tournaments");
            modelBuilder.Entity<Stage>().ToTable("stages");
            modelBuilder.Entity<StageFormat>().ToTable("stage_format");
            modelBuilder.Entity<MatchSe>().ToTable("match_se");
            modelBuilder.Entity<StageUserId>().ToView("stage_user_id");
            modelBuilder.Entity<MatchSeUserId>().ToView("matchse_user_id");
            base.OnModelCreating(modelBuilder);
        }
    }
}
