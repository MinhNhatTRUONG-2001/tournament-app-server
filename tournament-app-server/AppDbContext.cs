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
        public DbSet<StageUserId> StageUserIds { get; set; }
        public DbSet<MatchSeUserId> MatchSeUserIds { get; set; }
        public DbSet<MatchRrUserId> MatchRrUserIds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("data"); // Configure table schema
            modelBuilder.Entity<Tournament>().ToTable("tournaments");
            modelBuilder.Entity<Stage>().ToTable("stages");
            modelBuilder.Entity<StageFormat>().ToTable("stage_format");
            modelBuilder.Entity<MatchSe>().ToTable("matches_se");
            modelBuilder.Entity<MatchRr>().ToTable("matches_rr");
            modelBuilder.Entity<StageUserId>().ToView("stage_user_id").HasNoKey();
            modelBuilder.Entity<MatchSeUserId>().ToView("matchse_user_id").HasNoKey();
            modelBuilder.Entity<MatchRrUserId>().ToView("matchrr_user_id").HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}
