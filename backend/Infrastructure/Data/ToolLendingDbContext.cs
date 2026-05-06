using Microsoft.EntityFrameworkCore;
using ToolLendingPlatform.Domain;

namespace ToolLendingPlatform.Infrastructure.Data
{
    public class ToolLendingDbContext : DbContext
    {
        public ToolLendingDbContext(DbContextOptions<ToolLendingDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        // Future: DbSet<Tool>, DbSet<BorrowRequest>, etc.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
